using System;
using SimplerVR.Common.Editor;
using SimplerVR.Core.Interaction.Passive;
using UnityEditor;
using UnityEditor.AnimatedValues;
using UnityEngine;
using UnityEngine.UI;

namespace SimplerVR.Core.Interaction.Editor.Passives
{
    [CustomEditor(typeof(ChangeTextColor))]
    public class ChangeTextColorEditor : InteractableObjectEditor
    {
        private AnimBool transitionBool = new AnimBool();

        /// <summary>
        /// Draw the inspector UI.
        /// </summary>
        public override void OnInspectorGUI()
        {
            EditorGUILayout.Space();

            ChangeTextColor changeTextColorScript = (ChangeTextColor)this.target;

            // Creates the feature header.
            CreateInteractionHeader("CHANGE \nTEXT COLOR", "1.00", "2018");

            // Text
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(new GUIContent("Target Text", "The text to perform changes"), skin.label);
            changeTextColorScript.TargetText = (Text)EditorGUILayout.ObjectField(
                new GUIContent(""), changeTextColorScript.TargetText, typeof(Text), true);
            EditorGUILayout.EndHorizontal();

            // Color
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(new GUIContent("Normal Color", "The color of the element when not selected"), skin.label);
            changeTextColorScript.NormalColor = EditorGUILayout.ColorField(new GUIContent(""), changeTextColorScript.NormalColor);
            EditorGUILayout.EndHorizontal();

            // Color
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(new GUIContent("Selected Color", "The color of the element when selected"), skin.label);
            changeTextColorScript.HighlightColor = EditorGUILayout.ColorField(new GUIContent(""), changeTextColorScript.HighlightColor);
            EditorGUILayout.EndHorizontal();

            // Parabolic pointer here
            transitionBool.target = EditorGUILayout.ToggleLeft("Perform Timed Transition", changeTextColorScript.TransitionColor,
                Array.Find(skin.customStyles, element => element.name == Constants.SubtitleGUIStyle));
            changeTextColorScript.TransitionColor = transitionBool.target;

            EditorGUILayout.Space();
            //Extra block that can be toggled on and off.
            if (EditorGUILayout.BeginFadeGroup(transitionBool.faded))
            {
                EditorGUI.indentLevel++;

                // Float
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(new GUIContent("Fade Duration", "How long, in seconds, the fade-in/fade-out animation should take"), skin.label);
                changeTextColorScript.TransitionTime = EditorGUILayout.FloatField(changeTextColorScript.TransitionTime);
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.Space();
                EditorGUI.indentLevel--;
            }
            EditorGUILayout.EndFadeGroup();
            Repaint();
        }
    }
}
