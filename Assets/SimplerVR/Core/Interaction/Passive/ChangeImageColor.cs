using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace SimplerVR.Core.Interaction.Passive
{
    /// <summary>
    /// Changes a sprite color.
    /// </summary>
    public class ChangeImageColor : InteractableObject
    {
        [HideInInspector]
        public bool TransitionColor;
        [HideInInspector]
        public float TransitionTime;
        [HideInInspector]
        public Color NormalColor;
        [HideInInspector]
        public Color HighlightColor;
        [HideInInspector]
        public Image TargetImage;

        private bool isRunning = false;
        
        void Update()
        {
            if (IsWithinGrasp)
            {
                if (!isRunning)
                {
                    DoBehaviour();
                    Debug.Log("Started Highlighting");
                }          
            }
            else
            {
                if (isRunning)
                {
                    StopBehaviour();
                    Debug.Log("Stopping Highlight");
                }                   
            }
        }

        // ---------------------------------------------------------------------------

        public void DoBehaviour()
        {
            if (TargetImage == null)
            {
                Debug.LogError("TargetImage not defined.");
                return;
            }

           isRunning = true;
            if (TransitionColor)
            {
                StopAllCoroutines();
                StartCoroutine(FadeInColor());
            }
            else
                TargetImage.color = HighlightColor;
        }

        // ---------------------------------------------------------------------------

        public void StopBehaviour()
        {
            if (TargetImage == null)
            {
                Debug.LogError("TargetImage not defined.");
                return;
            }
                
            isRunning = false;
            /* ChangeColor: Restore the background color.*/
            if (TransitionColor)
            {
                StopAllCoroutines();
                StartCoroutine(FadeOutColor());
            }
            else
                TargetImage.color = NormalColor;
        }

        // ---------------------------------------------------
        IEnumerator FadeInColor()
        {
            float timeCount = Time.time;
            while (TargetImage.color != HighlightColor)
            {
                float t = (Time.time - timeCount) / TransitionTime;
                TargetImage.color = Color.Lerp(TargetImage.color, HighlightColor, t);
                yield return null;
            }
            yield break;
        }

        // ---------------------------------------------------
        IEnumerator FadeOutColor()
        {
            float timeCount = Time.time;
            while (TargetImage.color != NormalColor)
            {

                float t = (Time.time - timeCount) / TransitionTime;
                TargetImage.color = Color.Lerp(TargetImage.color, NormalColor, t);
                yield return null;
            }
            yield break;
        }
    }

}
