using UnityEngine;

public enum EnemyType {
    Easy,
    Medium,
    Hard,
    Boss
}

[CreateAssetMenu(fileName = "NewEnemyData", menuName = "DAMAS DENIS/Enemy Data")]
public class EnemyData : ScriptableObject {
    [Header("General Info")]
    public string enemyName;
    [TextArea] public string description;
    
    [Header("Combat")]
    [Tooltip("If false (e.g. a flyer), this enemy ignores blockers and walks past them.")]
    public bool canBeBlocked = true;
    [Tooltip("Damage dealt to the unit blocking this enemy, per hit.")]
    public float attackDamage = 5f;
    [Tooltip("Seconds between hits on the blocking unit.")]
    [Min(0.05f)] public float attackInterval = 1f;
    
    [Header("Visuals")]
    public GameObject enemyPrefab;

    [Header("Stats")]
    public float maxHealth;
    public float damage;
    public float speed;
    public int reward;
    public EnemyType type;
}