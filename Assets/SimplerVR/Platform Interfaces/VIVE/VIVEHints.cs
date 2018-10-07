using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SimplerVR.Core.Controller.Hint;
using UnityEngine;
using UnityEngine.UI;

namespace SimplerVR.PlatformInterfaces.VIVE
{
    public class VIVEHints : Hint
    {

        public override void SetText(string newText)
        {
            /* How the text is set*/
            this.transform.Find("Panel/Text").gameObject.GetComponent<Text>().text = newText;
        }

        public override string GetText()
        {
            /* How the text is set*/
            return this.transform.Find("Panel/Text").gameObject.GetComponent<Text>().text;
        }

        /// <summary>
        /// Create Hint Hooks for the VIVE Controller on the positions below.
        /// </summary>
        public override void LoadHintHooks()
        {
            List<Hint.HintHook> hintHooks = new List<Hint.HintHook>();

            /* Right Hand Hooks */
            hintHooks.Add(new HintHook(new Vector3(.1f, .0235f, -0.0518f), HintManager.ButtonID.RightOptions, new Vector3(.0051f, .0235f, -0.0526f)));
            hintHooks.Add(new HintHook(new Vector3(.1f, .0235f, -0.085f), HintManager.ButtonID.RightTouchpad, new Vector3(.0225f, .0235f, -0.0841f)));
            hintHooks.Add(new HintHook(new Vector3(-.1f, 0, -0.075f), HintManager.ButtonID.RightTrigger, new Vector3(0, -0.0154f, -0.081f)));
            hintHooks.Add(new HintHook(new Vector3(-.1f, 0, -0.12f), HintManager.ButtonID.RightGrip, new Vector3(-.021f, 0.0011f, -0.1202f)));

            /* Left Hand Hooks */
            hintHooks.Add(new HintHook(new Vector3(.1f, .0235f, -0.0518f), HintManager.ButtonID.LeftOptions, new Vector3(.0051f, .0235f, -0.0526f)));
            hintHooks.Add(new HintHook(new Vector3(.1f, .0235f, -0.085f), HintManager.ButtonID.LeftTouchpad, new Vector3(.0225f, .0235f, -0.0841f)));
            hintHooks.Add(new HintHook(new Vector3(-.1f, 0, -0.075f), HintManager.ButtonID.LeftTrigger, new Vector3(0, -0.0154f, -0.081f)));
            hintHooks.Add(new HintHook(new Vector3(-.1f, 0, -0.12f), HintManager.ButtonID.LeftGrip, new Vector3(-.021f, 0.0011f, -0.1202f)));

            HintManager.Instance.SetHintHooks(hintHooks);
        }

        //-------------------------------------------------
        //  Set the hint position.
        //-------------------------------------------------
        public override void SetPosition(Vector3 newPosition)
        {
            this.gameObject.transform.localPosition = newPosition;
            this.gameObject.transform.localRotation = originalRotation;
            //this.gameObject.transform.rotation *= this.gameObject.transform.parent.rotation;
        }

        //-------------------------------------------------
        //  Set the hint position.
        //-------------------------------------------------
        public override void DrawLine(Vector3 lineEnd)
        {
            base.DrawLine(lineEnd);
        }
    }
}

