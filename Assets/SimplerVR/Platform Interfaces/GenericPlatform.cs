using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SimplerVR.PlatformInterfaces
{
    /// <summary>
    /// <para>All the platform interfaces inherid from this class.</para>
    /// <para>All platform interfaces are required to implement each of the methods below.</para>
    /// </summary>
    public abstract class GenericPlatform : ScriptableObject
    {
        /// <summary>
        /// Discover if the controller is the rightmost, compared to the HMD.
        /// </summary>
        /// <returns>True if it is. False if not.</returns>
        public abstract GameObject ReturnRightmostObject(GameObject object1, GameObject object2);

        /// <summary>
        /// Initializes the API.
        /// </summary>
        public abstract void Initialize();

        /// <summary>
        /// Returns the main cameras. If there is only one camera this array has lenght 1.
        /// </summary>
        /// <returns>An array with the player cameras.</returns>
        public abstract Camera[] GetCameras();

        /// <summary>
        /// Requests the chaperone boundaries of the SteamVR play area.  This doesn't work if you haven't performed Room Setup.   
        /// </summary>
        /// <param name="p0">Chaperone point 0</param>
        /// <param name="p1">Chaperone point 1</param>
        /// <param name="p2">Chaperone point 2</param>
        /// <param name="p3">Chaperone point 3</param>
        /// <returns> True if it was able toget the play area bounds.</returns>
        public abstract bool GetPlayAreaBounds(out Vector3 p0, out Vector3 p1, out Vector3 p2, out Vector3 p3);

        /// <summary>
        /// <para>Returns the transform associated with the player HMD game object.</para>
        /// <para>When there is no Play Area, this transform might be equato to GetPlayerTransform.</para>
        /// </summary>
        /// <returns>The head transform.</returns>
        public abstract Transform GetHeadTransform();

        /// <summary>
        /// Returns the transform associated with the player game object. That would be the center of the playe area on platforms that have a player area;
        /// </summary>
        /// <returns>The player transform.</returns>
        public abstract Transform GetPlayerTransform();
    }
}