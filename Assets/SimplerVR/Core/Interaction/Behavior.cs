using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace SimplerVR.Core.Interaction
{
    /// <summary>
    /// Behavior represent passive actions that happens to an object when the player is interacting with it.
    /// </summary>
    public class Behavior : InteractableObject
    {
        [HideInInspector]
        public bool isRunning;

        /// <summary>
        /// Start the main method.
        /// </summary>
        public virtual void DoBehavior()
        {
            isRunning = true;
        }

        /// <summary>
        /// Stop the main behavior method.
        /// </summary>
        public virtual void StopBehavior()
        {
            isRunning = false;
        }
    }
}
