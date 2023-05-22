using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StateManager : MonoBehaviour
{
    [SerializeField]
    private TCPServer server;
    [SerializeField]
    private TMPro.TMP_Text[] viewInfos;
    [SerializeField]
    private Button[] updateButtons;

    public void Process(string msg)
    {
        int idx = 0;
        this.viewInfos[idx].text = msg;
        this.updateButtons[idx].interactable = true;
    }

    public void SendCaptureAction(int idx)
    {
        RequestCaptureView info = new RequestCaptureView();
        info.type = "capture";
        info.snapshotIdx = idx;

        this.server.Send(JsonUtility.ToJson(info));
    }

    public void SendUpdateAction(int idx)
    {
        this.server.Send(this.viewInfos[idx].text);
    }

    [System.Serializable]
    public class RequestCaptureView
    {
        public string type;
        public int snapshotIdx;
    }
}