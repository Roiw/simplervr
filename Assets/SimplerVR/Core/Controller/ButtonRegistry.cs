using UnityEngine;
using System;

#if UNITY_EDITOR
using UnityEditor;
#endif 

namespace SimplerVR.Core.Controller
{
    /// <summary>
    /// <para>This class contains the the registry of a button and it's used to serialize button picks.</para>
    /// <para>Features use this class to serialize their button settings.</para>
    /// </summary>
    [Serializable]
    public class ButtonRegistry : ScriptableObject
    {
        /// <summary>
        /// In which button it runs.
        /// </summary>
        public Button.ButtonName Name;

        /// <summary>
        /// When the user do what action with the button? Press, release, hold, etc..?
        /// </summary>
        public Button.ButtonActions Action;

        /// <summary>
        /// Is the button on the right controller? (If a right controller exists).
        /// </summary>
        public bool IsRightControllerButton;

        /// <summary>
        /// Does the button overrides interactions?
        /// </summary>
        public bool OverrideInteraction;

        /// <summary>
        /// What's the feature method name for this button?
        /// </summary>
        public string methodName = "";

        /// <summary>
        /// Adds this object as an asset to another object.
        /// </summary>
        /// <param name="assetLocation">The path to the asset.</param>
        public void AddToAsset(string assetLocation)
        {
#if UNITY_EDITOR
            this.name = this.Name.ToString();
            AssetDatabase.AddObjectToAsset(this, assetLocation);
            EditorUtility.SetDirty(this);
            AssetDatabase.SaveAssets();        
#endif
        }

        /// <summary>
        /// Attempt to save the assets to serialized file.
        /// </summary>
        public void Save()
        {
#if UNITY_EDITOR
            EditorUtility.SetDirty(this);
#endif
        }

        public static void Delete(UnityEngine.Object obj, UnityEngine.Object parent)
        {
#if UNITY_EDITOR
            DestroyImmediate(obj, true);
            AssetDatabase.SaveAssets();
            EditorUtility.SetDirty(parent);
#endif
        }
    }
}
