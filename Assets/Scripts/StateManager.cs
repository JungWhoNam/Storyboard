using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;

public class StateManager : MonoBehaviour
{
    [SerializeField]
    private TCPServer server;
    [SerializeField]
    private List<ViewInfo> viewInfos = new();

    private Dictionary<int, byte[]> imageData = new();


    public void Process(string msg)
    {
        JObject j = JObject.Parse(msg);

        if (!j.ContainsKey("type") || !j.ContainsKey("action")) return;
        if ((string)j["type"] != "response") return;

        if ((string) j["action"] == "capture.view")
        {
            int idx = (int) j["snapshotIdx"];

            this.viewInfos[idx].UpdateView((JObject) j["camera"]);
        }
        else if ((string)j["action"] == "capture.image.setup")
        {
            int idx = (int)j["snapshotIdx"];
            //int w = (int)j["width"];
            //int h = (int)j["height"];
            int len = (int)j["length"];

            imageData[idx] = new byte[len];

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

            Debug.Log("Recieved a chunk... " + start + "~" + (start + chunck.Length));

            byte[] data = imageData[idx];
            for (int i = 0; i < chunck.Length; i++)
                data[start + i] = chunck[i];

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

            Debug.Log("Done capturing the image for " + idx);
            this.viewInfos[idx].SetImage(imageData[idx]);
            imageData.Remove(idx);
        }
    }

    public void SendReqestCapture(ViewInfo info)
    {
        JObject reqView = JObject.FromObject(new
        {
            type = "request",
            action = "capture.view",
            snapshotIdx = viewInfos.IndexOf(info)
        });
        this.server.Send(reqView.ToString(Formatting.None));

        JObject reqImage = JObject.FromObject(new
        {
            type = "request",
            action = "capture.image",
            snapshotIdx = viewInfos.IndexOf(info)
        });
        this.server.Send(reqImage.ToString(Formatting.None));
    }

    public void SendReqestUpdate(ViewInfo info)
    {
        JObject req = JObject.FromObject(new
        {
            type = "request",
            action = "update.view",
            snapshotIdx = viewInfos.IndexOf(info),
            camera = info.GetView()
        });

        this.server.Send(req.ToString(Formatting.None));
    }

    public void SendReqestUpdate(TransitionInfo info)
    {
        JObject req = JObject.FromObject(new
        {
            type = "request",
            action = "update.transition",
            amount = info.slider.value,
            from = info.from.GetView(),
            to = info.to.GetView()
        });

        this.server.Send(req.ToString(Formatting.None));
    }
}