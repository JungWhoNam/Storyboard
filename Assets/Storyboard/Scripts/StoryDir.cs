using System.IO;
using UnityEngine;

namespace VMail
{
    public class StoryDir : MonoBehaviour
    {
        [SerializeField]
        private TMPro.TMP_Text title;
        [SerializeField]
        private TMPro.TMP_Text dirPath;

        public void Initialize(string dirPath)
        {
            this.title.text = Path.GetFileName(dirPath);
            this.dirPath.text = dirPath;
        }

        public string GetDirPath()
        {
            return dirPath.text;
        }

    }
}