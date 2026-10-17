using System.Collections.Generic;
using UnityEngine;

namespace DamasDenis.Deployment
{
    /// <summary>
    /// Designer-authored list of every staff unit the game offers.
    /// Stands in for Arknights-style squad selection: the unit bar shows
    /// whatever this roster says is unlocked. Swap it for a player-built
    /// squad later without changing the bar's UI code.
    /// </summary>
    [CreateAssetMenu(fileName = "StaffRoster", menuName = "DAMAS DENIS/Staff Roster")]
    public sealed class StaffRoster : ScriptableObject
    {
        [SerializeField] private List<UnitDeploymentData> units = new List<UnitDeploymentData>();

        /// <summary>All units in the roster, locked or not.</summary>
        public IReadOnlyList<UnitDeploymentData> Units => units;

        /// <summary>Returns only units the player has unlocked.</summary>
        public List<UnitDeploymentData> GetUnlocked()
        {
            var result = new List<UnitDeploymentData>();
            foreach (var unit in units)
            {
                if (unit != null && unit.IsUnlocked)
                    result.Add(unit);
            }
            return result;
        }
    }
}