using System.IO;
using System.Collections.Generic;
using UnityEngine;

namespace VMail
{
    public class StoryDirManager : MonoBehaviour
    {
        public static string StoryDir;

        [SerializeField]
        private StoryEditor storyEditor;

        [Space(10)]
        [SerializeField]
        private StoryDir baseStoryDir;
        [SerializeField]
        private GameObject dirContainer;

        public List<StoryDir> storyDirs { get; protected set; }


        private void Awake()
        {
            StoryDirManager.StoryDir = Application.persistentDataPath;

            storyDirs = new();
        }

        private void OnEnable()
        {
            Refresh();
        }

        public void Open(StoryDir storyDir)
        {
            storyEditor.Load(storyDir.GetDirPath());
        }

        public void Delete(StoryDir storyDir)
        {
            Directory.Delete(storyDir.GetDirPath(), true);
            Refresh();
        }

        public void Refresh()
        {
            // remove existing dir UIs
            foreach (StoryDir storyDir in storyDirs)
            {
                GameObject.Destroy(storyDir.gameObject);
            }
            storyDirs.Clear();

            // add dir UIs
            foreach (string dir in Directory.GetDirectories(StoryDirManager.StoryDir, "*", SearchOption.TopDirectoryOnly))
            {
                StoryDir storyDir = Instantiate(this.baseStoryDir).GetComponent<StoryDir>();
                storyDir.Initialize(dir);
                storyDir.gameObject.SetActive(true);
                storyDir.transform.SetParent(this.dirContainer.transform, false);
                storyDirs.Add(storyDir);
            }
        }

    }
}