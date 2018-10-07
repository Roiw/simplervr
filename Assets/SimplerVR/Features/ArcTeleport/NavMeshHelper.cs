﻿#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System;
using System.Collections.Generic;
using SimplerVR.Features.ArcTeleport;

namespace SimplerVR.Features.ArcTeleport
{
    /// <summary>
    /// This handles the conversion from Unity NavMesh to Mesh and performs some computational geometry to find the borders of the mesh.
    /// </summary>
    public static class NavMeshHelper
    {
        public static void ClearNavMesh(NavMeshRenderer mesh, SerializedObject serializedObject, ArcTeleportData teleportSettings)
        {
            //Undo.RecordObject(mesh, "Clear Navmesh Data");

            // Note: Unity does not serialize "null" correctly so we set everything to empty objects
            Mesh m = new Mesh();

            m = teleportSettings.SelectableMesh;

            //serializedObject.Update();
            //SerializedProperty p_mesh = serializedObject.FindProperty("_SelectableMesh");
            //p_mesh.objectReferenceValue = m;
            //serializedObject.ApplyModifiedPropertiesWithoutUndo();
            mesh.SelectableMesh = mesh.SelectableMesh; // Make sure setter is called

            mesh.SelectableMeshBorder = new BorderPointSet[0];
        }

        public static void UpdateNavMesh(NavMeshRenderer mesh, SerializedObject serializedObject, ArcTeleportData teleportSettings)
        {
            UnityEngine.AI.NavMeshTriangulation tri = UnityEngine.AI.NavMesh.CalculateTriangulation();
            int vert_size, tri_size;

            CullNavmeshTriangulation(ref tri, teleportSettings.NavAreaMask, out vert_size, out tri_size);

            Mesh m = ConvertNavmeshToMesh(tri, vert_size, tri_size);
            // Can't use SerializedProperties here because BorderPointSet doesn't derive from UnityEngine.Object
            mesh.SelectableMeshBorder = FindBorderEdges(m);

            teleportSettings.SelectableMesh = m;
            mesh.SelectableMesh = mesh.SelectableMesh; // Make sure that setter is called
        }

        /// <summary>
        /// Modifies the given NavMesh so that only the Navigation areas are present in the mesh.  This is done only 
        /// by swapping, so that no new memory is allocated.
        /// </summary>
        /// <param name="navMesh">NavMesh data to modify</param>
        /// <param name="area">Area mask to include in returned mesh.  Areas outside of this mask are culled.</param>
        /// <param name="vert_size">New size of navMesh.vertices</param>
        /// <param name="tri_size">New size of navMesh.areas and one third of the size of navMesh.indices</param>
        public static void CullNavmeshTriangulation(ref UnityEngine.AI.NavMeshTriangulation navMesh, int area, out int vert_size, out int tri_size)
        {
            // Step 1: re-order triangles so that valid areas are in front.  Then determine tri_size.
            tri_size = navMesh.indices.Length / 3;
            for (int i = 0; i < tri_size; i++)
            {
                Vector3 p1 = navMesh.vertices[navMesh.indices[i * 3]];
                Vector3 p2 = navMesh.vertices[navMesh.indices[i * 3 + 1]];
                Vector3 p3 = navMesh.vertices[navMesh.indices[i * 3 + 2]];
                Plane p = new Plane(p1, p2, p3);
                bool vertical = Mathf.Abs(Vector3.Dot(p.normal, Vector3.up)) > 0.99f;

                // If the current triangle isn't flat (normal is up) or if it doesn't match
                // with the provided mask, we should cull it.
                if (((1 << navMesh.areas[i]) & area) == 0 || !vertical) // If true this triangle should be culled.
                {
                    // Swap area indices and triangle indices with the end of the array
                    int t_ind = tri_size - 1;

                    int t_area = navMesh.areas[t_ind];
                    navMesh.areas[t_ind] = navMesh.areas[i];
                    navMesh.areas[i] = t_area;

                    for (int j = 0; j < 3; j++)
                    {
                        int t_v = navMesh.indices[t_ind * 3 + j];
                        navMesh.indices[t_ind * 3 + j] = navMesh.indices[i * 3 + j];
                        navMesh.indices[i * 3 + j] = t_v;
                    }

                    // Then reduce the size of the array, effectively cutting off the previous triangle
                    tri_size--;
                    // Stay on the same index so that we can check the triangle we just swapped.
                    i--;
                }
            }

            // Step 2: Cull the vertices that aren't used.
            vert_size = 0;
            for (int i = 0; i < tri_size * 3; i++)
            {
                int prv = navMesh.indices[i];
                if (prv >= vert_size)
                {
                    int nxt = vert_size;

                    // Bring the current vertex to the end of the "active" array by swapping it with what's currently there
                    Vector3 t_v = navMesh.vertices[prv];
                    navMesh.vertices[prv] = navMesh.vertices[nxt];
                    navMesh.vertices[nxt] = t_v;

                    // Now change around the values in the triangle indices to reflect the swap
                    for (int j = i; j < tri_size * 3; j++)
                    {
                        if (navMesh.indices[j] == prv)
                            navMesh.indices[j] = nxt;
                        else if (navMesh.indices[j] == nxt)
                            navMesh.indices[j] = prv;
                    }

                    // Increase the size of the vertex array to reflect the changes.
                    vert_size++;
                }
            }
        }

        /// <summary>
        /// Converts a NavMesh (or a NavMesh area) into a standard Unity mesh.  This is later used
        /// to render the mesh on-screen using Unity's standard rendering tools.
        /// </summary>
        /// <param name="navMesh">Precalculated Nav Mesh Triangulation</param>
        /// <param name="vert_size">Size of vertex array</param>
        /// <param name="tri_size">Size of triangle array</param>
        /// <returns></returns>
        public static Mesh ConvertNavmeshToMesh(UnityEngine.AI.NavMeshTriangulation navMesh, int vert_size, int tri_size)
        {
            Mesh ret = new Mesh();

            if (vert_size >= 65535)
            {
                Debug.LogError("Playable NavMesh too big (vertex count >= 65535)!  Limit the size of the playable area using" +
                    "Area Masks.  For now no preview mesh will render.");
                return ret;
            }

            Vector3[] vertices = new Vector3[vert_size];
            for (int x = 0; x < vertices.Length; x++)
                // Note: Unity navmesh is offset 0.05m from the ground.  This pushes it down to 0
                vertices[x] = navMesh.vertices[x];

            int[] triangles = new int[tri_size * 3];
            for (int x = 0; x < triangles.Length; x++)
                triangles[x] = navMesh.indices[x];

            ret.name = "Navmesh";
            ret.vertices = vertices;
            ret.triangles = triangles;

            RemoveMeshDuplicates(ret);

            ret.RecalculateNormals();
            ret.RecalculateBounds();

            return ret;
        }

        /// <summary>
        /// Naive implementation of removing duplicate vertices in a mesh.  O(n^2).
        /// This is necessary because Unity NavMeshes for some reason have a whole bunch of duplicate vertices (or vertices
        /// that are very close together).  So some processing needs to be done go get rid of these.
        /// 
        /// If this becomes an actual performance hog, consider changing this to sort the vertices first using a more
        /// optimized process O(n lg n) then removing adjacent duplicates.
        /// </summary>
        /// <param name="m">Mesh to remove duplicates from</param>
        private static void RemoveMeshDuplicates(Mesh m)
        {
            Vector3[] verts = new Vector3[m.vertices.Length];
            for (int x = 0; x < verts.Length; x++)
                verts[x] = m.vertices[x];

            int[] elts = new int[m.triangles.Length];
            for (int x = 0; x < elts.Length; x++)
                elts[x] = m.triangles[x];

            int size = verts.Length;
            for (int x = 0; x < size; x++)
            {
                for (int y = x + 1; y < size; y++)
                {
                    Vector3 d = verts[x] - verts[y];
                    d.x = Mathf.Abs(d.x);
                    d.y = Mathf.Abs(d.y);
                    d.z = Mathf.Abs(d.z);
                    if (x != y && d.x < 0.05f && d.y < 0.05f && d.z < 0.05f)
                    {
                        verts[y] = verts[size - 1];
                        for (int z = 0; z < elts.Length; z++)
                        {
                            if (elts[z] == y)
                                elts[z] = x;

                            if (elts[z] == size - 1)
                                elts[z] = y;
                        }
                        size--;
                        y--;
                    }
                }
            }

            Array.Resize<Vector3>(ref verts, size);
            m.Clear();
            m.vertices = verts;
            m.triangles = elts;
        }

        /// <summary>
        /// Given some mesh m, calculates a number of polylines that border the mesh.  This may return more than
        /// one polyline if, for example, the mesh has holes in it or if the mesh is separated in two pieces.
        /// </summary>
        /// <param name="m">Input mesh</param>
        /// <returns>Array of cyclic polylines</returns>
        public static BorderPointSet[] FindBorderEdges(Mesh m)
        {
            // First, get together all the edges in the mesh and find out
            // how many times each edge is used.  Edges that are only used
            // once are border edges.

            // Key: edges (note that because of how the hashcode / equals() is set up, two equivalent edges will effectively
            //      be equal)
            // Value: How many times this edge shows up in the mesh.  Any keys with a value of 1 are border edges.
            Dictionary<Edge, int> edges = new Dictionary<Edge, int>();
            for (int x = 0; x < m.triangles.Length / 3; x++)
            {
                int p1 = m.triangles[x * 3];
                int p2 = m.triangles[x * 3 + 1];
                int p3 = m.triangles[x * 3 + 2];

                Edge[] e = new Edge[3];
                e[0] = new Edge(p1, p2);
                e[1] = new Edge(p2, p3);
                e[2] = new Edge(p3, p1);

                foreach (Edge d in e)
                {
                    int curval;
                    edges.TryGetValue(d, out curval); // 0 if nonexistant
                    edges[d] = curval + 1;
                }
            }

            // Next, consolidate all of the border edges into one List<Edge>
            List<Edge> border = new List<Edge>();
            foreach (KeyValuePair<Edge, int> p in edges)
            {
                if (p.Value == 1) // border edge == edge only used once.
                    border.Add(p.Key);
            }

            // Perform the following routine:
            // 1. Pick any unvisited edge segment [v_start,v_next] and add these vertices to the polygon loop.
            // 2. Find the unvisited edge segment [v_i,v_j] that has either v_i = v_next or v_j = v_next and add the 
            //    other vertex (the one not equal to v_next) to the polygon loop. Reset v_next as this newly added vertex, 
            //    mark the edge as visited and continue from 2.
            // 3. Traversal is done when we get back to v_start.
            // Source: http://stackoverflow.com/questions/14108553/get-border-edges-of-mesh-in-winding-order
            bool[] visited = new bool[border.Count];
            bool finished = false;
            int cur_index = 0;

            List<Vector3[]> ret = new List<Vector3[]>();

            while (!finished)
            {
                int[] raw = FindPolylineFromEdges(cur_index, visited, border);
                Vector3[] fmt = new Vector3[raw.Length];
                for (int x = 0; x < raw.Length; x++)
                    fmt[x] = m.vertices[raw[x]];
                ret.Add(fmt);

                finished = true;
                for (int x = 0; x < visited.Length; x++)
                {
                    if (!visited[x])
                    {
                        cur_index = x;
                        finished = false;
                        break;
                    }
                }
            }

            BorderPointSet[] ret_set = new BorderPointSet[ret.Count];
            for (int x = 0; x < ret.Count; x++)
            {
                ret_set[x] = new BorderPointSet(ret[x]);
            }
            return ret_set;
        }

        /// <summary>
        /// Given a list of edges, finds a polyline connected to the edge at index start.
        /// Guaranteed to run in O(n) time.  Assumes that each edge only has two neighbor edges.
        /// </summary>
        /// <param name="start">Starting index of edge</param>
        /// <param name="visited">Total of visited edges (perhaps from previous calls)</param>
        /// <param name="edges">List of edges</param>
        /// <returns></returns>
        private static int[] FindPolylineFromEdges(int start, bool[] visited, List<Edge> edges)
        {
            List<int> loop = new List<int>(edges.Count);
            loop.Add(edges[start].min);
            loop.Add(edges[start].max);
            visited[start] = true;

            // With each iteration of this while loop, we look for an edge that connects to the previous one
            // but hasn't been processed yet (to prevent simply finding the previous edge again, and to prevent
            // a hang if faulty data is given).
            while (loop[loop.Count - 1] != edges[start].min)
            {
                int cur = loop[loop.Count - 1];
                bool found = false;
                for (int x = 0; x < visited.Length; x++)
                {
                    // If we have visited this edge before, or both vertices on this edge
                    // aren't connected to cur, then skip the edge
                    if (!visited[x] && (edges[x].min == cur || edges[x].max == cur))
                    {
                        // The next vertex in the loop
                        int next = edges[x].min == cur ? edges[x].max : edges[x].min;
                        loop.Add(next);

                        // Mark the edge as visited and continue the outermost loop
                        visited[x] = true;
                        found = true;
                        break;
                    }
                }
                if (!found)// acyclic, so break.
                    break;
            }

            int[] ret = new int[loop.Count];
            loop.CopyTo(ret);
            return ret;
        }

        /// <summary>
        /// Represents an Edge.
        /// </summary>
        private struct Edge
        {
            public int min
            {
                get; private set;
            }
            public int max
            {
                get; private set;
            }

            public Edge(int p1, int p2)
            {
                // This is done so that if p1 and p2 are switched, the edge has the same hash
                min = p1 < p2 ? p1 : p2;
                max = p1 >= p2 ? p1 : p2;
            }

            public override bool Equals(object obj)
            {
                if (!(obj is Edge))
                    return false;

                Edge e = (Edge)obj;
                return e.min == min && e.max == max;
            }

            public override int GetHashCode()
            {
                // Small note: this breaks when you have more than 65535 vertices.
                return (min << 16) + max;
            }
        }

    }
}
#endif

