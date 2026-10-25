using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using DamasDenis.Stage;

public enum GameState { Boot, MainMenu, Lobby, Gameplay, Roaming, Review }

public sealed class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public const string MainMenuScene = "MainMenu";
    public const string LobbyScene = "Lobby";
    public const string GameplayScene = "Gameplay";
    public const string RoamingScene = "Roaming";
    public const string ReviewScene = "Review";

    public GameState State { get; private set; } = GameState.Boot;
    public string CurrentStage { get; private set; }
    public StageDefinition CurrentStageDefinition { get; private set; }
    public bool IsLoading { get; private set; }

    public event Action<GameState> StateChanged;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        // Assumes this persistent manager is bootstrapped from the boot scene.
        if (State == GameState.Boot)
            LoadMainMenu();
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    public void LoadMainMenu() => LoadSingleScene(MainMenuScene, GameState.MainMenu);
    public void LoadLobby() => LoadSingleScene(LobbyScene, GameState.Lobby);
    public void LoadReview() => LoadSingleScene(ReviewScene, GameState.Review);

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (AudioManager.Instance == null)
            return;

        switch (scene.name)
        {
            case "MainMenu":
                AudioManager.Instance.PlayMainMenuMusic();
                break;

            case "Gameplay":
                AudioManager.Instance.PlayGameplayMusic();
                break;

            case "Roaming":
                AudioManager.Instance.PlayRoamingMusic();
                break;
        }
    }

    private void LoadSingleScene(string sceneName, GameState nextState)
    {
        if (IsLoading || !CanLoadScene(sceneName))
            return;

        StartCoroutine(LoadSingleRoutine(sceneName, nextState));
    }

    public void StartGameplay(StageDefinition stageDefinition)
    {
        if (stageDefinition == null)
        {
            Debug.LogError("StartGameplay requires a valid StageDefinition.", this);
            return;
        }

        if (string.IsNullOrWhiteSpace(stageDefinition.SceneName))
        {
            Debug.LogError("The StageDefinition has no scene name.", stageDefinition);
            return;
        }

        StartGameplayInternal(stageDefinition.SceneName, stageDefinition);
    }

    public void StartGameplay(string stageScene)
    {
        StartGameplayInternal(stageScene, null);
    }

    private void StartGameplayInternal(string stageScene, StageDefinition stageDefinition)
    {
        if (IsLoading)
            return;

        if (!CanLoadScene(GameplayScene) || !CanLoadScene(stageScene))
            return;

        CurrentStageDefinition = stageDefinition;
        StartCoroutine(StartGameplayRoutine(stageScene));
    }

    public void ReloadStage()
    {
        if (IsLoading || string.IsNullOrWhiteSpace(CurrentStage))
            return;

        if (!SceneManager.GetSceneByName(CurrentStage).isLoaded)
        {
            Debug.LogError($"Cannot reload stage '{CurrentStage}' because it is not loaded.", this);
            return;
        }

        StartCoroutine(ReloadStageRoutine());
    }

    public void StartRoaming()
    {
        LoadSingleScene(RoamingScene, GameState.Roaming);
    }

    private IEnumerator LoadSingleRoutine(string sceneName, GameState nextState)
    {
        IsLoading = true;
        Time.timeScale = 1f;
        CurrentStage = null;
        CurrentStageDefinition = null;

        AsyncOperation operation = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Single);
        if (operation == null)
        {
            Debug.LogError($"Failed to begin loading scene '{sceneName}'.", this);
            IsLoading = false;
            yield break;
        }

        yield return operation;
        SetState(nextState);
        IsLoading = false;
    }

    private IEnumerator StartGameplayRoutine(string stageScene)
    {
        IsLoading = true;
        Time.timeScale = 1f;

        AsyncOperation gameplayLoad = SceneManager.LoadSceneAsync(GameplayScene, LoadSceneMode.Single);
        if (gameplayLoad == null)
        {
            Debug.LogError($"Failed to begin loading scene '{GameplayScene}'.", this);
            IsLoading = false;
            yield break;
        }

        yield return gameplayLoad;
        yield return LoadStageRoutine(stageScene);

        if (string.Equals(CurrentStage, stageScene, StringComparison.Ordinal))
            SetState(GameState.Gameplay);

        IsLoading = false;
    }

    private IEnumerator ReloadStageRoutine()
    {
        IsLoading = true;
        Time.timeScale = 1f;

        string stageScene = CurrentStage;
        Scene stage = SceneManager.GetSceneByName(stageScene);
        if (!stage.IsValid() || !stage.isLoaded)
        {
            Debug.LogError($"Cannot reload stage '{stageScene}': scene is not loaded.", this);
            IsLoading = false;
            yield break;
        }

        AsyncOperation unload = SceneManager.UnloadSceneAsync(stage);
        if (unload == null)
        {
            Debug.LogError($"Failed to unload stage '{stageScene}'.", this);
            IsLoading = false;
            yield break;
        }

        yield return unload;
        yield return LoadStageRoutine(stageScene);
        IsLoading = false;
    }

    private IEnumerator LoadStageRoutine(string stageScene)
    {
        AsyncOperation load = SceneManager.LoadSceneAsync(stageScene, LoadSceneMode.Additive);
        if (load == null)
        {
            Debug.LogError($"Failed to begin loading stage '{stageScene}'.", this);
            CurrentStage = null;
            yield break;
        }

        yield return load;

        Scene loadedStage = SceneManager.GetSceneByName(stageScene);
        if (!loadedStage.IsValid() || !loadedStage.isLoaded)
        {
            Debug.LogError($"Stage scene '{stageScene}' did not load successfully.", this);
            CurrentStage = null;
            yield break;
        }

        SceneManager.SetActiveScene(loadedStage);
        CurrentStage = stageScene;
    }

    private static bool CanLoadScene(string sceneName)
    {
        if (string.IsNullOrWhiteSpace(sceneName))
        {
            Debug.LogError("Cannot load a scene with an empty name.");
            return false;
        }

        if (!Application.CanStreamedLevelBeLoaded(sceneName))
        {
            Debug.LogError($"Scene '{sceneName}' is not available to load. Check its name and Build Settings / Build Profiles.");
            return false;
        }

        return true;
    }

    private void SetState(GameState next)
    {
        if (State == next)
            return;

        State = next;
        StateChanged?.Invoke(next);
    }
}
