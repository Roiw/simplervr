using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace SimplerVR.Core.Controller.Hint
{
    /*
            ---------------------------------------------------------------------------

            # PanelBackfaceResize #

            This will resize the backface sprite to cover the inverted letters on the back of the hint.

            ---------------------------------------------------------------------------
    */
    public class PanelBackfaceResize : MonoBehaviour
    {
        
        private RectTransform rect;
        private RectTransform parentRect;
        private Vector2 canvasProportions;
        private Vector3 canvasScale;

        // Use this for initialization
        void Start()
        {
            rect = this.GetComponent<RectTransform>();
            parentRect = this.transform.parent.GetComponent<RectTransform>();
            canvasScale = this.transform.parent.parent.GetComponent<RectTransform>().localScale;
        }

        // Update is called once per frame
        void Update()
        {
            //rect.sizeDelta = new Vector2(parentRect.sizeDelta.x, parentRect.sizeDelta.y);
            Text panelText = this.transform.parent.Find("Text").GetComponent<Text>();
            Vector3 panelPosition = this.transform.parent.transform.localPosition;
            HorizontalLayoutGroup lg = this.transform.parent.GetComponent<HorizontalLayoutGroup>();
            float displacementWidth = panelText.preferredWidth;
            float displacementHeight = panelText.preferredHeight;
            float widthScale = (displacementWidth + lg.padding.left + lg.padding.right) / (canvasScale.x * 100f * 100f * 2.25f);
            float heightScale = (displacementHeight + lg.padding.top + lg.padding.bottom) / (canvasScale.y * 100f * 100f * 1.5f);
            this.transform.localScale = new Vector2(widthScale, heightScale);

        }
    }
}

