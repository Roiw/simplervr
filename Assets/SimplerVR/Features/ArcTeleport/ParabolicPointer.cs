using UnityEngine;
using System.Collections.Generic;
using SimplerVR.Features.ArcTeleport;

namespace SimplerVR.Features.ArcTeleport
{

    public class ParabolicPointer : MonoBehaviour
    {

        public Vector3 SelectedPoint { get; private set; }
        public bool PointOnNavMesh { get; private set; }
        public float CurrentParabolaAngle { get; private set; }

        //// <summary>
        /// A reference to the settings of the teleport.
        /// </summary>
        public  ArcTeleportData settings;
        private ArcTeleportData TeleportSettings
        {
            get
            {
                if (settings != null)
                    return settings;

                settings = ArcTeleportData.LoadArcTeleportSettings();
                return settings;
            }
        }


        private Mesh ParabolaMesh;

        // Parabolic motion equation, y = p0 + v0*t + 1/2at^2
        private static float ParabolicCurve(float p0, float v0, float a, float t)
        {
            return p0 + v0 * t + 0.5f * a * t * t;
        }

        // Derivative of parabolic motion equation
        private static float ParabolicCurveDeriv(float v0, float a, float t)
        {
            return v0 + a * t;
        }

        // Parabolic motion equation applied to 3 dimensions
        private static Vector3 ParabolicCurve(Vector3 p0, Vector3 v0, Vector3 a, float t)
        {
            Vector3 ret = new Vector3();
            for (int x = 0; x < 3; x++)
                ret[x] = ParabolicCurve(p0[x], v0[x], a[x], t);
            return ret;
        }

        // Parabolic motion derivative applied to 3 dimensions
        private static Vector3 ParabolicCurveDeriv(Vector3 v0, Vector3 a, float t)
        {
            Vector3 ret = new Vector3();
            for (int x = 0; x < 3; x++)
                ret[x] = ParabolicCurveDeriv(v0[x], a[x], t);
            return ret;
        }

        // Sample a bunch of points along a parabolic curve until you hit gnd.  At that point, cut off the parabola
        // p0: starting point of parabola
        // v0: initial parabola velocity
        // a: initial acceleration
        // dist: distance between sample points
        // points: number of sample points
        // gnd: height of the ground, in meters above y=0
        // outPts: List that will be populated by new points
        private static bool CalculateParabolicCurve(Vector3 p0, Vector3 v0, Vector3 a, float dist, int points, int areaMask,
            List<Vector3> outPts, int collisionLayers)
        {
            outPts.Clear();
            outPts.Add(p0);

            Vector3 last = p0;
            float t = 0;

            for (int i = 0; i < points; i++)
            {
                t += dist / ParabolicCurveDeriv(v0, a, t).magnitude;
                Vector3 next = ParabolicCurve(p0, v0, a, t);

                Vector3 castHit;
                Vector3 hitParentPosition;
                bool endOnNavmesh;
                bool doSnapTeleport;
                bool cast = NavMeshRenderer.Linecast(last, next, out endOnNavmesh, out doSnapTeleport,
                    out castHit, out hitParentPosition, collisionLayers, areaMask);

                if (cast)
                {
                    outPts.Add(castHit);
                    if (doSnapTeleport)
                    {
                        outPts.Clear();
                        outPts.AddRange(CalculateParabolicSnap(p0, hitParentPosition, v0, a, dist, points));
                    }
                    return endOnNavmesh;
                }
                else
                    outPts.Add(next);

                last = next;
            }


            return false;
        }

        // Sample a bunch of points along a parabolic curve until you hit snapPoint.  At that point, cut off the parabola
        // s0: starting point of parabola
        // s: final parabole point
        // a: initial acceleration
        // dist: distance between sample points
        // points: number of sample points
        // gnd: height of the ground, in meters above y=0
        // outPts: List that will be populated by new points
        private static List<Vector3> CalculateParabolicSnap(Vector3 s0, Vector3 s, Vector3 v0, Vector3 a, float dist, int points)
        {
            List<Vector3> pointList = new List<Vector3>();
            float ti = 0; // Iteration time..
            float t; // Final Time..

            /* Start by adding the first point to the list. */
            pointList.Add(s0);

            /* Calcuate intial parabolic vertical velocity */
            //v0.y = Mathf.Sqrt((Mathf.Abs(s.y - s0.y) * a.y) / -4);

            /* Calculate the final parabolic time. */
            float delta = Mathf.Pow(v0.y, 2.0f) - 4.0f * 0.5f * a.y * (s0.y - s.y);
            float t1 = (-v0.y + Mathf.Sqrt(delta)) / (2.0f * 0.5f * a.y);
            float t2 = (-v0.y - Mathf.Sqrt(delta)) / (2.0f * 0.5f * a.y);
            t = t1 > t2 ? t1 : t2;

            /* Calculate the horizontals speeds. */
            v0.x = (s.x - s0.x) / t;
            v0.z = (s.z - s0.z) / t;

            for (int i = 0; i < points; i++)
            {
                ti += dist / ParabolicCurveDeriv(v0, a, ti).magnitude;
                if (ti >= t)
                {
                    pointList.Add(s);
                    return pointList;
                }
                pointList.Add(ParabolicCurve(s0, v0, a, ti));

            }
            return pointList;
        }

        private static Vector3 ProjectVectorOntoPlane(Vector3 planeNormal, Vector3 point)
        {
            Vector3 d = Vector3.Project(point, planeNormal.normalized);
            return point - d;
        }

        private void GenerateMesh(ref Mesh m, List<Vector3> points, Vector3 fwd, float uvoffset)
        {
            Vector3[] verts = new Vector3[points.Count * 2];
            Vector2[] uv = new Vector2[points.Count * 2];

            Vector3 right = Vector3.Cross(fwd, Vector3.up).normalized;

            for (int x = 0; x < points.Count; x++)
            {
                verts[2 * x] = points[x] - right * TeleportSettings.GraphicThickness / 2;
                verts[2 * x + 1] = points[x] + right * TeleportSettings.GraphicThickness / 2;

                float uvoffset_mod = uvoffset;
                if (x == points.Count - 1 && x > 1)
                {
                    float dist_last = (points[x - 2] - points[x - 1]).magnitude;
                    float dist_cur = (points[x] - points[x - 1]).magnitude;
                    uvoffset_mod += 1 - dist_cur / dist_last;
                }

                uv[2 * x] = new Vector2(0, x - uvoffset_mod);
                uv[2 * x + 1] = new Vector2(1, x - uvoffset_mod);
            }

            int[] indices = new int[2 * 3 * (verts.Length - 2)];
            for (int x = 0; x < verts.Length / 2 - 1; x++)
            {
                int p1 = 2 * x;
                int p2 = 2 * x + 1;
                int p3 = 2 * x + 2;
                int p4 = 2 * x + 3;

                indices[12 * x] = p1;
                indices[12 * x + 1] = p2;
                indices[12 * x + 2] = p3;
                indices[12 * x + 3] = p3;
                indices[12 * x + 4] = p2;
                indices[12 * x + 5] = p4;

                indices[12 * x + 6] = p3;
                indices[12 * x + 7] = p2;
                indices[12 * x + 8] = p1;
                indices[12 * x + 9] = p4;
                indices[12 * x + 10] = p2;
                indices[12 * x + 11] = p3;
            }

            m.Clear();
            m.vertices = verts;
            m.uv = uv;
            m.triangles = indices;
            m.RecalculateBounds();
            m.RecalculateNormals();
        }

        void Start()
        {
            ParabolaPoints = new List<Vector3>(TeleportSettings.PointCount);

            ParabolaMesh = new Mesh();
            ParabolaMesh.MarkDynamic();
            ParabolaMesh.name = "Parabolic Pointer";
            ParabolaMesh.vertices = new Vector3[0];
            ParabolaMesh.triangles = new int[0];
        }

        private List<Vector3> ParabolaPoints;

        void Update()
        {
            // 1. Calculate Parabola Points
            Vector3 velocity = transform.TransformDirection(TeleportSettings.InitialVelocity);
            Vector3 velocity_normalized;
            CurrentParabolaAngle = ClampInitialVelocity(ref velocity, out velocity_normalized);

            PointOnNavMesh = CalculateParabolicCurve(
                transform.position,
                velocity,
                TeleportSettings.Acceleration, TeleportSettings.PointSpacing, TeleportSettings.PointCount,
                TeleportSettings.NavAreaMask,
                ParabolaPoints, TeleportSettings.CollisionLayers.value);

            //Debug.Log("Parabolic Curve:" + PointOnNavMesh.ToString());

            SelectedPoint = ParabolaPoints[ParabolaPoints.Count - 1];

            // 2. Render Parabola graphics
            // Make sure that there is actually a point on the navmesh, and that all requisite art is available
            bool ShouldDrawMarker = PointOnNavMesh && TeleportSettings.SelectionPadMesh != null
                && TeleportSettings.SelectionPadFadeMaterial != null && TeleportSettings.SelectionPadBottomMaterial != null &&
                TeleportSettings.GoodSelectionPadCircle != null;

            if (ShouldDrawMarker)
            {
                // Draw Inside of Selection pad
                Graphics.DrawMesh(TeleportSettings.SelectionPadMesh, Matrix4x4.TRS(SelectedPoint + Vector3.up * 0.005f, Quaternion.identity, Vector3.one * 0.2f), TeleportSettings.SelectionPadFadeMaterial, gameObject.layer, null, 3);
                // Draw Bottom of selection pad
                Graphics.DrawMesh(TeleportSettings.SelectionPadMesh, Matrix4x4.TRS(SelectedPoint + Vector3.up * 0.005f, Quaternion.identity, Vector3.one * 0.2f), TeleportSettings.GoodSelectionPadCircle, gameObject.layer, null, 1);
                // Draw Bottom of selection pad
                Graphics.DrawMesh(TeleportSettings.SelectionPadMesh, Matrix4x4.TRS(SelectedPoint + Vector3.up * 0.005f, Quaternion.identity, Vector3.one * 0.2f), TeleportSettings.SelectionPadBottomMaterial, gameObject.layer, null, 2);
            }
            else
            {
                // Draw Bottom of selection pad
                Graphics.DrawMesh(TeleportSettings.SelectionPadMesh, Matrix4x4.TRS(SelectedPoint + Vector3.up * 0.005f, Quaternion.identity, Vector3.one * 0.2f), TeleportSettings.BadSelectionPadCircle, gameObject.layer, null, 1);
            }

            // Draw parabola (BEFORE the outside faces of the selection pad, to avoid depth issues)
            GenerateMesh(ref ParabolaMesh, ParabolaPoints, velocity, Time.time % 1);

            if (ShouldDrawMarker)
            {
                Graphics.DrawMesh(TeleportSettings.SelectionPadMesh, Matrix4x4.TRS(SelectedPoint + Vector3.up * 0.005f, Quaternion.identity, Vector3.one * 0.2f), TeleportSettings.SelectionPadFadeMaterial, gameObject.layer, null, 0);
                // Draw outside faces of selection pad AFTER parabola (it is drawn on top)
                Graphics.DrawMesh(ParabolaMesh, Matrix4x4.identity, TeleportSettings.GoodGraphicMaterial, gameObject.layer);
            }
            else
            {
                // Draw outside faces of selection pad AFTER parabola (it is drawn on top)
                Graphics.DrawMesh(ParabolaMesh, Matrix4x4.identity, TeleportSettings.BadGraphicMaterial, gameObject.layer);
            }
        }

        // Used when you can't depend on Update() to automatically update CurrentParabolaAngle
        // (for example, directly after enabling the component)
        public void ForceUpdateCurrentAngle()
        {
            Vector3 velocity = transform.TransformDirection(TeleportSettings.InitialVelocity);
            Vector3 d;
            CurrentParabolaAngle = ClampInitialVelocity(ref velocity, out d);
        }

        // Clamps the given velocity vector so that it can't be more than 45 degrees above the horizontal.
        // This is done so that it is easier to leverage the maximum distance (at the 45 degree angle) of
        // parabolic motion.
        //
        // Returns angle with reference to the XZ plane
        private float ClampInitialVelocity(ref Vector3 velocity, out Vector3 velocity_normalized)
        {
            // Project the initial velocity onto the XZ plane.  This gives us the "forward" direction
            Vector3 velocity_fwd = ProjectVectorOntoPlane(Vector3.up, velocity);

            // Find the angle between the XZ plane and the velocity
            float angle = Vector3.Angle(velocity_fwd, velocity);
            // Calculate positivity/negativity of the angle using the cross product
            // Below is "right" from controller's perspective (could also be left, but it doesn't matter for our purposes)
            Vector3 right = Vector3.Cross(Vector3.up, velocity_fwd);
            // If the cross product between forward and the velocity is in the same direction as right, then we are below the vertical
            if (Vector3.Dot(right, Vector3.Cross(velocity_fwd, velocity)) > 0)
                angle *= -1;

            // Clamp the angle if it is greater than 45 degrees
            if (angle > 45)
            {
                velocity = Vector3.Slerp(velocity_fwd, velocity, 45f / angle);
                velocity /= velocity.magnitude;
                velocity_normalized = velocity;
                velocity *= TeleportSettings.InitialVelocity.magnitude;
                angle = 45;
            }
            else
                velocity_normalized = velocity.normalized;

            return angle;
        }

#if UNITY_EDITOR
        private List<Vector3> ParabolaPoints_Gizmo;

        void OnDrawGizmos()
        {
            if (Application.isPlaying) // Otherwise the parabola can show in the game view
                return;

            if (ParabolaPoints_Gizmo == null)
                ParabolaPoints_Gizmo = new List<Vector3>(TeleportSettings.PointCount);

            Vector3 velocity = transform.TransformDirection(TeleportSettings.InitialVelocity);
            Vector3 velocity_normalized;
            CurrentParabolaAngle = ClampInitialVelocity(ref velocity, out velocity_normalized);

            bool didHit = CalculateParabolicCurve(
                transform.position,
                velocity,
                TeleportSettings.Acceleration, TeleportSettings.PointSpacing, TeleportSettings.PointCount,
                TeleportSettings.NavAreaMask,
                ParabolaPoints_Gizmo,
                TeleportSettings.CollisionLayers.value);

            Gizmos.color = Color.blue;
            for (int x = 0; x < ParabolaPoints_Gizmo.Count - 1; x++)
                Gizmos.DrawLine(ParabolaPoints_Gizmo[x], ParabolaPoints_Gizmo[x + 1]);
            Gizmos.color = Color.green;

            if (didHit)
                Gizmos.DrawSphere(ParabolaPoints_Gizmo[ParabolaPoints_Gizmo.Count - 1], 0.2f);
        }
#endif
    }

}