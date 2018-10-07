using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SimplerVR.Common.Editor;
using SimplerVR.Core.Interaction.Passive;
using UnityEditor;
using UnityEditor.AnimatedValues;
using UnityEngine;
using UnityEngine.UI;

namespace SimplerVR.Core.Interaction.Editor.Passives
{
    [CustomEditor(typeof(ChangeImageColor))]
    public class ChangeImageColorEditor : InteractableObjectEditor
    {
        private AnimBool transitionBool = new AnimBool();

        protected override void OnEnable()
        {
            base.OnEnable();
            ChangeImageColor changeImageColorScript = (ChangeImageColor)this.target;
        }

        /// <summary>
        /// Draw the inspector UI.
        /// </summary>
        public override void OnInspectorGUI()
        {
            EditorGUILayout.Space();

            ChangeImageColor changeImageColorScript = (ChangeImageColor)this.target;

            // Creates the feature header.
            CreateInteractionHeader("CHANGE \nIMAGE COLOR", "1.00", "2018");

            // Image
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(new GUIContent("Target Image", "The image to perform changes"), skin.label);
            changeImageColorScript.TargetImage = (Image)EditorGUILayout.ObjectField(
               new GUIContent(""), changeImageColorScript.TargetImage, typeof(Image), true);
            EditorGUILayout.EndHorizontal();

            // Color
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(new GUIContent("Normal Color", "The color of the element when not selected"), skin.label);
            changeImageColorScript.NormalColor = EditorGUILayout.ColorField(new GUIContent(""), changeImageColorScript.NormalColor);
            EditorGUILayout.EndHorizontal();

            // Color
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(new GUIContent("Selected Color", "The color of the element when selected"), skin.label);
            changeImageColorScript.HighlightColor = EditorGUILayout.ColorField(new GUIContent(""), changeImageColorScript.HighlightColor);
            EditorGUILayout.EndHorizontal();

            // Parabolic pointer here
            transitionBool.target = EditorGUILayout.ToggleLeft("Perform Timed Transition", changeImageColorScript.TransitionColor,
                Array.Find(skin.customStyles, element => element.name == Constants.SubtitleGUIStyle));
            changeImageColorScript.TransitionColor = transitionBool.target;

            EditorGUILayout.Space();
            //Extra block that can be toggled on and off.
            if (EditorGUILayout.BeginFadeGroup(transitionBool.faded))
            {
                EditorGUI.indentLevel++;

                // Float
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(new GUIContent("Fade Duration", "How long, in seconds, the fade-in/fade-out animation should take"), skin.label);
                changeImageColorScript.TransitionTime = EditorGUILayout.FloatField(changeImageColorScript.TransitionTime);
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.Space();
                EditorGUI.indentLevel--;
            }
            EditorGUILayout.EndFadeGroup();
            Repaint();
        }
    }
}
