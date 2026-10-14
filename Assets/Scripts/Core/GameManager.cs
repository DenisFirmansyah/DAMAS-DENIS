using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public enum GameState { Boot, MainMenu, Gameplay, Review }

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public const string MainMenuScene = "MainMenu";
    public const string GameplayScene = "Gameplay";
    public const string ReviewScene   = "Review";

    public GameState State { get; private set; } = GameState.Boot;
    public string CurrentStage { get; private set; }
    public bool IsLoading { get; private set; }

    public event Action<GameState> StateChanged;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            enabled = false; // Start() must not run on the duplicate
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Start() => LoadMainMenu();

    // ---- Public API ----

    public void LoadMainMenu()
    {
        if (IsLoading) return;
        StartCoroutine(LoadSingleRoutine(MainMenuScene, GameState.MainMenu));
    }

    public void LoadReview()
    {
        if (IsLoading) return;
        StartCoroutine(LoadSingleRoutine(ReviewScene, GameState.Review));
    }

    public void StartGameplay(string stageScene)
    {
        if (IsLoading) return;
        StartCoroutine(StartGameplayRoutine(stageScene));
    }

    // Retry after a loss. Unloading + reloading Stage resets it.
    public void ReloadStage()
    {
        if (IsLoading || string.IsNullOrEmpty(CurrentStage)) return;
        StartCoroutine(ReloadStageRoutine());
    }

    // ---- Routines ----

    private IEnumerator LoadSingleRoutine(string scene, GameState next)
    {
        IsLoading = true;
        CurrentStage = null;
        yield return SceneManager.LoadSceneAsync(scene);
        SetState(next);
        IsLoading = false;
    }

    private IEnumerator StartGameplayRoutine(string stageScene)
    {
        IsLoading = true;
        yield return SceneManager.LoadSceneAsync(GameplayScene);
        yield return LoadStageRoutine(stageScene);
        SetState(GameState.Gameplay);
        IsLoading = false;
    }

    private IEnumerator ReloadStageRoutine()
    {
        IsLoading = true;
        string stage = CurrentStage;
        yield return SceneManager.UnloadSceneAsync(stage);
        yield return LoadStageRoutine(stage);
        IsLoading = false;
    }

    private IEnumerator LoadStageRoutine(string stageScene)
    {
        yield return SceneManager.LoadSceneAsync(stageScene, LoadSceneMode.Additive);
        SceneManager.SetActiveScene(SceneManager.GetSceneByName(stageScene));
        CurrentStage = stageScene;
    }

    private void SetState(GameState next)
    {
        State = next;
        StateChanged?.Invoke(next);
    }
}