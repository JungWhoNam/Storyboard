using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace VMail.Utils.Web
{
    public class VMailWebSharer : MonoBehaviour
    {
        [SerializeField]
        private VMailWebManager manager;

        [SerializeField]
        private TMP_InputField dirUrlText;
        [SerializeField]
        private Button dirCopyBtn;

        private void OnEnable()
        {
            this.dirUrlText.text = this.manager.currVMailData == null ?
                "Please open and edit a message first." : this.manager.currVMailData.GetDirectoryURL();

            this.dirCopyBtn.interactable = this.manager.currVMailData != null;
        }

        public void CopyDirectoryURL()
        {
            GUIUtility.systemCopyBuffer = this.dirUrlText.text;
        }

    }
}