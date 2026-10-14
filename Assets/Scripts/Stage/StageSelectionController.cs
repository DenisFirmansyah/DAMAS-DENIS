using UnityEngine;
using UnityEngine.SceneManagement;

namespace DamasDenis.Stage
{
    /// <summary>UI entry point for normal and optional stages. Bind SelectStage from a button.</summary>
    public sealed class StageSelectionController : MonoBehaviour
    {
        public void SelectStage(StageDefinition stage)
        {
            if (stage == null || string.IsNullOrWhiteSpace(stage.SceneName))
            {
                Debug.LogError("SelectStage needs a StageDefinition with a scene name in Build Settings.", this);
                return;
            }

            SceneManager.LoadScene(stage.SceneName);
        }
    }
}
