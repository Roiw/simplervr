using UnityEngine;
using System.Collections.Generic;
using System;
using SimplerVR.Core.Camera;
using SimplerVR.Core.Controller;
using SimplerVR.PlatformInterfaces;
using static SimplerVR.Core.Controller.Button;

namespace SimplerVR.Features.ArcTeleport
{
    public class ArcTeleportManager : GenericFeatureManager, IControllerFeature
    {
        [HideInInspector]
        /// <summary>
        /// The name of the feature.
        /// </summary>
        public string FeatureName = "Arc Teleport";

        [Tooltip("Parabolic Pointer object to pull destination points from, and to assign to each controller.")]
        [HideInInspector]
        public ParabolicPointer Pointer;

        [HideInInspector]
        /// <summary>
        /// True if the user is teleporting.
        /// </summary>
        public bool teleporting = false;

        private Transform centerOfPlayArea;

        // Origin of the player's head
        private Transform headTransform;

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

        // BorderRenderer to render the chaperone bounds (when choosing a location to teleport to)
        private BorderRenderer roomBorder;

        // Animator used to fade in/out the teleport area.  This should have a boolean parameter "Enabled" where if true
        // the selectable area is displayed on the ground.
        private Animator navmeshAnimator;
        private int enabledAnimatorID;
        private int materialFadeID;

        /// <summary>
        /// The controller holding the teleport button.
        /// </summary>
        private Controller activeController;
        private Controller lastActiveController;

        /// <summary>
        /// The angle of the controller on the last click.
        /// </summary>
        private float lastClickAngle = 0;

        
        private bool fadingIn = false;
        private float teleportTimeMarker = -1;
        private Mesh PlaneMesh;

        private BorderRenderer borderRenderer;

        /// <summary>
        /// The platform currently being used.
        /// </summary>
        private GenericControllerPlatform platform;

        void Start()
        {
            centerOfPlayArea = coreSettings.CurrentPlatform.GetPlayerTransform();
            roomBorder = transform.Find("Navmesh").GetComponent<BorderRenderer>();
        }

        // This is useful basically to check the right moment to teleport.
        void Update()
        {
            // If we are currently teleporting (ie handling the fade in/out transition)...
            if (teleporting)
            {
                // Wait until half of the teleport time has passed before the next event (note: both the switch from fade
                // out to fade in and the switch from fade in to stop the animation is half of the fade duration)
                if (Time.time - teleportTimeMarker >= TeleportSettings.TeleportFadeDuration / 2)
                {
                    if (fadingIn)
                    {
                        // We have finished fading out
                        teleporting = false;
                    }
                    else
                    {
                        // We have finished fading in - time to teleport!
                        if (headTransform.position == centerOfPlayArea.position)
                            centerOfPlayArea.position = Pointer.SelectedPoint;
                        else
                        {
                            Vector3 offset = centerOfPlayArea.position - headTransform.position;
                            offset.y = 0;
                            centerOfPlayArea.position = (Pointer.SelectedPoint + offset);
                        }                    
                    }

                    teleportTimeMarker = Time.time;
                    fadingIn = !fadingIn;
                }
                return;
            }
        }


        #region Feature Event Methods

        /// <summary>
        /// This happens when the user realeases the button.
        /// </summary>
        public void Teleport()
        {
            #region Initialization Stuff maybe shouldn't be here
            SetUpFadePlane();
            
            if (platform == null)
                platform = (GenericControllerPlatform)coreSettings.CurrentPlatform;
            #endregion

            // If the user has decided to teleport (ie lets go of touchpad) then remove all visual indicators
            // related to selecting things and actually teleport
            // If the user has decided to cancel (ie squeezes grip button) then remove visual indicators and do nothing
            if (Pointer.PointOnNavMesh)
            {
                // Begin teleport sequence
                teleporting = true;
                teleportTimeMarker = Time.time;
                // Starts the fade!
                CameraManager.Instance.StartFadingCamera();
            }

            // Reset active controller, disable pointer, disable visual indicators
            activeController = null;
            Pointer.enabled = false;
            if (navmeshAnimator != null)
                navmeshAnimator.SetBool(enabledAnimatorID, false);

            Pointer.transform.parent = null;
            Pointer.transform.position = Vector3.zero;
            Pointer.transform.rotation = Quaternion.identity;
            Pointer.transform.localScale = Vector3.one;

            /* Activate laser */
            Debug.Log("Teleport Requesting Laser: ON");
            ControllerManager.Instance.RequestDisplayLaser(this.gameObject.GetInstanceID());


        }

        /// <summary>
        /// This happens while the user is holding down the teleport button.
        /// </summary>
        public void WhileHoldingButtonDown()
        {
            Debug.Log("Chosing where to teleport!");
            Controller newController;
            List<ButtonRegistry> regs = TeleportSettings.ButtonsSelected;
            // Figuring which controller just pressed the button.
            for (int i = 0; i < regs.Count; i++)
            {
                // If true, we found which controller to get (the one that owns TeleportButtons[i] button).
                if (ControllerManager.Instance.CheckButtonInteracting(regs[i].Name, Button.ButtonActions.HoldDown, regs[i].IsRightControllerButton))
                {
                    if (regs[i].IsRightControllerButton)
                        newController = ControllerManager.Instance.GetRightController().GetComponent<Controller>();
                    else
                        newController = ControllerManager.Instance.GetLeftController().GetComponent<Controller>();

                    lastActiveController = activeController == newController ? lastActiveController : activeController;
                    activeController = newController;
                }
            }


            // Set active controller to this controller, and enable the parabolic pointer and visual indicators
            // that the user can use to determine where they are able to teleport.                 
            Pointer.transform.parent = activeController.transform;
            Pointer.transform.localPosition = Vector3.zero;
            Pointer.transform.localRotation = Quaternion.identity;
            Pointer.transform.localScale = Vector3.one;
            Pointer.enabled = true;

            if (navmeshAnimator != null)
                navmeshAnimator.SetBool(enabledAnimatorID, true);

            Pointer.ForceUpdateCurrentAngle();
            lastClickAngle = Pointer.CurrentParabolaAngle;

            if (platform == null)
                platform = (GenericControllerPlatform)coreSettings.CurrentPlatform;

            if (headTransform == null)
                headTransform = platform.GetHeadTransform();

            // The user is still deciding where to teleport and has the touchpad held down.
            // Note: rendering of the parabolic pointer / marker is done in ParabolicPointer
            Vector3 offset = headTransform.position - centerOfPlayArea.position;
            offset.y = 0;

            // Haptic feedback click every [HaptickClickAngleStep] degrees
            float angleClickDiff = Pointer.CurrentParabolaAngle - lastClickAngle;
            if (platform.UseHapticPulse())
            {
                if (Mathf.Abs(angleClickDiff) > TeleportSettings.HapticClickAngleStep)
                {
                    lastClickAngle = Pointer.CurrentParabolaAngle;
                    platform.TriggerHapticPulse(.5f, activeController);
                }
            }

            /* Deactivate Laser */
            ControllerManager.Instance.RequestHideLaser(this.gameObject.GetInstanceID());
        }

        #endregion 

        /// <summary>
        /// Creates a fade plane to fade the camera.
        /// </summary>
        private void SetUpFadePlane()
        {
            // Standard plane mesh used for "fade out" graphic when you teleport
            // This way you don't need to supply a simple plane mesh in the inspector
            if (PlaneMesh == null)
                PlaneMesh = new Mesh();

            Vector3[] verts = new Vector3[] { new Vector3(-1, -1, 0), new Vector3(-1, 1, 0), new Vector3(1, 1, 0), new Vector3(1, -1, 0) };

            int[] elts = new int[] { 0, 1, 2, 0, 2, 3 };

            PlaneMesh.vertices = verts;
            PlaneMesh.triangles = elts;
            PlaneMesh.RecalculateBounds();

            // Set some standard variables
            materialFadeID = Shader.PropertyToID("_Fade");
            enabledAnimatorID = Animator.StringToHash("Enabled");
        }

        /// <summary>
        /// Returns the name of the feature.
        /// </summary>
        /// <returns>The name of the feature.</returns>
        public override string GetFeatureName()
        {
            return FeatureName;
        }

        /// <summary>
        /// Returns the teleport method.
        /// </summary>
        /// <returns>Returns the teleport method.</returns>
        public Action GetTeleportMethod()
        {
            return Teleport;
        }

        /// <summary>
        /// Returns the method to run when holding the button.
        /// </summary>
        /// <returns>Returns the method to run when holding the button.</returns>
        public Action GetHoldingButtonAction()
        {
            return WhileHoldingButtonDown;
        }

        /// <summary>
        /// Returns the feature type.
        /// </summary>
        /// <returns>Feature type</returns>
        public override Type GetFeatureType()
        {
            return typeof(ArcTeleportManager);
        }

        /// <summary>
        /// Creates the registry of a button (ButtonRegistry) on the serializable data class.
        /// </summary>
        /// <param name="name">The ButtonName of the button.</param>
        /// <param name="action">The ButtonActions of the button.</param>
        /// <param name="overridesInteraction">True if this button overrides interaction.</param>
        /// <param name="isRightController">True if this button stays on the right controller.</param>
        private void SaveButtonChoice(Button.ButtonName name, Button.ButtonActions action, bool overridesInteraction, bool isRightController)
        {
            ButtonRegistry registry = ScriptableObject.CreateInstance<ButtonRegistry>();
            registry.Name = name;
            registry.Action = action;
            registry.OverrideInteraction = overridesInteraction;
            registry.IsRightControllerButton = isRightController;

            if (action == Button.ButtonActions.PressUp)
                registry.methodName = nameof(Teleport);
            else if (action == Button.ButtonActions.HoldDown)
                registry.methodName = nameof(WhileHoldingButtonDown);
            else
            {
                Debug.LogError("Could not find a method to register for this action, is there something off?");
                return;
            }

            // Append it to the teleport Settings object.
            registry.AddToAsset(TeleportSettings.GetPathToData()); // This is a costy operation..

            if (!TeleportSettings.ButtonsSelected.Contains(registry))
                TeleportSettings.ButtonsSelected.Add(registry);

            TeleportSettings.Save();// This is a costy operation..
        }

        /// <summary>
        /// Removes a registry from the list to be serialized.
        /// </summary>
        /// <param name="name">The ButtonName of the button.</param>
        /// <param name="action">The ButtonActions of the button.</param>
        /// <param name="overridesInteraction">True if this button overrides interaction.</param>
        /// <param name="isRightController">True if this button stays on the right controller.</param>
        private void RemoveButtonChoice(Button.ButtonName name, Button.ButtonActions action, bool overridesInteraction, bool isRightController)
        {
            ButtonRegistry registry = ScriptableObject.CreateInstance<ButtonRegistry>();
            registry.Name = name;
            registry.Action = action;
            registry.OverrideInteraction = overridesInteraction;
            registry.IsRightControllerButton = isRightController;


            if (action == Button.ButtonActions.PressUp)
                registry.methodName = nameof(Teleport);
            else if (action == Button.ButtonActions.HoldDown)
                registry.methodName = nameof(WhileHoldingButtonDown);
            else
                Debug.LogError("Could not find a method to register for this action, is there something off?");

            if (TeleportSettings.ButtonsSelected.Contains(registry))
                TeleportSettings.ButtonsSelected.Remove(registry);
        }

        #region Interface Methods

        /// <summary>
        ///  Register the buttons on the platform and on this feature serializeble registries.
        /// </summary>
        /// <param name="userSelectedButtons">The name of the selected buttons.</param>
        /// <param name="userSelectedActions">The actions of each button.</param>
        /// <param name="userSelectionIsRight">If each button is on the right hand side.</param>
        /// <param name="overrideInteraction">If each button should override interaction.</param>
        public void RegisterActionButtons(Button.ButtonName[] userSelectedButtons, Button.ButtonActions[] userSelectedActions,
            bool[] userSelectionIsRight, bool[]  overrideInteraction)
        {
            // For the case of the Arc teleport we must, add a fixed HoldDown for the same button.

            // Adding an extra button to be used as a HoldDown
            Button.ButtonName[] buttonName = new Button.ButtonName[userSelectedButtons.Length * 2];
            int j = 0;
            for (int i = 0; i < userSelectedButtons.Length; i++)
            {
                buttonName[j] = userSelectedButtons[i];
                buttonName[j + 1] = userSelectedButtons[i];
                j += 2;
            }

            // Making the extra button have the holddown action.
            Button.ButtonActions[] buttonActions = new Button.ButtonActions[userSelectedActions.Length * 2];
            j = 0;
            for (int i = 0; i < userSelectedActions.Length; i++)
            {
                buttonActions[j] = userSelectedActions[i];
                buttonActions[j + 1] = Button.ButtonActions.HoldDown;
                j += 2;
            }

            bool[] position = new bool[userSelectionIsRight.Length * 2];
            j = 0;
            for (int i = 0; i < userSelectionIsRight.Length; i++)
            {
                position[j] = userSelectionIsRight[i];
                position[j + 1] = userSelectionIsRight[i];
                j += 2;
            }

            // Retrieve the Actions(methods) from the ArcTeleportManager.
            Action teleportMethod = GetTeleportMethod();
            Action holdingMethod = GetHoldingButtonAction();

            // Attempt to register them on the current selected platform.

            // Retrieve the platform.
            GenericControllerPlatform platform;

            if (coreSettings.CurrentPlatform.GetType().IsSubclassOf(typeof(GenericControllerPlatform)))
                platform = (GenericControllerPlatform)coreSettings.CurrentPlatform;
            else
            {
                Debug.LogError("Platform doesn't support controllers. ArcTeleport require controls to work.");
                return;
            }

            //  Foreach button register it on the platform.
            for (int i = 0; i < buttonName.Length; i++)
            {
                // Register feature method. Register the teleport method to PushUp events and the selection method to HoldDown events.
                if (buttonActions[i] == Button.ButtonActions.HoldDown)
                {
                    /* Registering the method (On the Platform Controller). */
                    platform.GetPlatformControls().AddButton(buttonName[i], buttonActions[i], position[i], holdingMethod, false, GetFeatureType());

                    // Saving a reference to the button on the settings to be serialized.
                    SaveButtonChoice(buttonName[i], buttonActions[i], false, position[i]);

                }
                else if (buttonActions[i] == Button.ButtonActions.PressUp)
                {
                    /* Registering the method (On the Platform Controller). */
                    platform.GetPlatformControls().AddButton(buttonName[i], buttonActions[i], position[i], teleportMethod, false, GetFeatureType());

                    // Saving a reference to the button on the settings to be serialized.
                    SaveButtonChoice(buttonName[i], buttonActions[i], false, position[i]);
                }
                else
                    Debug.LogError("Something is of Arc Teleport is not set to either HoldDown or PressUp");
            }
        }

        /// <summary>
        ///  Clear selected buttons locally.
        /// </summary>
        public void ClearButtonRegistries()
        {
            if (TeleportSettings.ButtonsSelected.Count == 0)
            {
                // This happens during initialization
                // No button recorded on the feature.
                return;
            }

            // Clear the list of buttons we have.
            for (int i = 0; i < TeleportSettings.ButtonsSelected.Count; i++)
            {
                ButtonRegistry.Delete(TeleportSettings.ButtonsSelected[i], TeleportSettings);
            }
            TeleportSettings.ButtonsSelected.Clear();

        }

        /// <summary>
        /// Returns a list of ButtonRegistry for each button.
        /// </summary>
        /// <returns></returns>
        public List<ButtonRegistry> GetAllButtonRegistries()
        {
            return TeleportSettings.GetSelectedButtons();
        }

        /// <summary>
        /// Returns the right method from a feature given a ButtonRegistry.
        /// </summary>
        /// <param name="buttonRegistry">A button registry.</param>
        /// <returns>A method</returns>
        public Action GetButtonMethod(ButtonRegistry buttonRegistry)
        {
            if (TeleportSettings.ButtonsSelected == null)
            {
                Debug.LogError("ButtonsSelected on TeleportSettings not initialized!");
                return null;
            }

            // Retrieving the button.
            List<ButtonRegistry> matchButtons = TeleportSettings.ButtonsSelected.FindAll(b => b.Action == buttonRegistry.Action &&
                                                       b.Name == buttonRegistry.Name &&
                                                       b.IsRightControllerButton == buttonRegistry.IsRightControllerButton &&
                                                       b.OverrideInteraction == buttonRegistry.OverrideInteraction);
            if (matchButtons.Count == 0)
            {
                Debug.LogError("No buttons were found for this ButtonRegistry.");
                return null;
            }
            else if (matchButtons.Count > 1)
            {
                Debug.Log("Multiple buttons with the same registry found. Something could be wrong with the serialization.");
            }

            // Finding the proper method.
            if (matchButtons[0].methodName == nameof(Teleport))
                return Teleport;
            else if (matchButtons[0].methodName == nameof(WhileHoldingButtonDown))
                return WhileHoldingButtonDown;

            return null;
        }

        #endregion
    }
}
