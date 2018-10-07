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
    public class ChangeTextColor : InteractableObject
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
        public Text TargetText;

        private bool isRunning = false;

        void Update()
        {
            if (IsWithinGrasp)
            {
                if (!isRunning)
                    DoBehaviour();
            }
            else
            {
                if (isRunning)
                    StopBehaviour();
            }
        }

        // ---------------------------------------------------------------------------

        public void DoBehaviour()
        {
            if (TargetText == null)
            {
                Debug.LogError("Target Text not defined.");
                return;
            }

            isRunning = true;
            if (TransitionColor)
            {
                StopAllCoroutines();
                StartCoroutine(FadeInColor());
            }
            else
                TargetText.color = HighlightColor;
        }

        // ---------------------------------------------------------------------------

        public void StopBehaviour()
        {
            if (TargetText == null)
            {
                Debug.LogError("Target Text not defined.");
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
                TargetText.color = NormalColor;
        }

        // ---------------------------------------------------
        IEnumerator FadeInColor()
        {
            float timeCount = Time.time;
            while (TargetText.color != HighlightColor)
            {
                float t = (Time.time - timeCount) / TransitionTime;
                TargetText.color = Color.Lerp(TargetText.color, HighlightColor, t);
                yield return null;
            }
            yield break;
        }

        // ---------------------------------------------------
        IEnumerator FadeOutColor()
        {
            float timeCount = Time.time;
            while (TargetText.color != NormalColor)
            {

                float t = (Time.time - timeCount) / TransitionTime;
                TargetText.color = Color.Lerp(TargetText.color, NormalColor, t);
                yield return null;
            }
            yield break;
        }
    }

}
