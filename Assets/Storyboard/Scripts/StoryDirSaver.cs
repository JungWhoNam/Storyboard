using System.IO;
using UnityEngine;
using UnityEngine.UI;

namespace VMail
{
    public class StoryDirSaver : MonoBehaviour
    {
        [SerializeField]
        private StoryEditor storyEditor;

        [Space(10)]
        [SerializeField]
        private InputField title;
        [SerializeField]
        private Text status;
        [SerializeField]
        private Button cancelBtn;
        [SerializeField]
        private Button saveBtn;


        private void OnEnable()
        {
            Story story = storyEditor.GetCurrentStory();
            title.text = story.dirPath == null ? "" : Path.GetFileName(story.dirPath);
            title.interactable = true;
            status.text = "";
            cancelBtn.interactable = true;
            saveBtn.interactable = true;
        }

        public void Save()
        {
            // check if the story is null or empty.
            Story story = storyEditor.GetCurrentStory();
            if (story == null || story.pages.Count <= 0)
            {
                status.text = "the story is empty.";
                return;
            }

            // check the dir name is valid
            if (title.text == "")
            {
                status.text = "the name input is not set.";
                return;
            }

            // check the dir name is valid
            string dirPath = Path.Combine(StoryDirManager.StoryDir, title.text);
            if (Directory.Exists(dirPath))
            {
                status.text = "the name already exists.";
                return;
            }

            // save the story
            storyEditor.Save(dirPath);
            title.interactable = false;
            status.text = "Successfuly saved the story.";
            cancelBtn.interactable = false;
            saveBtn.interactable = false;
        }

    }
}