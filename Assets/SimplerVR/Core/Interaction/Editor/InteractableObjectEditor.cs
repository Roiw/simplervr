using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEditor.AnimatedValues;
using static SimplerVR.Core.Controller.Button;
using System;
using SimplerVR.Common.Editor;
using SimplerVR.Core.Controller;
using SimplerVR.PlatformInterfaces;

namespace SimplerVR.Core.Interaction.Editor
{
    public class InteractableObjectEditor : UnityEditor.Editor
    {
        /// <summary>
        /// The skin to use on the editor.
        /// </summary>
        protected GUISkin skin;

        /// <summary>
        /// The header texture bar texture.
        /// </summary>
        private Texture headerTexture;

        /// <summary>
        /// <para>If you are using the CreateSelectionButton method. The button selected will have it's index saved on this variable.</para>
        /// <para>Otherwise the value is 0.</para>
        /// </summary>
        protected Button.ButtonName[] userSelectedButtons = { 0 };

        /// <summary>
        /// Works similat to the userSelectedButtons but for ButtonActions instead of ButtonNames.
        /// </summary>
        protected Button.ButtonActions[] userSelectedActions = { 0 };

        /// <summary>
        /// Controls if this has been initialized.
        /// </summary>
        private bool hasInitialized = false;

        protected virtual void OnEnable()
        {
            skin = Resources.Load<GUISkin>(Constants.SkinFileName);
            headerTexture = Resources.Load<Texture>(Common.Editor.Constants.Features.HeaderTextureFileName);
        }

        /// <summary>
        /// Runs every inspector update.
        /// </summary>
        void OnInspectorUpdate()
        {
            Repaint();
        }

        /// <summary>
        /// Draw the inspector UI.
        /// </summary>
        public override void OnInspectorGUI()
        {

        }

        //private void CreateBehaviourDropdown(int amountOfItems, List<Type> behaviourNames)
        //{
        //    for (int k = 0; k < amountOfItems; k++)
        //    {
        //        // Create the labels that will be shown on the screen.
        //        GUIContent[] items = new GUIContent[amountOfItems];

        //        // Write all the infor on the label we just created.
        //        for (int i = 0; i < amountOfItems; i++)
        //        {
        //            items[i] = new GUIContent();
        //            items[i].text = behaviourNames[i].Name;
        //        }

        //        bool changesWereMade = false;

        //        Type[] newUserSelection = new Type[amountOfItems];

        //        // Start writing on the screen.
        //        EditorGUILayout.BeginHorizontal();
        //        // Write on the window

        //        // This is the index on the controllerLabels array of the previously selected button.
        //        int selectedIndex = Array.FindIndex(items, element => element.text.Equals(userSelectedBehaviours[k].Name));

        //        // This is newly selected index by the user. We must transform it int a Type to save it.
        //        int newlySelectedIndex = EditorGUILayout.Popup(selectedIndex, items);
        //        newUserSelection[k] = behaviourNames[newlySelectedIndex];

        //        if (!(newUserSelection[k] == userSelectedBehaviours[k]))
        //            changesWereMade = true;

        //        EditorGUILayout.EndHorizontal();
        //        EditorGUILayout.Space();

        //        // If there were changes do something..
        //        if (changesWereMade)
        //        {
        //            // Loop through the list of components to add missing ones.
        //            for (int i = 0; i < newUserSelection.Length; i++)
        //            {
        //                Type foundType = null;
        //                foundType = Array.Find(userSelectedBehaviours, b => b == newUserSelection[i]);
        //                // Add the missing component to this game object.
        //                if (foundType == null)
        //                {
        //                    Component p = null;
        //                    // Case we didn't find this type among the behaviours types already added.
        //                    if (interactableObject != null)
        //                        p = interactableObject.gameObject.AddComponent(newUserSelection[i]);
        //                    if (p == null || interactableObject == null)
        //                        Debug.LogError("Problem adding game object. Check here.");
        //                }
        //            }

        //            // Loop through the list of components to remove unused ones.

        //            // Update the list of added components.
        //            userSelectedBehaviours = newUserSelection;
        //        }
        //    }
        //}

        /// <summary>
        /// Creates a pickable dropdown of buttons, so player selects which button will activate the feature.
        /// </summary>
        /// <param name="fixedButtonNames">An array of buttons to be displayed. If null will display all the available buttons for the selected platform.</param>
        /// <param name="fixedButtonActions">An array of actions that can be used with this feature. If null will use all available for the selected platform.</param>
        protected virtual void CreateSelectionButton(Button.ButtonName[] fixedButtonNames,
            params Button.ButtonActions[] fixedButtonActions)
        {
            InteractableObject interactableObject = (InteractableObject)this.target;
            if (!(interactableObject is IActiveInteraction))
            {
                // If the interactable object in question don't implement IActiveInteraction there is nothing we can do..
                Debug.LogError("OOps! Seems like you are trying to implement an active interaction but you are not implementing IActiveInteraction");
                return;
            }

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(new GUIContent("Activation Button"));
            interactableObject.AmountOfButtons = EditorGUILayout.IntField(interactableObject.AmountOfButtons);
            EditorGUILayout.EndHorizontal();

            bool changesWereMade = false;

            Button.ButtonName[] newUserSelection = new Button.ButtonName[interactableObject.AmountOfButtons];
            Button.ButtonActions[] newUserSelectionActions = new Button.ButtonActions[interactableObject.AmountOfButtons];

            // Load any previous choices.
            LoadSelectedButtons();

            for (int k = 0; k < interactableObject.AmountOfButtons; k++)
            {
                // Get the available buttons and positions for this platform.
                Button.ButtonName[] controllerButtons;
                if (fixedButtonNames != null)
                    controllerButtons = fixedButtonNames;
                else
                    controllerButtons = SupportedPlatforms.GetButtons(Core.CoreSettings.EditorSelectedPlatform);

                Button.ButtonActions[] buttonActions;
                if (fixedButtonActions.Length > 0)
                    buttonActions = fixedButtonActions;
                else
                    buttonActions = SupportedPlatforms.GetButtonActions(Core.CoreSettings.EditorSelectedPlatform);

                string[] positions = SupportedPlatforms.GetPositions(Core.CoreSettings.EditorSelectedPlatform);

                // Create the labels that will be shown on the screen.
                GUIContent[] controllerLabels = new GUIContent[controllerButtons.Length];
                GUIContent[] buttonActionLabels = new GUIContent[buttonActions.Length];
                GUIContent[] positionLabels = new GUIContent[positions.Length];

                // Write all the infor on the labels we just created.
                for (int i = 0; i < controllerButtons.Length; i++)
                {
                    controllerLabels[i] = new GUIContent();
                    controllerLabels[i].text = controllerButtons[i].ToString();
                }
                for (int i = 0; i < buttonActions.Length; i++)
                {
                    buttonActionLabels[i] = new GUIContent();
                    buttonActionLabels[i].text = buttonActions[i].ToString();
                }

                // Start writing on the screen.
                EditorGUILayout.BeginHorizontal();
                // Write on the window

                // This is the index on the controllerLabels array of the previously selected button.
                int selectedIndex = Array.FindIndex(controllerLabels, element => element.text.Equals(userSelectedButtons[k].ToString()));
                // This is the index on the buttonActionLabels array of the previously selected button.
                int selectedIndexActionButton = Array.FindIndex(buttonActionLabels, element => element.text.Equals(userSelectedActions[k].ToString()));

                // This is newly selected index by the user. We must transform it int a ControllerButtons to save it.
                int newlySelectedIndex = EditorGUILayout.Popup(selectedIndex, controllerLabels);
                newUserSelection[k] = (Button.ButtonName)Enum.Parse(typeof(Button.ButtonName), controllerLabels[newlySelectedIndex].text, true);

                int newlySelectedIndexActionButton = EditorGUILayout.Popup(selectedIndexActionButton, buttonActionLabels);
                newUserSelectionActions[k] = (Button.ButtonActions)Enum.Parse(typeof(Button.ButtonActions), buttonActionLabels[newlySelectedIndexActionButton].text, true);

                if (!(newUserSelection[k] == userSelectedButtons[k]))
                    changesWereMade = true;
                if (!(newUserSelectionActions[k] == userSelectedActions[k]))
                    changesWereMade = true;
                if (!hasInitialized)
                    changesWereMade = true;

                EditorGUILayout.EndHorizontal();
                EditorGUILayout.Space();
            }

            // Check if there was changes to the selected buttons or position (right or left) || !newUserSelectionIsRight.Equals(userSelectionIsRight) || !newUserSelectionActions.Equals(userSelectedActions)
            if (changesWereMade)
            {
                // If so update the inspector.
                userSelectedButtons = newUserSelection;
                userSelectedActions = newUserSelectionActions;

                // Register everythingy
                RegisterSelectedButtons();
               
                hasInitialized = true;
            }
        }

        /// <summary>
        /// Create a header for this interaction. 
        /// </summary>
        /// <param name="title">The interaction name.</param>
        /// <param name="version">It's version.</param>
        /// <param name="year">It's year.</param>
        protected virtual void CreateInteractionHeader(string title, string version, string year)
        {
            // Create the header bar.
            EditorGUILayout.LabelField(new GUIContent(title),
                Array.Find(skin.customStyles, element => element.name == Common.Editor.Constants.Features.HeaderGUIStyle));

            EditorGUILayout.Space();

            // Add the current version. 
            EditorGUILayout.LabelField(Constants.VersionPrefix + version + " - " + year,
                Array.Find(skin.customStyles, element => element.name == Constants.VersionGUIStyle));
        }

        /// <summary>
        /// Load the selected buttons.
        /// </summary>
        protected virtual void LoadSelectedButtons()
        {
            InteractableObject interactableObject = (InteractableObject)this.target;

            if (interactableObject.ButtonsSelected == null)
                interactableObject.ButtonsSelected = new List<ButtonRegistry>();

            userSelectedButtons = new Button.ButtonName[interactableObject.AmountOfButtons];
            userSelectedActions = new Button.ButtonActions[interactableObject.AmountOfButtons];

            // Clone the list.
            List<ButtonRegistry> tempList = new List<ButtonRegistry>();
            tempList.AddRange(interactableObject.ButtonsSelected);

            // We need templist because the amount of buttons on buttonSelected 
            // is bigger than the amount of buttons we will show on the UI.

            // Foreach button
            for (int i = 0; i < interactableObject.ButtonsSelected.Count; i++)
            {
                // Pick a button and see if it already was registered.
                ButtonRegistry buttonToRegister = tempList.Find(button => button == interactableObject.ButtonsSelected[i]);
                if (buttonToRegister == null)
                    continue; // If so continue to the next.

                // Case not register it, so it shows in the component inspector.
                userSelectedButtons[i] = buttonToRegister.Name;
                userSelectedActions[i] = buttonToRegister.Action;

                // Now remove him and all the same others from the list.
                tempList.RemoveAll(b => b.Name == buttonToRegister.Name && b.Action == buttonToRegister.Action);
            }
        }

        /// <summary>
        /// Register the selected buttons on the interaction.
        /// </summary>
        private void RegisterSelectedButtons()
        {
            InteractableObject interactableObject = (InteractableObject)this.target;

            // We must add for all known positions! The ControllerManager always calls the correct position Method.
            Button.ButtonName[] finalButtonNames = new Button.ButtonName[interactableObject.AmountOfButtons * 2];
            Button.ButtonActions[] finalButtonActions = new Button.ButtonActions[interactableObject.AmountOfButtons * 2];
            bool[] finalPositions = new bool[interactableObject.AmountOfButtons * 2];

            // Creating positions and setting final buttons to be passed.
            for (int i = 0; i < interactableObject.AmountOfButtons; i++)
            {
                // Copy buttons
                finalButtonNames[i] = userSelectedButtons[i];
                finalButtonNames[i + 1] = userSelectedButtons[i];

                // Copy Actions
                finalButtonActions[i] = finalButtonActions[i];
                finalButtonActions[i + 1] = finalButtonActions[i];

                // Create Positions
                finalPositions[i] = true;
                finalPositions[i + 1] = false;
            }

            // Calling unregister and register methods.
            IActiveInteraction interaction = interactableObject as IActiveInteraction;

            // Unregister all buttons..
            interactableObject.ButtonsSelected.Clear();

            // Register all buttons..
            Debug.Log("Registering...");

            if (interactableObject.ButtonsSelected == null)
                interactableObject.ButtonsSelected = new List<ButtonRegistry>();

            // Foreach userSelectedButton we add the button.
            for (int i = 0; i < finalButtonNames.Length; i++)
            {
                ButtonRegistry reg = ScriptableObject.CreateInstance<ButtonRegistry>();
                reg.Name = finalButtonNames[i];
                reg.Action = finalButtonActions[i];
                reg.IsRightControllerButton = finalPositions[i];
                reg.OverrideInteraction = true;
                interactableObject.ButtonsSelected.Add(reg);
            }

            
            // This saves the button selection.
            SerializedProperty buttons =  serializedObject.FindProperty("ButtonsSelected");
            buttons.arraySize = interactableObject.ButtonsSelected.Count;
            //int j = 0;
            buttons.Next(true); // Go further on this property (Ends up on Array)
            buttons.Next(true); // Go further on again.. (Ends up probaly on the array size.
            buttons.Next(true); // Go further on more time.. (Ends up on first element)
            foreach (ButtonRegistry b in interactableObject.ButtonsSelected)
            {
                buttons.objectReferenceValue = b;
                buttons.Next(false); // move to the next element. (The argument here is false because the next element is adjacent to this
                                                                 // if it was true we would go to a property inside the current element).
            }

            serializedObject.ApplyModifiedProperties();
        }

        /// <summary>
        /// This is used to fix buttons in case the component was copied but it's values were never serialized.
        /// </summary>
        public void FixRegisterMethod()
        {
            InteractableObject interactableObject = (InteractableObject)this.target;
            if (!(interactableObject is IActiveInteraction))
            {
                RegisterSelectedButtons();
                Debug.Log("Fixed!");
            }
        }
    }
}
