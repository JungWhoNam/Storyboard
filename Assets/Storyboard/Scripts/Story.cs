using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace VMail
{
    public class Transition
    {
        public Page from;
        public Page to;
        public float amt;
    }

    public class Story : MonoBehaviour
    {
        protected static int SortByPosX(Page p0, Page p1)
        {
            return p0.GetComponent<RectTransform>().anchoredPosition.x.CompareTo(p1.GetComponent<RectTransform>().anchoredPosition.x);
        }

        [SerializeField]
        protected Page basePage;
        [SerializeField]
        protected GameObject pageContainer;
        [SerializeField]
        protected GameObject addPagePanel;

        public string dirPath { get; private set; }
        public List<Page> pages { get; protected set; }

        public bool isEditMode { get; protected set; }
        public bool isModified { get; private set; }


        private void Awake()
        {
            this.pages = new ();
        }

        public void Initialize(string dirPath, List<Texture2D> images, List<JObject> viewInfos, List<List<(string, string)>> comments)
        {
            if (images.Count != viewInfos.Count || viewInfos.Count != comments.Count)
            {
                Debug.LogError("Numbers of images, view infos, and comments should be the same.");
                return;
            }

            this.Clear();

            this.dirPath = dirPath;

            for (int i = 0; i < images.Count; i++)
            {
                this.AddPage(images[i], viewInfos[i], comments[i]);
            }
        }

        public Page AddPage(Texture2D image, JObject viewInfo, List<(string, string)> comments)
        {
            return this.AddPage(this.pages.Count, image, viewInfo, comments);
        }

        public Page AddPage(int at, Texture2D image, JObject viewInfo, List<(string, string)> comments)
        {
            if (at < 0 || at > this.pages.Count)
            {
                Debug.LogError("the index is wrong: " + at + " " + this.pages.Count);
                return null;
            }

            // create a page
            Page pg = Instantiate(this.basePage).GetComponent<Page>();
            pg.Initialize(image, viewInfo, comments);

            // update the page and add to the list
            pg.gameObject.SetActive(true);
            pg.transform.SetParent(this.pageContainer.transform, false);
            pg.transform.SetSiblingIndex(this.pageContainer.transform.childCount - 2);
            this.pages.Insert(at, pg);

            for (int i = 0; i < this.pages.Count; i++)
            {
                this.pages[i].name = "Page - " + i;
            }

            this.isModified = true;

            return pg;
        }

        public void Remove(Page page)
        {
            this.pages.Remove(page);
            GameObject.Destroy(page.gameObject);

            this.isModified = true;
        }

        public void Clear()
        {
            foreach (Page page in this.pages)
            {
                GameObject.Destroy(page.gameObject);
            }
            this.pages.Clear();

            this.dirPath = null;

            this.isModified = true;
        }

        public void UpdatePageSizes(float pageAspect)
        {
            foreach (Page page in this.pages)
            {
                Vector2 size = page.GetComponent<RectTransform>().sizeDelta;
                page.GetComponent<RectTransform>().sizeDelta = new Vector2(size.y * pageAspect, size.y);
            }

            if (this.addPagePanel != null) // update the add page panel size
            {
                Vector2 size = this.addPagePanel.GetComponent<RectTransform>().sizeDelta;
                this.addPagePanel.GetComponent<RectTransform>().sizeDelta = new Vector2(size.y * pageAspect, size.y);
            }
        }

        public void RefreshLayout()
        {
            this.pageContainer.GetComponent<HorizontalLayoutGroup>().enabled = false;
            this.pageContainer.GetComponent<HorizontalLayoutGroup>().enabled = true;
        }

        public void UpdateOrderOfPages()
        {
            // based on the x-position, order the pages
            this.pages.Sort(SortByPosX);

            // update the order of the Page gameobjects based on the order of the list "pages"
            for (int i = 0; i < this.pages.Count; i++)
            {
                this.pages[i].transform.SetSiblingIndex(i);
                this.pages[i].gameObject.name = "Page - " + i;
            }

            this.isModified = true;
        }

        public void SetMode(bool editMode)
        {
            this.addPagePanel.gameObject.SetActive(editMode);
            foreach (Page page in this.pages)
            {
                page.SetMode(editMode);
            }
            this.isEditMode = editMode;
        }

        public void SetDirPath(string dirPath)
        {
            this.dirPath = dirPath;
        }

        public float GetDistFromFirstToEndPages()
        {
            if (this.pages.Count <= 1) return 0f;

            Vector2 pageSize = this.pages.Count <= 0 ? Vector2.zero : this.pages[0].GetComponent<RectTransform>().sizeDelta;
            float spacing = this.pageContainer.GetComponent<HorizontalLayoutGroup>().spacing;
            return (this.pages.Count - 1) * pageSize.x + (this.pages.Count - 1) * spacing;
        }

        public float GetOffsetFirstPage()
        {
            if (this.pages.Count <= 0) return 0f;

            Vector2 pageSize = this.pages.Count <= 0 ? Vector2.zero : this.pages[0].GetComponent<RectTransform>().sizeDelta;
            float spacing = this.pageContainer.GetComponent<HorizontalLayoutGroup>().padding.left;
            return spacing + pageSize.x * 0.5f;
        }

    }
}