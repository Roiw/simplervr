using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;
using SimplerVR.Core.Controller;
using static SimplerVR.Core.Controller.Button;

namespace SimplerVR.PlatformInterfaces.VIVE
{
    public class VIVEControls : ControlSet
    {

        /// <summary>
        /// This is a list of the available buttons. This must be specified
        /// </summary>
        private List<Button.ButtonName> availbleRightButtons = SupportedPlatforms.GetButtons(SupportedPlatforms.Platforms.VIVE)
            .OfType<Button.ButtonName>().ToList();


        private List<Button.ButtonName> availbleLeftButtons = SupportedPlatforms.GetButtons(SupportedPlatforms.Platforms.VIVE)
            .OfType<Button.ButtonName>().ToList();

        private List<Button.ButtonActions> availableRightButtonActions = SupportedPlatforms.GetButtonActions(SupportedPlatforms.Platforms.VIVE)
            .OfType<Button.ButtonActions>().ToList();

        private List<Button.ButtonActions> availableLeftButtonActions = SupportedPlatforms.GetButtonActions(SupportedPlatforms.Platforms.VIVE)
            .OfType<Button.ButtonActions>().ToList();


        /// <summary>
        /// Use rightButtons instead of this!
        /// </summary>
        private List<Button> internalRightButtons;

        /// <summary>
        /// The Right buttons.
        /// </summary>
        private List<Button> rightButtons
        {
            get
            {
                if (internalRightButtons == null)
                {
                    internalRightButtons = new List<Button>();
                }
                return internalRightButtons;
            }
        }

        /// <summary>
        /// Use leftButtons instead of this!
        /// </summary>
        private List<Button> internalLeftButtons;

        /// <summary>
        /// The left buttons.
        /// </summary>
        private List<Button> leftButtons
        {
            get
            {
                if (internalLeftButtons == null)
                {
                    internalLeftButtons = new List<Button>();
                }
                return internalLeftButtons;
            }
        }

        /// <summary>
        /// All buttons have their registered methods cleared.
        /// </summary>
        public override void ClearAllButtons()
        {
            if (internalRightButtons != null)
                internalRightButtons.Clear();
            if (internalLeftButtons != null)
                internalLeftButtons.Clear();
        }

        /// <summary>
        /// Return a list of buttons available by this controller.
        /// </summary>
        /// <returns></returns>
        public override List<Button.ButtonName> GetAvailableButtonsList(bool isRight)
        {
            if (isRight)
                return availbleRightButtons;
            else
                return availbleLeftButtons;
        }

        /// <summary>
        /// Return a list of ButtonActions available by this controller.
        /// </summary>
        /// <returns></returns>
        public override List<Button.ButtonActions> GetAvailableButtonActionsList(bool isRight)
        {
            if (isRight)
                return availableRightButtonActions;
            else
                return availableLeftButtonActions;
        }

        /// <summary>
        /// Return the method defined for the specific button.
        /// </summary>
        /// <param name="button">The button enum, listed under Controller. </param>
        /// <param name="action">The button action related to the method.</param>
        /// <param name="isRight">True if this button is on the right hand.</param>
        /// <param name="overridesInteraction">True if this button method overrides interaction.</param>
        /// <returns>A method to be invoked.</returns>
        public override List<Action> GetButtonMethods(Button.ButtonName button, Button.ButtonActions action, params Options[] options)
        {
            bool isRight = false;
            bool isLeft = false;
            bool overrideInteraction = false;
            bool dontOverrideInteraction = false;
            for (int i = 0; i < options.Length; i++)
            {
                isRight = (options[i] == Options.isRight) ? true : isRight;
                isLeft = (options[i] == Options.isLeft) ? true : isLeft;
                overrideInteraction = (options[i] == Options.OverrideInteraction) ? true : overrideInteraction;
                dontOverrideInteraction = (options[i] == Options.DontOverrideInteraction) ? true : dontOverrideInteraction;
            }

            List<Action> actions = new List<Action>();
            List<Button> buttons = null;
            // Return all the buttons.
            if (isRight && overrideInteraction && isLeft && dontOverrideInteraction || !isRight && !overrideInteraction && !isLeft && !dontOverrideInteraction)
            {
                buttons = rightButtons.FindAll(b => b.Name == button && b.Action == action);
                buttons.AddRange(leftButtons.FindAll(b => b.Name == button && b.Action == action));
            }
            // Return all right buttons
            else if (isRight && !(overrideInteraction ^ dontOverrideInteraction))
            {
                buttons = rightButtons.FindAll(b => b.Name == button && b.Action == action);
            }
            // Return all left buttons
            else if (isLeft && !(overrideInteraction ^ dontOverrideInteraction))
            {
                buttons = leftButtons.FindAll(b => b.Name == button && b.Action == action);
            }
            // Return all right buttons with override.
            else if (isRight && overrideInteraction)
            {
                buttons = rightButtons.FindAll(b => b.Name == button && b.Action == action && b.OverridesInteraction == true);
            }
            // Return all left buttons with override.
            else if (isLeft && overrideInteraction)
            {
                buttons = leftButtons.FindAll(b => b.Name == button && b.Action == action && b.OverridesInteraction == true);
            }
            // Return all right buttons without override.
            else if (isRight && overrideInteraction)
            {
                buttons = rightButtons.FindAll(b => b.Name == button && b.Action == action && b.OverridesInteraction == false);
            }
            // Return all left buttons without override.
            else if (isLeft && overrideInteraction)
            {
                buttons = leftButtons.FindAll(b => b.Name == button && b.Action == action && b.OverridesInteraction == false);
            }
            // Return all buttons with override.
            else if (overrideInteraction && !(isRight ^ isLeft))
            {
                buttons = rightButtons.FindAll(b => b.Name == button && b.Action == action && b.OverridesInteraction == true);
                buttons.AddRange(leftButtons.FindAll(b => b.Name == button && b.Action == action && b.OverridesInteraction == true));
            }
            // Return all buttons without override
            else if (dontOverrideInteraction && !(isRight ^ isLeft))
            {
                buttons = rightButtons.FindAll(b => b.Name == button && b.Action == action && b.OverridesInteraction == false);
                buttons.AddRange(leftButtons.FindAll(b => b.Name == button && b.Action == action && b.OverridesInteraction == false));
            }
            else
                Debug.LogError("Something is off could not identify which buttons return.");

            if (buttons != null)
            {
                foreach (Button b in buttons)
                {
                    actions.Add(b.GetMethod());
                }
                return actions;
            }
            else
            {
                Debug.LogError("Could not find button.");
                return null;
            }
        }

        /// <summary>
        /// Sets the button method on this control set.
        /// </summary>
        /// <param name="button">Which button we are setting.</param>
        /// <param name="action">The button action related to the method.</param>
        /// <param name="isRight">True if this button is on the right controller.</param>
        /// <param name="method">The method that it will execute when run.</param>
        /// <param name="shouldOverrideInteraction">True if it should override ongoing interactions.</param>
        /// <param name="featureOwner">The type of the feature that instatiated this button. Null if not a feature button.</param>
        public override void AddButton(Button.ButtonName name, Button.ButtonActions action, bool isRight,
            Action method, bool shouldOverrideInteraction, Type featureOwner)
        { 
        
            Button button = new Button(name, action, isRight, shouldOverrideInteraction, method, featureOwner);

            if (isRight)
            {
                rightButtons.Add(button);
                // You can add a verification if you are worried about several methods using the same button.
            }
            else
            {
                leftButtons.Add(button);
                // You can add a verification if you are worried about several methods using the same button.
            }
        }

        /// <summary>
        /// Returns a new class with the same attributes as this one.
        /// </summary>
        /// <returns>A ControlSet class with the same attributes of this one.</returns>
        public override ControlSet CloneThisClass()
        {
            VIVEControls returnClass = (VIVEControls)this.MemberwiseClone();
            returnClass.ClearAllButtons();
            return returnClass;
        }

        /// <summary>
        /// Remove the button method from this control set.
        /// </summary>
        /// <param name="button">Which button we are setting.</param>
        /// <param name="action">The button action related to the method.</param>
        /// <param name="isRight">True if this button is on the right controller.</param>
        /// <param name="method">The method that it will execute when run.</param>
        /// <param name="featureOwner">The type of the feature that instatiated this button.</param>
        protected override void RemoveButton(Button.ButtonName name, Button.ButtonActions action, bool isRight, Action method, Type featureOwner)
        {
            Button button;
            if (isRight)
            {
                button = rightButtons.Find(b => b.Name == name && b.Action == action && b.GetMethod() == method && b.FeatureOwner == featureOwner);
                if ( button == null)
                {
                    Debug.LogError("Could not find the button you are trying to delete.");
                    return;
                }
                rightButtons.Remove(button);
            }
            else
            {
                button = leftButtons.Find(b => b.Name == name && b.Action == action && b.GetMethod() == method && b.FeatureOwner == featureOwner);
                if (button == null)
                {
                    Debug.LogError("Could not find the button you are trying to delete.");
                    return;
                }
                leftButtons.Remove(button);
            }             
        }

        /// <summary>
        /// Remove all buttons of the given owner.
        /// </summary>
        /// <param name="owner">The type owner of the buttons.</param>
        public override void RemoveAllButtons(Type owner)
        {
            leftButtons.RemoveAll(b => b.FeatureOwner == owner);
            rightButtons.RemoveAll(b => b.FeatureOwner == owner);
        }


        /// <summary>
        /// Return all the buttons the are linked to a specific feature type.
        /// </summary>
        /// <param name="featureType">A feature type.</param>
        /// <returns>A list of Buttons</returns>
        public override List<Button> GetFeatureButtons(Type featureType)
        {
            List<Button> buttons;
            buttons = leftButtons.FindAll(b => b.FeatureOwner == featureType);
            buttons.AddRange(rightButtons.FindAll(b => b.FeatureOwner == featureType));

            return buttons;
        }
    }
}
