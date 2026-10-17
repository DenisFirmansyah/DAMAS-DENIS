using System.Collections.Generic;
using Core.Waypoint;
using UnityEngine;

namespace Enemies
{
    /// <summary>
    /// AI Navigation & Spawning: Defines the enemy route and spawn point for a stage map.
    /// Place this in each stage scene to configure enemy pathing.
    /// </summary>
    public class EnemyPath : MonoBehaviour
    {
        public static EnemyPath Instance { get; private set; }
        public static EnemyPath main => Instance;

        [Header("Path Configuration")]
        [Tooltip("The initial waypoint target for spawning enemies")]
        public Transform startPoint;

        [Tooltip("Ordered waypoint route for enemies")]
        public Transform[] path;

        public Vector3 SpawnPosition
        {
            get
            {
                if (path != null && path.Length > 0 && path[0] != null)
                    return path[0].position;

                if (startPoint != null)
                    return startPoint.position;

                return transform.position;
            }
        }

        public Waypoint StartWaypoint
        {
            get
            {
                if (startPoint != null)
                {
                    Waypoint wp = startPoint.GetComponent<Waypoint>();
                    if (wp != null) return wp;
                }

                if (path != null && path.Length > 0 && path[0] != null)
                {
                    Waypoint wp = path[0].GetComponent<Waypoint>();
                    if (wp != null) return wp;
                }

                return null;
            }
        }

        private void Awake()
        {
            Instance = this;
        }

        private void OnDestroy()
        {
            if (Instance == this)
                Instance = null;
        }
    }
}

/// <summary>
/// Backward-compatibility alias for legacy LevelManager.
/// </summary>
public class LevelManager : Enemies.EnemyPath
{
}
