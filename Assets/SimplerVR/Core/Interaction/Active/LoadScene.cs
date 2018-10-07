using System;
using System.Collections.Generic;
using SimplerVR.Core.Camera;
using SimplerVR.Core.Controller;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace SimplerVR.Core.Interaction.Active
{
    /// <summary>
    /// Loads an scene.
    /// </summary>
    [Serializable] 
    public class LoadScene : InteractableObject, IActiveInteraction
    {
        [HideInInspector]
        public string SceneName;   
        [HideInInspector]
        public Vector3 DesiredPosition;

        private bool usePlacePlayer = true;

        void OnEnable()
        {
            isPassive = false;
        }

        public void Awake()
        {
            isPassive = false;

            if (ButtonsSelected.Contains(null))
                Debug.LogError("Buttons were not initialized on:" + this.gameObject.name + ". Please go to the object LoadScene" +
                    " component and select (or-reselect a button). ");


        }

        /// <summary>
        /// Main method of the load scene interaction.
        /// </summary>
        public void LoadAScene()
        {
            if (usePlacePlayer)
            {
                // Sets everything to place the player in an specific position on the next scene load.
                Transform HMD = coreSettings.CurrentPlatform.GetHeadTransform();
                Transform playAreaCenter = coreSettings.CurrentPlatform.GetPlayerTransform();

                if (HMD != null && playAreaCenter != null && playAreaCenter != HMD)
                {
                    Vector3 offset = playAreaCenter.position - HMD.position;
                    offset.y = 0;

                    LoadSceneTransitionData.PlacePlayer = true;
                    LoadSceneTransitionData.PlacePosition = DesiredPosition + offset;
                    SceneManager.sceneLoaded += OnSceneLoaded;
                }
                else if (playAreaCenter == HMD)
                {
                    LoadSceneTransitionData.PlacePlayer = true;
                    LoadSceneTransitionData.PlacePosition = HMD.position;
                    SceneManager.sceneLoaded += OnSceneLoaded;
                }                  
                else
                    Debug.LogError("Could not find player, something is off.");

                CameraManager.Instance.SetAfterFadeMethod(ChangeScene);
                CameraManager.Instance.StartFadingCamera();

            }
        }

        /// <summary>
        /// Changes the scene.
        /// </summary>
        public void ChangeScene()
        {
            SceneManager.LoadScene(SceneName);
        }

        // Called when the scene is loaded.
        void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            if (LoadSceneTransitionData.PlacePlayer)
            {
                CoreSettings coreSettingsAsset = CoreSettings.LoadCoreSettings();
                coreSettings.CurrentPlatform.GetPlayerTransform().position = LoadSceneTransitionData.PlacePosition;
                LoadSceneTransitionData.PlacePlayer = false; // Reset the player status.
                SceneManager.sceneLoaded -= OnSceneLoaded; //Unregister this evenet.
            }
        }

        //// called first
        //void OnEnable()
        //{
        //    Debug.Log("OnEnable called");
        //    SceneManager.sceneLoaded += OnSceneLoaded;
        //}

        /// <summary>
        /// Register all the buttons on the control set.
        /// </summary>
        /// <param name="userSelectedButtons">The ButtonNames</param>
        /// <param name="userSelectedActions">The ButtonActions</param>
        public void RegisterButtons(Button.ButtonName[] userSelectedButtons, Button.ButtonActions[] userSelectedActions, bool[] positions)
        {
            if (ButtonsSelected == null)
                ButtonsSelected = new List<ButtonRegistry>();

            // Foreach userSelectedButton we add the button.
            for (int i=0; i< userSelectedButtons.Length; i++)
            {
                ButtonRegistry reg = ScriptableObject.CreateInstance<ButtonRegistry>();
                reg.Name = userSelectedButtons[i];
                reg.Action = userSelectedActions[i];
                reg.IsRightControllerButton = positions[i];
                reg.OverrideInteraction = true;
                ButtonsSelected.Add(reg);
            }          
        }

        /// <summary>
        /// Clears all buttons.
        /// </summary>
        public void UnregisterButtons()
        {
            this.ButtonsSelected.Clear();
        }

        /// <summary>
        /// Given a ButtonName and a ButtonAction return the list of actions to perform.
        /// </summary>
        /// <param name="name">The ButtonName for to return the selected actions.</param>
        /// <param name="action">The ButtonAction for to return the selected actions.</param>
        /// <returns>A list of actions.</returns>
        public List<Action> GetButtonMethods(Button.ButtonName name, Button.ButtonActions action)
        {
            List<Action> returnActions = new List<Action>();
            if (ButtonsSelected == null)
            {
                Debug.LogError("No button selected.");
                return null;
            }
            if (ButtonsSelected.Contains(null))
                Debug.LogError("This object interaction: "+this.gameObject.name+" is not properly initialized! Maybe you copied its values." +
                    "Please go to it's game object inspector to update it automatically.");

            // If this button is registered return the action.
            if (ButtonsSelected.Find(reg => reg.Name == name && reg.Action == action))
                returnActions.Add(LoadAScene);

            return returnActions;
        }
    }

    /// <summary>
    /// This is used by the load scene to transition data.
    /// </summary>
    public static class LoadSceneTransitionData
    {
        public static bool PlacePlayer = false;
        public static Vector3 PlacePosition = Vector3.zero;
    }
}
