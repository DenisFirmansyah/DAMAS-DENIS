using UnityEngine;

public enum EnemyType {
    Easy,
    Medium,
    Hard,
    Boss
}

[CreateAssetMenu(fileName = "NewEnemyData", menuName = "TD/Enemy Data")]
public class EnemyData : ScriptableObject {
    [Header("General Info")]
    public string enemyName;
    [TextArea] public string description;
    
    [Header("Visuals")]
    public GameObject enemyPrefab;

    [Header("Stats")]
    public float maxHealth;
    public float damage;
    public float speed;
    public int reward;
    public EnemyType type;
}