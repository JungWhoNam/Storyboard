using System.IO;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace VMail
{
    public class StoryEditor : MonoBehaviour
    {
        public static string Author = "Bob";

        [SerializeField]
        private Story story;
        [SerializeField]
        private StoryPlayer storyPlayer;

        [SerializeField]
        private StateManager stateManager;

        [Header("Viewers")]
        public Viewer.ViewerComment viewerComments;
        public Viewer.ViewerAlphaBlendPages viewerCurrPage;


        public Story GetCurrentStory()
        {
            return story;
        }

        public void CaptureSnapshot()
        {
            if (story == null) return;

            // send request messages
            stateManager.SendReqestCaptureView(this.story.pages.Count);
        }

        public void AddSnapshot(int index)
        {
            if (stateManager.imageData == null) return;
            if (stateManager.viewData == null) return;

            // create a texture
            Texture2D image = new Texture2D(2, 2);
            ImageConversion.LoadImage(image, stateManager.imageData);

            story.AddPage(image, stateManager.viewData, new List<(string, string)>()); ;

            UpdateMessagePlayer();
            // right now we assume a new shot is added at the end.
            storyPlayer.SetTransitionValueAsRatio(1f);
            this.UpdateStateFromCurrentTransition();
        }

        public void RemovePage(Page page)
        {
            story.Remove(page);

            UpdateMessagePlayer();

            UpdateStateFromCurrentTransition();
        }

        public void UpdateMessagePlayer()
        {
            if (storyPlayer == null) return;

            storyPlayer.SetTransitionSize(0f, story.pages.Count - 1.0001f);
            storyPlayer.gameObject.SetActive(story.pages.Count > 1);

            // update the size of the player (slider)
            if (story.pages.Count > 1)
            {
                RectTransform rt = this.storyPlayer.GetComponent<RectTransform>();
                rt.sizeDelta = new Vector2(this.story.GetDistFromFirstToEndPages(), rt.sizeDelta.y);

                Vector3 pos = rt.localPosition;
                rt.localPosition = new Vector3(this.story.GetOffsetFirstPage(), pos.y, pos.z);
            }
        }

        public void UpdateStateFromCurrentTransition()
        {
            if (storyPlayer == null) return;

            Transition t = this.GetCurrentTransition();
            UpdateState(t);
        }

        public void UpdateState(Page page)
        {
            if (storyPlayer == null) return;

            for (int i = 0; i < story.pages.Count; i++)
            {
                if (story.pages[i] == page)
                {
                    storyPlayer.SetTransitionValue(i);
                }
            }

            UpdateStateFromCurrentTransition();
        }

        public void UpdateState(Transition t)
        {
            if (t == null) return;

            float threshold = 0.00025f; // threshold

            // a page mode
            if (t.from == null || t.to == null || t.amt < threshold || t.amt > (1f - threshold))
            {
                // get the current page
                Page p = null;
                if (t.from == null) p = t.to;
                else if (t.to == null) p = t.from;
                else if (t.amt < threshold) p = t.from;
                else if (t.amt > (1f - threshold)) p = t.to;

                stateManager.SendReqestUpdate(p);

                if (viewerComments != null)
                    viewerComments.SetState(p);
                if (viewerCurrPage != null)
                    viewerCurrPage.SetState(p);
            }
            else // a transition mode
            {
                stateManager.SendReqestUpdate(t);

                if (viewerComments != null)
                    viewerComments.SetState(t);
                if (viewerCurrPage != null)
                    viewerCurrPage.SetState(t);
            }
        }

        public Transition GetCurrentTransition()
        {
            return this.GetTransition(storyPlayer.GetTransitionValue());
        }

        public Page GetCurrentPage()
        {
            float v = storyPlayer.GetTransitionValue();

            if (v - (int)v >= 0.9999f) return story.pages[(int)v + 1];

            if (v - (int)v <= 0.0001f) return story.pages[(int)v];

            return null;
        }

        public Transition GetTransition(float v)
        {
            if (storyPlayer == null) return null;
            if (story.pages.Count <= 0) return null;

            Transition currTransition = new();
            if (story.pages.Count == 1)
            {
                currTransition.amt = 0.9999f;
                currTransition.to = story.pages[0];
            }
            else
            {
                int fromIdx = (int)v;
                currTransition.from = fromIdx < 0 ? null : story.pages[fromIdx];
                int toIdx = fromIdx + 1;
                currTransition.to = toIdx >= story.pages.Count ? null : story.pages[toIdx];
                float amt = v - fromIdx;
                currTransition.amt = amt;
            }

            return currTransition;
        }

        public Transition GetTransitionFromRatio(float zeroToOne)
        {
            if (storyPlayer == null) return null;

            float min = storyPlayer.GetMinValue();
            float max = storyPlayer.GetMaxValue();
            float v = (max - min) * Mathf.Clamp01(zeroToOne) + min;

            return this.GetTransition(v);
        }

        public void Clear()
        {
            story.Clear();
            UpdateMessagePlayer();
        }

        public void RemoveAndSave(string dirPath)
        {
            if (story == null || story.pages.Count <= 0) return;

            if (Directory.Exists(dirPath))
                Directory.Delete(dirPath, true);

            Directory.CreateDirectory(dirPath);

            // save the json and image files
            for (int i = 0; i < story.pages.Count; i++)
            {
                Page page = story.pages[i];

                // save the json
                string fPath = Path.Combine(dirPath, i + "-view.json");
                Utils.Tools.SaveJsonFile(fPath, page.viewInfo.ToString());

                // save the json for comments
                string fPathComments = Path.Combine(dirPath, i + "-comments.json");
                Utils.Tools.SaveJsonFile(fPathComments, JsonConvert.SerializeObject(page.comments, Formatting.Indented));

                // save the image
                string imgPath = Path.Combine(dirPath, i + ".png");
                Utils.Tools.SaveTexture2D(imgPath, page.GetImage());
            }

            // save the index.html
            string src = Path.Combine(Application.streamingAssetsPath, "Storyboard", "index.html");
            File.Copy(src, dirPath + "/index.html");

            story.SetDirPath(dirPath);
        }

        public void Load(string dirPath)
        {
            List<JObject> jsons = new();
            List<Texture2D> images = new();
            List<List<(string, string)>> comments = new();

            int cnt = 0;
            while (true)
            {
                string jsonPath = Path.Combine(dirPath, cnt + "-view.json");
                string jsonPathComments = Path.Combine(dirPath, cnt + "-comments.json");
                string imgPath = Path.Combine(dirPath, cnt + ".png");

                if (!File.Exists(jsonPath) || !File.Exists(jsonPathComments)|| !File.Exists(imgPath) )
                    break;

                jsons.Add(JObject.Parse(File.ReadAllText(jsonPath)));
                images.Add(Utils.Tools.LoadTexture2D(imgPath));
                comments.Add(JsonConvert.DeserializeObject<List<(string, string)>>(File.ReadAllText(jsonPathComments)));

                cnt += 1;
            }

            if (jsons.Count <= 0 || images.Count <= 0)
                return;

            story.Clear();
            story.Initialize(dirPath, images, jsons, comments);

            UpdateMessagePlayer();
            storyPlayer.SetTransitionValueAsRatio(0f);
            UpdateStateFromCurrentTransition();
        }

    }
}