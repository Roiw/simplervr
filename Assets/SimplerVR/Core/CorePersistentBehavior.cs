using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace SimplerVR.Core
{
    /// <summary>
    /// <para>This is class is not destroyed throughout all scenes.</para>
    /// <para>In that way it carries Core RUNTIME configurations throughout the project.</para>
    /// <para>It's good practice to AVOID putting things in here.</para>
    /// <para>You can also add things to the CoreSettings class, that class is persistent throughout scenes and even after reopening Unity.</para>
    /// </summary>
    public class CorePersistentBehavior : MonoBehaviour
    {
        public static CorePersistentBehavior Instance = null;

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

        void Awake()
        {
            // Guarantees that this object is not duplicated throughout the project on runtime.
            if (Instance != null)
            {
                Destroy(this.gameObject);
                Destroy(this);
                return;
            }
            else
            {
                Instance = this;
                this.transform.parent = null;
                DontDestroyOnLoad(this.gameObject);
            }
        }
    }
}
