﻿using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.Rendering;
using System.Collections.Generic;
using SimplerVR.Features.ArcTeleport;


namespace SimplerVR.Features.ArcTeleport
{
    /// <summary>
    /// A version of Unity's baked navmesh that is converted to a (serializable) component.  This allows the navmesh 
    /// used for Vive navigation to be separated form the AI Navmesh.  ViveNavMesh also handles the rendering of the 
    /// NavMesh grid in-game.
    /// </summary>
    [ExecuteInEditMode]
    public class NavMeshRenderer : MonoBehaviour
    {
        //// <summary>
        /// A reference to the settings of the teleport.
        /// </summary>
        private ArcTeleportData settings;
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

        /// A Mesh that represents the "Selectable" area of the world.  This is converted from Unity's NavMesh in ViveNavMeshEditor
        public Mesh SelectableMesh
        {
            get { return TeleportSettings.SelectableMesh; }
            set { TeleportSettings.SelectableMesh = value; Cleanup(); } // Cleanup because we need to change the mesh inside command buffers
        }

        /// \brief The border points of SelectableMesh.  This is automatically generated in ViveNavMeshEditor.
        /// 
        /// This is an array of Vector3 arrays, where each Vector3 array is the points in a polyline.  These polylines combined
        /// describe the borders of SelectableMesh.  We have to use BorderPointSets instead of a jagged Vector3[][] array because
        /// Unity can't serialize jagged arrays for some reason.
        public BorderPointSet[] SelectableMeshBorder
        {
            get { return _SelectableMeshBorder; }
            set { _SelectableMeshBorder = value; Border.Points = _SelectableMeshBorder; }
        }
        [SerializeField]
        [HideInInspector]
        private BorderPointSet[] _SelectableMeshBorder;

        private BorderRenderer Border;

        private static Dictionary<Camera, CommandBuffer> cameras = new Dictionary<Camera, CommandBuffer>();

        void Start()
        {
            if (SelectableMesh == null)
                SelectableMesh = new Mesh();
            if (_SelectableMeshBorder == null)
                _SelectableMeshBorder = new BorderPointSet[0];

            Border = GetComponent<BorderRenderer>();
            Border.Points = SelectableMeshBorder;
#if UNITY_EDITOR
            UnityEditor.SceneView.RepaintAll();
#endif
        }

        private void Cleanup()
        {
            foreach (var cam in cameras)
            {
                if (cam.Key)
                {
                    cam.Key.RemoveCommandBuffer(CameraEvent.AfterForwardOpaque, cam.Value);
                }
            }
            cameras.Clear();
        }

        public void OnEnable()
        {
            Cleanup();
        }

        public void OnDisable()
        {
            Cleanup();
        }

        void OnRenderObject()
        {
            if (TeleportSettings.DisplayTeleportNavmesh)
            {
                // We have to use command buffers instead of Graphics.DrawMesh because of strange depth issues that I am experiencing
                // with Graphics.Drawmesh (perhaps Graphics.DrawMesh is called before all opaque objects are rendered?)
                var act = gameObject.activeInHierarchy && enabled;
                if (!act)
                {
                    Cleanup();
                    return;
                }

                // If _SelectableMesh == null there is a crash in Unity 5.4 beta (apparently you can't pass null to CommandBuffer::DrawMesh now).
                if (!TeleportSettings.SelectableMesh || !TeleportSettings.NavMeshRender)
                    return;

                var cam = Camera.current;
                if (!cam || cam.cameraType == CameraType.Preview || ((1 << gameObject.layer) & Camera.current.cullingMask) == 0)
                    return;

                CommandBuffer buf = null;
                if (cameras.ContainsKey(cam))
                    return;

                buf = new CommandBuffer();
                // Note: Mesh is drawn slightly pushed upwards to avoid z-fighting issues
                buf.DrawMesh(TeleportSettings.SelectableMesh, Matrix4x4.TRS(Vector3.up * 0.005f, Quaternion.identity, Vector3.one), TeleportSettings.NavMeshRender, 0, 0);
                cameras[cam] = buf;
                cam.AddCommandBuffer(CameraEvent.AfterForwardOpaque, buf);
            }
        }

        void OnValidate()
        {
            Border = GetComponent<BorderRenderer>();
            Border.Points = SelectableMeshBorder;
        }

        /// \brief Casts a ray against the Navmesh and attempts to calculate the ray's worldspace intersection with it.
        /// 
        /// This uses Physics raycasts to perform the raycast calculation, so the teleport surface must have a collider
        /// on it.
        /// 
        /// \param p1 First (origin) point of ray
        /// \param p2 Last (end) point of ray
        /// \param pointOnNavmesh If the raycast hit something on the navmesh.
        /// \param hitPoint If hit, the point of the hit.  Otherwise zero.
        /// 
        /// \return If the raycast hit something.
        public static bool Linecast(Vector3 p1, Vector3 p2, out bool pointOnNavmesh, out bool doSnapTeleport,
            out Vector3 hitPoint, out Vector3 hitPointObjectPosition, int CollisionLayer, int areaMask)
        {
            RaycastHit hit;
            Vector3 dir = p2 - p1;
            float dist = dir.magnitude;
            dir /= dist;
            doSnapTeleport = false;
            if (Physics.Raycast(p1, dir, out hit, dist, CollisionLayer))
            {
                /* If the object we are colliding with is a snapTeleport*/
                if (hit.collider.gameObject.layer == LayerMask.NameToLayer("SnapTeleport"))
                {
                    pointOnNavmesh = true; /* We should teleport to it.*/
                    hitPoint = hit.point;
                    hitPointObjectPosition = hit.collider.gameObject.transform.position;
                    doSnapTeleport = true;
                    return true;
                }

                if (Vector3.Dot(Vector3.up, hit.normal) < 0.99f)
                {
                    pointOnNavmesh = false;
                    hitPoint = hit.point;
                    hitPointObjectPosition = hit.collider.gameObject.transform.position;
                    return true;
                }

                hitPoint = hit.point;
                hitPointObjectPosition = hit.collider.gameObject.transform.position;
                UnityEngine.AI.NavMeshHit navHit;
                pointOnNavmesh = UnityEngine.AI.NavMesh.SamplePosition(hitPoint, out navHit, 0.5f, areaMask); // _NavAreaMask);

                // This is necessary because NavMesh.SamplePosition does a sphere intersection, not a projection onto the mesh or
                // something like that.  This means that in some scenarios you can have a point that's not actually on/above
                // the NavMesh but is right next to it.  However, if the point is above a Navmesh position that has a normal
                // of (0,1,0) we can assume that the closest position on the Navmesh to any point has the same x/z coordinates
                // UNLESS that point isn't on top of the Navmesh.
                if (!Mathf.Approximately(navHit.position.x, hitPoint.x) ||
                    !Mathf.Approximately(navHit.position.z, hitPoint.z))
                    pointOnNavmesh = false;

                return true;
            }
            pointOnNavmesh = false;
            hitPoint = Vector3.zero;
            hitPointObjectPosition = Vector3.zero;
            return false;
        }
    }

    [System.Serializable]
    public class BorderPointSet
    {
        public Vector3[] Points;

        public BorderPointSet(Vector3[] Points)
        {
            this.Points = Points;
        }
    }
}

    
    