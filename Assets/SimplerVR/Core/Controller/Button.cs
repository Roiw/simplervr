using System;
using UnityEngine;

namespace SimplerVR.Core.Controller
{
    /// <summary>
    /// <para>Class with definitions and methods(events) for a specific button.</para>
    /// </summary>
    public class Button
    {
        /// <summary>
        /// The types of buttons that exist in the application.
        /// </summary>
        public enum ButtonName {Trigger, Touchpad, Grip, ApplicationMenu} // If you add new items remember to update the HintConvert script.

        /// <summary>
        /// Some codes for debug.
        /// </summary>
        public enum RegisterCode { ActionRegistered, ButtonAlreadyDefined, CouldNotFindButton, UndefinedError}

        /// <summary>
        /// The types of actions a button can perform.
        /// </summary>
        public enum ButtonActions { PressUp, HoldDown }

        /// <summary>
        /// This button type.
        /// </summary>
        public ButtonName Name;

        /// <summary>
        /// This button action type.
        /// </summary>
        public ButtonActions Action;

        /// <summary>
        /// True if this button represent a right controller button. 
        /// </summary>
        public readonly bool IsRightControllerButton;

        /// <summary>
        /// The feature type that uses this button.
        /// </summary>
        public Type FeatureOwner;

        /// <summary>
        /// True if this button overrides an interaction.
        /// </summary>
        public bool OverridesInteraction;

        /// <summary>
        /// True if this button can override interaction when pressed up.
        /// </summary>
        public bool OverrideInteraction_PressedUp;

        /// <summary>
        /// True if this button can override interaction when on hold down.
        /// </summary>
        public bool OverrideInteraction_HoldDown;

        /// <summary>
        /// The method to run.
        /// </summary>
        private Action buttonDelegate;

        /// <summary>
        /// The event that controls the delegate.
        /// </summary>
        event Action buttonEvent
        {
            add
            {
                if (buttonDelegate != null)
                    Debug.LogError("Event was already registered, would you like more than one action to be invoked?");
                else
                    buttonDelegate = value;
            }
            remove { buttonDelegate = null; }
        }


        /// <summary>
        /// Gets the method from this button.
        /// </summary>
        /// <param name="buttonAction">The button action related to the method.</param>
        /// <returns>The method to execute.</returns>
        public Action GetMethod()
        {
            Action returnAction = null;
            returnAction = buttonDelegate;

            if (returnAction == null)
                Debug.Log("Pressing button: " + this.Name + " Input type: " + this.Action + " Controller position: " + 
                    this.IsRightControllerButton + ". However no action registered." );

            return returnAction;
        }

        /// <summary>
        /// Constructor for the button that is linked to a feature.
        /// </summary>
        /// <param name="buttonType">The type of the button.</param>
        /// <param name="isRight">True if it represents a button on the right controller.</param>
        public Button(ButtonName buttonType, ButtonActions action, bool isRight, bool overridesInteraction, Action method, Type featureOwner)
        {
            this.IsRightControllerButton = isRight;
            Name = buttonType;
            Action = action;
            buttonDelegate = method;
            FeatureOwner = featureOwner;
        }
    }
}
