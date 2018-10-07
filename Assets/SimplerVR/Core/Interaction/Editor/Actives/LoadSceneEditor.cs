using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SimplerVR.Core.Controller;
using SimplerVR.Core.Interaction.Active;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using static SimplerVR.Core.Controller.Button;

namespace SimplerVR.Core.Interaction.Editor.Actives
{
    [CustomEditor(typeof(LoadScene))]
    public class LoadSceneEditor: InteractableObjectEditor
    {
        protected override void OnEnable()
        {
            base.OnEnable();
            LoadScene loadScene = (LoadScene)this.target;
            loadScene.SetActiveInteraction(true);
        }

        /// <summary>
        /// Draw the inspector UI.
        /// </summary>
        public override void OnInspectorGUI()
        {
            LoadScene loadScene = (LoadScene)this.target;

            // Creates the feature header.
            CreateInteractionHeader("LOAD \nSCENE", "1.00", "2018");

            // String
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(new GUIContent("Scene to teleport", "Remember this scene must be on the build settings."), skin.label);
            loadScene.SceneName = EditorGUILayout.TextField(loadScene.SceneName);
            EditorGUILayout.EndHorizontal();

            // Vector3
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(new GUIContent("Spawn Position", "Position to spawn the player"), skin.label);
            loadScene.DesiredPosition = EditorGUILayout.Vector3Field(new GUIContent(""), loadScene.DesiredPosition);
            EditorGUILayout.EndHorizontal();

            // Creates the dropdown button select.
            CreateSelectionButton(null, Button.ButtonActions.PressUp);

            EditorUtility.SetDirty(loadScene);
            if (GUI.changed)
            {
                EditorSceneManager.MarkSceneDirty(loadScene.gameObject.scene);
            }
        }
    }
}
