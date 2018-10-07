using System;
using System.Collections.Generic;
using UnityEngine;

namespace SimplerVR.Core.Controller
{
    /// <summary>
    /// <para>A ControlSet is used to specify a set of Button and manage events hooked to them.</para>
    /// <para>You can make your own ControlSet (or use one created for a platform) to implent control events for your controllers.</para>
    /// <para>This is the base class for every ControlSet class. 
    /// Being so it defines the minimum to be implemented by a ControlSet</para>
    /// </summary>
    public abstract class ControlSet
    {

        public enum Options { invalid, isRight, isLeft, OverrideInteraction, DontOverrideInteraction }

        /// <summary>
        /// A list with all the buttons of this ControlSet.
        /// </summary>
        protected List<Button> controllerButtons = new List<Button>();

        /// <summary>
        /// Return the method defined for the specific button.
        /// </summary>
        /// <param name="button">The button enum, listed under Controller. ButtonName</param>
        /// <param name="action">The button action related to the method.</param>
        /// <param name="isRight">True if this button is on the right hand.</param>
        /// <param name="overridesInteraction">True if this button method overrides interaction.</param>
        /// <returns>A method to be invoked.</returns>
        public abstract List<Action> GetButtonMethods(Button.ButtonName button, Button.ButtonActions action, params Options[] options);

        /// <summary>
        /// All buttons have their registered methods cleared.
        /// </summary>
        public abstract void ClearAllButtons();

        /// <summary>
        /// Return a list of buttons available by this controller.
        /// </summary>
        /// <returns></returns>
        public abstract List<Button.ButtonName> GetAvailableButtonsList(bool isRight);

        /// <summary>
        /// Return a list of ButtonActions available by this controller.
        /// </summary>
        /// <param name="isRight"></param>
        /// <returns></returns>
        public abstract List<Button.ButtonActions> GetAvailableButtonActionsList(bool isRight);

        /// <summary>
        /// Sets the button method on this control set.
        /// </summary>
        /// <param name="button">Which button we are setting.</param>
        /// <param name="action">The button action related to the method.</param>
        /// <param name="isRight">True if this button is on the right controller.</param>
        /// <param name="method">The method that it will execute when run.</param>
        /// <param name="shouldOverrideInteraction">True if it should override ongoing interactions.</param>
        /// <param name="featureOwner">The type of the feature that instatiated this button.</param>
        public abstract void AddButton(Button.ButtonName button, Button.ButtonActions action, bool isRight,
            Action method, bool shouldOverrideInteraction, Type featureOwner);

        /// <summary>
        /// Remove the button method from this control set.
        /// </summary>
        /// <param name="button">Which button we are setting.</param>
        /// <param name="action">The button action related to the method.</param>
        /// <param name="isRight">True if this button is on the right controller.</param>
        /// <param name="method">The method that it will execute when run.</param>
        /// <param name="owner">The type of the object that instatiated this button. This should be unique.</param>
        protected abstract void RemoveButton(Button.ButtonName button, Button.ButtonActions action, bool isRight, Action method, Type owner);

        /// <summary>
        /// Remove all buttons of the given owner.
        /// </summary>
        /// <param name="owner">The type owner of the buttons.</param>
        public abstract void RemoveAllButtons(Type owner);

        /// <summary>
        /// Returns a new class with the same attributes as this one.
        /// </summary>
        /// <returns>A ControlSet class with the same attributes of this one.</returns>
        public abstract ControlSet CloneThisClass();

        /// <summary>
        /// Return a list of all the buttons registered on the platform for the feature.
        /// </summary>
        /// <param name="featureType">The type of the feature.</param>
        /// <returns>A list of Button.</returns>
        public abstract List<Button> GetFeatureButtons(Type featureType);
    }

}
