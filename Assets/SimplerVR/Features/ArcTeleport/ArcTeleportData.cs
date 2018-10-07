using System;
using UnityEngine;
using System.Collections.Generic;
using SimplerVR.Core.Controller;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace SimplerVR.Features.ArcTeleport
{
    /// <summary>
    /// <para>Holds all the configuration settings for the Arc Teleport.</para>
    /// <para> We do this so all the feature information stays serialized and can be exchanged between your teamates.</para>
    /// </summary>
    public class ArcTeleportData : ScriptableObject, IControllerFeatureData
    {

        private const string dataFileLocation = Common.Constants.API.ProjectRoot + "/Features/ArcTeleport/Resources/ArcTeleport/Data/ArcTeleport.asset";
        private const string resourcesDataLocation = "ArcTeleport/Data/ArcTeleport";

        /// <summary>
        /// A serializable registry of the buttons were selected on the ArcTeleportWindow for using this feature.
        /// </summary>
        public List<ButtonRegistry> ButtonsSelected;

        #region NavMesh Settings

        public Material NavMeshRender;
        public int NavAreaMask = ~0;  // Initialize to all
        [NonSerialized]
        public Mesh SelectableMesh;

        public Material BorderRenderMaterial;
        public float BorderRenderHeight;

        public bool DisplayTeleportNavmesh = true;

        private Mesh lastSavedMesh;

        #endregion

        #region Pointer Settings

        public Vector3 InitialVelocity = new Vector3(0, 0, 10);
        public Vector3 Acceleration = new Vector3(0, -9.8f, 0);

        public int PointCount = 10;
        public float PointSpacing = 0.3f;
        public float GraphicThickness = 0.05f;

        public Material GoodGraphicMaterial;
        public Material BadGraphicMaterial;

        public Mesh SelectionPadMesh;

        public Material SelectionPadFadeMaterial;
        public Material GoodSelectionPadCircle;
        public Material BadSelectionPadCircle;
        public Material SelectionPadBottomMaterial;

        public LayerMask CollisionLayers;

        // Measure in degrees of how often the controller should respond with a haptic click.  Smaller value=faster clicks
        public float HapticClickAngleStep = 10;

        // How long, in seconds, the fade-in/fade-out animation should take
        public float TeleportFadeDuration = 0.2f;

        #endregion

        /// <summary>
        /// Load the serialized settings.
        /// </summary>
        /// <returns>An ArcTeleportData instance.</returns>
        public static ArcTeleportData LoadArcTeleportSettings()
        {
            ArcTeleportData data = null;

            if (Application.isEditor)
            {
#if UNITY_EDITOR
                // Load the asset from the serializable asset (runs on editor).
                data = AssetDatabase.LoadAssetAtPath(dataFileLocation, typeof(ArcTeleportData)) as ArcTeleportData;

                if (data == null)
                {
                    // If we can't load create a new one.
                    data = CreateInstance<ArcTeleportData>();
                    data.Initialize();
                    AssetDatabase.CreateAsset(data, dataFileLocation);
                    AssetDatabase.SaveAssets();
                }
#endif
            }
            else
            {
                // Load the asset from the resources, (Runs in execution time).
                data = Resources.Load(resourcesDataLocation, typeof(ArcTeleportData)) as ArcTeleportData;

                if (data == null)
                {
                    // If we can't load create a new one.
                    data = CreateInstance<ArcTeleportData>();
                    data.Initialize();
                }
            }
            return data;
        }

        /// <summary>
        /// Initialize this class.
        /// </summary>
        public void OnEnable()
        {
            
        }

        private void Initialize()
        {
            // Load Good Material
            GoodGraphicMaterial = Resources.Load<Material>("ArcTeleport/Materials/Parabolic Selector_Green");
            // Load Bad Material
            BadGraphicMaterial = Resources.Load<Material>("ArcTeleport/Materials/Parabolic Selector_Red");

            CollisionLayers = ~0; // Initialize colliding with everything.

            // Load Selection Pad Mesh
            SelectionPadMesh = Resources.Load<Mesh>("ArcTeleport/selectionpad");
            SelectableMesh = Resources.Load<Mesh>("ArcTeleport/navmesh");

            // Load Selection Pad Material 1
            BadSelectionPadCircle = Resources.Load<Material>("ArcTeleport/Materials/Selection Pad/BadCircle");
            // Load Selection Pad Material 2
            GoodSelectionPadCircle = Resources.Load<Material>("ArcTeleport/Materials/Selection Pad/GoodCircle");
            // Load Selection Pad Material 3
            SelectionPadFadeMaterial = Resources.Load<Material>("ArcTeleport/Materials/Selection Pad/Fade");
            // Load Selection Pad Material 4
            SelectionPadBottomMaterial = Resources.Load<Material>("ArcTeleport/Materials/Selection Pad/Base");

            NavMeshRender = Resources.Load<Material>("ArcTeleport/Materials/Navmesh");
            BorderRenderMaterial = Resources.Load<Material>("ArcTeleport/Materials/Border");

            ButtonsSelected = new List<ButtonRegistry>();
        }

        /// <summary>
        /// Save changes to disk. This only runs in editor mode.
        /// </summary>
        public void Save()
        {
#if UNITY_EDITOR
            // Set dirty so Unity knows it must serialize the class.
            EditorUtility.SetDirty(this);

            Debug.Log("Saving..");

            // Create an asset to store the Mesh.
            if (SelectableMesh != lastSavedMesh && SelectableMesh != null)
            {
                AssetDatabase.CreateAsset(SelectableMesh, Common.Constants.API.ProjectRoot + "/Features/ArcTeleport/Resources/ArcTeleport/navmesh");
                lastSavedMesh = SelectableMesh;
            }
#endif
        }

        /// <summary>
        /// Return this feature type.
        /// </summary>
        /// <returns>Returns type.</returns>
        public Type GetFeatureType()
        {
            return typeof(ArcTeleportManager);
        }

        /// <summary>
        /// Returns a list of the selected buttons by the UI.
        /// </summary>
        /// <returns>A ButtonRegistry list of selected buttons.</returns>
        public List<ButtonRegistry> GetSelectedButtons()
        {
            return ButtonsSelected;
        }

        /// <summary>
        /// Gets the path to the serialized data.
        /// </summary>
        /// <returns>The path to the serialized data.</returns>
        public string GetPathToData()
        {
            return dataFileLocation;
        }
    }
}
