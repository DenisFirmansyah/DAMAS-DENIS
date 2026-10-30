
using UnityEngine;

public enum WeaponType
{
    M4,
    M14,
    Pistol
}

[CreateAssetMenu(
    fileName = "NewWeapon",
    menuName = "Military/Weapon Data"
)]
public class WeaponData : ScriptableObject
{
    [Header("Identity")]
    public WeaponType weaponType;
    public string weaponName;

    [Header("Visual")]
    public Sprite weaponSprite;
    public Vector2 weaponOffset;
    public float weaponRotation;
    [Min(0)] public int sortingOrderOffset = 1;

    [Header("Combat")]
    [Min(0f)]
    public float damage = 10f;
    [Min(0f)]
    public float attackRange = 5f;
    [Min(0.01f)]
    public float fireInterval = 0.15f;
    [Min(0f)]
    public float projectileSpeed = 15f;

    [Header("Projectile")]
    public GameObject projectilePrefab;

    [Header("Animation States")]
    [Tooltip("Animator state played whenever this weapon attacks.")]
    public string attackAnimationState = "Soldier_Shoot";
    [Tooltip("Animator state played while the character moves.")]
    public string moveAnimationState = "Soldier_Run";
    [Tooltip("Optional state played after movement stops. Leave empty to keep the current pose.")]
    public string idleAnimationState;
}
