using TMPro;
using UnityEngine;
using UnityEngine.UI;
using DamasDenis.Core;
using DamasDenis.Stage;
using DamasDenis.Deployment;

public sealed class GameplayUIController : UIScreen
{
    [Header("HUD - Stage")]
    [SerializeField] private Image healthBarFill;
    [SerializeField] private TextMeshProUGUI timeText;
    [SerializeField] private Image pauseButton;
    [SerializeField] private Sprite playImage;
    [SerializeField] private Sprite pausedImage;
    [SerializeField] private TextMeshProUGUI starTwoObjective;
    [SerializeField] private TextMeshProUGUI starThreeObjective;

    [Header("Gameplay Screens")]
    [SerializeField] private StageDirector stageDirector;
    [SerializeField] private GameObject pauseMenu;
    [SerializeField] private GameObject winMenu;
    [SerializeField] private GameObject gameOverMenu;

    [SerializeField] private DeploymentController deploymentController;
    

    private StageState subscribedStageState;
    private bool isPaused;
    private bool isGameOver;

    private void Start()
    {
        SubscribeToState();
        RefreshAll();

        if (stageDirector != null)
        {
            stageDirector.StageFinished -= OnStageFinished; 
            stageDirector.StageFinished += OnStageFinished;
        }
    }

       private void Update()
    {
        if (!Input.GetKeyDown(KeyCode.Escape) || isGameOver || GamePause.Has(PauseReason.Dialogue)) return;
        if (deploymentController != null && deploymentController.IsPlacing) { deploymentController.CancelPlacement(); return; }
        if (isPaused) ResumeGame(); else PauseGame();
    }

    protected override void OnEnable()
    {
        if (stageDirector != null) stageDirector.StageFinished -= OnStageFinished;
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
        if (stageDirector != null) stageDirector.StageFinished -= OnStageFinished;

        UnsubscribeFromState();
        if (GamePause.IsPaused)
            GamePause.ReleaseAll();
    }

    private void OnStageFinished(StageRecord record)
    {
        if (record != null && record.won) ShowWin(record);
        else Lose();
    }

    private void SubscribeToState()
    {
        StageState stageState = StageState.Instance;
        if (stageState != null && subscribedStageState != stageState)
        {
            UnsubscribeFromState();
            subscribedStageState = stageState;
            subscribedStageState.OnHealthChanged += UpdateHealthUI;
            subscribedStageState.OnTimeChanged += UpdateTimeLeftUI;
        }
    }

    private void UnsubscribeFromState()
    {
        if (subscribedStageState == null)
            return;

        subscribedStageState.OnHealthChanged -= UpdateHealthUI;
        subscribedStageState.OnHealthChanged -= UpdateTimeLeftUI;
        subscribedStageState = null;
    }

    private void RefreshAll()
    {
        if (subscribedStageState != null)
        {
            UpdateHealthUI(subscribedStageState.CurrentHealth);
            UpdateTimeLeftUI(subscribedStageState.RemainingSeconds);
            UpdateObjectives();
        }
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

    private void UpdateTimeLeftUI(float timeLeft)
    {
        if (timeText == null)
            return;

        timeLeft = Mathf.Max(0f, timeLeft);

        int totalSeconds = Mathf.CeilToInt(Mathf.Max(0f, timeLeft));
        timeText.text = $"{totalSeconds / 60:00}:{totalSeconds % 60:00}";
    }

    private void UpdateObjectives()
    {
        if (starTwoObjective && starThreeObjective != null)
            starTwoObjective.text = "* {}";
            starThreeObjective.text = "* {}";
    }

    public void PauseGame()
    {
        if (isPaused || isGameOver)
            return;

        isPaused = true;
        GamePause.Hold(PauseReason.PauseMenu);
        if (pauseMenu != null)
            pauseMenu.SetActive(true);
            pauseButton.sprite = pausedImage;
    }

    public void ResumeGame()
    {
        if (isGameOver)
            return;

        isPaused = false;
        GamePause.Release(PauseReason.PauseMenu);
        if (pauseMenu != null)
            pauseMenu.SetActive(false);
            pauseButton.sprite = playImage;
    }

    private void ShowWin(StageRecord record)
    {
        GamePause.Hold(PauseReason.GameOver);
        if (winMenu != null) winMenu.SetActive(true);
        // later: show stars and objectives from the record (record.events, record.elapsedSeconds)
    }

    public void Lose()
    {
        if (isGameOver)
            return;

        isGameOver = true;
        isPaused = false;
        GamePause.Hold(PauseReason.GameOver);

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

        GamePause.ReleaseAll();
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

        GamePause.ReleaseAll();
        isPaused = false;
        isGameOver = false;
        GameManager.Instance.LoadLobby();
    }

    public void RoamingMode()
    {
        if (GameManager.Instance == null)
        {
            Debug.LogError("Cannot start roaming because GameManager.Instance is missing.", this);
            return;
        }

        GameManager.Instance.StartRoaming();
    }
}
