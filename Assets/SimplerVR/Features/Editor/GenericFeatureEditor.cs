using System;
using SimplerVR.Common;
using SimplerVR.Core;
using SimplerVR.Core.Controller;
using SimplerVR.PlatformInterfaces;
using UnityEditor;
using UnityEngine;
using static SimplerVR.Core.Controller.Button;
using static SimplerVR.Common.Editor.Constants;

namespace SimplerVR.Features.Editor
{
    public class GenericFeatureEditor : EditorWindow
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
        /// <para>If you are using the CreateSelectionButton method. The button selected will have it's index saved on this variable.</para>
        /// <para>Otherwise the value is 0.</para>
        /// </summary>
        protected Button.ButtonName[] userSelectedButtons = { 0 };

        /// <summary>
        /// Works similat to the userSelectedButtons but for ButtonActions instead of ButtonNames.
        /// </summary>
        protected Button.ButtonActions[] userSelectedActions = { 0 };

        /// <summary>
        /// <para>If you are using the CreateSelectionButton method. If the selected button is on the right controller this variable will be true.</para>
        /// <para>Otherwise the value is 0.</para>
        /// </summary>
        protected bool[] userSelectionIsRight = { true };

        /// <summary>
        /// An array of the selected buttons by the user.
        /// </summary>
        protected ButtonRegistry[] selectedButtons;

        /// <summary>
        /// The amount of buttons used for the feature.
        /// </summary>
        protected int amountOfButtons = 1;

        /// <summary>
        /// The last message sent from the feature.
        /// </summary>
        private string lastMessage = "";

        /// <summary>
        /// True if the message has a positive meaning.
        /// </summary>
        private bool isGoodMessage = true;

        /// <summary>
        /// This guarantees that on the first run we will register the events.
        /// </summary>
        private bool hasInitialized = false;

        /// <summary>
        /// Runs every inspector update.
        /// </summary>
        void OnInspectorUpdate()
        {
            Repaint();
        }

        //// <summary>
        /// A reference to the settings of the core of the API.
        /// </summary>
        private CoreSettings coreSettingsAsset;
        protected CoreSettings coreSettings
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
        /// Runs as soon as the window start.
        /// </summary>
        protected virtual void OnEnable()
        {
            skin = Resources.Load<GUISkin>(SkinFileName);
            headerTexture = Resources.Load<Texture>(Common.Editor.Constants.Features.HeaderTextureFileName); 
        }

        /// <summary>
        /// Create feature window header.
        /// </summary>
        /// <param name="featureName">The name of the file containing the picture (PNG).</param>
        /// <param name="version">A text containing the version.</param>
        protected virtual void CreateHeader(string featureName, string version, string year)
        {
            // Create the header bar.
            EditorGUILayout.LabelField(new GUIContent(featureName),
                Array.Find(skin.customStyles, element => element.name == Common.Editor.Constants.Features.HeaderGUIStyle));

            EditorGUILayout.Space();

            // Add the current version. 
            EditorGUILayout.LabelField(VersionPrefix + version + " - " + year,
                Array.Find(skin.customStyles, element => element.name == VersionGUIStyle));
        }


        /// <summary>
        /// Creates a pickable dropdown of buttons, so player selects which button will activate the feature.
        /// </summary>
        /// <param name="registerActions">A method to register the buttons on the platform.</param>
        /// <param name="unregisterActions"> A method to unregister the buttons on the platform.</param>
        /// <param name="featureType">The type of the feature for instance(ArcTeleport).</param>
        /// <param name="fixedButtonNames">An array of buttons to be displayed. If null will display all the available buttons for the selected platform.</param>
        /// <param name="fixedButtonActions">An array of actions that can be used with this feature. If null will use all available for the selected platform.</param>
        /// <param name="initialValues">An array of initial values that were serialized.</param>
        protected virtual void CreateSelectionButton(Action<GameObject> registerActions, Action<GameObject> unregisterActions, 
            Type featureType, Button.ButtonName[] fixedButtonNames, Button.ButtonActions[] fixedButtonActions, params ButtonRegistry[] initialValues)
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(new GUIContent("Activation Button"));
            amountOfButtons = EditorGUILayout.IntField(amountOfButtons);
            EditorGUILayout.EndHorizontal();

            bool changesWereMade = false;

            // No previous serialized settings setting or this was already serialized.
            // We only load from the serialized settings when the window is open.
            if (initialValues.Length != 0 && !hasInitialized)
                amountOfButtons = initialValues.Length;

            // Initialize vectors that will hold the new selection by the user..
            Button.ButtonName[] newUserSelection = new Button.ButtonName[amountOfButtons];
            Button.ButtonActions[] newUserSelectionActions = new Button.ButtonActions[amountOfButtons];
            bool[] newUserSelectionIsRight = new bool[amountOfButtons];

            // Expand or retract the vectors of selected options.
            if (amountOfButtons < userSelectedButtons.Length)
            {
                // Retract..
                userSelectedButtons = userSelectedButtons.DescreaseArray(userSelectedButtons, userSelectedButtons.Length - amountOfButtons);
                userSelectedActions = userSelectedActions.DescreaseArray(userSelectedActions, userSelectedActions.Length - amountOfButtons);
                userSelectionIsRight = newUserSelectionIsRight.DescreaseArray(userSelectionIsRight, userSelectionIsRight.Length - amountOfButtons);
            }
            else if (amountOfButtons > userSelectedButtons.Length)
            {
                // Expand..
                userSelectedButtons = userSelectedButtons.ExpandArray(userSelectedButtons, amountOfButtons - userSelectedButtons.Length);
                userSelectedActions = userSelectedActions.ExpandArray(userSelectedActions, amountOfButtons - userSelectedActions.Length);
                userSelectionIsRight = newUserSelectionIsRight.ExpandArray(userSelectionIsRight, amountOfButtons - userSelectionIsRight.Length);
            }

            // If it haven't been initialized yet and we have values to initialize do it.
            if (!hasInitialized && initialValues.Length != 0)
            {
                // Copy initial values to the user selected buttons.
                if (amountOfButtons >= initialValues.Length)
                {
                    for (int i = 0; i < initialValues.Length; i++)
                    {
                        userSelectedButtons[i] = initialValues[i].Name;
                        userSelectedActions[i] = initialValues[i].Action;
                        userSelectionIsRight[i] = initialValues[i].IsRightControllerButton;
                    }
                }
            }

            // Now we set each dropdown..
            for (int k = 0; k < amountOfButtons; k++)
            {
                // Get the available buttons for this platform.. 
                Button.ButtonName[] controllerButtons;
                if (fixedButtonNames != null)
                    controllerButtons = fixedButtonNames;
                else
                    controllerButtons = SupportedPlatforms.GetButtons(Core.CoreSettings.EditorSelectedPlatform);

                // Get the available actions for this platform.. 
                Button.ButtonActions[] buttonActions;
                if (fixedButtonActions.Length > 0)
                    buttonActions = fixedButtonActions;
                else
                    buttonActions = SupportedPlatforms.GetButtonActions(Core.CoreSettings.EditorSelectedPlatform);

                // Get the availble positions for this platform..
                string[] positions = SupportedPlatforms.GetPositions(Core.CoreSettings.EditorSelectedPlatform);

                // Create the labels that will be shown on the screen.
                GUIContent[] controllerLabels = new GUIContent[controllerButtons.Length];
                GUIContent[] buttonActionLabels = new GUIContent[buttonActions.Length];
                GUIContent[] positionLabels = new GUIContent[positions.Length];

                // Write all the info on the labels we just created.
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
                // This is the index on the buttonActionLabels array of the previously selected button.
                int selectedIndexActionButton = Array.FindIndex(buttonActionLabels, element => element.text.Equals(userSelectedActions[k].ToString()));

                // This is newly selected index by the user. We must transform it int a ControllerButtons to save it.
                int newlySelectedIndex = EditorGUILayout.Popup(selectedIndex, controllerLabels);
                newUserSelection[k] = (Button.ButtonName)Enum.Parse(typeof(Button.ButtonName), controllerLabels[newlySelectedIndex].text, true);

                int newlySelectedIndexActionButton = EditorGUILayout.Popup(selectedIndexActionButton, buttonActionLabels);
                newUserSelectionActions[k] = (Button.ButtonActions)Enum.Parse(typeof(Button.ButtonActions), buttonActionLabels[newlySelectedIndexActionButton].text, true);

                if (positions[selectedPosition].Equals("Right"))
                    newUserSelectionIsRight[k] = true;
                else
                    newUserSelectionIsRight[k] = false;

                if (!(newUserSelection[k] == userSelectedButtons[k]))
                    changesWereMade = true;
                if (!(newUserSelectionIsRight[k] == userSelectionIsRight[k]))
                    changesWereMade = true;
                if (!(newUserSelectionActions[k] == userSelectedActions[k]))
                    changesWereMade = true;
                if (!hasInitialized)
                {
                    hasInitialized = true;
                    changesWereMade = true;
                }
                    

                EditorGUILayout.EndHorizontal();
                EditorGUILayout.Space();
            }

            // Check if there was changes to the selected buttons or position (right or left) || !newUserSelectionIsRight.Equals(userSelectionIsRight) || !newUserSelectionActions.Equals(userSelectedActions)
            if (changesWereMade)
            {
                // If so update the inspector.
                userSelectedButtons = newUserSelection;
                userSelectedActions = newUserSelectionActions;
                userSelectionIsRight = newUserSelectionIsRight;

                // Update on the main script (send info to the core too).
                GameObject feature = FindFeatureOfType(featureType);

                // if (hasInitialized)
                unregisterActions(feature);

                Debug.Log("Registering...");
                registerActions(feature);
            }
        }

        /// <summary>
        /// <para>A label that displays the information regarding operations done at the feature windows.</para>
        /// <para>Useful for the user to have feedback on his actions.</para>
        /// </summary>
        protected virtual void CreateStatusLabel()
        {
            if (isGoodMessage)
                EditorGUILayout.LabelField(new GUIContent(lastMessage));
            else
                EditorGUILayout.LabelField(new GUIContent(lastMessage)); // Add diferent layouts.
        }

        /// <summary>
        /// <para>Manages the CreateAddButton, CreateUpdateButton and CreateRemoveButton.</para>
        /// <para>This method</para>
        /// </summary>
        /// <param name="setupMethod"></param>
        /// <param name="featureType">The type of the feature.</param>
        /// <param name="featureName">The feature name.</param>
        protected virtual void CreateFeatureManagementButtons(Action<GameObject> setupMethod, string featureName, Type featureType)
        {
            // Check if feature exists.
            GameObject Feature = null;
            Feature = FindFeatureOfType(featureType);

            // Case we found the feature. Add Remove and Update buttons.
            if (Feature != null)
            {
                CreateRemoveButton(featureType);
            }
            // Case we havent found the feature. Add Create button.
            else
                CreateAddButton(setupMethod, featureName, featureType);

        }

        /// <summary>
        /// <para>Manages the CreateAddButton, CreateUpdateButton and CreateRemoveButton.</para>
        /// </summary>
        /// <param name="setupMethod">A method to setup the feature.</param>
        /// <param name="unregisterEvents">A method to unregister events from the Feature</param>
        /// <param name="featureName">The feature name.</param>
        /// <param name="featureType">The type of the feature.</param>
        protected virtual void CreateFeatureManagementButtons(Action<GameObject> setupMethod, Action<GameObject> unregisterEvents, string featureName, Type featureType)
        {
            // Check if feature exists.
            GameObject Feature = null;
            Feature = FindFeatureOfType(featureType);

            // Case we found the feature. Add Remove and Update buttons.
            if (Feature != null)
            {
                CreateRemoveButton(unregisterEvents, featureType);
            }
            // Case we havent found the feature. Add Create button.
            else
                CreateAddButton(setupMethod, featureName, featureType);

        }

        /// <summary>
        /// Create a button to add the feature to the Core system.
        /// </summary>
        /// <param name="setupMethod">A method for configuring the created object0 .Setting ups its variables.</param>
        /// <param name="featureType">The type of the feature.</param>
        /// <param name="featureName">The feature name.</param>
        protected virtual void CreateAddButton(Action<GameObject> setupMethod, string featureName, Type featureType)
        {
            Rect k = EditorGUILayout.BeginHorizontal("Button");
            if (GUI.Button(k, GUIContent.none, skin.customStyles[0]))
                OnClickAddFeature(setupMethod, featureName, featureType);

            EditorGUILayout.LabelField(new GUIContent("Add Feature", "This will add this feature to the system."),
                Array.Find(skin.customStyles, element => element.name == ButtonLabelUIStyle));
            EditorGUILayout.EndHorizontal();
        }

        /// <summary>
        /// Create a button to remove the feature to the Core system.
        /// </summary>
        /// <param name="featureType">The type of the feature.</param>
        protected virtual void CreateRemoveButton(Type featureType)
        {
            Rect k = EditorGUILayout.BeginHorizontal("Button");
            if (GUI.Button(k, GUIContent.none, skin.customStyles[0]))
                OnClickRemoveFeature(featureType);
            EditorGUILayout.LabelField(new GUIContent("Remove Feature", "This will add this feature to the system."),
                Array.Find(skin.customStyles, element => element.name == ButtonLabelUIStyle));
            EditorGUILayout.EndHorizontal();
        }

        /// <summary>
        /// Create a button to remove the feature to the Core system.
        /// </summary>
        /// <param name="unregisterMethod">A method to unregister events of the feature</param>
        /// <param name="featureType">The type of the feature.</param>
        protected virtual void CreateRemoveButton(Action<GameObject> unregisterMethod, Type featureType)
        {
            Rect k = EditorGUILayout.BeginHorizontal("Button");
            if (GUI.Button(k, GUIContent.none, skin.customStyles[0]))
                OnClickRemoveFeature(unregisterMethod, featureType);
            EditorGUILayout.LabelField(new GUIContent("Remove Feature", "This will add this feature to the system."),
                Array.Find(skin.customStyles, element => element.name == ButtonLabelUIStyle));
            EditorGUILayout.EndHorizontal();
        }

        /// <summary>
        /// Creates a save button.
        /// </summary>
        public void CreateSaveButton(Action save)
        {
            Rect k = EditorGUILayout.BeginHorizontal("Button");
            if (GUI.Button(k, GUIContent.none, skin.customStyles[0]))
                save.Invoke();

            EditorGUILayout.LabelField(new GUIContent("Save Changes", "This will save changes to disk."),
                Array.Find(skin.customStyles, element => element.name == ButtonLabelUIStyle));
            EditorGUILayout.EndHorizontal();
        }

        #region Click Methods

        private void OnClickAddFeature(Action<GameObject> setupMethod, string featureName, Type featureType)
        {
            // Sets the feature as not initialized.
            hasInitialized = false;

            // Check if feature has already been added.
            GameObject FeaturesGameObject = GameObject.Find(Constants.API.RootObjectName + "/" + Constants.API.RootFeatureObjectName);

            // Look into each feature game object
            for (int i = 0; i < FeaturesGameObject.transform.childCount; i++)
            {
                // If != null means we found this feature. Since features are atomic (can only have one) this will not be spawn.
                if (FeaturesGameObject.transform.GetChild(i).GetComponent(featureType) != null)
                {
                    lastMessage = "Feature already exist on the system, therefore a new one will not be added.";
                    return;
                }
            }

            // Since the feature don't exist we will create it.

            /* Create the game object */
            GameObject obj = new GameObject(featureName);
            // Set as child of the features game object.
            obj.transform.SetParent(GameObject.Find(Constants.API.RootObjectName + "/" + Constants.API.RootFeatureObjectName).transform);

            /* Add the proper script */
            setupMethod(obj);

            lastMessage = "Feature created successfully.";
        }

        /// <summary>
        /// Removes this feature.
        /// </summary>
        /// <param name="featureType">This feature type.</param>
        private void OnClickRemoveFeature(Type featureType)
        {

            // Check if feature exists.
            GameObject FeaturesGameObject = GameObject.Find(Constants.API.RootObjectName + "/" + Constants.API.RootFeatureObjectName);
            GameObject Feature = null;
            // Look into each feature game object
            for (int i = 0; i < FeaturesGameObject.transform.childCount; i++)
            {
                // If != null means we found this feature. Since features are atomic (can only have one) this will not be spawn.
                if (FeaturesGameObject.transform.GetChild(i).GetComponent(featureType) != null)
                {
                    Feature = FeaturesGameObject.transform.GetChild(i).gameObject;
                }
            }

            if (Feature == null)
            {
                lastMessage = "Feature not found on the system, therefore nothing will be deleted.";
                return;
            }

            

            /// Remove the component from the game object so we can add a new updated one.
            DestroyImmediate(Feature);

            lastMessage = "Feature removed successfully.";
        }

        /// <summary>
        /// Removes this feature.
        /// </summary>
        /// <param name="unregisterEvents">A method for unregistering the events related to this feature.</param>
        /// <param name="featureType">This feature type</param>
        private void OnClickRemoveFeature(Action<GameObject> unregisterEvents, Type featureType)
        {

            // Finds the feature game object.
            GameObject Feature = FindFeatureOfType(featureType);
           
            if (Feature == null)
            {
                lastMessage = "Feature not found on the system, therefore nothing will be deleted.";
                return;
            }

            /// Remove all methods.
            unregisterEvents(Feature);

            /// Remove the component from the game object so we can add a new updated one.
            DestroyImmediate(Feature);

            lastMessage = "Feature removed successfully.";
        }

        #endregion

        /// <summary>
        /// Attempts to find a Feature with the specified type.
        /// </summary>
        /// <param name="featureType">The type of the feature we want to find.</param>
        /// <returns>The feature of type (featureType) or null. </returns>
        protected GameObject FindFeatureOfType(Type featureType)
        {
            // Check if feature exists.
            GameObject FeaturesGameObject = GameObject.Find(Constants.API.RootObjectName + "/" + Constants.API.RootFeatureObjectName);
            if (FeaturesGameObject == null)
            {
                //Debug.LogError("Can't find feature game object. Maybe it was deleted.");
                return null;
            }
                
            GameObject Feature = null;
            // Look into each feature game object
            for (int i = 0; i < FeaturesGameObject.transform.childCount; i++)
            {
                // If != null means we found this feature. Since features are atomic (can only have one) this will not be spawn.
                if (FeaturesGameObject.transform.GetChild(i).GetComponent(featureType) != null)
                {
                    Feature = FeaturesGameObject.transform.GetChild(i).gameObject;
                }
            }

            return Feature;
        }
    }
}
