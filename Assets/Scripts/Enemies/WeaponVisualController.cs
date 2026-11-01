using System.Collections.Generic;
using UnityEngine;

namespace DamasDenis.Enemies
{
    [DisallowMultipleComponent]
    public sealed class WeaponVisualController : MonoBehaviour
    {
        private readonly HashSet<int> missingStates = new HashSet<int>();

        private WeaponData weapon;
        private Transform characterRoot;
        private Transform weaponMount;
        private Animator animator;
        private GameObject weaponInstance;
        private Vector3 weaponBaseScale;
        private string currentActionState;
        private bool facingLeft;

        public static WeaponVisualController Configure(GameObject character, WeaponData data, string mountPath)
        {
            if (character == null || data == null) return null;

            WeaponVisualController controller = character.GetComponentInChildren<WeaponVisualController>(true);
            if (controller == null)
                controller = character.AddComponent<WeaponVisualController>();

            controller.Initialize(
                data,
                character.transform,
                mountPath,
                character.GetComponentInChildren<Animator>(true));
            return controller;
        }

        public void Initialize(
            WeaponData weaponData,
            Transform root,
            string mountPath,
            Animator characterAnimator)
        {
            weapon = weaponData;
            characterRoot = root;
            animator = characterAnimator;
            currentActionState = null;

            if (weapon == null || characterRoot == null) return;

            weaponMount = characterRoot;
            if (!string.IsNullOrWhiteSpace(mountPath))
            {
                Transform configuredMount = characterRoot.Find(mountPath);
                if (configuredMount == null)
                {
                    Debug.LogError(
                        $"{name}: weapon mount '{mountPath}' was not found under '{characterRoot.name}'. Using the character root.",
                        this);
                }
                else
                {
                    weaponMount = configuredMount;
                }
            }

            if (weaponInstance != null)
            {
                Destroy(weaponInstance);
                weaponInstance = null;
            }

            if (weapon.weaponPrefab == null)
            {
                Debug.LogError($"{name}: '{weapon.weaponName}' has no weapon prefab assigned.", this);
                return;
            }

            weaponInstance = Instantiate(weapon.weaponPrefab, weaponMount, false);
            weaponInstance.name = weapon.weaponPrefab.name;
            Transform weaponTransform = weaponInstance.transform;
            weaponTransform.localPosition = weapon.weaponOffset;
            weaponTransform.localRotation = Quaternion.Euler(0f, 0f, weapon.weaponRotation);
            weaponBaseScale = weaponTransform.localScale;

            ApplyFacing();
        }

        public void PlayAttack()
        {
            if (weapon == null) return;
            PlayState(weapon.attackAnimationState, true);
        }

        public void SetMoving(bool isMoving)
        {
            if (weapon == null) return;

            if (isMoving)
            {
                PlayState(weapon.moveAnimationState);
            }
            else if (currentActionState == weapon.moveAnimationState &&
                     !string.IsNullOrWhiteSpace(weapon.idleAnimationState))
            {
                PlayState(weapon.idleAnimationState);
            }
        }

        public void SetFacing(bool isFacingLeft)
        {
            facingLeft = isFacingLeft;
            ApplyFacing();
        }

        private void ApplyFacing()
        {
            if (weaponInstance == null || weapon == null) return;

            Transform weaponTransform = weaponInstance.transform;
            Vector2 offset = weapon.weaponOffset;
            weaponTransform.localPosition = new Vector3(facingLeft ? -offset.x : offset.x, offset.y, 0f);
            weaponTransform.localScale = new Vector3(
                facingLeft ? -Mathf.Abs(weaponBaseScale.x) : Mathf.Abs(weaponBaseScale.x),
                weaponBaseScale.y,
                weaponBaseScale.z);
        }

        private void PlayState(string stateName, bool restart = false)
        {
            if (animator == null || string.IsNullOrWhiteSpace(stateName)) return;
            if (!restart && currentActionState == stateName) return;

            int stateHash = Animator.StringToHash(stateName);
            if (animator.runtimeAnimatorController == null || !animator.HasState(0, stateHash))
            {
                if (missingStates.Add(stateHash))
                    Debug.LogWarning($"{name}: Animator state '{stateName}' is not available.", this);
                return;
            }

            animator.Play(stateHash, 0, 0f);
            currentActionState = stateName;
        }
    }
}
