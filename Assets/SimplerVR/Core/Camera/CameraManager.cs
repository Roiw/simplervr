using SimplerVR.Common;
using UnityEngine;
using UnityEngine.Events;

namespace SimplerVR.Core.Camera
{
    /// <summary>
    /// This class is responsible taking care of the cameras.
    /// </summary>
    public class CameraManager : MonoBehaviour
    {
        private static CameraManager instance;
        public static CameraManager Instance
        {
            get
            {
                //  We don't have an instance of the Camera Manager.
                if (instance == null)
                {
                    Debug.LogError("Missing Camera Manager instance!");
                    return null;
                }
                else
                    return instance;
            }
        }

        /// <summary>
        /// A reference to the settings of the core of the API.
        /// </summary>
        private CoreSettings coreSettingsAsset;
        private CoreSettings coreSettings
        {
            get
            {
                if (coreSettingsAsset != null)
                    return coreSettingsAsset;

                coreSettingsAsset = CoreSettings.LoadCoreSettings();
                return coreSettingsAsset;
            }
        }

        [HideInInspector]
        public GameObject MainCamera;

        [Header("FadeVariables")]
        public GameObject FadeSpherePrefab;
        public float MenuFadeTime;
        public float TotalTeleportFadeDuration = 0.2f;


        /* # Fade variables # 
        Note: 
           On this script fade is being done in two different ways: 
           1. Through the FadePlane scipt for the teleport and others (legacy).
           2. Through the FadeSphere, which is used for the UI.                 
        */
        public bool Fading = false, FadedOut = false;
        private float FadeTimeMarker;
        private bool fadeInOnly = false;
        private event afterFade afterFadeMethod;
        private GameObject spawnedFadeSphere;
        public delegate void afterFade();

        private void Awake()
        {
            CreateInstance();
        }

        // Use this for initialization
        void Start()
        {
            /* Check if you are using this or not */
            afterFadeMethod = null;
            Fading = true;
            FadeTimeMarker = Time.time - (TotalTeleportFadeDuration / 2);
        }


        // Update is called once per frame
        void Update()
        {
            if (MainCamera == null)
            {
                UnityEngine.Camera[] cameras = coreSettings.CurrentPlatform.GetCameras();
                if (cameras.Length > 0)
                    MainCamera = cameras[0].gameObject;
                else
                    Debug.LogError("Couldn't retrieve any camera from the Platform.");
            }
                
            // DoFade();
            if (Fading)
            {
                if (Time.time - FadeTimeMarker >= TotalTeleportFadeDuration / 2)
                {
                    /* It gets inside the IF condition half way through the fade duration */
                    if (FadedOut)
                    {
                        Fading = false;
                        FadedOut = !FadedOut;
                    }
                    else
                    {
                        /* Finished fading all the way into black, time to move the camera */
                        FadedOut = !FadedOut;
                        if (afterFadeMethod != null)
                        {
                            afterFadeMethod(); // Note that FadeOut MUST be true when it gets in the afterFadeMethod.
                            afterFadeMethod = null;
                        }
                    }

                    /* At this point here, a half of the fade process has been completed so we
                       reset the time marker. FadeOut status was changed.
                    */
                    FadeTimeMarker = Time.time;

                    if (fadeInOnly)
                    {
                        Fading = false;
                    }

                }
            }
        }



        #region Fade Auxiliary Methods

        public bool GetFadingStatus()
        {
            return Fading;
        }
        public bool GetFadeOutStatus()
        {
            return FadedOut;
        }
        public float GetTimeFadeMarker()
        {
            return FadeTimeMarker;
        }
        public float GetTotalFadeDuration()
        {
            return TotalTeleportFadeDuration;
        }
        public void SetAfterFadeMethod(afterFade fadeMethod)
        {
            afterFadeMethod = fadeMethod;
        }


        //-------------------------------------------------
        // Performs a regular fade in and fade out.
        //-------------------------------------------------
        public void StartFadingCamera()
        {
            // Notify that camera if fading (start the fading "engine").
            TotalTeleportFadeDuration = 0.2f;
            Fading = true;
            fadeInOnly = false;
            // Initialize time counter.
            FadeTimeMarker = Time.time;
        }

        //-------------------------------------------------
        // Performs a regular fade in and fade out.
        //-------------------------------------------------
        public void StartFadingCamera(float time)
        {
            // Notify that camera if fading (start the fading "engine").
            TotalTeleportFadeDuration = time;
            Fading = true;
            fadeInOnly = false;
            // Initialize time counter.
            FadeTimeMarker = Time.time;
        }

        //-------------------------------------------------
        // Performs a fade-in in using a sphere around the camera.
        //-------------------------------------------------
        public void FadeInCamera(float time, UnityAction afterFade)
        {
            /* If a FadeSphere exists. */
            if (spawnedFadeSphere != null)
                Destroy(spawnedFadeSphere);

            /* Spawn and configure the FadeSphere. */
            spawnedFadeSphere = GameObject.Instantiate(FadeSpherePrefab, Vector3.zero, Quaternion.identity, MainCamera.transform);
            spawnedFadeSphere.transform.localPosition = Vector3.zero;
            Fade fade = spawnedFadeSphere.GetComponent<Fade>();
            fade.TotalFadeTime = time;
            fade.FadeOnStart = false;
            fade.FadeStartValue = 0;
            fade.FadeEndValue = 0.965f;
            fade.ExecuteAfterFade.AddListener(afterFade);
            fade.StartFading();
        }
        /* Performs only a Fade Out.*/
        public void FadeOutCamera(float time)
        {
            if (spawnedFadeSphere == null)
                return;

            /* Spawn and configure the FadeSphere. */
            Fade fade = spawnedFadeSphere.GetComponent<Fade>();
            fade.TotalFadeTime = time;
            fade.FadeOnStart = false;
            fade.FadeStartValue = 0.965f;
            fade.FadeEndValue = 0;
            fade.ExecuteAfterFade.RemoveAllListeners();
            fade.ExecuteAfterFade.AddListener(DestroyFadeSphere);
            fade.StartFading();
        }

        private void DestroyFadeSphere()
        {
            Destroy(spawnedFadeSphere);
        }
        #endregion

        public void SetCamera()
        {
            MainCamera = GameObject.FindGameObjectWithTag("MainCamera");
        }

        /// <summary>
        /// Create this manager instance.
        /// </summary>
        private void CreateInstance()
        {
            GameObject manager = GameObject.Find(Constants.API.RootObjectName + "/"
                + Constants.API.Core.RootObjectName + "/Camera Manager");

            if (manager == null)
            {
                // Creating the Manager
                GameObject newManager = new GameObject("Camera Manager");
                newManager.AddComponent<CameraManager>();
                newManager.transform.parent = GameObject.Find(Constants.API.RootObjectName + "/"
                    + Constants.API.Core.RootObjectName).transform;
                instance = newManager.GetComponent<CameraManager>();
            }
            else
                instance = manager.GetComponent<CameraManager>();

        }

    }

}