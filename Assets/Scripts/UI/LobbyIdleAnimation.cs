using System.Collections;
using UnityEngine;

namespace DamasDenis.UI
{
    [DisallowMultipleComponent]
    public sealed class LobbyIdleAnimation : MonoBehaviour
    {
        [Header("Animator")]
        [SerializeField] private Animator animator;
        [SerializeField] private int animatorLayer;
        [SerializeField] private string baseIdleState = "Idle_1";
        [SerializeField] private string[] randomIdleStates = { "Idle_2", "Idle_3", "Idle_4" };

        [Header("Timing")]
        [Min(0f)] [SerializeField] private float minInterval = 5f;
        [Min(0f)] [SerializeField] private float maxInterval = 10f;
        [Min(0f)] [SerializeField] private float crossFadeDuration = 0.15f;

        private int baseIdleStateHash;
        private int[] randomIdleStateHashes;
        private Coroutine idleRoutine;
        private int previousIdleIndex = -1;

        private void OnEnable()
        {
            if (animator == null)
                animator = GetComponentInChildren<Animator>(true);

            if (!ValidateConfiguration())
                return;

            idleRoutine = StartCoroutine(PlayIdleSequence());
        }

        private void OnDisable()
        {
            if (idleRoutine == null)
                return;

            StopCoroutine(idleRoutine);
            idleRoutine = null;
        }

        private bool ValidateConfiguration()
        {
            if (animator == null)
            {
                Debug.LogError($"{nameof(LobbyIdleAnimation)} needs an Animator reference.", this);
                return false;
            }

            if (animator.runtimeAnimatorController == null)
            {
                Debug.LogError($"{nameof(LobbyIdleAnimation)} Animator has no Runtime Animator Controller.", this);
                return false;
            }

            if (animatorLayer < 0 || animatorLayer >= animator.layerCount)
            {
                Debug.LogError($"{nameof(LobbyIdleAnimation)} has an invalid Animator layer index.", this);
                return false;
            }

            if (maxInterval < minInterval)
            {
                Debug.LogError($"{nameof(LobbyIdleAnimation)} max interval must be greater than or equal to min interval.", this);
                return false;
            }

            if (string.IsNullOrWhiteSpace(baseIdleState))
            {
                Debug.LogError($"{nameof(LobbyIdleAnimation)} needs a base idle state name.", this);
                return false;
            }

            if (randomIdleStates == null || randomIdleStates.Length != 3)
            {
                Debug.LogError($"{nameof(LobbyIdleAnimation)} needs exactly three random idle state names.", this);
                return false;
            }

            baseIdleStateHash = Animator.StringToHash(baseIdleState);
            if (!animator.HasState(animatorLayer, baseIdleStateHash))
            {
                Debug.LogError($"Animator state '{baseIdleState}' was not found on layer {animatorLayer}.", this);
                return false;
            }

            randomIdleStateHashes = new int[randomIdleStates.Length];
            for (int i = 0; i < randomIdleStates.Length; i++)
            {
                string stateName = randomIdleStates[i];
                if (string.IsNullOrWhiteSpace(stateName))
                {
                    Debug.LogError($"Random idle state at index {i} has no name.", this);
                    return false;
                }

                int stateHash = Animator.StringToHash(stateName);
                if (stateHash == baseIdleStateHash)
                {
                    Debug.LogError($"Random idle state '{stateName}' duplicates the base idle state.", this);
                    return false;
                }

                for (int previousIndex = 0; previousIndex < i; previousIndex++)
                {
                    if (stateHash == randomIdleStateHashes[previousIndex])
                    {
                        Debug.LogError($"Random idle state '{stateName}' is assigned more than once.", this);
                        return false;
                    }
                }

                if (!animator.HasState(animatorLayer, stateHash))
                {
                    Debug.LogError($"Animator state '{stateName}' was not found on layer {animatorLayer}.", this);
                    return false;
                }

                randomIdleStateHashes[i] = stateHash;
            }

            return true;
        }

        private IEnumerator PlayIdleSequence()
        {
            PlayState(baseIdleStateHash);

            while (true)
            {
                yield return new WaitForSeconds(Random.Range(minInterval, maxInterval));

                int idleIndex = ChooseRandomIdleIndex();
                PlayState(randomIdleStateHashes[idleIndex]);
                yield return WaitForStateToFinish(randomIdleStates[idleIndex]);
                PlayState(baseIdleStateHash);
            }
        }

        private int ChooseRandomIdleIndex()
        {
            int index = Random.Range(0, randomIdleStateHashes.Length);
            while (randomIdleStateHashes.Length > 1 && index == previousIdleIndex)
                index = Random.Range(0, randomIdleStateHashes.Length);

            previousIdleIndex = index;
            return index;
        }

        private IEnumerator WaitForStateToFinish(string stateName)
        {
            while (isActiveAndEnabled && animator != null && animator.isActiveAndEnabled)
            {
                AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(animatorLayer);
                if (stateInfo.IsName(stateName) && stateInfo.normalizedTime >= 1f)
                    yield break;

                yield return null;
            }
        }

        private void PlayState(int stateHash)
        {
            if (crossFadeDuration > 0f)
                animator.CrossFadeInFixedTime(stateHash, crossFadeDuration, animatorLayer, 0f);
            else
                animator.Play(stateHash, animatorLayer, 0f);
        }
    }
}
