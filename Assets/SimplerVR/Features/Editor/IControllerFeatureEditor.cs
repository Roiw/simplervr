using UnityEngine;

namespace SimplerVR.Features.Editor
{
    public interface IControllerFeatureEditor
    {
        /// <summary>
        /// <para>This method calls the registering method on the arc teleport passing the selected buttons from this UI.</para>
        /// <para>Runs everytime the button has been changed.</para>
        /// </summary>
        /// <param name="obj">The feature game object.</param>
        void RegisterActions(GameObject obj);

        /// <summary>
        /// <para>This method undo what the RegisterAction method does.</para>
        /// <para>Basically it removes from the button the methods registered on it by the Arc Teleport.</para>
        /// </summary>
        /// <param name="obj">The feature game object.</param>
        void UnregisterActions(GameObject obj);

        /// <summary>
        /// Creates and setup the feature on obj.
        /// </summary>
        /// <param name="obj">The object to spawn and configure the feature.</param>
        void SetupFeature(GameObject obj);

    }
}
