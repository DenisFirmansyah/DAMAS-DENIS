
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class ExpandableMenu : MonoBehaviour
{
    [Header("Menu Reference")]
    [SerializeField] private RectTransform menuButton;
    [SerializeField] private RectTransform[] subButtons;

    [Header("Animation Settings")]
    [SerializeField] private float slideDuration = 0.25f;
    [SerializeField] private float staggerDelay = 0.05f;

    private Vector2[] targetPositions;
    private CanvasGroup[] canvasGroups;

    private Coroutine animationCoroutine;
    private bool isOpen;

    public bool IsOpen => isOpen;

    private void Awake()
    {
        int count = subButtons.Length;

        targetPositions = new Vector2[count];
        canvasGroups = new CanvasGroup[count];

        for (int i = 0; i < count; i++)
        {
            // Simpan posisi asli setiap tombol
            targetPositions[i] = subButtons[i].anchoredPosition;

            // Ambil atau buat CanvasGroup
            canvasGroups[i] = subButtons[i]
                .GetComponent<CanvasGroup>();

            if (canvasGroups[i] == null)
                canvasGroups[i] = subButtons[i]
                    .gameObject.AddComponent<CanvasGroup>();

            // Posisi awal tersembunyi
            subButtons[i].anchoredPosition =
                menuButton.anchoredPosition;

            canvasGroups[i].alpha = 0;
            canvasGroups[i].interactable = false;
            canvasGroups[i].blocksRaycasts = false;
        }
    }

    public void ToggleMenu()
    {
        if (isOpen)
            CloseMenu();
        else
            OpenMenu();
    }

    public void OpenMenu()
    {
        if (isOpen) return;

        isOpen = true;
        PlayAnimation(true);
    }

    public void CloseMenu()
    {
        if (!isOpen) return;

        isOpen = false;
        PlayAnimation(false);
    }

    private void PlayAnimation(bool opening)
    {
        if (animationCoroutine != null)
            StopCoroutine(animationCoroutine);

        animationCoroutine = StartCoroutine(
            AnimateMenu(opening)
        );
    }

    private IEnumerator AnimateMenu(bool opening)
    {
        int count = subButtons.Length;

        if (!opening)
        {
            // Langsung nonaktifkan interaksi
            foreach (CanvasGroup group in canvasGroups)
            {
                group.interactable = false;
                group.blocksRaycasts = false;
            }
        }

        // Animasi tiap tombol
        for (int i = 0; i < count; i++)
        {
            StartCoroutine(
                AnimateButton(i, opening)
            );

            if (staggerDelay > 0)
                yield return new WaitForSeconds(staggerDelay);
        }

        yield return new WaitForSeconds(slideDuration);

        if (opening && isOpen)
        {
            foreach (CanvasGroup group in canvasGroups)
            {
                group.interactable = true;
                group.blocksRaycasts = true;
            }
        }

        animationCoroutine = null;
    }

    private IEnumerator AnimateButton(int index, bool opening)
    {
        RectTransform button = subButtons[index];
        CanvasGroup group = canvasGroups[index];

        Vector2 startPosition = button.anchoredPosition;
        Vector2 endPosition = opening
            ? targetPositions[index]
            : menuButton.anchoredPosition;

        float startAlpha = group.alpha;
        float endAlpha = opening ? 1f : 0f;

        float elapsed = 0f;

        while (elapsed < slideDuration)
        {
            elapsed += Time.deltaTime;

            float t = Mathf.Clamp01(elapsed / slideDuration);

            // Smooth animation
            float smoothT = Mathf.SmoothStep(0f, 1f, t);

            button.anchoredPosition = Vector2.Lerp(
                startPosition,
                endPosition,
                smoothT
            );

            group.alpha = Mathf.Lerp(
                startAlpha,
                endAlpha,
                smoothT
            );

            yield return null;
        }

        button.anchoredPosition = endPosition;
        group.alpha = endAlpha;
    }
}
