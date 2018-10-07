using System;
using System.Collections.Generic;
using SimplerVR.Core.Controller;

namespace SimplerVR.Features
{
    public interface IControllerFeatureData
    {
        /// <summary>
        /// Returns the feature type.
        /// </summary>
        /// <returns>The feature type.</returns>
        Type GetFeatureType();

        /// <summary>
        /// Serializes the settings on a file.
        /// </summary>
        void Save();

        /// <summary>
        /// Returns a list of the selected buttons by the UI.
        /// </summary>
        /// <returns>A ButtonRegistry list of selected buttons.</returns>
        List<ButtonRegistry> GetSelectedButtons();

        /// <summary>
        /// Gets the path to the serialized data.
        /// </summary>
        /// <returns>The path to the serialized data.</returns>
        string GetPathToData();
    }
}
