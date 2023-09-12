using UnityEngine;
using UnityEngine.UI;

using TMPro;

namespace VMail.Viewer
{
    public class ViewerComment : MonoBehaviour, IViewer
    {
        public static float Threshold = 0.00025f;

        [SerializeField]
        private TMP_InputField inputField;
        [SerializeField]
        private TMP_Text outputField;
        [SerializeField]
        private Scrollbar scrollbar;
        [SerializeField]
        private Text onOffText;

        private Page currPage = null;


        void OnEnable()
        {
            this.inputField.onSubmit.AddListener(AddComment);
            if (this.onOffText != null)
            {
                this.onOffText.text = ">>";
            }
        }

        void OnDisable()
        {
            this.inputField.onSubmit.RemoveListener(AddComment);
            if (this.onOffText != null)
            {
                this.onOffText.text = "<<";
            }
        }

        // ============================================= INTERFACE =============================================

        public void OpenMessage(Story story)
        {
            // nothing to do
        }

        public void SetState(Page page)
        {
            this.currPage = null;
            if (page != null)
            {
                this.currPage = page;
            }
            this.UpdateState();
        }

        public void SetState(Transition transition)
        {
            this.currPage = null;

            if (transition != null)
            {
                if (transition.amt <= ViewerComment.Threshold)
                {
                    this.currPage = transition.from == null ? null : transition.from;
                }
                else if (transition.amt >= (1f - ViewerComment.Threshold))
                {
                    this.currPage = transition.to == null ? null : transition.to;
                }
            }

            this.UpdateState();
        }

        // ==========================================================================================

        public void ToggleOnOff()
        {
            this.gameObject.SetActive(!this.gameObject.activeSelf);
        }

        public void SetInputFieldVisiblity(bool v)
        {
            this.inputField.gameObject.SetActive(v);
        }

        public void AddComment(string comment)
        {
            if (this.currPage == null)
            {
                Debug.LogWarning("the current page is null.");
                return;
            }
            if (string.IsNullOrEmpty(comment))
            {
                Debug.LogWarning("entered an empty string.");
                return;
            }

            // adds a new comment
            this.currPage.AddComment(StoryEditor.Author, comment);

            // update the state
            this.UpdateState();
        }

        public void Clear()
        {
            this.currPage = null;
            this.inputField.text = string.Empty;
            this.outputField.text = "";
            this.scrollbar.value = 0; // set the scrollbar to the bottom when next text is submitted.
            this.inputField.interactable = false;
        }

        private void UpdateState()
        {
            this.inputField.text = string.Empty;
            this.outputField.text = "";
            if (this.currPage != null)
            {
                foreach ((string author, string comment) in this.currPage.comments)
                {
                    this.outputField.text += "<#08003D>[" + author + "]</color> " + comment + "\n";
                }
            }

#if UNITY_STANDALONE_WIN || UNITY_WEBGL
            this.inputField.ActivateInputField();
#endif

            this.scrollbar.value = 0; // set the scrollbar to the bottom when next text is submitted.

            this.inputField.interactable = this.currPage != null;
        }

    }
}