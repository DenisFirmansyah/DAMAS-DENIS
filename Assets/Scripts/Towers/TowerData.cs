using UnityEngine;

[CreateAssetMenu(fileName = "NewTowerData", menuName = "TD/Tower Data")]
public class TowerData : ScriptableObject {
    [Header("General Info")]
    public string towerName;
    
    [Header("Visuals")]
    public GameObject towerPrefab;
    public GameObject projectilePrefab;

    [Header("Economy")]
    public int cost;

    [Header("Combat Stats")]
    public float range;
    public float attackSpeed;
    public float projectileSpeed;
    public float damage;
    public float maxHealth; 
    public SpotType type;
    public bool requiresRotation;
}