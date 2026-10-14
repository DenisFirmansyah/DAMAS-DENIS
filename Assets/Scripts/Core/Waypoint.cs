using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Core.Waypoint
{
   public class Waypoint : MonoBehaviour
    {
        // Drag the NEXT point(s) here in the Inspector.
        // If it's a straight line, drag 1 point.
        // If it's a branch, drag 2 or 3 points.
        public List<Waypoint> nextPoints; 

        public Waypoint GetNextStep()
        {
            if (nextPoints.Count == 0) return null; // End of the line

            // UNIFORM RANDOM: Each point has an equal "range" of probability.
            int randomIndex = Random.Range(0, nextPoints.Count);
            return nextPoints[randomIndex];
        }
    } 
}
