using UnityEngine;
using UnityEngine.UI;

namespace DamasDenis.Stage
{
    [RequireComponent(typeof(Button))]
    public sealed class StageSelectButton : MonoBehaviour
    {
        [SerializeField] private StageSelectionController selectionController;
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
