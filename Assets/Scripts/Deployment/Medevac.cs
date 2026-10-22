using UnityEngine;

namespace DamasDenis.Deployment
{
    /// <summary>Why a medevac can't start right now.</summary>
    public enum MedevacBlock { None, NotDeployed, AlreadyEvacuating, NoCharges, AtCapacity }

    /// <summary>
    /// The player's only way to pull a unit off the board. Owns the charges (which recharge
    /// one at a time) and the limit on how many units can be in process at once.
    /// </summary>
    public sealed class Medevac : MonoBehaviour
    {
        [Header("Charges")]
        [Min(1)] [SerializeField] private int maxCharges = 2;
        [Min(0.1f)] [SerializeField] private float secondsPerCharge = 20f;

        [Header("Capacity")]
        [Tooltip("How many units can be mid-extraction at the same time.")]
        [Min(1)] [SerializeField] private int maxConcurrent = 2;

        private int charges;
        private float rechargeTimer;
        private int active;

        /// <summary>Charges available now.</summary>
        public int Charges => charges;
        /// <summary>Most charges the medevac can hold.</summary>
        public int MaxCharges => maxCharges;
        /// <summary>Units being extracted right now.</summary>
        public int ActiveCount => active;
        /// <summary>Most units that can be extracted at once.</summary>
        public int MaxConcurrent => maxConcurrent;
        /// <summary>Seconds until the next charge, or 0 when full.</summary>
        public float SecondsToNextCharge => charges >= maxCharges ? 0f : Mathf.Max(0f, secondsPerCharge - rechargeTimer);

        private void Awake() => charges = maxCharges;

        private void Update()
        {
            if (charges >= maxCharges)
            {
                rechargeTimer = 0f;
                return;
            }

            rechargeTimer += Time.deltaTime;
            if (rechargeTimer >= secondsPerCharge)
            {
                rechargeTimer -= secondsPerCharge;
                charges++;
            }
        }

        /// <summary>Whether this unit could be evacuated right now, and if not, why.</summary>
        public MedevacBlock Check(StaffRetreatController unit)
        {
            if (unit == null || !unit.IsDeployed) return MedevacBlock.NotDeployed;
            if (unit.IsProcessing) return MedevacBlock.AlreadyEvacuating;
            if (charges <= 0) return MedevacBlock.NoCharges;
            if (active >= maxConcurrent) return MedevacBlock.AtCapacity;
            return MedevacBlock.None;
        }

        /// <summary>Spends a charge and starts the unit's extraction. False if not allowed.</summary>
        public bool TryEvacuate(StaffRetreatController unit)
        {
            if (Check(unit) != MedevacBlock.None) return false;

            charges--;
            active++;
            unit.Begin(OnEvacuationEnded);
            return true;
        }

        private void OnEvacuationEnded()
        {
            if (this == null) return;   // scene is unloading; Unity's == also catches destroyed objects
            active = Mathf.Max(0, active - 1);
        }
    }
}