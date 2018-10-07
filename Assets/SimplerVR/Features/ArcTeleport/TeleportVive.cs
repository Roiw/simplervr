//using SmashMountain.VR.Core;
//using System.Collections;
//using UnityEngine;
//using UnityEngine.Events;
//using Valve.VR;
//using Valve.VR.InteractionSystem;
//using SmashMountain.VR.Core.Camera;
//using SmashMountain.VR.Core.Controller;

//namespace SmashMountain.VR.Features.ArcTeleport
//{
//    [AddComponentMenu("Vive Teleporter/Vive Teleporter")]
//    [RequireComponent(typeof(Camera), typeof(BorderRenderer))]
//    public class TeleportVive : MonoBehaviour
//    {

//        [Tooltip("Parabolic Pointer object to pull destination points from, and to assign to each controller.")]
//        public ParabolicPointer Pointer;
//        // Origin of SteamVR tracking space
//        [Tooltip("Origin of the SteamVR tracking space")]
//        public Transform OriginTransform;
//        // Origin of the player's head
//        [Tooltip("Transform of the player's head")]
//        public Transform HeadTransform;


//        // How long, in seconds, the fade-in/fade-out animation should take
//        [Tooltip("Duration of the \"blink\" animation (fading in and out upon teleport) in seconds.")]
//        public float TeleportFadeDuration = 0.2f;
//        // Measure in degrees of how often the controller should respond with a haptic click.  Smaller value=faster clicks
//        [Tooltip("The player feels a haptic pulse in the controller when they raise / lower the controller by this many degrees.  Lower value = faster pulses.")]
//        public float HapticClickAngleStep = 10;

//        // BorderRenderer to render the chaperone bounds (when choosing a location to teleport to)
//        private BorderRenderer roomBorder;

//        // Animator used to fade in/out the teleport area.  This should have a boolean parameter "Enabled" where if true
//        // the selectable area is displayed on the ground.
//        [SerializeField]
//        [Tooltip("Animator with a boolean \"Enabled\" parameter that is set to true when the player is choosing a place to teleport.")]
//        private Animator navmeshAnimator;
//        private int enabledAnimatorID;

//        // Material used to render the fade in/fade out quad
//        [Tooltip("Material used to render the fade in/fade out quad.")]
//        public Material FadeMaterial;
//        private int materialFadeID;

//        // SteamVR controllers that should be polled.
//        [Tooltip("Array of SteamVR controllers that may used to select a teleport destination.")]
//        public Hand Controller1;
//        public Hand Controller2;
//        [Tooltip("If this is marked player will teleport if pressed the right controller touchpad. Unmarking will use the left hand.")]
//        public bool TeleportUsingRight = true;
//        [Tooltip("Mark this is you want to see the square were the shaperone bounds are when performing a teleport.")]
//        public bool DrawShaperoneBounds = false;

//        private Hand activeController;
//        private Hand lastActiveController;
//        private float lastClickAngle = 0;
//        public bool teleporting = false;
//        private bool fadingIn = false;
//        private float teleportTimeMarker = -1;

//        private Mesh PlaneMesh;

//        void Start()
//        {
//            // Disable the pointer graphic (until the user holds down on the touchpad)
//            Pointer.enabled = false;

//            // Standard plane mesh used for "fade out" graphic when you teleport
//            // This way you don't need to supply a simple plane mesh in the inspector
//            PlaneMesh = new Mesh();
//            Vector3[] verts = new Vector3[]
//            {
//            new Vector3(-1, -1, 0),
//            new Vector3(-1, 1, 0),
//            new Vector3(1, 1, 0),
//            new Vector3(1, -1, 0)
//            };
//            int[] elts = new int[] { 0, 1, 2, 0, 2, 3 };
//            PlaneMesh.vertices = verts;
//            PlaneMesh.triangles = elts;
//            PlaneMesh.RecalculateBounds();

//            // Set some standard variables
//            materialFadeID = Shader.PropertyToID("_Fade");
//            enabledAnimatorID = Animator.StringToHash("Enabled");

//            roomBorder = GetComponent<BorderRenderer>();

//            Vector3 p0, p1, p2, p3;
//            if (GetChaperoneBounds(out p0, out p1, out p2, out p3))
//            {
//                // Rotate to match camera rig rotation
//                var originRotationMatrix = Matrix4x4.TRS(Vector3.zero, OriginTransform.rotation, Vector3.one);

//                BorderPointSet p = new BorderPointSet(new Vector3[] {
//                originRotationMatrix * p0,
//                originRotationMatrix * p1,
//                originRotationMatrix * p2,
//                originRotationMatrix * p3,
//                originRotationMatrix * p0,
//            });
//                roomBorder.Points = new BorderPointSet[]
//                {
//                p
//                };
//            }

//            roomBorder.enabled = false;

//        }

//        // \brief Requests the chaperone boundaries of the SteamVR play area.  This doesn't work if you haven't performed
//        //        Room Setup.
//        // \param p0, p1, p2, p3 Points that make up the chaperone boundaries.
//        // 
//        // \returns If the play area retrieval was successful
//        public static bool GetChaperoneBounds(out Vector3 p0, out Vector3 p1, out Vector3 p2, out Vector3 p3)
//        {
//            var initOpenVR = (!SteamVR.active && !SteamVR.usingNativeSupport);
//            if (initOpenVR)
//            {
//                var error = EVRInitError.None;
//                OpenVR.Init(ref error, EVRApplicationType.VRApplication_Other);
//            }

//            var chaperone = OpenVR.Chaperone;
//            HmdQuad_t rect = new HmdQuad_t();
//            bool success = (chaperone != null) && chaperone.GetPlayAreaRect(ref rect);
//            p0 = new Vector3(rect.vCorners0.v0, rect.vCorners0.v1, rect.vCorners0.v2);
//            p1 = new Vector3(rect.vCorners1.v0, rect.vCorners1.v1, rect.vCorners1.v2);
//            p2 = new Vector3(rect.vCorners2.v0, rect.vCorners2.v1, rect.vCorners2.v2);
//            p3 = new Vector3(rect.vCorners3.v0, rect.vCorners3.v1, rect.vCorners3.v2);
//            if (!success)
//                Debug.LogWarning("Failed to get Calibrated Play Area bounds!  Make sure you have tracking first, and that your space is calibrated.");

//            if (initOpenVR)
//                OpenVR.Shutdown();

//            return success;
//        }

//        void OnPostRender()
//        {
//            if (teleporting)
//            {
//                // Perform the fading in/fading out animation, if we are teleporting.  This is essentially a triangle wave
//                // in/out, and the user teleports when it is fully black.
//                float alpha = Mathf.Clamp01((Time.time - teleportTimeMarker) / (TeleportFadeDuration / 2));
//                if (fadingIn)
//                    alpha = 1 - alpha;

//                Matrix4x4 local = Matrix4x4.TRS(Vector3.forward * 0.3f, Quaternion.identity, Vector3.one);
//                FadeMaterial.SetPass(0);
//                FadeMaterial.SetFloat(materialFadeID, alpha);
//                Graphics.DrawMeshNow(PlaneMesh, transform.localToWorldMatrix * local);
//            }
//        }

//        void Update()
//        {
//            // If we are currently teleporting (ie handling the fade in/out transition)...
//            if (teleporting)
//            {
//                // Wait until half of the teleport time has passed before the next event (note: both the switch from fade
//                // out to fade in and the switch from fade in to stop the animation is half of the fade duration)
//                if (Time.time - teleportTimeMarker >= TeleportFadeDuration / 2)
//                {
//                    if (fadingIn)
//                    {
//                        // We have finished fading out
//                        teleporting = false;
//                    }
//                    else
//                    {
//                        // We have finished fading in - time to teleport!
//                        Vector3 offset = OriginTransform.position - HeadTransform.position;
//                        offset.y = 0;
//                        OriginTransform.position = Pointer.SelectedPoint + offset;
//                    }

//                    teleportTimeMarker = Time.time;
//                    fadingIn = !fadingIn;
//                }
//                return;
//            }

//            // At this point, we are NOT actively teleporting.  So now we care about controller input.
//            if (activeController != null)
//            {
//                // Here, there is an active controller - that is, the user is holding down on the trackpad.
//                // Poll controller for pertinent button data
//                //int index = (int)ActiveController.index;            
//                var device = activeController.controller; /*SteamVR_Controller.Input(index);*/
//                bool shouldTeleport = device.GetPressUp(SteamVR_Controller.ButtonMask.Touchpad);
//                bool shouldCancel = false; /*device.GetPressUp(SteamVR_Controller.ButtonMask.Grip);*/
//                if (shouldTeleport || shouldCancel)
//                {
//                    // If the user has decided to teleport (ie lets go of touchpad) then remove all visual indicators
//                    // related to selecting things and actually teleport
//                    // If the user has decided to cancel (ie squeezes grip button) then remove visual indicators and do nothing
//                    if (shouldTeleport && Pointer.PointOnNavMesh)
//                    {
//                        // Begin teleport sequence
//                        teleporting = true;
//                        teleportTimeMarker = Time.time;
//                        CameraManager.Instance.StartFadingCamera();
//                    }

//                    // Reset active controller, disable pointer, disable visual indicators
//                    activeController = null;
//                    Pointer.enabled = false;
//                    roomBorder.enabled = false;
//                    //RoomBorder.Transpose = Matrix4x4.TRS(OriginTransform.position, Quaternion.identity, Vector3.one);
//                    if (navmeshAnimator != null)
//                        navmeshAnimator.SetBool(enabledAnimatorID, false);

//                    Pointer.transform.parent = null;
//                    Pointer.transform.position = Vector3.zero;
//                    Pointer.transform.rotation = Quaternion.identity;
//                    Pointer.transform.localScale = Vector3.one;

//                    /* Activate laser */
//                    Debug.Log("Teleport Requesting Laser: ON");
//                    ControllerManager.Instance.RequestDisplayLaser(this.gameObject.GetInstanceID());
//                }
//                else
//                {
//                    // The user is still deciding where to teleport and has the touchpad held down.
//                    // Note: rendering of the parabolic pointer / marker is done in ParabolicPointer
//                    Vector3 offset = HeadTransform.position - OriginTransform.position;
//                    offset.y = 0;

//                    // Render representation of where the chaperone bounds will be after teleporting
//                    if (DrawShaperoneBounds)
//                        roomBorder.Transpose = Matrix4x4.TRS(Pointer.SelectedPoint - offset, Quaternion.identity, Vector3.one);

//                    // Haptic feedback click every [HaptickClickAngleStep] degrees
//                    float angleClickDiff = Pointer.CurrentParabolaAngle - lastClickAngle;
//                    if (Mathf.Abs(angleClickDiff) > HapticClickAngleStep)
//                    {
//                        lastClickAngle = Pointer.CurrentParabolaAngle;
//                        device.TriggerHapticPulse();
//                    }

//                    /* Deactivate Laser */
//                    Debug.Log("Teleport Requesting Laser: OFF");
//                    ControllerManager.Instance.RequestHideLaser(this.gameObject.GetInstanceID());
//                }
//            }
//            else
//            {
//                // At this point the user is not holding down on the touchpad at all or has canceled a teleport and hasn't
//                // let go of the touchpad.  So we wait for the user to press the touchpad and enable visual indicators
//                // if necessary.
//                // This is set in a way that we only enable indicators if the right controller(on index 1) is being pressed.

//                // If we don't have any controller on, return.
//                Hand handWithController = IdentifyCorrectHand();
//                if (handWithController == null)
//                    return;

//                // If check if on the hand object there is a controller.
//                SteamVR_Controller.Device device = handWithController.controller;
//                if (device == null)
//                    return;

//                //If user pressed the touchpad.
//                if (device.GetPressDown(SteamVR_Controller.ButtonMask.Touchpad))
//                {
//                    // Set active controller to this controller, and enable the parabolic pointer and visual indicators
//                    // that the user can use to determine where they are able to teleport.
//                    activeController = IdentifyCorrectHand();
//                    lastActiveController = activeController;

//                    Pointer.transform.parent = activeController.transform;
//                    Pointer.transform.localPosition = Vector3.zero;
//                    Pointer.transform.localRotation = Quaternion.identity;
//                    Pointer.transform.localScale = Vector3.one;
//                    Pointer.enabled = true;
//                    // Disabled the room border
//                    if (DrawShaperoneBounds)
//                        roomBorder.enabled = true;
//                    if (navmeshAnimator != null)
//                        navmeshAnimator.SetBool(enabledAnimatorID, true);

//                    Pointer.ForceUpdateCurrentAngle();
//                    lastClickAngle = Pointer.CurrentParabolaAngle;
//                }

//            }
//        }


//        //-------------------------------------------------
//        // Identifies the Correct Hand to control the teleport.
//        //-------------------------------------------------
//        private Hand IdentifyCorrectHand()
//        {
//            Hand leftHand;
//            Hand rightHand;

//            /* Case there is only one controller on or none. */
//            if (Controller1 == null && Controller2 == null)
//            {
//                /* Try to recover if not return.*/
//                FindHands();
//                return null;
//            }

//            if (Controller2 == null && Controller1 != null)
//                return Controller1;
//            if (Controller1 == null && Controller2 != null)
//                return Controller2;

//            /* Case both controllers on. */
//            if (Controller1.GuessCurrentHandType() == Hand.HandType.Left)
//            {
//                leftHand = Controller1;
//                rightHand = Controller2;
//            }
//            else
//            {
//                leftHand = Controller2;
//                rightHand = Controller1;
//            }

//            if (TeleportUsingRight)
//                return rightHand;
//            else
//                return leftHand;
//        }

//        //-------------------------------------------------
//        // A fade interface for other cameras
//        //
//        // Note: Replicate Teleport Fade uses this.
//        //-------------------------------------------------
//        public void FadeOtherCamera()
//        {
//            if (teleporting)
//            {
//                // Perform the fading in/fading out animation, if we are teleporting.  This is essentially a triangle wave
//                // in/out, and the user teleports when it is fully black.
//                float alpha = Mathf.Clamp01((Time.time - teleportTimeMarker) / (TeleportFadeDuration / 2));
//                if (fadingIn)
//                    alpha = 1 - alpha;

//                Matrix4x4 local = Matrix4x4.TRS(Vector3.forward * 0.3f, Quaternion.identity, Vector3.one);
//                FadeMaterial.SetPass(0);
//                FadeMaterial.SetFloat(materialFadeID, alpha);
//                Graphics.DrawMeshNow(PlaneMesh, transform.localToWorldMatrix * local);
//            }
//        }

//        //-------------------------------------------------
//        // Private void find the Hands controllers
//        //-------------------------------------------------
//        private void FindHands()
//        {
//            GameObject hand1 = GameObject.Find("Player/SteamVRObjects/Hand1");
//            if (hand1 != null)
//            {
//                Controller1 = hand1.GetComponent<Hand>();
//                if (Controller1 != null)
//                    Debug.Log("TeleportVive: Found Hand1");
//                else
//                    Debug.LogError("TeleportVive: Hand1 not found, please make sure the path" +
//                              " Player/SteamVRObjects/Hand1 leads to the Hand object.");
//            }
//            GameObject hand2 = GameObject.Find("Player/SteamVRObjects/Hand2");
//            if (hand1 != null)
//            {
//                Controller2 = hand2.GetComponent<Hand>();
//                if (Controller2 != null)
//                    Debug.Log("TeleportVive: Found Hand2");
//                else
//                    Debug.LogError("TeleportVive: Hand2 not found, please make sure the path" +
//                              " Player/SteamVRObjects/Hand2 leads to the Hand object.");

//            }
//        }
//    }
//}

