using UnityEngine;
using TMPro;
using UnityEngine.UI;
using DamasDenis.Core;
using DamasDenis.Stage;
using UnityEngine.SceneManagement;

public class UIManager : MonoBehaviour
{
    [Header("UI Elements (Stage)")]
    public TextMeshProUGUI goldText;
    public Image healthBarFill;

    [Header("UI Elements (Persistent)")]
    public TextMeshProUGUI researchCreditsText;

    [Header("Layer Menu")]
    public GameObject pauseMenu;
    public GameObject gameOverMenu;
    
    private void Start()
    {
        if (StageState.Instance != null)
        {
            StageState.Instance.OnStageResourceChanged += UpdateGoldUI;
            StageState.Instance.OnHealthChanged += UpdateHealthUI;
            UpdateGoldUI(StageState.Instance.CurrentStageResources);
            UpdateHealthUI(StageState.Instance.CurrentHealth);
        }

        if (PlayerState.Instance != null)
        {
            PlayerState.Instance.OnResearchCreditsChanged += UpdateResearchCreditsUI;
            UpdateResearchCreditsUI(PlayerState.Instance.ResearchCredits);
        }
    }

    private void OnDestroy()
    {
        if (StageState.Instance != null)
        {
            StageState.Instance.OnStageResourceChanged -= UpdateGoldUI;
            StageState.Instance.OnHealthChanged -= UpdateHealthUI;
        }

        if (PlayerState.Instance != null)
        {
            PlayerState.Instance.OnResearchCreditsChanged -= UpdateResearchCreditsUI;
        }
    }

    void UpdateGoldUI(int newGoldAmount)
    {
        if (goldText == null) return;
        int max = StageState.Instance != null ? StageState.Instance.MaxStageResources : 100;
        goldText.text = newGoldAmount.ToString() + "/" + max.ToString();
    }

    void UpdateHealthUI(float currentHealth)
    {
        if (healthBarFill == null) return;
        float max = StageState.Instance != null && StageState.Instance.MaxHealth > 0 ? StageState.Instance.MaxHealth : 100f;
        healthBarFill.fillAmount = currentHealth / max;
        Debug.Log(currentHealth / max);
    }

    void UpdateResearchCreditsUI(int credits)
    {
        if (researchCreditsText != null)
            researchCreditsText.text = credits.ToString();
    }

    public void PauseGame() {
        Time.timeScale = 0;
        pauseMenu.SetActive(true);
    }

    public void ResumeGame() {
        Time.timeScale = 1;
        pauseMenu.SetActive(false);
    }

    public void Lose()
    {
        Time.timeScale = 0f;
        gameOverMenu.SetActive(true);
    }

    public void RestartLevel() {
        Time.timeScale = 1;                       // a paused retry would otherwise start frozen
        GameManager.Instance.ReloadStage();
    }

    public void MainMenu() {                      // consider renaming to ExitToLobby
        Time.timeScale = 1;
        GameManager.Instance.LoadLobby();         // use whatever your public method is called
    }
}