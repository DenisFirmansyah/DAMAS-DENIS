using UnityEngine;

public sealed class MainMenuController : UIScreen
{
    [Header("Menu Screens")]
    [SerializeField] private GameObject mainMenu;
    [SerializeField] private GameObject levelMenu;
    [SerializeField] private GameObject optionsMenu;
    [SerializeField] private GameObject creditsMenu;

    public static MainMenuController Instance { get; private set; }

    protected override void Awake()
    {
        base.Awake();

        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }

    public void PlayGame() => GameManager.Instance.LoadLobby();

    public void Options() => ShowOnly(optionsMenu);

    public void Credits() => ShowOnly(creditsMenu);

    public void BackToMainMenu() => ShowOnly(mainMenu);

    public void BackToLevelMenu() => ShowOnly(levelMenu);

    // Compatibility for existing OptionsScreen button wiring.
    public void Back() => BackToMainMenu();

    public void ExitGame()
    {
        Application.Quit();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }

    private void ShowOnly(GameObject target)
    {
        if (target == null)
        {
            Debug.LogWarning("A MainMenuController screen reference is not assigned.", this);
            return;
        }

        SetActiveIfAssigned(mainMenu, target == mainMenu);
        SetActiveIfAssigned(levelMenu, target == levelMenu);
        SetActiveIfAssigned(optionsMenu, target == optionsMenu);
        SetActiveIfAssigned(creditsMenu, target == creditsMenu);
    }

    private void SetActiveIfAssigned(GameObject screen, bool active)
    {
        if (screen != null)
            screen.SetActive(active);
    }
}
