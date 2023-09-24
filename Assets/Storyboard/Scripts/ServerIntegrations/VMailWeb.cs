using TMPro;
using UnityEngine;

namespace VMail.Utils.Web
{
    public class VMailWeb : MonoBehaviour
    {
        [SerializeField]
        private TMP_Text description;
        [SerializeField]
        private TMP_Text dirUrlText;

        public VMailData vMailData { get; private set; }

        public void Init(VMailData vMailData)
        {
            this.vMailData = vMailData;

            this.description.text = vMailData.name + ", " + vMailData.lastModifiedDesktop.ToString("yyyy-MM-dd HH:mm:ss");
            this.dirUrlText.text = vMailData.GetDirectoryURL();
        }

        public void CopyDirectoryURL()
        {
            GUIUtility.systemCopyBuffer = this.dirUrlText.text;
        }

    }
}