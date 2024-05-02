using Newtonsoft.Json.Linq;
using System;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace VMail.Utils.Web
{
    public class WebIntegration : MonoBehaviour
    {
        public static readonly string ServerURL = "http://localhost/"; // Make sure to put "/" at the end.
        public static readonly string ServerDataDir = "data/"; // Make sure to put "/" at the end.

        [SerializeField]
        private ProgressBar progressBar;


        private static async Task<string> Get(string url)
        {
            try
            {
                using var req = UnityWebRequest.Get(url);
                req.SendWebRequest();
                while (!req.isDone)
                    await Task.Yield();

                if (req.result != UnityWebRequest.Result.Success)
                {
                    Debug.LogError($"Failed: {req.error}");
                    return null;
                }

                return req.downloadHandler.text;
            }
            catch (Exception ex)
            {
                Debug.LogError($"Failed: {ex.Message}");
                return null;
            }
        }

        private static async Task<string> Post(string url, WWWForm form)
        {
            try
            {
                using var req = UnityWebRequest.Post(url, form);
                req.SendWebRequest();
                while (!req.isDone)
                    await Task.Yield();

                if (req.result != UnityWebRequest.Result.Success)
                {
                    Debug.LogError($"Failed: {req.error}");
                    return null;
                }

                return req.downloadHandler.text;
            }
            catch (Exception ex)
            {
                Debug.LogError($"Failed: {ex.Message}");
                return null;
            }
        }

        public string GetVMailDirLocal(int vmailID)
        {
            string dataDirPath = Path.Combine(Application.persistentDataPath, "vmails");

            if (!Directory.Exists(dataDirPath))
                Directory.CreateDirectory(dataDirPath);

            return Path.Combine(dataDirPath, vmailID.ToString());
        }

        // ---------------------------------------------- DB related ----------------------------------------------

        public async Task<List<VMailData>> GetAvailableVMails()
        {
            var res = await WebIntegration.Get(WebIntegration.ServerURL + "GetVMails.php");
            if (res == null || res == "-1")
                return null;

            JArray jsonArray = JArray.Parse(res);

            List<VMailData> vmails = new List<VMailData>();
            if (jsonArray != null)
            {
                for (int i = 0; i < jsonArray.Count; i++)
                {
                    JObject curr = (JObject)jsonArray[i];

                    vmails.Add(
                        new VMailData(
                            (int)curr["ID"],
                            (string)curr["name"],
                            (string)curr["lastModifiedDesktop"],
                            (string)curr["lastModifiedMobile"],
                            (string)curr["lastModifiedServer"]
                        ));
                }
            }

            return vmails;
        }

        public async Task<int> InsertToVMailDB(string name)
        {
            WWWForm form = new();
            form.AddField("name", name);
            form.AddField("lastModifiedDesktop", DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss"));
            form.AddField("lastModifiedMobile", DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss"));
            form.AddField("lastModifiedServer", DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss"));

            var res = await WebIntegration.Post(WebIntegration.ServerURL + "CreateVMail.php", form);
            if (res == null || res == "-1")
            {
                Debug.LogWarning("failed to add a row in the db: " + name);
                return -1;
            }

            return int.TryParse(res, out int id) ? id : -1;
        }

        public async Task<bool> UpdateVMailDB(VMailData vMailData)
        {
            WWWForm form = new();
            form.AddField("ID", vMailData.ID.ToString());
            form.AddField("name", vMailData.name);
            form.AddField("lastModifiedDesktop", vMailData.lastModifiedDesktop.ToString("yyyy-MM-dd HH:mm:ss"));
            form.AddField("lastModifiedMobile", vMailData.lastModifiedMobile.ToString("yyyy-MM-dd HH:mm:ss"));
            form.AddField("lastModifiedServer", vMailData.lastModifiedServer.ToString("yyyy-MM-dd HH:mm:ss"));

            var res = await WebIntegration.Post(WebIntegration.ServerURL + "UpdateVMail.php", form);
            if (res == null || res == "-1")
            {
                Debug.LogWarning("failed to update the row in the db: " + name);
                return false;
            }
            return true;
        }


        // ---------------------------------------------- Download related ----------------------------------------------

        private async Task<bool> DownloadFile(string filePathInServer, string filePathInLocal)
        {
            WWWForm form = new();
            form.AddField("filePath", filePathInServer);

            try
            {
                using var req = UnityWebRequest.Post(WebIntegration.ServerURL + "GetFile.php", form);
                req.SendWebRequest();
                while (!req.isDone)
                    await Task.Yield();

                if (req.result != UnityWebRequest.Result.Success)
                {
                    Debug.LogError($"Failed: {req.error}");
                    return false;
                }
                if (req.downloadHandler.text == "-1" || req.downloadHandler.data.Length <= 0)
                {
                    Debug.LogWarning("failed to download the file: " + filePathInServer);
                    return false;
                }

                File.WriteAllBytes(filePathInLocal, req.downloadHandler.data);
                return true;
            }
            catch (Exception ex)
            {
                Debug.LogError($"Failed: {ex.Message}");
                return false;
            }
        }

        private async Task<List<string>> GetFileNamesInDir(string dirPath)
        {
            WWWForm form = new();
            form.AddField("dirPath", dirPath);

            var res = await WebIntegration.Post(WebIntegration.ServerURL + "GetFileNames.php", form);

            if (res == null || res == "-1")
            {
                Debug.LogWarning("failed to get names of files in the directory: " + dirPath);
                return null;
            }

            return res.Split(new char[] { ',' }).ToList<string>();
        }

        private async Task<bool> DownloadFilesInDir(string dirPathInServer, string dirPathInLocal)
        {
            // get the list of file names in the server directory
            List<string> fileNames = await GetFileNamesInDir(dirPathInServer);
            if (fileNames == null)
                return false;

            // download the files in the local directory
            List<Task<bool>> tasks = new();
            for (int i = 0; i < fileNames.Count; i++)
            {
                string fPathInServer = Path.Combine(dirPathInServer, fileNames[i]);
                string fPathInLocal = Path.Combine(dirPathInLocal, fileNames[i]);

                tasks.Add(this.DownloadFile(fPathInServer, fPathInLocal));
            }
            await Task<bool>.WhenAll(tasks);

            // return false if not all the files are downloaded.
            foreach (Task<bool> task in tasks)
                if (!task.Result)
                    return false;

            return true;
        }

        public async Task<bool> DownloadVMailDir(int vmailID)
        {
            string dirPathInLocal = this.GetVMailDirLocal(vmailID);
            string dirPathInServer = Path.Combine(WebIntegration.ServerDataDir, vmailID.ToString());

            if (this.progressBar != null)
                this.progressBar.Initialize(2, "Downloading a VMail folder...", "Deleting the previous local files...");

            // create the vmail directory
            if (Directory.Exists(dirPathInLocal))
                Directory.Delete(dirPathInLocal, true);
            Directory.CreateDirectory(dirPathInLocal);

            // download files in the local folder
            if (this.progressBar != null)
                this.progressBar.IncreaseCnt("Downloading the new files...");
            bool res = await this.DownloadFilesInDir(dirPathInServer, dirPathInLocal);

            // show the result
            if (this.progressBar != null)
            {
                this.progressBar.Finish(res ? "Finished downloading the new files..." : "Failed...");
                this.progressBar.Close();
            }
            return res;
        }


        // ---------------------------------------------- Upload related ----------------------------------------------

        private async Task<bool> UploadFile(string dirPathInServer, string filePathInLocal)
        {
            byte[] bytes;
            try
            {
                bytes = File.ReadAllBytes(filePathInLocal);
            }
            catch (Exception ex)
            {
                Debug.LogError($"Failed: {ex.Message}");
                return false;
            }

            WWWForm form = new();
            form.AddBinaryData("file", bytes, filePathInLocal);
            form.AddField("dirPath", dirPathInServer);

            var res = await WebIntegration.Post(WebIntegration.ServerURL + "UploadFile.php", form);
            if (res == null || res == "-1")
            {
                Debug.LogWarning($"Failed to upload the file: {filePathInLocal}");
                return false;
            }

            return true;
        }

        private async Task<bool> UploadFilesInDir(string dirPathInServer, string dirPathInLocal, string searchPattern = "*.*")
        {
            // get the file paths of the files in the local directory
            string[] fPathsInLocalDir;
            try
            {
                fPathsInLocalDir = Directory.GetFiles(dirPathInLocal, searchPattern);
            }
            catch (Exception ex)
            {
                Debug.LogError($"Failed: {ex.Message}");
                return false;
            }

            // download the files in the local directory
            List<Task<bool>> tasks = new();
            for (int i = 0; i < fPathsInLocalDir.Length; i++)
            {
                tasks.Add(this.UploadFile(dirPathInServer, fPathsInLocalDir[i]));
            }
            await Task<bool>.WhenAll(tasks);

            // return false if not all the files are uploaded
            foreach (Task<bool> task in tasks)
                if (!task.Result)
                    return false;

            return true;
        }

        public async Task<bool> UploadVMailDir(int vmailID)
        {
            string dirPathInLocal = this.GetVMailDirLocal(vmailID);
            string dirPathInServer = Path.Combine(WebIntegration.ServerDataDir, vmailID.ToString()); // e.g., "data/7"

            if (this.progressBar != null)
                this.progressBar.Initialize(2, "Uploading a VMail folder...", "Uploading the files in the local directory...");

            // upload files in the local folder
            bool res = await this.UploadFilesInDir(dirPathInServer, dirPathInLocal);

            // show the result
            if (this.progressBar != null)
            {
                this.progressBar.Finish(res ? "Finished uploading the files..." : "Failed...");
                this.progressBar.Close();
            }
            return res;
        }

    }
}