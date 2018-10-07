using UnityEngine;
using System.Collections;
using SimplerVR.Features.ArcTeleport;


namespace SimplerVR.Features.ArcTeleport
{
    /// <summary>
    /// A generic component that renders a border using the given polylines.  The borders are double sided and are oriented
    /// upwards(ie normals are parallel to the XZ plane).
    /// </summary>
    [ExecuteInEditMode]
    public class BorderRenderer : MonoBehaviour
    {
        private Mesh[] CachedMeshes;
        /// Material used to render the border mesh.  Note: UVs are set up so that v=0->bottom and v=1->top of border
        private Material lastBorderMaterial;

        [System.NonSerialized]
        public Matrix4x4 Transpose = Matrix4x4.identity;

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

        /// Polylines that will be drawn.
        public BorderPointSet[] Points
        {
            get
            {
                return _Points;
            }
            set
            {
                _Points = value;
                RegenerateMesh();
            }
        }
        private BorderPointSet[] _Points;

        private float lastBorderHeight;

        /// <summary>
        /// Updates the border render after inspector updates.
        /// </summary>
        void Update()
        {
            if (CachedMeshes == null)
                return;


            if (TeleportSettings.BorderRenderHeight != lastBorderHeight
                || TeleportSettings.BorderRenderMaterial != lastBorderMaterial)
            {
                RegenerateMesh();

                if (TeleportSettings.BorderRenderMaterial != null)
                {
                    lastBorderMaterial = TeleportSettings.BorderRenderMaterial;
                }
                lastBorderHeight = TeleportSettings.BorderRenderHeight;
                RegenerateMesh();
            }

            if (TeleportSettings.DisplayTeleportNavmesh)
            {
                /// Draws the mesh
                foreach (Mesh m in CachedMeshes)
                    Graphics.DrawMesh(m, Transpose, lastBorderMaterial, 0);
            }
        }

        /// <summary>
        /// Regenerates the border mesh.
        /// </summary>
        public void RegenerateMesh()
        {
            if (Points == null)
            {
                CachedMeshes = new Mesh[0];
                return;
            }
            CachedMeshes = new Mesh[Points.Length];
            for (int x = 0; x < CachedMeshes.Length; x++)
            {
                if (Points[x] == null || Points[x].Points == null)
                    CachedMeshes[x] = new Mesh();
                else
                    CachedMeshes[x] = GenerateMeshForPoints(Points[x].Points);
            }
        }

        /// <summary>
        /// Generete a mesh based on points.
        /// </summary>
        /// <param name="Points">A list of points.</param>
        /// <returns>A mesh</returns>
        private Mesh GenerateMeshForPoints(Vector3[] Points)
        {
            if (Points.Length <= 1)
                return new Mesh();

            Vector3[] verts = new Vector3[Points.Length * 2];
            Vector2[] uv = new Vector2[Points.Length * 2];
            for (int x = 0; x < Points.Length; x++)
            {
                verts[2 * x] = Points[x];
                verts[2 * x + 1] = Points[x] + Vector3.up * lastBorderHeight;

                uv[2 * x] = new Vector2(x % 2, 0);
                uv[2 * x + 1] = new Vector2(x % 2, 1);
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

            Mesh m = new Mesh();
            m.vertices = verts;
            m.uv = uv;
            m.triangles = indices;
            m.RecalculateBounds();
            m.RecalculateNormals();
            return m;
        }
    }

}
