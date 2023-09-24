using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace VMail.Utils.Web
{
    public class VMailWebUploader : MonoBehaviour
    {
        [SerializeField]
        private InputField msgName;
        [SerializeField]
        private TMP_InputField dirUrlText;
        [SerializeField]
        private Button dirUrlCopy;
        [SerializeField]
        private ProgressBar progressBar;

        [Space(10)]
        [SerializeField]
        private VMailWebManager manager;


        public void SetUploadMode(bool overwrite)
        {
            this.msgName.interactable = !overwrite;

            if (overwrite)
            {
                this.msgName.text = this.manager.currVMailData == null ? "" : this.manager.currVMailData.name;
            }
        }

        public void CopyDirectoryURL()
        {
            GUIUtility.systemCopyBuffer = this.dirUrlText.text;
        }

        private void OnEnable()
        {
            this.msgName.text = this.manager.currVMailData == null ? "" : this.manager.currVMailData.name;

            this.dirUrlText.text = this.manager.currVMailData == null ? "" : this.manager.currVMailData.GetDirectoryURL();
            this.dirUrlCopy.interactable = this.manager.currVMailData != null;
        }

        public void CreateOrUpload()
        {
            if (string.IsNullOrEmpty(this.msgName.text))
            {
                return;
            }

            if (this.msgName.interactable) // create a vmail
            {
                this.manager.SaveVMail(this.msgName.text);
            }
            else // update an existing vmail
            {
                this.manager.UpdateVMail();
            }
        }

    }
}