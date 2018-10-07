using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.Events;

namespace SimplerVR.Core.Camera
{
    public class Fade : MonoBehaviour
    {
        /*
           ---------------------------------------------------------------------------

            # Fade Script #

            This script is used for fading an object in and out. 
            It's possible to set an event to be done halfway through the fade if needed.

            Right now it's currently supported to fade: Image and Text.

           ---------------------------------------------------------------------------
        */

        #region Public Variables
        public enum ObjectTypes { Image, Text, Material };

        [Header("General variables")]
        public ObjectTypes Type;
        public float TotalFadeTime;
        [Tooltip("If set to true, it will fade as soon as the object is Instantiated.")]
        public bool FadeOnStart = true;
        [Tooltip("If set to true, it will fade as soon as the object is Instantiated.")]
        public bool FadeOnEnable = false;
        [Tooltip("If set to true, it will fade all the way to FadeEndValue and back to FadeStartValue.")]
        public bool FadeInAndOut = false;
        [Range(0.0f, 1.0f)]
        public float FadeStartValue;
        [Range(0.0f, 1.0f)]
        public float FadeEndValue = 1.0f;

        [Header("Advanced variables")]
        [Tooltip("If set, will execute this at the end of the fade.")]
        public UnityEvent ExecuteAfterFade;

        #endregion

        #region Private Variables

        private float timeCounter;
        private float internal_FadeTime;
        private float internal_StartValue;
        private float internal_EndValue;
        private bool isFading = false;
        private bool loopFade = false;
        private bool secondFade = false;
        private Color colorFadeAuxiliary;
        private Color materialFadeAuxiliary;

        #endregion

        #region Specific case variables
        private Text textToFade;
        private Image imageToFade;
        private Material materialToFade;
        #endregion

        //-------------------------------------------------
        // Use this for initialization.
        //-------------------------------------------------
        void Start()
        {
            timeCounter = 0;
            loopFade = FadeInAndOut;
            internal_StartValue = FadeStartValue;
            internal_EndValue = FadeEndValue;

            if (FadeOnStart)
                StartFading();
            if (loopFade)
                internal_FadeTime = TotalFadeTime / 2;
            else
                internal_FadeTime = TotalFadeTime;
        }

        //-------------------------------------------------
        // Start the fading.
        //-------------------------------------------------
        public void StartFading()
        {
            // If already fading we must return..
            if (isFading)
                return;

            internal_StartValue = FadeStartValue;
            internal_EndValue = FadeEndValue;
            internal_FadeTime = TotalFadeTime;
            timeCounter = 0;
            isFading = true;

            // Case we are fading a text..
            if (Type == ObjectTypes.Text)
            {
                textToFade = this.GetComponent<Text>();
            }

            // Case we are fading an image..
            if (Type == ObjectTypes.Image)
            {
                imageToFade = this.GetComponent<Image>();
            }

            // Case we are fading a material..
            if (Type == ObjectTypes.Material)
            {
                MeshRenderer mesh = this.GetComponent<MeshRenderer>();
                if (mesh != null)
                    materialToFade = mesh.material;
            }
        }

        //-------------------------------------------------
        // Set's and start a fade.
        //-------------------------------------------------
        public void StartFading(float fadeStartValue, float fadeEndValue, float totalTime, bool fadeInOut)
        {

            FadeStartValue = fadeStartValue;
            FadeEndValue = fadeEndValue;
            TotalFadeTime = totalTime;
            FadeInAndOut = fadeInOut;
            StartFading();
        }

        //-------------------------------------------------
        // Set's and start a fade (string version).
        // Parameters is a coma separated string.
        //-------------------------------------------------
        public void StartFading(string parameters)
        {
            string[] array = parameters.Split(',');
            FadeStartValue = float.Parse(array[0]);
            FadeEndValue = float.Parse(array[1]);
            TotalFadeTime = float.Parse(array[2]);
            FadeInAndOut = false;
            isFading = false;
            StartFading();
        }

        //-------------------------------------------------
        // Update is called once per frame.
        //-------------------------------------------------
        void Update()
        {
            if (isFading)
            {
                timeCounter += Time.deltaTime;

                // Case we are fading a text..
                if (Type == ObjectTypes.Text && textToFade != null)
                {
                    colorFadeAuxiliary = textToFade.color;
                    colorFadeAuxiliary.a = Mathf.Lerp(internal_StartValue, internal_EndValue, timeCounter / internal_FadeTime);
                    textToFade.color = colorFadeAuxiliary;
                }
                // Case we are fading an image..
                if (Type == ObjectTypes.Image && imageToFade != null)
                {
                    colorFadeAuxiliary = imageToFade.color;
                    colorFadeAuxiliary.a = Mathf.Lerp(internal_StartValue, internal_EndValue, timeCounter / internal_FadeTime);
                    imageToFade.color = colorFadeAuxiliary;
                }
                // Case we are fading a material..
                if (Type == ObjectTypes.Material && materialToFade != null)
                {
                    materialFadeAuxiliary = materialToFade.color;
                    materialFadeAuxiliary.a = Mathf.Lerp(internal_StartValue, internal_EndValue, timeCounter / internal_FadeTime);
                    materialToFade.color = materialFadeAuxiliary;
                }

                // Check if we are done fading
                if (timeCounter > internal_FadeTime && isFading)
                {
                    /* Set the bool to not fading */
                    isFading = false;

                    /* If there is something set to Execute After Fade we should execute it now. */
                    if (ExecuteAfterFade != null)
                    {
                        ExecuteAfterFade.Invoke();
                        ExecuteAfterFade.RemoveAllListeners();
                    }

                    /* If this is a FadeInAndFadeOut we should restart the fade on the opposite direction. */
                    if (FadeInAndOut == true && secondFade == false)
                    {
                        float temp = internal_StartValue;
                        internal_StartValue = internal_EndValue;
                        internal_EndValue = temp;
                        isFading = true; // Must keep fading
                        secondFade = true; // Be sure to remember that we already fade one way.
                        timeCounter = 0; // Reset the time.
                    }
                    else if (secondFade == true)
                    {
                        secondFade = false; // Restore variable to it's original value.
                        isFading = false;
                        float temp = internal_StartValue;
                        internal_StartValue = internal_EndValue;
                        internal_EndValue = temp;
                    }

                }
            }
        }

        //-------------------------------------------------
        // Runs when object is enabled.
        //-------------------------------------------------
        void OnEnable()
        {
            if (FadeOnEnable)
                StartFading();
        }

    }
}
