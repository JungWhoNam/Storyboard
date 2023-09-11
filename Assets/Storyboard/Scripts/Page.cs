using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace VMail
{
    public class Page : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        [SerializeField]
        private RectTransform canvasRT;
        [SerializeField]
        private Image viewImage;
        [SerializeField]
        private Button closeButton;
        [SerializeField]
        private Color defaultColor = Color.gray;
        [SerializeField]
        private Color selectedColor = Color.magenta;
        [SerializeField]
        public UnityEngine.Events.UnityEvent onEndDragEvent;

        public JObject viewInfo { get; private set; }
        public List<(string author, string comment)> comments { get; private set; }

        private Vector3 fromMouseClickToUI; // a vector from the mouse position to the UI position
        private bool isEditMode = true;

        public bool isModified { get; private set; }


        public void Initialize(Texture2D image, JObject viewInfo, List<(string author, string comment)> comments)
        {
            this.viewInfo = viewInfo;
            this.comments = comments;
            this.viewImage.sprite = Sprite.Create(image, new Rect(0.0f, 0.0f, image.width, image.height), new Vector2(0.5f, 0.5f), 100);
            this.GetComponent<Image>().color = this.defaultColor;
        }

        public Texture2D GetImage()
        {
            return this.viewImage.sprite.texture;
        }

        public void AddComment(string author, string comment)
        {
            this.comments.Add((author, comment));

            this.isModified = true;
        }

        public void SetMode(bool editMode)
        {
            if (this.closeButton != null)
                this.closeButton.gameObject.SetActive(editMode);

            this.isEditMode = editMode;
        }

        public void SetSelected(bool selected)
        {
            this.GetComponent<Image>().color = selected ? this.selectedColor : this.defaultColor;
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            if (!this.isEditMode) return;
            if (!RectTransformUtility.RectangleContainsScreenPoint(this.canvasRT, eventData.position, eventData.pressEventCamera)) return;

            // compute how far the mouse position is from the UI position
            RectTransformUtility.ScreenPointToWorldPointInRectangle(this.canvasRT, eventData.position, eventData.pressEventCamera, out Vector3 mousePos);
            this.fromMouseClickToUI = mousePos - this.GetComponent<RectTransform>().position;
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (!this.isEditMode) return;
            if (!RectTransformUtility.RectangleContainsScreenPoint(this.canvasRT, eventData.position, eventData.pressEventCamera)) return;

            // move the UI
            RectTransformUtility.ScreenPointToWorldPointInRectangle(this.canvasRT, eventData.position, eventData.pressEventCamera, out Vector3 mousePos);
            this.GetComponent<RectTransform>().position = mousePos - this.fromMouseClickToUI;
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            if (!this.isEditMode) return;

            this.onEndDragEvent.Invoke();
        }

    }
}