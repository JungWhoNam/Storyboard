using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;
using UnityEngine.Events;

namespace VMail
{
    public class StateManager : MonoBehaviour
    {
        [SerializeField]
        private TCPServer server;

        [SerializeField]
        private Story story;

        [SerializeField]
        private Utils.ProgressBar progressBar;

        public JObject viewData { get; private set; }
        public byte[] imageData { get; private set; }

        public UnityEvent<int> onViewDataReceived;
        public UnityEvent<int> onImageDataReceived;

        public void Process(string msg)
        {
            JObject j = JObject.Parse(msg);

            if (!j.ContainsKey("type") || !j.ContainsKey("action")) return;
            if ((string)j["type"] != "response") return;

            if ((string)j["action"] == "capture.view")
            {
                int idx = (int)j["snapshotIdx"];
                viewData = (JObject)j["camera"];

                if (progressBar != null)
                    progressBar.Finish("received the view data.");

                onViewDataReceived.Invoke(idx);
            }
            else if ((string)j["action"] == "capture.image.setup")
            {
                int idx = (int)j["snapshotIdx"];
                //int w = (int)j["width"];
                //int h = (int)j["height"];
                int len = (int)j["length"];

                imageData = new byte[len];

                JObject reqView = JObject.FromObject(new
                {
                    type = "ack",
                    action = "capture.image.setup",
                    snapshotIdx = idx
                });
                this.server.Send(reqView.ToString(Formatting.None));
            }
            else if ((string)j["action"] == "capture.image.data")
            {
                int idx = (int)j["snapshotIdx"];
                int start = (int)j["start"];
                byte[] chunck = ((JArray)j["data"]["bytes"]).ToObject<byte[]>();

                //Debug.Log("Recieved a chunk... " + start + "~" + (start + chunck.Length));
                if (progressBar != null)
                    progressBar.SetPercentage((float)start / imageData.Length, "received the image data chunk.");

                for (int i = 0; i < chunck.Length; i++)
                    imageData[start + i] = chunck[i];

                JObject reqView = JObject.FromObject(new
                {
                    type = "ack",
                    action = "capture.image.data",
                    snapshotIdx = idx
                });
                this.server.Send(reqView.ToString(Formatting.None));
            }
            else if ((string)j["action"] == "capture.image.done")
            {
                int idx = (int)j["snapshotIdx"];

                //Debug.Log("Done capturing the image for " + idx);
                if (progressBar != null)
                    progressBar.Finish("received the image.");

                onImageDataReceived.Invoke(idx);
            }
        }

        public void SendReqestCaptureView(int pageIndex)
        {
            JObject reqView = JObject.FromObject(new
            {
                type = "request",
                action = "capture.view",
                snapshotIdx = pageIndex
            });
            this.server.Send(reqView.ToString(Formatting.None));

            if (progressBar != null)
                progressBar.Initialize(1, "Capturing the current view...", "sent a request to the client.");
        }

        public void SendReqestCaptureImage(int pageIndex)
        {
            JObject reqImage = JObject.FromObject(new
            {
                type = "request",
                action = "capture.image",
                snapshotIdx = pageIndex
            });
            this.server.Send(reqImage.ToString(Formatting.None));

            if (progressBar != null)
                progressBar.Initialize(100, "Capturing the current image...", "sent a request to the client.");
        }

        public void SendReqestUpdate(Page pg)
        {
            JObject req = JObject.FromObject(new
            {
                type = "request",
                action = "update.view",
                snapshotIdx = story.pages.IndexOf(pg),
                camera = pg.viewInfo
            });
            this.server.Send(req.ToString(Formatting.None));
        }

        public void SendReqestUpdate(Transition t)
        {
            JObject req = JObject.FromObject(new
            {
                type = "request",
                action = "update.transition",
                amount = t.amt,
                from = t.from.viewInfo,
                to = t.to.viewInfo
            });
            this.server.Send(req.ToString(Formatting.None));
        }

        public void ClearCache()
        {
            imageData = null;
            viewData = null;
        }

    }
}