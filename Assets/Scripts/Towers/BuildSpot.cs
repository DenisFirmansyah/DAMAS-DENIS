using UnityEngine;

public enum SpotType { Melee, Range }

public class BuildSpot : MonoBehaviour
{
    public SpotType type;
    
    public bool IsOccupied { get; set; } = false;
}
