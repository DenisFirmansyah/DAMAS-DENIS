
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
    public GameObject weaponPrefab;
    public Vector2 weaponOffset;
    public float weaponRotation;

    [Header("Projectile")]
    public GameObject projectilePrefab;
    [Min(0.01f)] public float projectileSpeed = 15f;

    [Header("Animation States")]
    [Tooltip("Animator state played whenever this weapon attacks.")]
    public string attackAnimationState;
    [Tooltip("Animator state played while the character moves.")]
    public string moveAnimationState;
    [Tooltip("Optional state played after movement stops. Leave empty to keep the current pose.")]
    public string idleAnimationState;
}
