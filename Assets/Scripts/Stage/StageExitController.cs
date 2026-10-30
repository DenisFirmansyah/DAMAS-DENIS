using UnityEngine;
using DamasDenis.Stage;

/// <summary>Sends the player back to the lobby after a won stage.</summary>
public sealed class StageExitController : MonoBehaviour
{
    [SerializeField] private StageDirector director;

    private void OnEnable()  { director.StageFinished += OnStageFinished; }
    private void OnDisable() { director.StageFinished -= OnStageFinished; }

    private void OnStageFinished(StageRecord record)
    {
        if (!record.won) return;
        GameManager.Instance.StartRoaming();
    }
}