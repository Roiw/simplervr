using UnityEngine;

namespace SimplerVR.Common
{
    /// <summary>
    /// Several commom physics methods.
    /// </summary>
    public static class Physics
    {
        /// <summary>
        /// Parabolic motion equation applied to 3 dimensions
        /// </summary>
        /// <param name="p0">Starting point of parabola.</param>
        /// <param name="v0">Initial parabola velocity.</param>
        /// <param name="a">Initial acceleration.</param>
        /// <param name="t">The time.</param>
        /// <returns></returns>
        public static Vector3 ParabolicCurve(Vector3 p0, Vector3 v0, Vector3 a, float t)
        {
            Vector3 ret = new Vector3();
            for (int x = 0; x < 3; x++)
                ret[x] = ParabolicCurve(p0[x], v0[x], a[x], t);
            return ret;
        }

        // -------------------------------------------------------------------------------------------------------------------------------

        /// <summary>
        /// Parabolic motion derivative applied to 3 dimensions.
        /// </summary>
        /// <param name="v0">Initial velocity.</param>
        /// <param name="a">Aceleration.</param>
        /// <param name="t">Time.</param>
        /// <returns></returns>
        public static Vector3 ParabolicCurveDeriv(Vector3 v0, Vector3 a, float t)
        {
            Vector3 ret = new Vector3();
            for (int x = 0; x < 3; x++)
                ret[x] = ParabolicCurveDeriv(v0[x], a[x], t);
            return ret;
        }

        // -------------------------------------------------------------------------------------------------------------------------------

        /// <summary>
        /// Parabolic motion equation, y = p0 + v0*t + 1/2at^2
        /// </summary>
        /// <param name="p0">Initial point.</param>
        /// <param name="v0">Initial velocity.</param>
        /// <param name="a">Aceleration.</param>
        /// <param name="t">Time.</param>
        /// <returns></returns>
        public static float ParabolicCurve(float p0, float v0, float a, float t)
        {
            return p0 + v0 * t + 0.5f * a * t * t;
        }

        // -------------------------------------------------------------------------------------------------------------------------------

        /// <summary>
        /// Derivative of parabolic motion equation
        /// </summary>
        /// <param name="v0">Initial velocity.</param>
        /// <param name="a">Aceleration.</param>
        /// <param name="t">Time.</param>
        /// <returns></returns>
        public static float ParabolicCurveDeriv(float v0, float a, float t)
        {
            return v0 + a * t;
        }
    }
}
