using System;
using System.Collections;
using System.Collections.Generic;
using SimplerVR.Common;
using SimplerVR.Core.Controller;
using UnityEngine;
using Valve.VR;
using Valve.VR.InteractionSystem;

namespace SimplerVR.PlatformInterfaces.VIVE
{
    /// <summary>
    ///<para>Interface to communicate with the SteamVR API.</para> 
    ///<para> In case of version changes by Valve, we change this script.</para>
    /// </summary>
    public class VIVEPlatform : GenericControllerPlatform
    {       
        public GameObject cameraRightEye;
        public GameObject cameraLeftEye;
        public GameObject controller1;
        public GameObject controller2;

        #region Important Getters and Setters

        public GameObject CameraRightEye
        {
            get
            {
                try
                {
                    if (cameraRightEye == null)
                    {
                        GameObject platform = GetVIVEPlatformGameObject();
                        if (platform != null)
                            cameraRightEye = GetVIVEPlatformGameObject().transform.Find("Player/SteamVRObjects/VRCamera").gameObject;
                    }
                    return cameraRightEye;
                }
                catch
                {
                    Debug.LogError("Could not find the desired item maybe the location of an object is wrong.");
                    return null;
                }                  
            }
        }  
        public GameObject CameraLeftEye
        {
            get
            {
                try
                {
                    if (cameraLeftEye == null)
                    {
                        GameObject platform = GetVIVEPlatformGameObject();
                        if (platform != null)
                            cameraLeftEye = GetVIVEPlatformGameObject().transform.Find("Player/SteamVRObjects/VRCamera (LeftEye)").gameObject;
                    }
                    return cameraLeftEye;
                }
                catch
                {
                    Debug.LogError("Could not find the desired item maybe the location of an object is wrong.");
                    return null;
                }
            }
        }
        public GameObject Controller1
        {
            get
            {
                try
                {
                    if (controller1 == null)
                    {
                        GameObject platform = GetVIVEPlatformGameObject();
                        if (platform != null)
                            controller1 = GetVIVEPlatformGameObject().transform.Find("Player/SteamVRObjects/Hand1").gameObject;
                    }
                    return controller1;
                }
                catch
                {
                    Debug.LogError("Could not find the desired item maybe the location of an object is wrong.");
                    return null;
                }
            }
        }
        public GameObject Controller2
        {
            get
            {
                try
                {
                    if (controller2 == null)
                    {
                        GameObject platform = GetVIVEPlatformGameObject();
                        if (platform != null)
                            controller2 = GetVIVEPlatformGameObject().transform.Find("Player/SteamVRObjects/Hand2").gameObject;
                    }
                    return controller2;
                }
                catch
                {
                    Debug.LogError("Could not find the desired item maybe the location of an object is wrong.");
                    return null;
                }
            }
        }

        #endregion

        private VIVEControls vIVEControls;
        public VIVEControls VIVEControls
        {
            get
            {
                if (vIVEControls == null)
                    vIVEControls = new VIVEControls();

                return vIVEControls;
            }
            set
            {
                vIVEControls = value;
            }
        }

       

        /// <summary>
        ///  A simple initialization.
        ///  Remember this is a scriptable object so this don't get called on scene changes!!
        /// </summary>
        private void OnEnable()
        {
            Initialize();
        }

        /// <summary>
        /// Set the components on the required places on the SteamVR Prefab.
        /// </summary>
        public override void Initialize()
        {
            /// Add controllers
            //ControllerManager.Instance.RegisterControllerGameObject(Controller1);
            //ControllerManager.Instance.RegisterControllerGameObject(Controller2);

            GameObject vivePlatformGameObject = GetVIVEPlatformGameObject();

            controller1 = vivePlatformGameObject.transform.Find("Player/SteamVRObjects/Hand1").gameObject;
            controller2 = vivePlatformGameObject.transform.Find("Player/SteamVRObjects/Hand2").gameObject;
            cameraRightEye = vivePlatformGameObject.transform.Find("Player/SteamVRObjects/VRCamera").gameObject;
            cameraLeftEye = vivePlatformGameObject.transform.Find("Player/SteamVRObjects/VRCamera (LeftEye)").gameObject;
        }

        /// <summary>
        /// Returns the vive platform game object.
        /// </summary>
        /// <returns></returns>
        private GameObject GetVIVEPlatformGameObject()
        {
            try
            {
                // Find the controllers and all the required components.
                GameObject root = GameObject.Find(Constants.API.RootObjectName)
                    .transform.Find(Constants.API.RootPlatformObjectName).gameObject;

                GameObject vivePlatformGameObject = root.transform.Find("VIVE").gameObject;
                return vivePlatformGameObject;
            }
            catch
            {
                Debug.LogError(" Could not find the platform game object or " +
                    "the root object game object. Make sure you didn't rename anything.");
                return null;
            }
           
        }

        /// <summary>
        /// Tells if this object is the rightmost (compared to the HMD).
        /// </summary>
        /// <param name="Object1">First object to compare.</param>
        /// <param name="Object1">Second object to compare.</param>
        /// <returns>The rightmost object.</returns>
        public override GameObject ReturnRightmostObject(GameObject object1, GameObject object2)
        {
            Vector3 sa = object1.transform.position;
            Vector3 sb = object2.transform.position;

            Vector3 saProject = Vector3.Project(sa, CameraRightEye.transform.right);
            Vector3 sbProject = Vector3.Project(sb, CameraRightEye.transform.right);

            float dotResult1 = Vector3.Dot(saProject, CameraRightEye.transform.right);
            float dotResult2 = Vector3.Dot(sbProject, CameraRightEye.transform.right);

            if (dotResult1 > dotResult2)
                return object1;
            else
                return object2;
        }

        /// <summary>
        /// <para>Returns true if the ButtonAction on the specified controller GameObject and ButtonName button is happening.</para>
        /// <para>Else returns null.</para>
        /// </summary>
        /// <param name="button">The ControllerButtons for this button.</param>
        /// <param name="action">A button action.</param>
        /// <param name="controller">The Controller game object.</param>
        /// <returns>True if the ButtonAction on the specified controller GameObject and ButtonName button is happening.</returns>
        public override bool GetActionStatus(Button.ButtonName button, Button.ButtonActions action, GameObject controller)
        {
            Hand controllerHand = controller.GetComponent<Hand>();
            if (controllerHand == null)
            {
                Debug.LogError("Could't find the 'Hand' component on the supplied controller gameobject.");
                return false;
            }
            else if (controllerHand.controller != null)
            {
                if (action == Button.ButtonActions.PressUp)
                    return controllerHand.controller.GetPressUp(TranslateControllerButton(button));
                else if (action == Button.ButtonActions.HoldDown)
                    return controllerHand.controller.GetPressDown(TranslateControllerButton(button));
                else
                    Debug.Log("Could not find desired action maybe it's not implemented by the platform.");
            }
            else
            {
                // Control might be still initializing.. return false.
                Debug.Log("Controller could be still initializing.. no interaction identified.");
                return false;
            }
            return false;
        }

        /// <summary>
        /// Translate the controller button to the internal vive language.
        /// </summary>
        /// <param name="button">The button provided from our VR framework</param>
        /// <returns>SteamVR ulong</returns>
        private ulong TranslateControllerButton(Core.Controller.Button.ButtonName button)
        {
            if (button == Core.Controller.Button.ButtonName.Touchpad)
                return SteamVR_Controller.ButtonMask.Touchpad;
            else if (button == Core.Controller.Button.ButtonName.Trigger)
                return SteamVR_Controller.ButtonMask.Trigger;
            else if (button == Core.Controller.Button.ButtonName.Grip)
                return SteamVR_Controller.ButtonMask.Grip;
            else if (button == Core.Controller.Button.ButtonName.ApplicationMenu)
                return SteamVR_Controller.ButtonMask.ApplicationMenu;
            else
                Debug.LogError("Could not find proper button on the definition something is wrong!");

            return new ulong();
        }

        /// <summary>
        /// Returns the ControlSet used by this platform.
        /// </summary>
        /// <returns>The ControlSet</returns>
        public override ControlSet GetPlatformControls()
        {
            return this.VIVEControls;
        }

        /// <summary>
        /// Returns the main cameras.If there is only one camera this array has lenght 1.
        /// </summary>
        /// <returns>An array with the player cameras.</returns>
        public override Camera[] GetCameras()
        {
            GameObject rootPlatform = GameObject.Find(Common.Constants.API.RootObjectName + "/" + Common.Constants.API.RootPlatformObjectName);
            Camera cam1 = rootPlatform.transform.Find("VIVE/Player/SteamVRObjects/VRCamera").GetComponent<Camera>();
            Camera cam2 = rootPlatform.transform.Find("VIVE/Player/SteamVRObjects/VRCamera (LeftEye)").GetComponent<Camera>();
            Camera[] camera = new Camera[2];
            camera[0] = cam1;
            camera[1] = cam2;

            return camera;
        }

        /// <summary>
        /// Requests the chaperone boundaries of the SteamVR play area.  This doesn't work if you haven't performed Room Setup.   
        /// </summary>
        /// <param name="p0">Chaperone point 0</param>
        /// <param name="p1">Chaperone point 1</param>
        /// <param name="p2">Chaperone point 2</param>
        /// <param name="p3">Chaperone point 3</param>
        /// <returns>If the play area retrieval was successful</returns>
        public override bool GetPlayAreaBounds(out Vector3 p0, out Vector3 p1, out Vector3 p2, out Vector3 p3)
        {
            var initOpenVR = (!SteamVR.active && !SteamVR.usingNativeSupport);
            if (initOpenVR)
            {
                var error = EVRInitError.None;
                OpenVR.Init(ref error, EVRApplicationType.VRApplication_Other);
            }

            var chaperone = OpenVR.Chaperone;
            HmdQuad_t rect = new HmdQuad_t();
            bool success = (chaperone != null) && chaperone.GetPlayAreaRect(ref rect);
            p0 = new Vector3(rect.vCorners0.v0, rect.vCorners0.v1, rect.vCorners0.v2);
            p1 = new Vector3(rect.vCorners1.v0, rect.vCorners1.v1, rect.vCorners1.v2);
            p2 = new Vector3(rect.vCorners2.v0, rect.vCorners2.v1, rect.vCorners2.v2);
            p3 = new Vector3(rect.vCorners3.v0, rect.vCorners3.v1, rect.vCorners3.v2);
            if (!success)
                Debug.LogWarning("Failed to get Calibrated Play Area bounds!  Make sure you have tracking first, and that your space is calibrated.");

            if (initOpenVR)
                OpenVR.Shutdown();

            return success;
        }

        /// <summary>
        /// Retrieves the head transform of the player.
        /// </summary>
        /// <returns>The head transform.</returns>
        public override Transform GetHeadTransform()
        {
            GameObject rootPlatform = GameObject.Find(Common.Constants.API.RootObjectName + "/" + Common.Constants.API.RootPlatformObjectName);
            return rootPlatform.transform.Find("VIVE/Player/SteamVRObjects/VRCamera");
        }

        /// <summary>
        /// True if the platform controllers support haptic pulse (vibration).
        /// </summary>
        /// <returns>True if the platform controllers support haptic pulse (vibration).</returns>
        public override bool UseHapticPulse()
        {
            return true;
        }

        /// <summary>
        /// Triggers the platform haptic pulse.
        /// </summary>
        /// <param name="time">The duration of the pulse. 0 if nonstop.</param>
        /// <param name="controller">The controller to pulse.</param>
        public override void TriggerHapticPulse(float time, Controller controller)
        {
            // Find which controller to pulse.
            Controller controller1 = Controller1.GetComponent<Controller>();
            Controller controller2 = Controller2.GetComponent<Controller>();
            Hand handToVibrate = null;
            if (controller1 != null)
            {
                // Find controller hand.
                if (controller1.Equals(controller))
                    handToVibrate = controller1.gameObject.GetComponent<Hand>();
            }
            else if (controller2 != null)
            {
                // Find controller hand.
                if (controller1.Equals(controller))
                    handToVibrate = controller1.gameObject.GetComponent<Hand>();
            }
            else
            {
                Debug.LogError("Could not vibrate the controller. Can't find controller scripts on the controllers.");
                return;
            }

            ushort timeInMs = Convert.ToUInt16(Mathf.Abs(time * 1000f));

            // Pulse.
            if (handToVibrate != null)
            {
                handToVibrate.controller.TriggerHapticPulse(timeInMs);
            }
            else
                Debug.LogError("None of the controllers had a Controller script equal to the one received.");
        }

        /// <summary>
        /// Returns the game objects that represents the Controller.
        /// </summary>
        /// <returns>An array of game object tha represent the controllers. Position 0 is the right controller 1 is the left controller.</returns>
        public override GameObject[] GetControllerObject()
        {
            GameObject rightmost = ReturnRightmostObject(Controller1, Controller2);
            GameObject leftmost = null;
            if (rightmost == Controller1)
                leftmost = Controller2;
            else
                leftmost = Controller1;

            GameObject[] array = new GameObject[] { rightmost, leftmost};

            return array;
        }

        /// <summary>
        /// Returns the play area center. On platforms that don't have a play area this returns null.
        /// </summary>
        /// <returns>The play area transform.</returns>
        public override Transform GetPlayerTransform()
        {
            GameObject rootPlatform = GameObject.Find(Common.Constants.API.RootObjectName + "/" + Common.Constants.API.RootPlatformObjectName);
            return rootPlatform.transform.Find("VIVE/Player");
        }

        /// <summary>
        /// <para>Returns a GameObject that will be used to grab a game object.</para>
        /// <para>This will be used to attach objects to the controller, it could be a hint or an object that we want to attach to the controller.</para>
        /// <para>All those objects will use this position as a reference for placement.</para>
        /// </summary>
        /// <param name="isRight">True if we want this on the right hand.</param>
        /// <returns></returns>
        public override GameObject GetControllerAttachPoint(bool isRight)
        {
            if (isRight)
                return GetControllerObject()[0].transform.Find("Attach_ControllerTip").gameObject;
            else
                return GetControllerObject()[1].transform.Find("Attach_ControllerTip").gameObject;
        }
    }

}

