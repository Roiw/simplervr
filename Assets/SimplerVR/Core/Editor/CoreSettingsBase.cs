using SimplerVR.Core.Interaction;
using SimplerVR.Core.Interaction.Editor;
using System;
using System.Collections.Generic;
using System.Reflection;
using SimplerVR.Common;
using SimplerVR.Core.Controller;
using SimplerVR.Core.Controller.Hint;
using SimplerVR.Features;
using SimplerVR.PlatformInterfaces;
using UnityEditor;
using UnityEditor.AnimatedValues;
using UnityEditorInternal;
using UnityEngine;
using static SimplerVR.Common.Editor.Constants;
using static SimplerVR.Core.Controller.Button;

namespace SimplerVR.Core.Editor
{
    /// <summary>
    /// The Core class can be edited through the inspector game object and through the menu window.
    /// For simplification sake, this base class have all the methods that are called on both UI's.
    /// </summary>
    public static class CoreSettingsBase
    {
        // Lerps a number from 0 to 1.
        private static AnimBool laserPointer = new AnimBool(true);

        private static AnimBool hintAnimBool = new AnimBool(true);

        public static void DrawUI(CoreSettings coreSettings)
        {
            //serializedCoreSettings.Update();

            EditorGUILayout.Space();

            CreateHeader("SETTINGS", "1.0", "2018");

            GUIStyle subtitleStyle = new GUIStyle { fontSize = 15, fontStyle = FontStyle.Bold };

            coreSettings.UpdateFeaturesList();
            DisplayList(coreSettings);


            EditorGUILayout.Space();
            EditorGUILayout.Space();

            #region Laser Pointer Settings

            laserPointer.target = coreSettings.UseLaserInteraction;
            laserPointer.target = EditorGUILayout.ToggleLeft(" Laser Pointer", laserPointer.target, subtitleStyle);
            coreSettings.UseLaserInteraction = laserPointer.target;

            //Extra block that can be toggled on and off.
            if (EditorGUILayout.BeginFadeGroup(laserPointer.faded))
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.Space();
                coreSettings.LaserOnLeftHand = EditorGUILayout.Toggle(
                    new GUIContent("Laser On Left Hand", "Check if you want the laser on the lef" +
                    "t hand, otherwise it will be displayed on the right."), coreSettings.LaserOnLeftHand);

                coreSettings.LaserLenght = EditorGUILayout.FloatField(
                    new GUIContent("Laser Lenght", "Laser Lenght in meters."), coreSettings.LaserLenght);

                coreSettings.LaserMaterial = (Material)EditorGUILayout.ObjectField(
                    new GUIContent("Laser Material", "A material for the laser."), coreSettings.LaserMaterial,
                    typeof(Material), true);

                coreSettings.LaserCollision = EditorGUILayout.MaskField(
                    new GUIContent("Collision Layers", "Every layer selected here will collide with the laser."),
                    coreSettings.LaserCollision, InternalEditorUtility.layers);

                coreSettings.LaserCollisionMaterial = (Material)EditorGUILayout.ObjectField(
                    new GUIContent("Collision Material", "The material that will show on the collision sprite."),
                    coreSettings.LaserCollisionMaterial, typeof(Material), true);

                coreSettings.LaserCollisionScale = EditorGUILayout.FloatField(
                    new GUIContent("Collision Sprite Scale", "Scale of the laser collision sprite."),
                    coreSettings.LaserCollisionScale);

                coreSettings.LaserCollisionSprite = (Sprite)EditorGUILayout.ObjectField(
                    new GUIContent("Collision Sprite", "The sprite that will show once a " +
                    "laser collides with anything"), coreSettings.LaserCollisionSprite, typeof(Sprite), true);

                EditorGUI.indentLevel--;
            }
            EditorGUILayout.EndFadeGroup();

            #endregion

            EditorGUILayout.Space();
            EditorGUILayout.Space();

            #region Hint Settings

            hintAnimBool.target = coreSettings.UseDefaultHints;
            hintAnimBool.target = EditorGUILayout.ToggleLeft(" Default Hints", hintAnimBool.target, subtitleStyle);
            coreSettings.UseDefaultHints = hintAnimBool.target;

            if (EditorGUILayout.BeginFadeGroup(hintAnimBool.faded))
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.Space();
                CreateHintSelectionDropdown(coreSettings);
                EditorGUI.indentLevel--;
            }
            EditorGUILayout.EndFadeGroup();

            #endregion

            EditorGUILayout.Space();
            EditorGUILayout.Space();
            EditorGUILayout.Space();

            #region Save Button
            Rect k = EditorGUILayout.BeginHorizontal("Button");
            if (GUI.Button(k, GUIContent.none, skin.customStyles[0]))
                coreSettings.Save(coreSettings);

            EditorGUILayout.LabelField(new GUIContent("Save Changes", "This will save changes to disk."),
                Array.Find(skin.customStyles, element => element.name == ButtonLabelUIStyle));
            EditorGUILayout.EndHorizontal();
            #endregion

        }

        /// <summary>
        /// The skin to use on the editor.
        /// </summary>
        private static GUISkin skin = Resources.Load<GUISkin>(SkinFileName);

        /// <summary>
        /// Display a list of features.
        /// </summary>
        private static void DisplayList( CoreSettings coreSettings)
        {
            EditorGUILayout.Space();
            EditorGUILayout.Space();

            EditorGUILayout.LabelField("Features Activated");
            EditorGUILayout.Space();

            if (coreSettings.FeaturesSelected == null)
            {
                EditorGUILayout.LabelField("Cannot display feature list.. add prefab first!.");
                return;
            }

            // Display list
            for (int i = 0; i < coreSettings.FeaturesSelected.Count; i++)
            {
                if (coreSettings.FeaturesSelected[i] == null && coreSettings.FeaturesSelected.Count > 0)
                {
                    Debug.LogError("One of the features is not properly initialized. Please re-add the features.");
                    continue;
                }
                   
                Rect rec = EditorGUILayout.BeginHorizontal();

                // Display the property fields in two ways.
                EditorGUILayout.LabelField(coreSettings.FeaturesSelected[i].GetFeatureName()/*MyName.stringValue*/);
                if (GUI.Button(new Rect(rec.position.x + rec.size.x/2, rec.position.y, rec.size.y, rec.size.y), new GUIContent(Resources.Load<Texture>(DeleteIconFileName)),
                    Array.Find(skin.customStyles, element => element.name == Common.Editor.Constants.DeleteButtonIconUIStyle)))
                {
                    // Remove the feature.
                    RemoveFeature(coreSettings.FeaturesSelected[i]);
                }

                EditorGUILayout.EndHorizontal();

                EditorGUILayout.Space();
                EditorGUILayout.Space();
            }
        }

        /// <summary>
        /// Removes a feature from the feature list.
        /// </summary>
        private static void RemoveFeature(GenericFeatureManager genericFeature)
        {
            GameObject FeaturesGameObject = GameObject.Find(Constants.API.RootObjectName + "/" + Constants.API.RootFeatureObjectName);

            Type typeToRemove = genericFeature.GetType();

            // Add features 
            for (int i = 0; i < FeaturesGameObject.transform.childCount; i++)
            {
                // If not add the feature.. 
                if (FeaturesGameObject.transform.GetChild(i).GetComponent<Features.GenericFeatureManager>().GetType() == typeToRemove)
                    GameObject.DestroyImmediate(FeaturesGameObject.transform.GetChild(i).gameObject);
            }
        }   

        /// <summary>
        /// Create feature window header.
        /// </summary>
        /// <param name="featureName">The name of the file containing the picture (PNG).</param>
        /// <param name="version">A text containing the version.</param>
        public static void CreateHeader(string title, string version, string year)
        {
            // Create the header bar.
            EditorGUILayout.LabelField(new GUIContent(title),
                Array.Find(skin.customStyles, element => element.name == Common.Editor.Constants.Core.HeaderGUIStyle));

            EditorGUILayout.Space();

            // Add the current version. 
            EditorGUILayout.LabelField(VersionPrefix + version + " - " + year,
                Array.Find(skin.customStyles, element => element.name == VersionGUIStyle));
        }

        private static void CreateHintSelectionDropdown(CoreSettings coreSettings)
        {
            int oldAmountOfButtons = 1;
            int amountOfButtons = oldAmountOfButtons;

            // No previous serialized settings setting or this was already serialized.
            // We only load from the serialized settings when the window is open.
            if (coreSettings.DefaultHintButtons.Count != 0 /*&& !hasInitialized*/)
                oldAmountOfButtons = coreSettings.DefaultHintButtons.Count;

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(new GUIContent("Amount of Hints"));
            amountOfButtons = EditorGUILayout.IntField(oldAmountOfButtons);
            EditorGUILayout.EndHorizontal();

            bool changesWereMade = false;
            
            Button.ButtonName[] userSelectedButtons = new Button.ButtonName[coreSettings.DefaultHintButtons.Count];
            string[] userSelectedContents = coreSettings.DefaultHintsContent.ToArray();
            bool[] userSelectionIsRight = coreSettings.DefaultHintPositions.ToArray();

            for (int i = 0; i < coreSettings.DefaultHintButtons.Count; i++)
                userSelectedButtons[i] = HintConvert.ConvertToButtonName(coreSettings.DefaultHintButtons[i]);

            // Initialize vectors that will hold the new selection by the user..
            Button.ButtonName[] newUserSelection = new Button.ButtonName[amountOfButtons];
            string[] newUserInputedContent = new string[amountOfButtons];
            bool[] newUserSelectionIsRight = new bool[amountOfButtons];

            // Expand or retract the vectors of selected options.
            if (amountOfButtons < userSelectedButtons.Length)
            {
                // Retract..
                userSelectedButtons = userSelectedButtons.DescreaseArray(userSelectedButtons, userSelectedButtons.Length - amountOfButtons);
                userSelectedContents = userSelectedContents.DescreaseArray(userSelectedContents, userSelectedContents.Length - amountOfButtons);
                userSelectionIsRight = newUserSelectionIsRight.DescreaseArray(userSelectionIsRight, userSelectionIsRight.Length - amountOfButtons);
            }
            else if (amountOfButtons > userSelectedButtons.Length)
            {
                // Expand..
                userSelectedButtons = userSelectedButtons.ExpandArray(userSelectedButtons, amountOfButtons - userSelectedButtons.Length);
                userSelectedContents = userSelectedContents.ExpandArray(userSelectedContents, amountOfButtons - userSelectedContents.Length);
                userSelectionIsRight = newUserSelectionIsRight.ExpandArray(userSelectionIsRight, amountOfButtons - userSelectionIsRight.Length);
            }

            // Now we set each dropdown..
            for (int k = 0; k < amountOfButtons; k++)
            {
                // Get the available buttons for this platform.. 
                Button.ButtonName[] controllerButtons;
                    controllerButtons = SupportedPlatforms.GetButtons(CoreSettings.EditorSelectedPlatform);

                // Get the availble positions for this platform..
                string[] positions = SupportedPlatforms.GetPositions(CoreSettings.EditorSelectedPlatform);

                // Create the labels that will be shown on the screen.
                GUIContent[] controllerLabels = new GUIContent[controllerButtons.Length];
                GUIContent[] positionLabels = new GUIContent[positions.Length];

                // Write all the info on the labels we just created.
                for (int i = 0; i < controllerButtons.Length; i++)
                {
                    controllerLabels[i] = new GUIContent();
                    controllerLabels[i].text = controllerButtons[i].ToString();
                }
                for (int i = 0; i < positions.Length; i++)
                {
                    positionLabels[i] = new GUIContent();
                    positionLabels[i].text = positions[i];
                }

                // Start writing on the screen.
                EditorGUILayout.BeginHorizontal();
                // Write on the window
                int selectedPosition = 0;
                if (userSelectionIsRight[k])
                    selectedPosition = EditorGUILayout.Popup(1, positionLabels);
                else
                    selectedPosition = EditorGUILayout.Popup(0, positionLabels);

                // This is the index on the controllerLabels array of the previously selected button.
                int selectedIndex = Array.FindIndex(controllerLabels, element => element.text.Equals(userSelectedButtons[k].ToString()));

                string inputedContent = "";
                if ( k < coreSettings.DefaultHintsContent.Count)
                    inputedContent = EditorGUILayout.TextField(coreSettings.DefaultHintsContent[k]);

                // This is the newly inputed content.
                newUserInputedContent[k] = inputedContent;
                if (inputedContent == null)
                    continue; // We dont register inputs..

                // This is newly selected index by the user. We must transform it int a ControllerButtons to save it.
                int newlySelectedIndex = EditorGUILayout.Popup(selectedIndex, controllerLabels);
                newUserSelection[k] = (Button.ButtonName)Enum.Parse(typeof(Button.ButtonName), controllerLabels[newlySelectedIndex].text, true);
       

                if (positions[selectedPosition].Equals("Right"))
                    newUserSelectionIsRight[k] = true;
                else
                    newUserSelectionIsRight[k] = false;

                if (!(newUserSelection[k] == userSelectedButtons[k]))
                    changesWereMade = true;
                if (!(newUserSelectionIsRight[k] == userSelectionIsRight[k]))
                    changesWereMade = true;
                if (!(newUserInputedContent[k].Equals(userSelectedContents[k])))
                    changesWereMade = true;
                if (amountOfButtons != oldAmountOfButtons)
                    changesWereMade = true;

                EditorGUILayout.EndHorizontal();
                EditorGUILayout.Space();
            }

            // Check if there was changes 
            if (changesWereMade)
            {
                coreSettings.DefaultHintButtons.Clear();
                coreSettings.DefaultHintPositions.Clear();
                coreSettings.DefaultHintsContent.Clear();

                // If so update the inspector.
                for (int i = 0; i< newUserSelection.Length; i++)
                {
                    coreSettings.DefaultHintButtons.Add( HintConvert.ConvertNames(newUserSelection[i], newUserSelectionIsRight[i]));
                    coreSettings.DefaultHintPositions.Add(newUserSelectionIsRight[i]);
                    coreSettings.DefaultHintsContent.Add(newUserInputedContent[i]);
                }

                // Serialize Changes!
                coreSettings.Save(coreSettings);
            }
            return;
        }
    }
}
