using System;
using SimplerVR.Core;
using UnityEngine;

namespace SimplerVR.Features
{
    /// <summary>
    /// All the features in the system should implement a manager that is a child of this class.
    /// </summary>
    public abstract class GenericFeatureManager : MonoBehaviour
    {
        /// <summary>
        /// <para>This is a backup name for the feature.</para>
        /// <para>You must create a variable on the Feature called FeatureName and have it set there.</para>
        /// </summary>
        [HideInInspector]
        public string BackupName = "Couldn't find FeatureName variable on Feature class.";

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

        /// <summary>
        /// Returns the name of the feature.
        /// </summary>
        /// <returns>The name of the feature.</returns>
        public abstract string GetFeatureName();

        /// <summary>
        /// Returns the feature type.
        /// </summary>
        /// <returns>Feature type</returns>
        public abstract Type GetFeatureType();

    }
}
