using DamasDenis.Deployment;
using System.Collections.Generic;
using UnityEngine;

namespace DamasDenis.Enemies
{
    /// <summary>
    /// AI Navigation & Spawning: Defines the enemy route and spawn point for a stage map.
    /// Place this in each stage scene to configure enemy pathing.
    /// </summary>
    public class EnemyPath : MonoBehaviour
    {

        [Header("Editor Validation")]
        [Tooltip("Scene view draws a route segment red when two consecutive points are farther apart than this (world units).")]
        [Min(0.1f)] [SerializeField] private float maxStepDistance = 1.1f;

        [Header("Path Configuration")]
        [Tooltip("The initial waypoint target for spawning enemies")]
        public Transform startPoint;

        [Tooltip("Ordered waypoint route for enemies")]
        public Transform[] path;

        public int PointCount => path != null ? path.Length : 0;

        public Vector3 GetPoint(int index)
        {
            MapCell cell = GetCell(index);
            return cell != null ? cell.SurfacePoint : path[index].position;
        }

        private MapCell[] cellCache;

        /// <summary>The MapCell at a route point, or null if that point isn't a cell.</summary>
        public MapCell GetCell(int index)
        {
            if (path == null || index < 0 || index >= path.Length || path[index] == null)
                return null;

            if (cellCache == null || cellCache.Length != path.Length)
                cellCache = new MapCell[path.Length];

            if (cellCache[index] == null)
                cellCache[index] = path[index].GetComponent<MapCell>();

            return cellCache[index];
        }

        public Vector3 SpawnPosition
        {
            get
            {
                if (path != null && path.Length > 0 && path[0] != null)
                    return GetPoint(0);

                if (startPoint != null)
                    return startPoint.position;

                return transform.position;
            }
        }

        private void OnDrawGizmos()
        {
            if (path == null) return;

            for (int i = 0; i < path.Length; i++)
            {
                if (path[i] == null) continue;

                Gizmos.color = Color.yellow;
                Gizmos.DrawSphere(path[i].position, 0.08f);

        #if UNITY_EDITOR
                UnityEditor.Handles.Label(path[i].position + Vector3.up * 0.2f, i.ToString());
        #endif

                if (i + 1 < path.Length && path[i + 1] != null)
                {
                    Vector3 a = path[i].position, b = path[i + 1].position;
                    a.y = 0f; b.y = 0f;
                    float gap = Vector3.Distance(a, b);
                    Gizmos.color = gap > maxStepDistance ? Color.red : Color.yellow;
                    Gizmos.DrawLine(path[i].position, path[i + 1].position);
                }
            }
        }
    }
}