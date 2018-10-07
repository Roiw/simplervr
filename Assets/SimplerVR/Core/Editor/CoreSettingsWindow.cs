using SimplerVR.Common;
using UnityEditor;
using UnityEditor.AnimatedValues;
using UnityEditorInternal;
using UnityEngine;
using static SimplerVR.Common.Editor.Constants;
using static SimplerVR.Common.Constants;

namespace SimplerVR.Core.Editor
{
    /// <summary>
    /// The class for the CoreSettings window UI.
    /// </summary>
    public class CoreSettingsWindow : EditorWindow
    {
        /// <summary>
        /// The skin to use on the editor.
        /// </summary>
        protected GUISkin skin;

        /// <summary>
        /// The header texture bar texture.
        /// </summary>
        protected Texture headerTexture;

        /// <summary>
        /// A serialized reference to the core settings.
        /// </summary>
        public SerializedObject SerializedCoreSettings;

        /// <summary>
        /// Serialized property reference to the list of features.
        /// </summary>
        public SerializedProperty ListFeaturesSelected;

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

        /// <summary>
        /// Creates a button on Unity interface and starts this window.
        /// </summary>
        [MenuItem(Constants.API.APIName + "/Settings")]
        public static void CreateCoreSettingsWindow()
        {
            // Create a window.
            CoreSettingsWindow window = EditorWindow.GetWindow<CoreSettingsWindow>("CoreSettingsWindow", true,
                Common.Editor.Constants.WindowTypes);
            window.titleContent = new GUIContent("Settings", Resources.Load<Texture>(SmashIconFileName));
            window.minSize = new Vector2(WindowWidth, WindowMinHeight);
            window.maxSize = new Vector2(WindowWidth, WindowMaxHeight);

        }

        /// <summary>
        /// Runs as soon as the window start.
        /// </summary>
        protected void OnEnable()
        {
            skin = Resources.Load<GUISkin>(SkinFileName);
            headerTexture = Resources.Load<Texture>(Common.Editor.Constants.Features.HeaderTextureFileName); // Change it with the specific for the Arc Teleport

            //SerializedCoreSettings = new SerializedObject(coreSettings);
            //ListFeaturesSelected = SerializedCoreSettings.FindProperty("FeaturesSelected"); // Find the List in our script and create a refrence of it
        }

        /// <summary>
        /// Run every inspector update.
        /// </summary>
        void OnInspectorUpdate()
        {
            Repaint();
        }

        public void OnGUI()
        {
            CoreSettingsBase.DrawUI(coreSettings);

            this.Repaint();
        }
        
    }
}

