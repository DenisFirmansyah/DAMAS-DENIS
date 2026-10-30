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
        private SpriteRenderer bodyRenderer;
        private SpriteRenderer weaponRenderer;
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
                character.GetComponentInChildren<Animator>(true),
                character.GetComponentInChildren<SpriteRenderer>(true));
            return controller;
        }

        public void Initialize(
            WeaponData weaponData,
            Transform root,
            string mountPath,
            Animator characterAnimator,
            SpriteRenderer characterRenderer)
        {
            weapon = weaponData;
            characterRoot = root;
            animator = characterAnimator;
            bodyRenderer = characterRenderer;
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

            if (weaponRenderer == null)
            {
                Transform existingWeapon = weaponMount.Find("Weapon Visual");
                GameObject weaponObject = existingWeapon != null
                    ? existingWeapon.gameObject
                    : new GameObject("Weapon Visual");
                if (existingWeapon == null)
                    weaponObject.transform.SetParent(weaponMount, false);

                weaponRenderer = weaponObject.GetComponent<SpriteRenderer>();
                if (weaponRenderer == null)
                    weaponRenderer = weaponObject.AddComponent<SpriteRenderer>();
            }
            else if (weaponRenderer.transform.parent != weaponMount)
            {
                weaponRenderer.transform.SetParent(weaponMount, false);
            }

            weaponRenderer.transform.localPosition = weapon.weaponOffset;
            weaponRenderer.transform.localRotation = Quaternion.Euler(0f, 0f, weapon.weaponRotation);
            weaponRenderer.sprite = weapon.weaponSprite;
            weaponRenderer.enabled = weapon.weaponSprite != null;

            if (bodyRenderer != null)
            {
                weaponRenderer.sortingLayerID = bodyRenderer.sortingLayerID;
                weaponRenderer.sortingOrder = bodyRenderer.sortingOrder + weapon.sortingOrderOffset;
            }

            if (weapon.weaponSprite == null)
                Debug.LogWarning($"{name}: '{weapon.weaponName}' has no weapon sprite assigned.", this);

            SetFacing(facingLeft);
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
            if (weaponRenderer == null || weapon == null) return;

            Vector2 offset = weapon.weaponOffset;
            weaponRenderer.transform.localPosition = new Vector3(
                facingLeft ? -offset.x : offset.x,
                offset.y,
                0f);
            weaponRenderer.flipX = facingLeft;
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
