using System.Collections.Generic;
using UnityEngine;
using System;
using SimplerVR.Common;
using SimplerVR.Features;
using SimplerVR.PlatformInterfaces;
using SimplerVR.PlatformInterfaces.VIVE;
#if UNITY_EDITOR
using UnityEditor;
#endif


namespace SimplerVR.Core
{
    /// <summary>
    /// <para>This class hold variables for setting the core systems of the API.</para>
    /// <para> We do this on a ScriptableObject so all the feature information stays serialized and can be exchanged between your teamates.</para>
    /// </summary>
    public class CoreSettings : ScriptableObject
    {
        
        /// <summary>
        /// Availble VR platforms, this is used to select a platform in the editor. The default is the VIVE Platform.
        /// </summary>
        [SerializeField]
        public static SupportedPlatforms.Platforms EditorSelectedPlatform;

        public bool UseDefaultHints;

        // This will be a content string.
        public List<string> DefaultHintsContent;

        // This will be the name of the buttons.
        public List<string> DefaultHintButtons;

        // True if the position is Right false is it's left.
        public List<bool> DefaultHintPositions;

        //public GameObject HintCanvas;

        public bool UseLaserInteraction;

        public bool LaserOnLeftHand;

        public float LaserLenght;

        public Material LaserMaterial;

        public LayerMask LaserCollision;

        public Sprite LaserCollisionSprite;

        public Material LaserCollisionMaterial;

        private GameObject featuresGameObject;

        public float LaserCollisionScale;

        private GenericPlatform activePlatform = null;

        /// <summary>
        /// The VR platform currently being used.
        /// </summary>
        public GenericPlatform CurrentPlatform
        {
            get
            {
                if (activePlatform == null)
                {
                    if (EditorSelectedPlatform == SupportedPlatforms.Platforms.VIVE)
                    {
                        activePlatform = CreateInstance<VIVEPlatform>();
                    }                    
                }
                return activePlatform;
            }
        }


        /// <summary>
        /// Features selected through the UI.
        /// </summary>
        [NonSerialized]
        public List<GenericFeatureManager> FeaturesSelected;

        /// <summary>
        /// The loaded core settings.
        /// </summary>
       // CoreSettings core = null;

        /// <summary>
        /// Load the serialized core settings.
        /// </summary>
        /// <returns>A core settings instance</returns>
        public static CoreSettings LoadCoreSettings()
        {
            CoreSettings core = null;

            if (Application.isEditor)
            {
#if UNITY_EDITOR
                // Create the coreSettings asset.
                core = AssetDatabase.LoadAssetAtPath(Constants.API.Core.DataLocation, typeof(CoreSettings)) as CoreSettings;

                if (core == null)
                {
                    core = CreateInstance<CoreSettings>();
                    core.Initialize();
                    AssetDatabase.CreateAsset(core, Common.Constants.API.Core.DataLocation);
                    AssetDatabase.SaveAssets();

                }
#endif
            }
            else
            {
                // Create the coreSettings asset.
                core = Resources.Load(Constants.API.Core.ResourceLocation, typeof(CoreSettings)) as CoreSettings;

                if (core == null)
                {
                    core = CreateInstance<CoreSettings>();
                    core.Initialize();

                }
                
            }
            return core;
        }

        public void Initialize()
        {
            Debug.Log("Core initialized with default settings..");
            FeaturesSelected = new List<GenericFeatureManager>();
            EditorSelectedPlatform = SupportedPlatforms.Platforms.VIVE;
            UseDefaultHints = Constants.API.Core.UseDefaultHints;
            DefaultHintButtons = new List<string>();
            DefaultHintPositions = new List<bool>();
            DefaultHintsContent = new List<string>();
            UseLaserInteraction = Constants.API.Core.UseLaserInteraction;
            LaserOnLeftHand = Constants.API.Core.LaserOnLeftHand;
            LaserLenght = Constants.API.Core.LaserLenght;
            LaserCollisionScale = Constants.API.Core.LaserCollisionScale;
            LaserCollisionSprite = Resources.Load<Sprite>(Constants.API.Core.LaserCollisionSpriteLocation);
            LaserMaterial = Resources.Load<Material>(Constants.API.Core.LaserMaterialLocation);
            LaserCollisionMaterial = Resources.Load<Material>(Constants.API.Core.LaserCollisionMaterialLocation);
            //HintCanvas = Resources.Load<GameObject>
        }

        /// <summary>
        /// Initialize this class.
        /// </summary>
        public void OnEnable()
        {
            // Initializing the list with the current features.
            UpdateFeaturesList();

        }

        /// <summary>
        /// Check the Features game object for new features.
        /// </summary>
        public void UpdateFeaturesList()
        {
            // Finds the feature's root object.
            if(featuresGameObject == null)
                featuresGameObject = GameObject.Find(Constants.API.RootObjectName + "/" + Constants.API.RootFeatureObjectName);

            if (featuresGameObject == null)
                return;          

            // Checking if list need initialization
            if (FeaturesSelected == null)
            {
                Debug.Log("Initializing it..");
                FeaturesSelected = new List<GenericFeatureManager>();
            }

            // Clear the internal list. 
            FeaturesSelected.Clear();

            // Clear all the internal buttons references. Will be added back later.
            if (CurrentPlatform.GetType().IsSubclassOf(typeof(GenericControllerPlatform)))
            {
                ((GenericControllerPlatform)CurrentPlatform).GetPlatformControls().ClearAllButtons();
            }

            // For every object child of Features Root object.
            for (int i = 0; i < featuresGameObject.transform.childCount; i++)
            {
                GameObject featureObject = featuresGameObject.transform.GetChild(i).gameObject;

                // Add feature to the list of features..
                GenericFeatureManager feature = featureObject.GetComponent<GenericFeatureManager>();
                FeaturesSelected.Add(feature);

                // If feature implements IControllerFeature and our platform support controllers.    
                if (feature is IControllerFeature && CurrentPlatform.GetType().IsSubclassOf(typeof(GenericControllerPlatform)))
                {
                    // Get all buttons registry from the feature.       
                    IControllerFeature controllerFeature = feature as IControllerFeature;
                    List<Controller.ButtonRegistry> registries = controllerFeature.GetAllButtonRegistries();

                    // Send all buttons registry to the current platform.
                    Controller.ControlSet controls = ((GenericControllerPlatform)CurrentPlatform).GetPlatformControls();
                    controls.RemoveAllButtons(feature.GetFeatureType());
                    foreach (Controller.ButtonRegistry reg in registries)
                    {
                        Action method = controllerFeature.GetButtonMethod(reg);
                        // Register the method on the platform. (Added back the references..)
                        if (method != null)
                            controls.AddButton(reg.Name, reg.Action, reg.IsRightControllerButton, method, reg.OverrideInteraction, feature.GetFeatureType());
                    }
                }
            }
        }

#if UNITY_EDITOR

        /// <summary>
        /// Save changes to disk.
        /// </summary>
        public void Save(CoreSettings core)
        {
            Debug.Log("Core Saved");
            EditorUtility.SetDirty(core);
        }

        /// <summary>
        /// Not done yet..
        /// </summary>
        public void InitializeAllInteractions()
        {
            List<GameObject> objs = new List<GameObject>();
            objs.AddRange(GameObject.FindGameObjectsWithTag(Constants.API.InteractionTagName));

            List<Interaction.InteractableObject> interactiveObjects = new List<Interaction.InteractableObject>();

            ///List<>

            foreach (GameObject obj in objs)
            {
                interactiveObjects.AddRange(obj.GetComponents<Interaction.InteractableObject>());

            }

        }
       

        [MenuItem("AssetTest/Test Load")]
        static void TestLoadMenu()
        {      
            CoreSettings core = CreateInstance<CoreSettings>();
            AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);
            AssetDatabase.ImportAsset(Constants.API.Core.DataLocation);


            // Create the coreSettings asset.
            core = AssetDatabase.LoadAssetAtPath(Constants.API.Core.DataLocation, typeof(CoreSettings)) as CoreSettings;

            if (core == null)
                Debug.Log("Test Load: Failed.");
            else
                Debug.Log("Test load:" + core.LaserLenght + "\n");

        }
#endif

    }
}


