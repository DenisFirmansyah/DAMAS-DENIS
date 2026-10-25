using TMPro;
using UnityEngine;
using UnityEngine.UI;
using DamasDenis.Core;
using DamasDenis.Stage;

public sealed class GameplayUIController : UIScreen
{
    [Header("HUD - Stage")]
    [SerializeField] private TextMeshProUGUI goldText;
    [SerializeField] private Image healthBarFill;

    [Header("HUD - Persistent")]
    [SerializeField] private TextMeshProUGUI researchCreditsText;

    [Header("Gameplay Screens")]
    [SerializeField] private GameObject pauseMenu;
    [SerializeField] private GameObject gameOverMenu;

    private StageState subscribedStageState;
    private PlayerState subscribedPlayerState;
    private bool isPaused;
    private bool isGameOver;

    protected override void OnEnable()
    {
        base.OnEnable();
        SubscribeToState();
        RefreshAll();
    }

    protected override void OnDisable()
    {
        UnsubscribeFromState();
        base.OnDisable();
    }

    private void OnDestroy()
    {
        UnsubscribeFromState();
        if (Time.timeScale == 0f)
            Time.timeScale = 1f;
    }

    private void SubscribeToState()
    {
        StageState stageState = StageState.Instance;
        if (stageState != null && subscribedStageState != stageState)
        {
            UnsubscribeStageState();
            subscribedStageState = stageState;
            subscribedStageState.OnStageResourceChanged += UpdateGoldUI;
            subscribedStageState.OnHealthChanged += UpdateHealthUI;
        }

        PlayerState playerState = PlayerState.Instance;
        if (playerState != null && subscribedPlayerState != playerState)
        {
            UnsubscribePlayerState();
            subscribedPlayerState = playerState;
            subscribedPlayerState.OnResearchCreditsChanged += UpdateResearchCreditsUI;
        }
    }

    private void UnsubscribeFromState()
    {
        UnsubscribeStageState();
        UnsubscribePlayerState();
    }

    private void UnsubscribeStageState()
    {
        if (subscribedStageState == null)
            return;

        subscribedStageState.OnStageResourceChanged -= UpdateGoldUI;
        subscribedStageState.OnHealthChanged -= UpdateHealthUI;
        subscribedStageState = null;
    }

    private void UnsubscribePlayerState()
    {
        if (subscribedPlayerState == null)
            return;

        subscribedPlayerState.OnResearchCreditsChanged -= UpdateResearchCreditsUI;
        subscribedPlayerState = null;
    }

    private void RefreshAll()
    {
        if (subscribedStageState != null)
        {
            UpdateGoldUI(subscribedStageState.CurrentStageResources);
            UpdateHealthUI(subscribedStageState.CurrentHealth);
        }

        if (subscribedPlayerState != null)
            UpdateResearchCreditsUI(subscribedPlayerState.ResearchCredits);
    }

    private void UpdateGoldUI(int amount)
    {
        if (goldText == null)
            return;

        int max = subscribedStageState != null ? subscribedStageState.MaxStageResources : 100;
        goldText.text = $"{amount}/{max}";
    }

    private void UpdateHealthUI(float currentHealth)
    {
        if (healthBarFill == null)
            return;

        float max = subscribedStageState != null && subscribedStageState.MaxHealth > 0f
            ? subscribedStageState.MaxHealth
            : 100f;

        healthBarFill.fillAmount = Mathf.Clamp01(currentHealth / max);
    }

    private void UpdateResearchCreditsUI(int credits)
    {
        if (researchCreditsText != null)
            researchCreditsText.text = credits.ToString();
    }

    public void PauseGame()
    {
        if (isPaused || isGameOver)
            return;

        isPaused = true;
        Time.timeScale = 0f;
        if (pauseMenu != null)
            pauseMenu.SetActive(true);
    }

    public void ResumeGame()
    {
        if (isGameOver)
            return;

        isPaused = false;
        Time.timeScale = 1f;
        if (pauseMenu != null)
            pauseMenu.SetActive(false);
    }

    public void Lose()
    {
        if (isGameOver)
            return;

        isGameOver = true;
        isPaused = false;
        Time.timeScale = 0f;

        if (pauseMenu != null)
            pauseMenu.SetActive(false);
        if (gameOverMenu != null)
            gameOverMenu.SetActive(true);
    }

    public void RestartLevel()
    {
        if (GameManager.Instance == null)
        {
            Debug.LogError("Cannot restart because GameManager.Instance is missing.", this);
            return;
        }

        Time.timeScale = 1f;
        isPaused = false;
        isGameOver = false;
        GameManager.Instance.ReloadStage();
    }

    public void ReturnToLobby()
    {
        if (GameManager.Instance == null)
        {
            Debug.LogError("Cannot return to lobby because GameManager.Instance is missing.", this);
            return;
        }

        Time.timeScale = 1f;
        isPaused = false;
        isGameOver = false;
        GameManager.Instance.LoadLobby();
    }
}
