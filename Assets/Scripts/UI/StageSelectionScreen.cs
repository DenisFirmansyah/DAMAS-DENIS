using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using DamasDenis.Core;
using DamasDenis.Stage;

namespace DamasDenis.UI
{
    public sealed class StageSelectionScreen : UIScreen
    {
        [Header("Data")]
        [Tooltip("Stages in play order. Optional stages never block progression.")]
        [SerializeField] private List<StageDefinition> stages = new List<StageDefinition>();

        [Header("UI")]
        [SerializeField] private TextMeshProUGUI stageNameText;
        [SerializeField] private TextMeshProUGUI stageDescText;
        [SerializeField] private Button previousButton;
        [SerializeField] private Button nextButton;
        [SerializeField] private Button playButton;

        [Header("Stars")]
        [Tooltip("Exactly one Image per star, left to right.")]
        [SerializeField] private Image[] starImages;
        [SerializeField] private Sprite starFilled;
        [SerializeField] private Sprite starEmpty;
        [SerializeField] private TextMeshProUGUI starTwoObjective;
        [SerializeField] private TextMeshProUGUI starThreeObjective;

        private int viewedIndex;
        private int reachableIndex;

        protected override void Awake()
        {
            base.Awake();
            if (previousButton != null) previousButton.onClick.AddListener(ShowPrevious);
            if (nextButton != null) nextButton.onClick.AddListener(ShowNext);
            if (playButton != null) playButton.onClick.AddListener(PlayViewedStage);
        }

        // LobbyController opens screens with SetActive(), which never calls Show(),
        // so OnEnable is the one hook that always runs when this screen appears.
        protected override void OnEnable()
        {
            base.OnEnable();
            RecomputeProgress();
            viewedIndex = reachableIndex;
            Refresh();
        }

        /// <summary>Moves to the previous (already completed) stage.</summary>
        public void ShowPrevious()
        {
            if (viewedIndex <= 0) return;
            viewedIndex--;
            Refresh();
        }

        /// <summary>Moves to the next stage, only if progress allows it.</summary>
        public void ShowNext()
        {
            if (viewedIndex >= reachableIndex) return;
            viewedIndex++;
            Refresh();
        }

        /// <summary>Starts the stage currently on display. Replaces LobbyController.SelectStage.</summary>
        public void PlayViewedStage()
        {
            StageDefinition stage = GetStage(viewedIndex);
            if (stage == null || string.IsNullOrWhiteSpace(stage.SceneName))
            {
                Debug.LogError("PlayViewedStage needs a StageDefinition with a valid scene name.", this);
                return;
            }

            if (GameManager.Instance == null)
            {
                Debug.LogError("Cannot start gameplay because GameManager.Instance is missing.", this);
                return;
            }

            GameManager.Instance.StartGameplay(stage);
        }

        /// <summary>
        /// Finds the furthest stage the player may see: walk the list until the first
        /// unfinished required stage. Everything before it is completed, it is "current".
        /// </summary>
        private void RecomputeProgress()
        {
            PlayerState player = PlayerState.Instance;
            reachableIndex = 0;

            for (int i = 0; i < stages.Count; i++)
            {
                reachableIndex = i;
                StageDefinition stage = stages[i];
                if (stage == null) continue;

                bool done = player.HasCompletedStage(stage.StageId);
                if (!done && !stage.IsOptionalStage)
                    return;
            }
        }

        private void Refresh()
        {
            StageDefinition stage = GetStage(viewedIndex);
            PlayerState player = PlayerState.Instance;
            bool completed = stage != null && player.HasCompletedStage(stage.StageId);

            if (stageNameText != null)
                stageNameText.text = stage != null ? stage.StageName : "No stage";

            if (stageDescText != null)
                stageDescText.text = stage != null ? stage.StageDesc : "No stage";

            if (starTwoObjective && starThreeObjective != null)
                starTwoObjective.text = stage != null ? stage.StarTwoObjective.Title: "No Obj";
                starThreeObjective.text = stage != null ? stage.StarTwoObjective.Title : "No stage";

            if (previousButton != null) previousButton.interactable = viewedIndex > 0;
            if (nextButton != null) nextButton.interactable = viewedIndex < reachableIndex;
            if (playButton != null) playButton.interactable = stage != null;

            ShowStars(completed ? PlayerState.Instance.GetBestStars(stage.StageId) : 0);
        }

        private void ShowStars(int earned)
        {
            if (starImages == null) return;
            for (int i = 0; i < starImages.Length; i++)
            {
                if (starImages[i] == null) continue;
                starImages[i].sprite = i < earned ? starFilled : starEmpty;
            }
        }

        private StageDefinition GetStage(int index)
        {
            return index >= 0 && index < stages.Count ? stages[index] : null;
        }
    }
}