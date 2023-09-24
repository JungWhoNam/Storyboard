using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace VMail.Utils.Web
{
    public class VMailWebManager : MonoBehaviour
    {
        [SerializeField]
        private WebIntegration webIntegration;
        [SerializeField]
        private StoryEditor storyEditor;
        [SerializeField]
        private VMailWebUploader uploader;

        [Space(10)]
        [SerializeField]
        private GameObject container;
        [SerializeField]
        private VMailWeb baseVMailWeb;

        [Space(10)]
        [SerializeField]
        private Button saveBtn;
        [SerializeField]
        private Button saveAsBtn;

        public List<VMailWeb> vMailWebs = new List<VMailWeb>();
        public VMailData currVMailData { get; private set; }


        private void OnEnable()
        {
            this.RefreshAvailableVMails();
        }

        private async void RefreshAvailableVMails()
        {
            // clear the existing vmails
            foreach (VMailWeb vMailWeb in this.vMailWebs)
            {
                GameObject.Destroy(vMailWeb.gameObject);
            }
            this.vMailWebs.Clear();

            // get the list of available vmails
            List<VMailData> res = await this.webIntegration.GetAvailableVMails();
            if (res == null)
                return;

            // populate with the new ones
            foreach (VMailData data in res)
            {
                VMailWeb vMailWeb = GameObject.Instantiate<VMailWeb>(this.baseVMailWeb);
                vMailWeb.Init(data);
                vMailWeb.transform.SetParent(this.container.transform, false);
                vMailWeb.gameObject.name = "vmail - " + data.name;
                vMailWeb.gameObject.SetActive(true);
                this.vMailWebs.Add(vMailWeb);
            }
        }

        public void FilterMessages(string filter)
        {
            string input = filter.Trim();

            foreach (VMailWeb vMailWeb in this.vMailWebs)
            {
                if (input.Trim() == "")
                {
                    vMailWeb.gameObject.SetActive(true);
                }
                else // check for matches
                {
                    if (vMailWeb.vMailData.name.IndexOf(input, System.StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        vMailWeb.gameObject.SetActive(true);
                    }
                    else if (vMailWeb.vMailData.GetDirectoryURL().IndexOf(input, System.StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        vMailWeb.gameObject.SetActive(true);
                    }
                    else
                    {
                        vMailWeb.gameObject.SetActive(false);
                    }
                }
            }
        }

        public void SetCurrentMessage(VMailData data)
        {
            this.currVMailData = data;

            this.saveBtn.interactable = true;
            this.saveAsBtn.interactable = this.currVMailData != null;
        }

        public void ResetCurrentMessage()
        {
            this.SetCurrentMessage(null);
        }

        public void SaveCurrentMessage(bool overwrite)
        {
            if (this.currVMailData == null)
            {
                this.uploader.SetUploadMode(false);
            }
            else
            {
                this.uploader.SetUploadMode(overwrite);
            }

            this.uploader.gameObject.SetActive(true);
        }

        public async void OpenVMail(VMailWeb vMailWeb)
        {
            // download the folder
            bool res = await this.webIntegration.DownloadVMailDir(vMailWeb.vMailData.ID);
            if (!res)
                return;

            //open the downloaded folder
            this.storyEditor.Load(this.webIntegration.GetVMailDirLocal(vMailWeb.vMailData.ID));

            // set the current message
            this.SetCurrentMessage(vMailWeb.vMailData);
        }


        public async void SaveVMail(string name)
        {
            // add the information into the db
            int id = await this.webIntegration.InsertToVMailDB(name);
            if (id == -1)
                return;

            // save the current story into a local directory
            this.storyEditor.RemoveAndSave(this.webIntegration.GetVMailDirLocal(id));

            // upload the directory to the server
            bool res = await this.webIntegration.UploadVMailDir(id);
            if (!res)
                return;

            // set the new message
            this.SetCurrentMessage(new VMailData(id, name));
        }

        public async void UpdateVMail()
        {
            if (this.currVMailData == null)
            {
                Debug.LogWarning("the current vmail data is null.");
                return;
            }

            // save the vmail files
            this.storyEditor.RemoveAndSave(this.webIntegration.GetVMailDirLocal(this.currVMailData.ID));

            // upload the folder
            bool res = await this.webIntegration.UploadVMailDir(this.currVMailData.ID);
            if (!res)
                return;

            // update the information in the db
            this.currVMailData.lastModifiedDesktop = DateTime.UtcNow;
            await this.webIntegration.UpdateVMailDB(this.currVMailData);
        }

    }
}