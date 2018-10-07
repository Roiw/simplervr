using SimplerVR.Core;
using SimplerVR.Core.Controller;
using System;
using System.Collections.Generic;
using SimplerVR.Common;
using SimplerVR.Core.Camera;
using SimplerVR.Core.Interaction;
using SimplerVR.PlatformInterfaces;
using UnityEngine;
using SimplerVR.Features.ArcTeleport;
using Physics = SimplerVR.Common.Physics;

namespace SimplerVR.Core.Controller
{
    public class ControllerManager : MonoBehaviour
    {
        private static ControllerManager instance;
        public static ControllerManager Instance
        {
            get
            {
                //  We don't have an instance of the ControllerManager.
                if (instance == null)
                {
                    Debug.LogError("Missing Controller Manager instance!");
                    return null;
                }
                else
                    return instance;
            }
        }

        /// <summary>
        /// The generic controller platform associated with the coreSettings.
        /// </summary>
        private GenericControllerPlatform genericControllerPlatform
        {
            get
            {
                return (GenericControllerPlatform)coreSettings.CurrentPlatform;
            }
        }

        /// <summary>
        /// A reference to the settings of the core of the API.
        /// </summary>
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

        [HideInInspector]
        /// <summary>
        /// A list of the game objects that contain controllers.
        /// </summary>
        public List<GameObject> ControllerGameObjects;

        public GameObject RightController;
        public GameObject LeftController;


        #region Laser Variables

        /// <summary>
        /// True if the laser is active at the moment.
        /// </summary>
        private bool laserEnabled = false;

        /// <summary>
        /// True if the application is using laser.
        /// </summary>
        public bool UseLaserInteraction
        {
            get { return coreSettings.UseLaserInteraction; }
        }

        /// <summary>
        /// True if laser should appear on the left hand.
        /// </summary>
        public bool LaserOnLeft
        {
            get { return coreSettings.LaserOnLeftHand; }
        }

        /// <summary>
        /// The exetension of the laser in meters.
        /// </summary>
        public float LaserLenght
        {
            get { return coreSettings.LaserLenght; }
        }

        /// <summary>
        /// Every layer selected here will collide with the laser.
        /// </summary>
        public LayerMask LaserCollision
        {
            get { return coreSettings.LaserCollision; }
        }

        /// <summary>
        /// The sprite that will show once a laser collides with anything.
        /// </summary>
        public Sprite LaserCollisionSprite
        {
            get { return coreSettings.LaserCollisionSprite; }
        }

        /// <summary>
        /// The material that will show on the collision sprite.
        /// </summary>
        public Material LaserCollisionMaterial
        {
            get { return coreSettings.LaserCollisionMaterial; }
        }

        /// <summary>
        /// A material for the laser.
        /// </summary>
        public Material LaserMaterial
        {
            get { return coreSettings.LaserMaterial; }
        }

        /// <summary>
        /// Scale of the laser collision sprite.
        /// </summary>
        public float LaserCollisionScale
        {
            get { return coreSettings.LaserCollisionScale; }
        }

        [HideInInspector]
        /// <summary>
        /// List of object ids that want the laser overlay disabled. 
        /// </summary>
        public List<int> RequestingLaserOverlay;

        /// <summary>
        /// List of GameObject Id's that are currently requesting the teleport to be disabled.
        /// </summary>
        public List<int> DisablingLaser;

        [HideInInspector]
        /// <summary>
        /// True if the laser overlay is active. 
        /// </summary>
        public bool LaserOverlay = true;

        [HideInInspector]
        /// <summary>
        /// The object where the laser will be spawned.
        /// </summary>
        public GameObject LaserHolder;

        /// <summary>
        /// Information regarding the current laser hit (notified by the controller).
        /// </summary>
        private RaycastHit laserHit;

        /// <summary>
        /// True if the laser is hitting an object (notified by the controller).
        /// </summary>
        private bool laserIsHitting = false;

        /// <summary>
        /// Object that holds the laser collision  (Dot)
        /// </summary>
        private GameObject LaserCollisionObj;

        #endregion

        [HideInInspector]
        /// <summary>
        /// The fixed control used by the controller manager. Defined on the Core Settings. 
        /// </summary>
        public ControlSet FixedControl
        {
            get
            {
                // Attemp to get a fixed control from the Current Platform.
                if (coreSettings.CurrentPlatform.GetType().IsSubclassOf(typeof(GenericControllerPlatform)))
                {
                    return ((GenericControllerPlatform)coreSettings.CurrentPlatform).GetPlatformControls();
                }
                else
                {
                    Debug.LogError("The platform selected doesn't seem to support controls.");
                    return null;
                }
            }
        }

        /// <summary>
        /// List of GameObject Id's that are currently requesting the teleport to be disabled.
        /// </summary>
        public List<int> DisablingTeleport;

        [HideInInspector]
        /// <summary>
        /// If the Teleport should be enabled.
        /// </summary>
        public bool TeleportEnabled = true;

        /// <summary>
        /// True if the LeftController is Interacting.
        /// </summary>
        public bool isLeftControllerInteracting;

        /// <summary>
        /// True if the RightController is Interacting.
        /// </summary>
        public bool isRightControllerInteracting;

        /// <summary>
        /// A reference to the RightController Controller Script.
        /// </summary>
        private Controller rightControllerScript;

        /// <summary>
        /// A reference to the LeftController Controller Script.
        /// </summary>
        private Controller leftControllerScript;

        #region Controller Management


        // Pressed Up: Pressed up buttons have their value set to true when the button is realeased.
        /* Right Hand Pressed Up Buttons */
        private List<KeyValuePair<Button.ButtonName, bool>> pressedUp_RightControllerButtons = new List<KeyValuePair<Button.ButtonName, bool>>();
        /* Left Hand Pressed Up Buttons */
        private List<KeyValuePair<Button.ButtonName, bool>> pressedUp_LeftControllerButtons = new List<KeyValuePair<Button.ButtonName, bool>>();

        // Hold Down: Hold down buttons have their values set to true while the button is being pressed down.
        /* Right Hand Pressed Up Buttons */
        private List<KeyValuePair<Button.ButtonName, bool>> holdDown_RightControllerButtons = new List<KeyValuePair<Button.ButtonName, bool>>();
        /* Left Hand Pressed Up Buttons */
        private List<KeyValuePair<Button.ButtonName, bool>> holdDown_LeftControllerButtons = new List<KeyValuePair<Button.ButtonName, bool>>();

        private List<PressControl> controllerPressControl = new List<PressControl>();

        private List<bool> registeredControllers;

        /// <summary>
        /// A data structure that holds a button press by the user.
        /// </summary>
        private class PressControl
        {
            public Button.ButtonName Name; // The Button Name.
            public Button.ButtonActions Action; // The action (PressedUp, HoldDown, etc..).
            public bool isActionActive = false; // True if the button action is active right now (for example being pressed or holded down). 
            public bool isRightController = true; // True if the button press was on the right controller.

            public PressControl(Button.ButtonName name, Button.ButtonActions action, bool active, bool isRight)
            {
                Name = name; Action = action; isActionActive = active; isRightController = isRight;
            }
        }

        #endregion

        void Awake()
        {
            CreateInstance();

            registeredControllers = new List<bool>();
            RegisterControllers();
        }

        /// Use this for initialization
        void Start()
        {
            isLeftControllerInteracting = false;
            isRightControllerInteracting = false;
            CheckControllerPosition();
        }

        /// Update is called once per frame
        void Update()
        {
            // Registering controllers and checking if we have them..
            if (!RegisterControllers())
                return;

            LaserManagement();
            CheckControllerPosition();
            coreSettings.UpdateFeaturesList();

            #region Part I: Check what's being pressed on controllers.

            List<PressControl> updatedPressControl = new List<PressControl>();

            /* For the Right hand check what it's being pressed. */
            if (RightController != null)
            {
                List<Button.ButtonName> rightButtons = FixedControl.GetAvailableButtonsList(true);
                List<Button.ButtonActions> rightButtonActions = FixedControl.GetAvailableButtonActionsList(true);
                foreach (Button.ButtonName btn in rightButtons)
                {
                    foreach (Button.ButtonActions actions in rightButtonActions)
                    {
                        updatedPressControl.Add(new PressControl(btn, actions, genericControllerPlatform.GetActionStatus(btn, actions, RightController), true));
                    }
                }
            }
            /* For the Left hand check what is being pressed. */
            if (LeftController != null)
            {
                List<Button.ButtonName> leftButtons = FixedControl.GetAvailableButtonsList(false);
                List<Button.ButtonActions> leftButtonActions = FixedControl.GetAvailableButtonActionsList(false);
                foreach (Button.ButtonName btn in leftButtons)
                {
                    foreach (Button.ButtonActions actions in leftButtonActions)
                    {
                        updatedPressControl.Add(new PressControl(btn, actions, genericControllerPlatform.GetActionStatus(btn, actions, LeftController), false));
                    }
                }

            }

            controllerPressControl.Clear(); // Clean last check results..
            controllerPressControl.AddRange(updatedPressControl); // Add new results..

            #endregion

            #region Part II: Perform Actions Based on Buttons Pressed.

            foreach (PressControl control in controllerPressControl)
            {
                // If pressed.. 
                if (control.isActionActive)
                {
                    // Case its right.
                    if (control.isRightController)
                    {
                        // Check all interactable object interacting with the controller..
                        foreach (InteractableObject obj in rightControllerScript.InteractableObjects)
                        {
                            // Case this object is an Active object.
                            if (obj.IsActiveInteraction())
                            {
                                // Get all the interaction methods for that button.
                                List<Action> methods = ((IActiveInteraction)obj).GetButtonMethods(control.Name, control.Action);

                                CallAction(control.Name, control.Action, methods, control.isRightController);
                            }
                        }
                        // Case there is no interaction.
                        if (rightControllerScript.InteractableObjects.Count == 0)
                            CallAction(control.Name, control.Action, true);
                    }
                    // Case its left.
                    else
                    {
                        // Check all interactable object interacting with the controller..
                        foreach (InteractableObject obj in leftControllerScript.InteractableObjects)
                        {
                            // Case this object is an Active object.
                            if (obj.IsActiveInteraction())
                            {
                                // Get all the interaction methods for that button.
                                List<Action> methods = ((IActiveInteraction)obj).GetButtonMethods(control.Name, control.Action);

                                CallAction(control.Name, control.Action, methods, control.isRightController);
                            }
                        }
                        // Case there is no interaction.
                        if (leftControllerScript.InteractableObjects.Count == 0)
                            CallAction(control.Name, control.Action, false);
                    }
                }
            }

            #endregion
        }

        /// <summary>
        /// Runs whenever a new level is loaded.
        /// </summary>
        void OnLevelWasLoaded()
        {
            ClearTeleport();
        }

        /// <summary>
        /// Figures out if should call the interaction method or the feature method for control.
        /// </summary>
        /// <param name="buttonPressed">The button pressed.</param>
        /// <param name="action">The button action related to the method.</param>
        /// <param name="interactionMethods">An interaction method.</param>
        /// <param name="isRight">True if right hand.</param>
        private void CallAction(Button.ButtonName buttonPressed, Button.ButtonActions action, List<Action> interactionMethods, bool isRight)
        {
            ControlSet platformControls = ((GenericControllerPlatform)coreSettings.CurrentPlatform).GetPlatformControls();
            if (platformControls == null)
                Debug.LogError("No controller scheme found.");

            // This list are the methods that override the controller interaction.
            List<Action> methodsThatOverride;

            if (isRight)
                methodsThatOverride = platformControls.GetButtonMethods(buttonPressed, action,
                ControlSet.Options.isRight, ControlSet.Options.OverrideInteraction);
            else
                methodsThatOverride = platformControls.GetButtonMethods(buttonPressed, action,
                    ControlSet.Options.isLeft, ControlSet.Options.OverrideInteraction);

            // If there is something the list above meant that we will not call the interaction,istead we will call the method.
            if (methodsThatOverride.Count > 0)
            {
                foreach (Action act in methodsThatOverride)
                    act.Invoke();

                // Don't call interaction method.
                Debug.Log("Interaction was overriden by a button.");
                return;
            }
            else
            {
                // On this case there is no method that overrides an interaction, we call the interaction normally.
                foreach (Action act in interactionMethods)
                    act.Invoke();
            }


        }

        /// <summary>
        /// This is called if the button is not interacting, thus we call the normal method defined by a feature.
        /// </summary>
        /// <param name="buttonPressed">The button pressed.</param>
        /// <param name="action">The button action related to the method.</param>
        /// <param name="fixedMethod">The fixed method.</param>
        private void CallAction(Button.ButtonName buttonPressed, Button.ButtonActions action, bool isRight)
        {
            ControlSet platformControls = ((GenericControllerPlatform)coreSettings.CurrentPlatform).GetPlatformControls();
            if (platformControls == null)
                Debug.LogError("No controller scheme found.");

            // This list are the methods that override the controller interaction.
            List<Action> methods;

            if (isRight)
                methods = platformControls.GetButtonMethods(buttonPressed, action,
                ControlSet.Options.isRight);
            else
                methods = platformControls.GetButtonMethods(buttonPressed, action,
                    ControlSet.Options.isLeft);

            if (methods.Count == 0)
                Debug.Log("Pressing the button: " + buttonPressed + ". Action type: " + action +
                    ". Is on right hand? " + isRight + ". But no method found..");

            // Run the methods..
            foreach (Action act in methods)
                act.Invoke();
        }

        /// <summary>
        /// Check if a given button and action is interacting. Has interacted in the last frame.
        /// </summary>
        /// <param name="button">A button.</param>
        /// <param name="actions">An action.</param>
        /// <returns></returns>
        public bool CheckButtonInteracting(Button.ButtonName name, Button.ButtonActions actions, bool isRight)
        {
            PressControl ctrl = controllerPressControl.Find(btn => btn.Name == name && btn.Action == actions && btn.isRightController == isRight);

            if (ctrl != null)
                return ctrl.isActionActive;
            else
            {
                return false;
                Debug.LogError("This button don't seem to be registered.. Something could be off");
            }
        }

        /// <summary>
        /// Verify if all controllers are registered if not register the controller.
        /// </summary>
        /// <param name="plat"></param>
        private bool RegisterControllers()
        {
            // Registering controllers..
            GenericControllerPlatform plat = (GenericControllerPlatform)(coreSettings.CurrentPlatform);
            if (plat != null)
            {
                GameObject[] controls;
                controls = plat.GetControllerObject();
                if (registeredControllers.Count != controls.Length)
                {
                    registeredControllers.Clear();
                    for (int i = 0; i < controls.Length; i++)
                        if (RegisterControllerGameObject(controls[i]))
                            registeredControllers.Add(true);
                }
                if (registeredControllers.Count != controls.Length)
                    return false;
                else
                    return true;
            }
            else
            {
                Debug.Log("Not a controller platform.");
                return false;
            }
        }

        #region Teleport Methods

        // -------------------------------------------------------------------------------------------------------------------------------

        /// <summary>
        /// Disable the teleport and mark who disabled it.
        /// </summary>
        /// <param name="objectID">The object identification number. Every game object in Unity has one.</param>
        public void DisableTeleport(int objectID)
        {
            //Debug.LogError("Disable / Enabling teleport currently not implemented."); // Should be implemented on the core settings feature.
            return;
            //if (DisablingTeleport == null)
            //    DisablingTeleport = new List<int>();

            ///* Disable teleport if enabled. */
            //if (TeleportEnabled)
            //{
            //    TeleportEnabled = false;
            //    TeleportVive[] teleComponents = CameraManager.Instance.MainCamera.GetComponents<TeleportVive>();
            //    foreach (TeleportVive t in teleComponents)
            //        t.enabled = false;
            //}
            ///* Add this objectID to the list. */
            //DisablingTeleport.Add(objectID);
        }

        // -------------------------------------------------------------------------------------------------------------------------------

        /// <summary>
        /// Remove an object that was disabling the teleport 
        /// from the list.
        /// </summary>
        /// <param name="objectID">The object identification number. Every game object in Unity has one.</param>
        public void EnableTeleport(int objectID)
        {
            //Debug.LogError("Disable / Enabling teleport currently not implemented."); // Should be implemented on the core settings feature.
            return;

            //if (DisablingTeleport == null)
            //    DisablingTeleport = new List<int>();

            ///* Attempt to remove this objectID to the list. */
            //DisablingTeleport.Remove(objectID);

            ///* If the list is empty enable teleport. */
            //if (DisablingTeleport.Count == 0)
            //{
            //    TeleportEnabled = true;
            //    TeleportVive[] teleComponents = CameraManager.Instance.MainCamera.GetComponents<TeleportVive>();
            //    foreach (TeleportVive t in teleComponents)
            //        t.enabled = true;
            //}
        }

        // -------------------------------------------------------------------------------------------------------------------------------

        /// <summary>
        /// Fully clear the disabling list of elements.
        /// </summary>
        public void ClearTeleport()
        {
            // To be implemented..

            //if (DisablingTeleport == null)
            //    DisablingTeleport = new List<int>();

            ///* Remove this objectID to the list. */
            //DisablingTeleport.Clear();

            ///* Enable teleport. */
            //TeleportEnabled = true;
            //if (CameraManager.Instance.MainCamera == null)
            //    CameraManager.Instance.SetCamera();
            //TeleportVive[] teleComponents = CameraManager.Instance.MainCamera.GetComponents<TeleportVive>();
            //foreach (TeleportVive t in teleComponents)
            //    t.enabled = true;
        }

        #endregion

        #region Hand Methods

        // -------------------------------------------------------------------------------------------------------------------------------

        /// <summary>
        /// Check and define which hand is which based on the distance from the HMD.
        /// </summary>
        private void CheckControllerPosition()
        {
            if (ControllerGameObjects.Count < 2)
                return;

            RightController = coreSettings.CurrentPlatform.ReturnRightmostObject(ControllerGameObjects[0], ControllerGameObjects[1]);

            if (RightController == ControllerGameObjects[0])
                LeftController = ControllerGameObjects[1];
            else
                LeftController = ControllerGameObjects[0];

            rightControllerScript = RightController.GetComponent<Controller>();
            leftControllerScript = LeftController.GetComponent<Controller>();

            // Unity keeps initializing my list with one NULL interaction. I don't seems to find where that happens
            // The following code fixes the problem.
            if (rightControllerScript.InteractableObjects.Contains(null))
                rightControllerScript.InteractableObjects.Clear();

            if (leftControllerScript.InteractableObjects.Contains(null))
                leftControllerScript.InteractableObjects.Clear();
        }

        /// <summary>
        /// Returns the attach point on the specified controller.
        /// </summary>
        /// <param name="isRight">True if the controller is on the right hand.</param>
        /// <returns></returns>
        public GameObject GetControllerAttachPosition(bool isRight)
        {
            // Registering controllers..
            GenericControllerPlatform platform = (GenericControllerPlatform)(coreSettings.CurrentPlatform);
            if (platform != null)
            {
                return platform.GetControllerAttachPoint(isRight);
            }
            else
            {
                Debug.LogError("This platform doesn't support controllers.");
                return null;
            }             
        }

        // -------------------------------------------------------------------------------------------------------------------------------

        /// <summary>
        /// Returns the Right hand according to the HMD
        /// </summary>
        /// <returns>The right controller object.</returns>
        public GameObject GetRightController()
        {
            CheckControllerPosition();
            return RightController;
        }

        // -------------------------------------------------------------------------------------------------------------------------------

        /// <summary>
        /// Returns the Left hand according to the HMD.
        /// </summary>
        /// <returns>The left controller object.</returns>
        public GameObject GetLeftController()
        {
            CheckControllerPosition();
            return LeftController;
        }

        #endregion

        #region Module Interface

        // -------------------------------------------------------------------------------------------------------------------------------

        /// <summary>
        /// Register a game object as a controller. This object will be assigned a Controller script.
        /// </summary>
        /// <param name="ControllerObject">The object to be assigned as a controller.</param>
        public bool RegisterControllerGameObject(GameObject ControllerObject)
        {
            try
            {
                if (ControllerGameObjects == null)
                    ControllerGameObjects = new List<GameObject>();

                ControllerObject.AddComponent<Controller>();
                ControllerGameObjects.Add(ControllerObject);
                CheckControllerPosition();
                return true;
            }
            catch (Exception e)
            {
                return false;
            }
        }

        public GameObject GetControllerInteracting()
        {
            return null;
        }

        #endregion

        #region Laser Methods

        // -------------------------------------------------------------------------------------------------------------------------------

        /// <summary>
        /// <para>Create the laser on the correct player controller.</para>
        /// <para>Only create laser if the controller manager allows!</para>
        /// <para>That means UserLaserInteraction is true.</para>
        /// </summary>
        private void DisplayLaser()
        {
            if (!this.UseLaserInteraction)
                return;

            GameObject attachToObject;
            GameObject laserHolder = new GameObject("LaserHolder");
            LineRenderer laserLine;

            /* Erease all lasers */
            Transform t = LeftController.transform.Find("Attach_ControllerTip/LaserHolder");
            if (t != null)
                GameObject.Destroy(LeftController.transform.Find("Attach_ControllerTip/LaserHolder").gameObject);

            t = RightController.transform.Find("Attach_ControllerTip/LaserHolder");
            if (t != null)
                GameObject.Destroy(RightController.transform.Find("Attach_ControllerTip/LaserHolder").gameObject);

            if (coreSettings.LaserOnLeftHand)
                attachToObject = LeftController.transform.Find("Attach_ControllerTip").gameObject;
            else
                attachToObject = RightController.transform.Find("Attach_ControllerTip").gameObject;

            /* Preparing the Laser Object*/
            laserHolder.transform.parent = attachToObject.transform;
            laserHolder.transform.localPosition = Vector3.zero;
            laserHolder.transform.localRotation = Quaternion.Euler(Vector3.zero);
            laserLine = laserHolder.AddComponent<LineRenderer>();

            /* Configuring the Laser Line */
            laserLine.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            laserLine.receiveShadows = false;
            laserLine.positionCount = 2;
            laserLine.SetPosition(0, Vector3.zero + Vector3.forward * .04f);
            laserLine.SetPosition(1, Vector3.forward * coreSettings.LaserLenght);
            laserLine.widthMultiplier = 0.003f;
            laserLine.useWorldSpace = false;
            laserLine.material = coreSettings.LaserMaterial;

            /* Saving a reference to the laser object. */
            LaserHolder = laserHolder;

            /* Creating a compatible raycast*/
            // laser = new Ray(laserHolder.transform.position, (laserHolder.transform.TransformDirection(Vector3.forward) * LaserLenght));
        }

        // -------------------------------------------------------------------------------------------------------------------------------

        /// <summary>
        /// Create the laser on the correct player controller
        /// </summary>
        /// <param name="hit">A RaycastHit resulting of the collision between the laser and something.</param>
        private void DisplayLaser(RaycastHit hit)
        {
            if (!this.UseLaserInteraction)
                return;

            GameObject attachToObject;
            GameObject laserHolder = new GameObject("LaserHolder");
            LineRenderer laserLine;

            /* Erease all lasers */
            Transform t = LeftController.transform.Find("Attach_ControllerTip/LaserHolder");
            if (t != null)
                GameObject.Destroy(LeftController.transform.Find("Attach_ControllerTip/LaserHolder").gameObject);

            t = RightController.transform.Find("Attach_ControllerTip/LaserHolder");
            if (t != null)
                GameObject.Destroy(RightController.transform.Find("Attach_ControllerTip/LaserHolder").gameObject);

            if (coreSettings.LaserOnLeftHand)
                attachToObject = LeftController.transform.Find("Attach_ControllerTip").gameObject;
            else
                attachToObject = RightController.transform.Find("Attach_ControllerTip").gameObject;

            /* Preparing the Laser Object*/
            laserHolder.transform.parent = attachToObject.transform;
            laserHolder.transform.localPosition = Vector3.zero;
            laserHolder.transform.localRotation = Quaternion.Euler(Vector3.zero);
            laserLine = laserHolder.AddComponent<LineRenderer>();

            /* Configuring the Laser Line */
            laserLine.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            laserLine.receiveShadows = false;
            laserLine.positionCount = 2;
            laserLine.SetPosition(0, Vector3.zero + Vector3.forward * .04f);
            laserLine.SetPosition(1, Vector3.forward * Vector3.Distance(laserHolder.transform.position, hit.point));
            laserLine.widthMultiplier = 0.003f;
            laserLine.useWorldSpace = false;
            laserLine.material = coreSettings.LaserMaterial;

            /* Saving a reference to the laser object. */
            LaserHolder = laserHolder;

            /* Creating a compatible raycast*/
            // laser = new Ray(laserHolder.transform.position, (laserHolder.transform.TransformDirection(Vector3.forward) * LaserLenght));
        }

        // -------------------------------------------------------------------------------------------------------------------------------

        /// <summary>
        /// Hide the laser, so it's not showing anymore.
        /// </summary>
        private void HideLaser()
        {
            /* If there is a laser on the right hand.  */
            if (RightController.transform.Find("Attach_ControllerTip").Find("LaserHolder") != null)
            {
                /* Delete the laser. */
                Destroy(RightController.transform.Find("Attach_ControllerTip").Find("LaserHolder").gameObject);
            }
            /* If there is a laser on the left hand .  */
            if (LeftController.transform.Find("Attach_ControllerTip").Find("LaserHolder") != null)
            {
                /* Delete the laser. */
                Destroy(LeftController.transform.Find("Attach_ControllerTip").Find("LaserHolder").gameObject);
            }

            DestroyLaserCollisionIndicator();
        }

        // -------------------------------------------------------------------------------------------------------------------------------

        /// <summary>
        /// Correct the laser position if the wrong hand is holding it.
        /// </summary>
        private void CorrectLaserHand()
        {
            if (!laserEnabled)
                return;

            if (RightController == null || LeftController == null)
            {
                Debug.LogError("One of the hands are missing. Laser selector won't work.");
                return;
            }

            /* If there is a laser on the right hand but the controller is set to have it on the Left.  */
            if (RightController.transform.Find("Attach_ControllerTip").Find("LaserHolder") != null && coreSettings.LaserOnLeftHand)
            {
                /* Delete the laser. */
                HideLaser();
                /* Create the laser again. */
                DisplayLaser();
            }
            /* If there is a laser on the left hand but the controller is set to have it on the right.  */
            if (LeftController.transform.Find("Attach_ControllerTip").Find("LaserHolder") != null && !coreSettings.LaserOnLeftHand)
            {
                /* Delete the laser. */
                HideLaser();
                /* Create the laser again. */
                DisplayLaser();
            }
        }

        // -------------------------------------------------------------------------------------------------------------------------------

        /// <summary>
        /// Add a request for laser overlay.
        /// </summary>
        /// <param name="objectID">The object identification number. Every game object in Unity has one.</param>
        public void RequestLaserOverlay(int objectID)
        {
            if (RequestingLaserOverlay == null)
                RequestingLaserOverlay = new List<int>();

            /* Cancel request */
            if (!LaserOverlay)
            {
                LaserOverlay = true;
                coreSettings.LaserCollisionMaterial.renderQueue = 5000;
                coreSettings.LaserMaterial.renderQueue = 5000;
            }
            /* Add this objectID to the list. */
            RequestingLaserOverlay.Add(objectID);
        }

        // -------------------------------------------------------------------------------------------------------------------------------

        /// <summary>
        /// Remove a request for laser overlay.
        /// </summary>
        /// <param name="objectID">The object identification number. Every game object in Unity has one.</param>
        public void RemoveRequestLaserOverlay(int objectID)
        {
            if (RequestingLaserOverlay == null)
                RequestingLaserOverlay = new List<int>();

            /* Remove this objectID to the list. */
            RequestingLaserOverlay.Remove(objectID);

            /* If the list is empty change render queue of laser material. */
            if (RequestingLaserOverlay.Count == 0 && LaserOverlay)
            {
                LaserOverlay = false;
                coreSettings.LaserCollisionMaterial.renderQueue = 2000;
                coreSettings.LaserMaterial.renderQueue = 2000;
            }
        }

        // -------------------------------------------------------------------------------------------------------------------------------

        /// <summary>
        /// Request to deactivate laser.
        /// </summary>
        /// <param name="objectID"> The GameObject ID of the object requesting the laser to be disabled. </param>
        public void RequestHideLaser(int objectID)
        {
            if (DisablingLaser == null)
                DisablingLaser = new List<int>();

            Debug.Log("Requesting to disable laser, ObjectID:" + objectID);

            // Check if this object isn't already blocking the laser.
            if (!DisablingLaser.Contains(objectID))
                /* Add this objectID to the list. */
                DisablingLaser.Add(objectID);
        }

        // -------------------------------------------------------------------------------------------------------------------------------

        /// <summary>
        ///<para> Request laser to be enabled, if there is no script requesting it to be disable. It will be enabled.</para>
        ///<para> If this object previously had disabled the laser, this will remove it from the list of objects that are disabling the laser.</para>
        /// </summary>
        /// <param name="objectID">The object identification number. Every game object in Unity has one.</param>
        public void RequestDisplayLaser(int objectID)
        {
            if (DisablingLaser == null)
                DisablingLaser = new List<int>();

            // Check if this object is blocking the laser.
            if (!DisablingLaser.Contains(objectID))
                return;

            Debug.Log("Requesting to enable laser, ObjectID:" + objectID);

            /* Remove this objectID to the list. */
            DisablingLaser.Remove(objectID);
        }

        // -------------------------------------------------------------------------------------------------------------------------------

        /// <summary>
        /// Set this if the laser is hitting something.
        /// </summary>
        /// <param name="hit">The hit structure of the laser.</param>
        public void SetLaserHitting(RaycastHit hit)
        {
            laserHit = hit;
            laserIsHitting = true;

            // This needs to be here because it needs to spawn way faster than the laser itself.
            SpawnLaserCollisionIndicator(hit);
        }

        // -------------------------------------------------------------------------------------------------------------------------------

        /// <summary>
        /// Notifies the laser is not hitting anything.
        /// </summary>
        public void SetLaserNotHitting()
        {
            laserIsHitting = false;
            DestroyLaserCollisionIndicator();
        }

        // -------------------------------------------------------------------------------------------------------------------------------

        /// <summary>
        /// Refresh the laser.
        /// </summary>
        private void LaserManagement()
        {
            if (!UseLaserInteraction)
                return;

            if (DisablingLaser == null)
                DisablingLaser = new List<int>();

            // If the application uses laser.. and there is nobody deactivating it. It should be enabled.
            if (UseLaserInteraction && DisablingLaser.Count == 0)
            {
                Debug.Log("Displaying Laser..");

                laserEnabled = true;
                if (laserIsHitting)
                    DisplayLaser(laserHit);
                else
                    DisplayLaser();
            }

            Debug.Log("Laser Status:" + laserEnabled);
            // Laser should be disabled.
            if (DisablingLaser.Count > 0 && laserEnabled == true)
            {
                Debug.Log("Disabling Laser..");
                laserEnabled = false;
                HideLaser();
            }
        }

        // -------------------------------------------------------------------------------------------------------------------------------

        /// <summary>
        /// Remove all requests for laser overlay.
        /// </summary>
        public void RemoveAllLaserOverLayRequest()
        {
            if (RequestingLaserOverlay == null)
                RequestingLaserOverlay = new List<int>();

            /* Removes all objects from list. */
            RequestingLaserOverlay.Clear();

            /* Remove laser overlay. */
            LaserOverlay = false;
            coreSettings.LaserCollisionMaterial.renderQueue = 2000;
            coreSettings.LaserMaterial.renderQueue = 2000;
        }

        // -------------------------------------------------------------------------------------------------------------------------------

        /// <summary>
        /// Creates an icon to indicate where the laser collided
        /// </summary>
        /// <param name="hit">The hit object of the collision</param>
        private void SpawnLaserCollisionIndicator(RaycastHit hit)
        {
            // If there is collision and collision sphere does not exist, create it
            if (LaserCollisionObj == null)
            {
                // Creates collision circle gameobj
                LaserCollisionObj = new GameObject();
                LaserCollisionObj.name = "LaserCollisionCircle";
                LaserCollisionObj.transform.localScale =
                    new Vector3(ControllerManager.Instance.LaserCollisionScale, ControllerManager.Instance.LaserCollisionScale, ControllerManager.Instance.LaserCollisionScale);

                // Adds sprite component to it
                SpriteRenderer rend = LaserCollisionObj.AddComponent<SpriteRenderer>();
                rend.sprite = ControllerManager.Instance.LaserCollisionSprite;
                rend.material = ControllerManager.Instance.LaserCollisionMaterial;

                // Positions it where laser is colliding
                // Adds a small spacing so sprite doesn't clamp inside the colliding object
                LaserCollisionObj.transform.position = hit.point;
                // Adds to a fix vector3 because sprite origin axis is not it's normal axis
                LaserCollisionObj.transform.rotation = Quaternion.LookRotation(-hit.normal);

            }
            else
            {
                LaserCollisionObj.transform.LookAt(-hit.normal);
                // Adds a small spacing so sprite doesn't clamp inside the colliding object
                LaserCollisionObj.transform.position = hit.point + hit.normal * 0.002f;
                // Adds to a fix vector3 because sprite origin axis is not it's normal axis
                LaserCollisionObj.transform.rotation = Quaternion.LookRotation(-hit.normal);
            }
        }

        // -------------------------------------------------------------------------------------------------------------------------------

        /// <summary>
        /// Destroy the collision indicator. 
        /// </summary>
        private void DestroyLaserCollisionIndicator()
        {
            if (LaserCollisionObj)
            {
                GameObject.Destroy(LaserCollisionObj);
            }
        }

        #endregion

        #region Auxiliary

        /// <summary>
        /// Create this manager instance.
        /// </summary>
        private void CreateInstance()
        {
            GameObject manager = GameObject.Find(Constants.API.RootObjectName + "/"
                + Constants.API.Core.RootObjectName + "/Controller Manager");

            if (manager == null)
            {
                // Creating the Manager
                GameObject newManager = new GameObject("Controller Manager");
                newManager.AddComponent<CameraManager>();
                newManager.transform.parent = GameObject.Find(Constants.API.RootObjectName + "/"
                    + Constants.API.Core.RootObjectName).transform;
                instance = newManager.GetComponent<ControllerManager>();
            }
            else
                instance = manager.GetComponent<ControllerManager>();

        }

        // -------------------------------------------------------------------------------------------------------------------------------

        /// <summary>
        /// Sample a bunch of points along a parabolic curve until you hit gnd.  At that point, cut off the parabola
        /// </summary>
        /// <param name="p0">Starting point of parabola.</param>
        /// <param name="v0">Initial parabola velocity.</param>
        /// <param name="a">Initial acceleration.</param>
        /// <param name="dist">Distance between sample points.</param>
        /// <param name="points">Number of sample points.</param>
        /// <param name="outPts">List that will be populated by new points.</param>
        /// <param name="collisionLayers">Layers to collide.</param>
        /// <returns></returns>
        private bool CalculateParabolicCurve(Vector3 p0, Vector3 v0, Vector3 a, float dist, int points, List<Vector3> outPts, int collisionLayers)
        {
            outPts.Clear();
            outPts.Add(p0);

            Vector3 last = p0;
            float t = 0;

            for (int i = 0; i < points; i++)
            {
                t += dist / Physics.ParabolicCurveDeriv(v0, a, t).magnitude;
                Vector3 next = Physics.ParabolicCurve(p0, v0, a, t);

                Vector3 castHit;
                bool cast = this.Linecast(last, next, out castHit, collisionLayers);

                if (cast)
                {
                    outPts.Add(castHit);
                }
                else
                    outPts.Add(next);

                last = next;
            }


            return false;
        }

        // -------------------------------------------------------------------------------------------------------------------------------

        /// <summary>
        /// This uses Physics raycasts to perform the raycast calculation, so the teleport surface must have a collider on it.
        /// If the raycast hit something.
        /// </summary>
        /// <param name="p1">First (origin) point of ray.</param>
        /// <param name="p2">Last (end) point of ray</param>
        /// <param name="hitPoint">If hit, the point of the hit.  Otherwise zero.</param>
        /// <param name="CollisionLayer">Layers to collide.</param>
        /// <returns></returns>
        public bool Linecast(Vector3 p1, Vector3 p2, out Vector3 hitPoint, int CollisionLayer)
        {
            RaycastHit hit;
            Vector3 dir = p2 - p1;
            float dist = dir.magnitude;
            dir /= dist;
            if (UnityEngine.Physics.Raycast(p1, dir, out hit, dist, CollisionLayer))
            {
                hitPoint = hit.point;
                return true;
            }
            hitPoint = Vector3.zero;
            return false;
        }

        #endregion


    }

}
