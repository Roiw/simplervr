using SimplerVR.PlatformInterfaces;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using SimplerVR.Core.Controller;
using SimplerVR.Core.Controller.Hint;

namespace SimplerVR.Core.Interaction
{
    /// <summary>
    ///  <para>Every object that will be interactable with controllers must have a 
    ///  script that extend InteractableObject.</para>
    ///  
    ///  <para>The script will override the current methods to something specific of the interaction
    ///  you want to create.</para>  
    /// </summary>
    [Serializable]
    public class InteractableObject : MonoBehaviour
    {
        /// <summary>
        /// This variable is set to true while this object is interacting.
        /// </summary>
        [HideInInspector]
        public bool IsWithinGrasp
        {
            get { return isWithinGrasp; }
            set
            {
                /* Check if isWithinGrasp Value changed. */
                if (isWithinGrasp != value)
                {
                    isWithinGrasp = value;
                }
            }
        }
        private bool isWithinGrasp;

        /// <summary>
        /// A list of buttons selected by the UI.
        /// </summary>
        [HideInInspector]
        public List<ButtonRegistry> ButtonsSelected;

        /// <summary>
        /// The amount of buttons used.
        /// </summary>
        [HideInInspector]
        public int AmountOfButtons = 1;

        /// <summary>
        /// If this is false it will not be interactable with any controller.
        /// </summary>
        public bool IsInteractable = true;

        /// <summary>
        /// If this variable is false IsInteractable will be false. However it doesn't guranteed that IsInteractable is true.
        /// The lack of enabled colliders will set the IsInteractable to false.
        /// </summary>
        protected bool ShouldBeInteractable = true;

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

        private ControlSet objectButtons;

        void Update()
        {
            CheckIfIsInteractable();
        }


        /// <summary>
        /// True if this is not an active kind of interaction.
        /// </summary>
        protected bool isPassive = true;

        /// <summary>
        /// True if this interaction respond to buttons
        /// </summary>
        /// <returns>True if this interaction respond to buttons</returns>
        public bool IsActiveInteraction()
        {
            return !isPassive;
        }

        /// <summary>
        /// Sets this interaction as an active interaciton (that means this interaction uses controllers to perform its features).
        /// </summary>
        /// <param name="isActive">True if this is an active platform.</param>
        public void SetActiveInteraction(bool isActive)
        {
            isPassive = !isActive;
        }

        /// <summary>
        /// Check if this object is interactable or not.
        /// </summary>
        protected void CheckIfIsInteractable()
        {
            if (ShouldBeInteractable == false)
            {
                IsInteractable = false;
                return;
            }
            
            // Check if there is colliders to check collision. If not mark this as not interactable.
            List<Collider> colliders = new List<Collider>();
            colliders.AddRange(this.GetComponents<Collider>());
            if (colliders.Count == 0)
            {
                IsInteractable = false;
                return;
            }

            // Even if there are colliders check if there is at least one active.
            int i = 0;
            foreach (Collider coly in colliders)
            {
                if (coly.enabled == false)
                    i++;
            }
            if (colliders.Count == i)
                IsInteractable = false;
            // Fullfils all rules.. should be interactable.
            else
                IsInteractable = true;
        }
        
        #region Hint Variables
        [Header("Hint Variables")]
        public List<HintManager.ButtonID> ButtonId;
        public bool hasHint = false;
        public string Hint = "";
        #endregion       

    }

}
