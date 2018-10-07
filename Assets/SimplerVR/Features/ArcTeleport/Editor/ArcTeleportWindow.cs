using UnityEngine;
using UnityEditor;
using UnityEditor.AnimatedValues;
using UnityEditorInternal;
using System;
using static SimplerVR.Common.Editor.Constants;
using static SimplerVR.Common.Constants;
using static SimplerVR.Core.Controller.Button;
using System.Collections.Generic;
using SimplerVR.Common;
using SimplerVR.Core.Controller;
using SimplerVR.Features.Editor;

namespace SimplerVR.Features.ArcTeleport.Editor
{
    public class ArcTeleportWindow : GenericFeatureEditor, IControllerFeatureEditor
    {
        private AnimBool pointerAnimBool = new AnimBool();
        private AnimBool navMeshAnimBool = new AnimBool();

        //// <summary>
        /// A reference to the settings of the teleport.
        /// </summary>
        private ArcTeleportData settings;
        private ArcTeleportData TeleportSettings
        {
            get
            {
                if (settings != null)
                    return settings;

                settings = ArcTeleportData.LoadArcTeleportSettings();
                return settings;
            }
        }


        /// <summary>
        /// Creates a button on Unity interface and starts this window.
        /// </summary>
        [MenuItem(Constants.API.APIName + "/" + Constants.API.RootFeatureObjectName + "/Arc Teleport")]
        static void CreateArcTeleportWindow()
        {
            ArcTeleportWindow window = EditorWindow.GetWindow<ArcTeleportWindow>("Arc Teleport", true,
                Common.Editor.Constants.WindowTypes);
            window.titleContent = new GUIContent("Arc Teleport", Resources.Load<Texture>(SmashIconFileName));
            window.minSize = new Vector2(WindowWidth, WindowMinHeight);
            window.maxSize = new Vector2(WindowWidth, WindowMaxHeight);

        }

        /// <summary>
        /// Add some initialization items.
        /// </summary>
        protected override void OnEnable()
        {
            base.OnEnable();
        }

        private void OnGUI()
        {
            // Creaters the Header Banner of the window.
            CreateHeader("ARC \nTELEPORT", "1.00", "2018");

            // A reference to the main script

            EditorGUILayout.Space();
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Settings",
                  Array.Find(skin.customStyles, element => element.name == SubtitleGUIStyle));
            EditorGUILayout.Space();
            EditorGUILayout.Space();

            EditorGUILayout.LabelField("General",
                Array.Find(skin.customStyles, element => element.name == Subtitle2GUIStyle));
            EditorGUILayout.Space();

            // Float
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(new GUIContent("Fade Duration", "How long, in seconds, the fade-in/fade-out animation should take"), skin.label);
            TeleportSettings.TeleportFadeDuration = EditorGUILayout.FloatField(TeleportSettings.TeleportFadeDuration);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space();
            EditorGUILayout.Space();
            // Parabolic pointer here
            pointerAnimBool.target = EditorGUILayout.ToggleLeft("Pointer Settings", pointerAnimBool.target,
                Array.Find(skin.customStyles, element => element.name == SubtitleGUIStyle));

            EditorGUILayout.Space();
            //Extra block that can be toggled on and off.
            if (EditorGUILayout.BeginFadeGroup(pointerAnimBool.faded))
            {
                EditorGUI.indentLevel++;

                EditorGUILayout.Space();
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(new GUIContent("Collide with: ", "What will the pointer collide with."), skin.label);
                LayerMask tempMask = EditorGUILayout.MaskField(InternalEditorUtility.LayerMaskToConcatenatedLayersMask(TeleportSettings.CollisionLayers), InternalEditorUtility.layers);
                TeleportSettings.CollisionLayers = InternalEditorUtility.ConcatenatedLayersMaskToLayerMask(tempMask);
                EditorGUILayout.EndHorizontal(); EditorGUILayout.Space();

                EditorGUILayout.Space();
                EditorGUILayout.Space();

                EditorGUILayout.LabelField("Arc Trajectory Properties",
                Array.Find(skin.customStyles, element => element.name == Subtitle2GUIStyle));
                EditorGUILayout.Space();

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(new GUIContent("Initial Velocity", "Used to draw the arc. We do an equation S=So+Vot-(1/2)at²"), skin.label);
                TeleportSettings.InitialVelocity = EditorGUILayout.Vector3Field(new GUIContent(""), TeleportSettings.InitialVelocity);
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(new GUIContent("Acceleration", "Used to draw the arc. We do an equation S=So+Vot-(1/2)at²"), skin.label);
                TeleportSettings.Acceleration = EditorGUILayout.Vector3Field(new GUIContent(""), TeleportSettings.Acceleration);
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.Space();
                EditorGUILayout.Space();

                EditorGUILayout.LabelField("Arc Style Properties",
                Array.Find(skin.customStyles, element => element.name == Subtitle2GUIStyle));
                EditorGUILayout.Space();

                // Int
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(new GUIContent("Point Count", "Number of points on the teleport arc."), skin.label);
                TeleportSettings.PointCount = EditorGUILayout.IntField(TeleportSettings.PointCount);
                EditorGUILayout.EndHorizontal();

                // Float
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(new GUIContent("Point Spacing", "The gap between the points on the teleport arc."), skin.label);
                TeleportSettings.PointSpacing = EditorGUILayout.FloatField(TeleportSettings.PointSpacing);
                EditorGUILayout.EndHorizontal();

                // Float
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(new GUIContent("Haptic Click Angle ", "Measure in degrees of how often the" +
                    "controller should respond with a haptic click.  Smaller value=faster clicks"), skin.label);
                TeleportSettings.HapticClickAngleStep = EditorGUILayout.FloatField(TeleportSettings.HapticClickAngleStep);
                EditorGUILayout.EndHorizontal();

                // Float
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(new GUIContent("Graphic Thickness", "How thick is the grphic"), skin.label);
                TeleportSettings.GraphicThickness = EditorGUILayout.FloatField(TeleportSettings.GraphicThickness);
                EditorGUILayout.EndHorizontal();

                // Material
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(new GUIContent("Good Material", "The material to display in case the teleport is valid."), skin.label);
                TeleportSettings.GoodGraphicMaterial = (Material)EditorGUILayout.ObjectField(TeleportSettings.GoodGraphicMaterial, typeof(Material), false);
                EditorGUILayout.EndHorizontal();

                // Material
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(new GUIContent("Bad Material", "The material to display in case the teleport is invalid."), skin.label);
                TeleportSettings.BadGraphicMaterial = (Material)EditorGUILayout.ObjectField(TeleportSettings.BadGraphicMaterial, typeof(Material), false);
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.Space();
                EditorGUILayout.Space();

                EditorGUILayout.LabelField("Selection Pad Properties",
                Array.Find(skin.customStyles, element => element.name == Subtitle2GUIStyle));
                EditorGUILayout.Space();

                // Mesh
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(new GUIContent("Good Material", "The material to display in case the teleport is valid."), skin.label);
                TeleportSettings.SelectionPadMesh = (Mesh)EditorGUILayout.ObjectField(TeleportSettings.SelectionPadMesh, typeof(Mesh), false);
                EditorGUILayout.EndHorizontal();

                // Material
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(new GUIContent("Fade Material", "The material to display in case the teleport is valid."), skin.label);
                TeleportSettings.GoodGraphicMaterial = (Material)EditorGUILayout.ObjectField(TeleportSettings.GoodGraphicMaterial, typeof(Material), false);
                EditorGUILayout.EndHorizontal();

                // Material
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(new GUIContent("Good Circle Material", "The material to display in case the teleport is invalid."), skin.label);
                TeleportSettings.BadGraphicMaterial = (Material)EditorGUILayout.ObjectField(TeleportSettings.BadGraphicMaterial, typeof(Material), false);
                EditorGUILayout.EndHorizontal();

                // Material
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(new GUIContent("Bad Circle", "The material to display in case the teleport is valid."), skin.label);
                TeleportSettings.GoodGraphicMaterial = (Material)EditorGUILayout.ObjectField(TeleportSettings.GoodGraphicMaterial, typeof(Material), false);
                EditorGUILayout.EndHorizontal();

                // Material
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(new GUIContent("Bottom Material", "The material to display in case the teleport is invalid."), skin.label);
                TeleportSettings.BadGraphicMaterial = (Material)EditorGUILayout.ObjectField(TeleportSettings.BadGraphicMaterial, typeof(Material), false);
                EditorGUILayout.EndHorizontal();


                EditorGUILayout.Space();
                EditorGUI.indentLevel--;
            }

            EditorGUILayout.EndFadeGroup();
            EditorGUILayout.Space();
            EditorGUILayout.Space();
            navMeshAnimBool.target = EditorGUILayout.ToggleLeft("NavMesh Settings", navMeshAnimBool.target,
               Array.Find(skin.customStyles, element => element.name == SubtitleGUIStyle));
            //Extra block that can be toggled on and off.
            if (EditorGUILayout.BeginFadeGroup(navMeshAnimBool.faded))
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.Space();
                EditorGUILayout.Space();

                EditorGUILayout.LabelField(new GUIContent("Once added the feature you must bake a NaveMesh and Add it on the button below." +
                    "\n\nThen if you have selected 'Display Teleportation Area', you will see the walkable navmesh.", ""),
                    Array.Find(skin.customStyles, element => element.name == BodyGUIStyle));

                EditorGUILayout.Space();

                // Bool
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(new GUIContent("Display Teleportation Area", ""), skin.label);
                TeleportSettings.DisplayTeleportNavmesh = EditorGUILayout.Toggle(TeleportSettings.DisplayTeleportNavmesh);
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.Space();
                EditorGUILayout.Space();

                EditorGUILayout.LabelField("Navmesh Render",
               Array.Find(skin.customStyles, element => element.name == Subtitle2GUIStyle));
                EditorGUILayout.Space();

                // Material
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(new GUIContent("NavMesh Material", ""), skin.label);
                TeleportSettings.NavMeshRender = (Material)EditorGUILayout.ObjectField(TeleportSettings.NavMeshRender, typeof(Material), false);
                EditorGUILayout.EndHorizontal();

                // Area Mask //
                string[] areas = GameObjectUtility.GetNavMeshAreaNames();
                int[] area_index = new int[areas.Length];
                int temp_mask = 0;
                for (int x = 0; x < areas.Length; x++)
                {
                    area_index[x] = GameObjectUtility.GetNavMeshAreaFromName(areas[x]);
                    temp_mask |= ((TeleportSettings.NavAreaMask >> area_index[x]) & 1) << x;
                }
                EditorGUI.BeginChangeCheck();
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(new GUIContent("Area Mask", "Where can you teleport on the navmesh?"), skin.label);
                temp_mask = EditorGUILayout.MaskField(temp_mask, areas);
                EditorGUILayout.EndHorizontal();
                if (EditorGUI.EndChangeCheck())
                {
                    TeleportSettings.NavAreaMask = 0;
                    for (int x = 0; x < areas.Length; x++)
                        TeleportSettings.NavAreaMask |= (((temp_mask >> x) & 1) == 1 ? 0 : 1) << area_index[x];
                    TeleportSettings.NavAreaMask = ~TeleportSettings.NavAreaMask;
                }

                EditorGUILayout.Space();
                EditorGUILayout.Space();

                EditorGUILayout.LabelField("Border Render",
               Array.Find(skin.customStyles, element => element.name == Subtitle2GUIStyle));
                EditorGUILayout.Space();

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(new GUIContent("Border Material", ""), skin.label);
                TeleportSettings.BorderRenderMaterial = (Material)EditorGUILayout.ObjectField(TeleportSettings.BorderRenderMaterial, typeof(Material), false);
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(new GUIContent("Border Height", ""), skin.label);
                TeleportSettings.BorderRenderHeight = EditorGUILayout.FloatField(TeleportSettings.BorderRenderHeight);
                EditorGUILayout.EndHorizontal();

                Rect k = EditorGUILayout.BeginHorizontal("Button");
                if (GUI.Button(k, GUIContent.none, skin.customStyles[0]))
                    OnNavmeshUpdateClick();

                EditorGUILayout.LabelField(new GUIContent("Add/Refresh Navmesh", "This will update the navmesh into the system."),
                    Array.Find(skin.customStyles, element => element.name == ButtonLabelUIStyle));
                EditorGUILayout.EndHorizontal();


                EditorGUILayout.Space();
                EditorGUI.indentLevel--;
            }
            EditorGUILayout.EndFadeGroup();

            EditorGUILayout.LabelField("Activation",
               Array.Find(skin.customStyles, element => element.name == SubtitleGUIStyle));

            EditorGUILayout.Space();
            EditorGUILayout.Space();
            // Create dropdown to select which buttons activate the feature.
            CreateSelectionButton(RegisterActions, UnregisterActions, typeof(ArcTeleportManager),
                null, new Button.ButtonActions[1] { Button.ButtonActions.PressUp }, GetSelectedButtons());
            EditorGUILayout.Space();
            EditorGUILayout.Space();

            CreateFeatureManagementButtons(SetupFeature, UnregisterActions, "Arc Teleport", typeof(ArcTeleportManager));
            EditorGUILayout.Space();
            EditorGUILayout.Space();
            CreateSaveButton(TeleportSettings.Save);

            Repaint();
        }

        /// <summary>
        /// Creates and setup the feature on obj.
        /// </summary>
        /// <param name="obj">The object to spawn and configure the feature.</param>
        public void SetupFeature(GameObject obj)
        {
            // Add component
            obj.AddComponent<ArcTeleportManager>();

            // Secondary scripts           
            GameObject navmesh = new GameObject();
            navmesh.name = "Navmesh";
            navmesh.transform.parent = obj.transform;
            navmesh.AddComponent<BorderRenderer>();
            navmesh.AddComponent<NavMeshRenderer>();
            navmesh.transform.position = Vector3.zero;

            GameObject pointer = new GameObject();
            pointer.name = "Pointer";
            pointer.transform.parent = obj.transform;
            pointer.AddComponent<ParabolicPointer>();
            pointer.transform.position = Vector3.zero;

            // Setup Component
            ArcTeleportManager teleportManager = obj.GetComponent<ArcTeleportManager>();

            ParabolicPointer pointerComponent = pointer.GetComponent<ParabolicPointer>();
            teleportManager.Pointer = pointerComponent;

            NavMeshRenderer navMeshHandler = navmesh.GetComponent<NavMeshRenderer>();
            BorderRenderer border = navmesh.GetComponent<BorderRenderer>();

            NavMeshHelper.ClearNavMesh(navMeshHandler, new SerializedObject(navMeshHandler), TeleportSettings);
            NavMeshHelper.UpdateNavMesh(navMeshHandler, new SerializedObject(navMeshHandler), TeleportSettings);
        }

        /// <summary>
        /// <para>This method calls the registering method on the arc teleport passing the selected buttons from this UI.</para>
        /// <para>Runs everytime the button has been changed.</para>
        /// </summary>
        /// <param name="obj">The feature game object.</param>
        public void RegisterActions(GameObject obj)
        {
            if (obj != null)
            {
                bool[] isRight = new bool[userSelectedButtons.Length]; // It defaults to false, implement this in the future.
                obj.GetComponent<ArcTeleportManager>().RegisterActionButtons(userSelectedButtons, userSelectedActions, userSelectionIsRight, isRight);
            }
        }

        /// <summary>
        /// <para>This method undo what the RegisterAction method does.</para>
        /// <para>Basically it removes from the button the methods registered on it by the Arc Teleport.</para>
        /// </summary>
        /// <param name="obj">The feature game object.</param>
        public void UnregisterActions(GameObject obj)
        {
            if (obj != null)
            {
                // We pass this information to the Arc Manager so it can register the proper methods to each button.
                obj.GetComponent<ArcTeleportManager>().ClearButtonRegistries();
            }
        }

        /// <summary>
        /// Execute when Update Navmesh button is clicked.
        /// </summary>
        private void OnNavmeshUpdateClick()
        {
            GameObject featureObject = FindFeatureOfType(typeof(ArcTeleportManager));
            if (featureObject == null)
                Debug.LogError("Add Arc Teleport before clicking Update Navmesh.");
            else
            {
                ArcTeleportManager arcTeleportManager = featureObject.GetComponent<ArcTeleportManager>();


                NavMeshRenderer mesh = featureObject.transform.Find("Navmesh").GetComponent<NavMeshRenderer>();

                // Area Mask //
                string[] areas = GameObjectUtility.GetNavMeshAreaNames();
                int[] area_index = new int[areas.Length];
                int temp_mask = 0;
                for (int x = 0; x < areas.Length; x++)
                {
                    area_index[x] = GameObjectUtility.GetNavMeshAreaFromName(areas[x]);
                    temp_mask |= ((TeleportSettings.NavAreaMask >> area_index[x]) & 1) << x;
                }
                EditorGUI.BeginChangeCheck();
                temp_mask = EditorGUILayout.MaskField("Area Mask", temp_mask, areas);
                if (EditorGUI.EndChangeCheck())
                {
                    TeleportSettings.NavAreaMask = 0;
                    for (int x = 0; x < areas.Length; x++)
                        TeleportSettings.NavAreaMask |= (((temp_mask >> x) & 1) == 1 ? 0 : 1) << area_index[x];
                    TeleportSettings.NavAreaMask = ~TeleportSettings.NavAreaMask;
                }

                // Sanity check for Null properties //
                bool HasMesh = (mesh.SelectableMesh != null && mesh.SelectableMesh.vertexCount != 0) || (mesh.SelectableMeshBorder != null && mesh.SelectableMeshBorder.Length != 0);

                bool MeshNull = mesh.SelectableMesh == null;
                bool BorderNull = mesh.SelectableMeshBorder == null;

                if (MeshNull || BorderNull)
                {
                    string str = "Internal Error: ";
                    if (MeshNull)
                        str += "Selectable Mesh == null.  ";
                    if (BorderNull)
                        str += "Border point array == null.  ";
                    str += "This may lead to strange behavior or serialization.  Try updating the mesh or delete and recreate the Navmesh object.  ";
                    str += "If you are able to consistently get a Vive Nav Mesh object into this state, please submit a bug report.";
                    EditorGUILayout.HelpBox(str, MessageType.Error);
                }

                UnityEngine.AI.NavMeshTriangulation tri = UnityEngine.AI.NavMesh.CalculateTriangulation();
                int vert_size, tri_size;

                NavMeshHelper.CullNavmeshTriangulation(ref tri, TeleportSettings.NavAreaMask, out vert_size, out tri_size);

                Mesh m = NavMeshHelper.ConvertNavmeshToMesh(tri, vert_size, tri_size);
                // Can't use SerializedProperties here because BorderPointSet doesn't derive from UnityEngine.Object
                mesh.SelectableMeshBorder = NavMeshHelper.FindBorderEdges(m);

                TeleportSettings.SelectableMesh = m;
                mesh.SelectableMesh = mesh.SelectableMesh;
                TeleportSettings.Save();
            }
        }

        /// <summary>
        /// Returns an Array of the previously selected buttons.
        /// </summary>
        /// <returns>An array of the previously selected buttons.</returns>
        private ButtonRegistry[] GetSelectedButtons()
        {
            if (TeleportSettings.ButtonsSelected == null)
                return null;

            // Get all the buttons that are of type PressUp.
            List<ButtonRegistry> buttonsToDisplay = TeleportSettings.ButtonsSelected.FindAll(b => b.Action == Button.ButtonActions.PressUp);

            // Transform buttonsToDisplay in an array.
            ButtonRegistry[] returnArray = buttonsToDisplay.ToArray();

            return returnArray;
        }
    }
}
