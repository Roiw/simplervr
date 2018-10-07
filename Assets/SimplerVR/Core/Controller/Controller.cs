using System.Collections.Generic;
using SimplerVR.Core.Controller.Hint;
using SimplerVR.Core.Interaction;
using UnityEngine;

namespace SimplerVR.Core.Controller
{
    public class Controller : MonoBehaviour
    {
        [Tooltip("Mark if you want hints.")]
        public bool HintsEnabled = true;

        public List<InteractableObject> InteractableObjects = new List<InteractableObject>();
        private GameObject lastHitGameObject;

        // Use this for initialization
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {
            if (ControllerManager.Instance.UseLaserInteraction)
            {
                /* Check if this controller has the laser. */

                /*
                   If the laser is on the LeftHand and this is the RightHand.
                   It means that this controller don't have the laser thus we return. */
                if (ControllerManager.Instance.LaserOnLeft && (ControllerManager.Instance.GetRightController() == this.gameObject))
                    return;

                /* If the laser is on the RightHand and this is the LeftHand.
                   It means that this controller don't have the laser thus we return. */
                if (!ControllerManager.Instance.LaserOnLeft && (ControllerManager.Instance.GetLeftController() == this.gameObject))
                    return;

                RaycastHit hit;
                GameObject laserObject = ControllerManager.Instance.LaserHolder;
                if (laserObject == null)
                    return;

                float laserLenght = ControllerManager.Instance.LaserLenght;
                Vector3 direction = (laserObject.transform.TransformDirection(Vector3.forward));

                if (Physics.Raycast(laserObject.transform.position, direction, out hit, laserLenght, ControllerManager.Instance.LaserCollision))
                {

                    ControllerManager.Instance.SetLaserHitting(hit);
                    ControllerManager.Instance.RequestDisplayLaser(this.gameObject.GetInstanceID());

                    //print("Found an object - Name: " + hit.transform.gameObject.name);
                    if (hit.transform.gameObject.tag == "ViveInteractable" && lastHitGameObject != hit.transform.gameObject)
                    {
                        // Notify old interacting that it stopped being hit
                        if (lastHitGameObject != null)
                            NotifyInterruptedInteraction(lastHitGameObject);

                        // The last interacted object is this new interaction
                        lastHitGameObject = hit.transform.gameObject;

                        // Notify the interaction.
                        NotifyInteraction(hit.transform.gameObject);
                        Debug.Log("Laser Interacting with:" + hit.transform.gameObject.name);
                    }
                    else if (hit.transform.gameObject.tag == "ViveInteractable" && lastHitGameObject == hit.transform.gameObject)
                    {
                        // Case interacting with the same object.. Do nothing..
                        Debug.Log("Laser Interacting with the same object.");                       
                    }
                    else
                    {
                        // Case interacting with a non interactable object.
                        if (lastHitGameObject != null)
                            NotifyInterruptedInteraction(lastHitGameObject);
                        lastHitGameObject = null;
                    }
                }
                else
                {
                    Debug.Log("Laser Not Interacting.");                 
                    if (lastHitGameObject != null)
                        NotifyInterruptedInteraction(lastHitGameObject);

                    lastHitGameObject = null;
                    ControllerManager.Instance.SetLaserNotHitting();
                    ControllerManager.Instance.RequestDisplayLaser(this.gameObject.GetInstanceID());
                }

            }

            // Check if one of our interactions should no longer e active.
            for (int i = 0; i < InteractableObjects.Count; i++)
                if (InteractableObjects[i].IsInteractable == false)
                    NotifyInterruptedInteraction(InteractableObjects[i]);

        }

        // Detects that the player is touching an object
        void OnTriggerEnter(Collider other)
        {
            if (other.tag == "ViveInteractable")
            {
                /* The object is VIVE interactable */
                NotifyInteraction(other.gameObject);
                Debug.Log("Touching object: " + other.gameObject.name);

            }
        }

        void OnTriggerExit(Collider other)
        {
            /* Interaction has stopped. */
            if (other.tag == "ViveInteractable")
            {
                NotifyInterruptedInteraction(other.gameObject);
            }
        }

        //-------------------------------------------------
        // Notify ControllerManager about an ongoing interaction.
        //-------------------------------------------------
        private void NotifyInteraction(GameObject interactionObject)
        {


            // Add everbody that is interacting from this interactionObject.
            InteractableObject[] interactableObjects = interactionObject.GetComponents<InteractableObject>();
            List<InteractableObject> objectsToRemove = new List<InteractableObject>();

            InteractableObjects.AddRange(interactableObjects);

            /* Notify the object that its being touched. */
            foreach (InteractableObject obj in InteractableObjects)
            {
                // If the object activelly dont want to be considered.. We save it for being removed later.
                // and continue to the next element.
                if (!obj.IsInteractable)
                {
                    objectsToRemove.Add(obj);
                    continue;
                }                    

                obj.IsWithinGrasp = true;

                /* Show Hint */
                if (obj.hasHint && HintsEnabled)
                {
                    if (ControllerManager.Instance.GetRightController() == this.gameObject)
                    {
                        List<HintManager.ButtonID> rights = obj.ButtonId.FindAll(b => b.ToString().Contains("Right"));
                        foreach (HintManager.ButtonID righty in rights)
                            HintManager.Instance.DisplayHint(obj.Hint, righty);
                    }
                    else if (ControllerManager.Instance.GetLeftController() == this.gameObject)
                    {
                        List<HintManager.ButtonID> lefts = obj.ButtonId.FindAll(b => b.ToString().Contains("Left"));
                        foreach (HintManager.ButtonID lefty in lefts)
                            HintManager.Instance.DisplayHint(obj.Hint, lefty);
                    }
                }
            }

            // Remove everybody that don't count for interaction.
            InteractableObjects.RemoveAll(r => objectsToRemove.Contains(r));

            if (InteractableObjects.Count == 0)
                return; // If we don't have any object interacting return.

            /* Set whitch controller is interacting */
            if (ControllerManager.Instance.GetRightController() == this.gameObject)
                ControllerManager.Instance.isRightControllerInteracting = true;
            else if (ControllerManager.Instance.GetLeftController() == this.gameObject)
                ControllerManager.Instance.isLeftControllerInteracting = true;
            else
                Debug.LogError("Can't tell which hand is interacting.");
        }

        //-------------------------------------------------
        // Notify ControllerManager an interaction has stopped.
        //-------------------------------------------------
        private void NotifyInterruptedInteraction(GameObject stopInteractionFromObject)
        {
            /* 
               Notify ControllerManager that the player is not 
               touching the object anymore. 
            */

            if (InteractableObjects == null)
                return;

            //IntObject.IsWithinGrasp = false;
            //Debug.Log("Interrupetd: " + IntObject.name);
            //IntObject = null;

            // If not touching anything reset variables to default.
            List<InteractableObject> thisObjects = new List<InteractableObject>();
            thisObjects.AddRange(stopInteractionFromObject.GetComponents<InteractableObject>());

            foreach (InteractableObject obj in thisObjects)
            {
                // Notify the object and update this controller list.
                obj.IsWithinGrasp = false; // Tells the object it's not interacting anymore.
                Debug.Log("Interrupted: " + obj.name);
                InteractableObjects.Remove(obj); // Remove it from the list of objects.

                /* Disable Hint */
                if (obj.hasHint && HintsEnabled)
                {
                    if (ControllerManager.Instance.GetRightController() == this.gameObject)
                    {
                        List<HintManager.ButtonID> rights = obj.ButtonId.FindAll(b => b.ToString().Contains("Right"));
                        foreach (HintManager.ButtonID righty in rights)
                            HintManager.Instance.HideHint(righty);
                    }
                    else if (ControllerManager.Instance.GetLeftController() == this.gameObject)
                    {
                        List<HintManager.ButtonID> lefts = obj.ButtonId.FindAll(b => b.ToString().Contains("Left"));
                        foreach (HintManager.ButtonID lefty in lefts)
                            HintManager.Instance.HideHint(lefty);
                    }
                }
            }

            if (InteractableObjects.Count > 0)
                return; // Return if we are interacting with at least 1 object.

            /* Set which controller is not interacting */
            if (ControllerManager.Instance.GetRightController() == this.gameObject)
                ControllerManager.Instance.isRightControllerInteracting = false;
            else if (ControllerManager.Instance.GetLeftController() == this.gameObject)
                ControllerManager.Instance.isLeftControllerInteracting = false;
            else
                Debug.LogError("Can't tell which hand is interacting.");
        }

        //-------------------------------------------------
        // Notify ControllerManager an interaction has stopped.
        //-------------------------------------------------
        private void NotifyInterruptedInteraction(InteractableObject interaction)
        {
            /* 
               Notify ControllerManager that the player is not 
               touching the object anymore. 
            */

            if (InteractableObjects == null)
                return;

            //IntObject.IsWithinGrasp = false;
            //Debug.Log("Interrupetd: " + IntObject.name);
            //IntObject = null;

            // Notify the object and update this controller list.
            interaction.IsWithinGrasp = false; // Tells the object it's not interacting anymore.
            Debug.Log("Interrupetd: " + interaction.name);
            InteractableObjects.Remove(interaction); // Remove it from the list of objects.

            /* Disable Hint */
            if (interaction.hasHint && HintsEnabled)
            {
                if (ControllerManager.Instance.GetRightController() == this.gameObject)
                {
                    List<HintManager.ButtonID> rights = interaction.ButtonId.FindAll(b => b.ToString().Contains("Right"));
                    foreach (HintManager.ButtonID righty in rights)
                        HintManager.Instance.HideHint(righty);
                }
                else if (ControllerManager.Instance.GetLeftController() == this.gameObject)
                {
                    List<HintManager.ButtonID> lefts = interaction.ButtonId.FindAll(b => b.ToString().Contains("Left"));
                    foreach (HintManager.ButtonID lefty in lefts)
                        HintManager.Instance.HideHint(lefty);
                }
            }


            if (InteractableObjects.Count > 0)
                return; // Return if we are interacting with at least 1 object.

            /* Set which controller is not interacting */
            if (ControllerManager.Instance.GetRightController() == this.gameObject)
                ControllerManager.Instance.isRightControllerInteracting = false;
            else if (ControllerManager.Instance.GetLeftController() == this.gameObject)
                ControllerManager.Instance.isLeftControllerInteracting = false;
            else
                Debug.LogError("Can't tell which hand is interacting.");
        }

        #region Auxiliary  


        #endregion

    }
}