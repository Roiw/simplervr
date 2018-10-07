using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace SimplerVR.Core.Controller.Hint
{
    /// <summary>
    ///  This is the base class for a hint and will be on the HintPrefab.
    /// </summary>
    public class Hint : MonoBehaviour
    {
        [HideInInspector]
        public HintManager.ButtonID HintButton;
        public Material LinkMaterial;

        protected Quaternion originalRotation;

        void Awake()
        {
            originalRotation = this.transform.rotation;

            if ( HintManager.Instance.GetHintHooks() == null)
                LoadHintHooks();
        }

        public class HintHook
        {
            /* The position where the hook is going to be placed. Relative to the Attach Object on the VIVE controller. */
            public Vector3 HintPosition;
            /* The position where the line (that comes from the hint) is going to land on the controller. Relative to the Attach Object on the VIVE controller.*/
            public Vector3 LinePosition;
            public HintManager.ButtonID Button;

            public HintHook(Vector3 pos, HintManager.ButtonID button, Vector3 linePos)
            {
                HintPosition = pos;
                Button = button;
                LinePosition = linePos;
            }
        }

        //-------------------------------------------------
        //  Set the text on the prefab.
        //-------------------------------------------------
        public virtual void SetText(string newText)
        {
           
        }

        //-------------------------------------------------
        //  Set the text on the prefab.
        //-------------------------------------------------
        public virtual string GetText()
        {
            return "";
        }

        //-------------------------------------------------
        //  Set the image on the prefab.
        //-------------------------------------------------
        public virtual void SetImage(Image newImage)
        {
        }
        
        //-------------------------------------------------
        // Load the Hint Hooks.
        //-------------------------------------------------
        public virtual void LoadHintHooks()
        {
            
        }

        //-------------------------------------------------
        //  Set the hint position.
        //-------------------------------------------------
        public virtual void SetPosition(Vector3 newPosition)
        {

        }

        //-------------------------------------------------
        //  Draw line
        //-------------------------------------------------
        public virtual void DrawLine(Vector3 lineEnd)
        {
            /* Note: lineEnd is a reference for where the line is going to land in the controller
               However this value is relative to the 'Attach' GameObject in the controller.
            */

            /* Create the Line Renderer Object and Component. */
            GameObject lineRenderer = new GameObject();
            lineRenderer.name = "Line Renderer";
            lineRenderer.transform.SetParent(this.transform);
            lineRenderer.transform.rotation *= this.transform.rotation; // Rotation still not zero.

            /* Discover the position of the start of the line that connects hint and controller.*/
            GameObject lineStartObj = new GameObject();
            lineStartObj.transform.SetParent(this.transform);
            lineStartObj.name = "Line Start";
            Text panelText = this.transform.Find("Panel/Text").GetComponent<Text>();
            Vector3 panelPosition = this.transform.Find("Panel").transform.localPosition;
            HorizontalLayoutGroup lg = this.transform.Find("Panel").GetComponent<HorizontalLayoutGroup>();
            float displacement = panelText.preferredWidth / 2.0f;
            float positionX = panelPosition.x - (displacement + lg.padding.left) * this.transform.localScale.x;
            lineStartObj.transform.localPosition = new Vector3(positionX, panelPosition.y, panelPosition.z);

            /* We create a temporary object to hold the line and set it's parent as the 'Attach' object.  */
            GameObject lineEndObj = new GameObject();
            lineEndObj.name = "LineEnd";
            lineEndObj.transform.SetParent(this.transform.parent);

            /* Now we can easily set it's position.*/
            lineEndObj.transform.localPosition = lineEnd;
            lineEndObj.transform.SetParent(this.transform);
            lineEndObj.transform.localPosition *= this.transform.localScale.x;

            Vector3 temp = lineStartObj.transform.localPosition;
            temp.x = lineEndObj.transform.localPosition.x;
            lineEndObj.transform.localPosition = temp;

            /* set up the line renderer. */
            LineRenderer ln = lineRenderer.AddComponent<LineRenderer>();
            ln.SetPosition(0, lineStartObj.transform.localPosition);
            ln.SetPosition(1, lineEndObj.transform.localPosition);
            ln.alignment = LineAlignment.Local;
            ln.useWorldSpace = false;  

            /* Position the Line Renderer */
            ln.transform.localPosition = Vector3.zero;
            ln.widthMultiplier = 0.001f;
            ln.material = this.LinkMaterial;

        }
    }
}

