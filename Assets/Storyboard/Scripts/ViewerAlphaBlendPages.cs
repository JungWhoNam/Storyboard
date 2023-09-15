using UnityEngine;
using UnityEngine.UI;

namespace VMail.Viewer
{
    public class ViewerAlphaBlendPages : MonoBehaviour, IViewer
    {
        [SerializeField]
        private RawImage fromImageUI;
        [SerializeField]
        private RawImage toImageUI;

        private Texture2D prevFromImage;
        private Texture2D prevToImage;


        // ============================================= INTERFACE =============================================

        public void OpenMessage(Story story)
        {
            // nothing to do
        }

        public void SetState(Page page)
        {
            if (page == null)
                return;

            this.fromImageUI.enabled = true;
            this.toImageUI.enabled = false;

            Texture2D img = page.GetImage();
            if (this.prevFromImage != img)
            {
                this.fromImageUI.texture = img;
                this.prevFromImage = img;
            }
        }

        public void SetState(Transition transition)
        {
            if (transition == null || transition.from == null || transition.to == null)
                return;

            this.fromImageUI.enabled = true;
            this.toImageUI.enabled = true;

            Texture2D imgFrom = transition.from.GetImage();
            if (this.prevFromImage != imgFrom)
            {
                this.fromImageUI.texture = imgFrom;
                this.prevFromImage = imgFrom;
            }

            Texture2D toImage = transition.to.GetImage();
            if (this.prevToImage != toImage)
            {
                this.toImageUI.texture = toImage;
                this.prevToImage = toImage;
            }

            Color c = this.toImageUI.color;
            c.a = transition.amt;
            this.toImageUI.color = c;
        }

    }
}