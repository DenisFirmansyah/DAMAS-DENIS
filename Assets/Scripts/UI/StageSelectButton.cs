using UnityEngine;
using UnityEngine.UI;
using DamasDenis.Stage;

namespace DamasDenis.UI
{
    [RequireComponent(typeof(Button))]
    public sealed class StageSelectButton : MonoBehaviour
    {
        [SerializeField] private LobbyController selectionController;
        [SerializeField] private StageDefinition stage;

        private void Awake()
        {
            GetComponent<Button>().onClick.AddListener(Select);
        }

        private void Select()
        {
            if (selectionController != null)
                selectionController.SelectStage(stage);
        }
    }
}
