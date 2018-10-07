using UnityEngine;
using static SimplerVR.Core.Controller.Button;
using System.Collections.Generic;
using System;
using SimplerVR.Core.Controller;

namespace SimplerVR.PlatformInterfaces
{
    /// <summary>
    /// All interfaces that contain controller must implement this class. 
    /// </summary>
    public abstract class GenericControllerPlatform : GenericPlatform
    {
        ///// <summary>
        ///// Returns true if the button has just been released on the controller. 
        ///// </summary>
        ///// <param name="button">The ControllerButtons for this button.</param>
        ///// <param name="controller">The Controller game object.</param>
        ///// <returns>True if the button has just been released in the controller.</returns>
        //public abstract bool GetPressUp(ButtonName button, GameObject controller);

        ///// <summary>
        ///// Returns true if the button is being hold down.
        ///// </summary>
        ///// <param name="button">The ControllerButtons for this button.</param>
        ///// <param name="controller">The Controller game object.</param>
        ///// <returns>True if the button is being hold down.</returns>
        //public abstract bool GetHoldDown(ButtonName button, GameObject controller);

        /// <summary>
        /// <para>Returns true if the ButtonAction on the specified controller GameObject and ButtonName button is happening.</para>
        /// <para>Else returns null.</para>
        /// </summary>
        /// <param name="button">The ControllerButtons for this button.</param>
        /// <param name="action">A button action.</param>
        /// <param name="controller">The Controller game object.</param>
        /// <returns>True if the button is being hold down.</returns>
        public abstract bool GetActionStatus(Button.ButtonName button, Button.ButtonActions action, GameObject controller);

        /// <summary>
        /// Returns the ControlSet used by the platform.
        /// </summary>
        /// <returns></returns>
        public abstract ControlSet GetPlatformControls();

        /// <summary>
        /// Returns the game objects that represents the Controller.
        /// </summary>
        /// <returns>An array of game object tha represent the controllers. Position 0 is the right controller 1 is the left controller.</returns>
        public abstract GameObject[] GetControllerObject();

        /// <summary>
        /// <para>Returns a GameObject that will be used to grab a game object.</para>
        /// <para>This will be used to attach objects to the controller, it could be a hint or an object that we want to attach to the controller.</para>
        /// <para>All those objects will use this position as a reference for placement.</para>
        /// </summary>
        /// <param name="isRight">True if we want this on the right hand.</param>
        /// <returns></returns>
        public abstract GameObject GetControllerAttachPoint(bool isRight);

        /// <summary>
        /// True if the platform controllers support haptic pulse (vibration).
        /// </summary>
        /// <returns>True if the platform controllers support haptic pulse (vibration).</returns>
        public abstract bool UseHapticPulse();

        /// <summary>
        /// Triggers the platform haptic pulse.
        /// </summary>
        /// <param name="time">The duration of the pulse in sec. 0 if nonstop.</param>
        /// <param name="controller">The controller to pulse.</param>
        public abstract void TriggerHapticPulse(float time, Controller controller);
    }
}
