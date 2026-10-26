using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System;

public class DialogueController : MonoBehaviour
{
    [Header("UI Elements")]
    public TextMeshProUGUI speakerNameText;
    public TextMeshProUGUI dialogueText;
    public GameObject dialoguePanel;

    [Header("Dialogue Data")]
    public DialogueData[] lines;
    public float textDelay;
    public float panelAppearDelay = 0.5f; // Jeda tambahan sebelum panel muncul
    public bool playOnStart = true;

    private int index;
    private Coroutine typingCoroutine;
    private Action onDialogueFinished;
    private bool isPlaying;
    private bool pausesGameplay;

    void Start()
    {
        dialogueText.text = string.Empty;  
        dialoguePanel.SetActive(false);
        pausesGameplay = true;
        if (playOnStart && lines != null && lines.Length > 0)
            StartCoroutine(FirstWait(1f));
    }

    void Update()
    {
        if (isPlaying && Input.GetMouseButtonDown(0) && dialoguePanel.activeSelf) {
            if (dialogueText.text == lines[index].dialogue) {
                NextLine();
            } else {
                if (typingCoroutine != null) StopCoroutine(typingCoroutine);
                dialogueText.text = lines[index].dialogue;
            }
        }
    }

    void StartDialogue() {
        // Kita gunakan Coroutine baru untuk menangani jeda panel
        StartCoroutine(ShowPanelWithDelay());
    }

    public void Play(DialogueData[] dialogueLines, Action completed = null, bool pauseGameplay = true)
    {
        GamePause.Release(PauseReason.Dialogue);
        if (typingCoroutine != null) StopCoroutine(typingCoroutine);
        StopAllCoroutines();
        lines = dialogueLines;
        onDialogueFinished = completed;
        pausesGameplay = pauseGameplay;

        if (lines == null || lines.Length == 0)
        {
            completed?.Invoke();
            return;
        }

        isPlaying = true;
        StartDialogue();
    }

    IEnumerator ShowPanelWithDelay() {
        // 1. Hentikan waktu game terlebih dahulu
        if (pausesGameplay) GamePause.Hold(PauseReason.Dialogue);

        // 2. Tunggu selama beberapa saat (menggunakan Realtime)
        yield return new WaitForSecondsRealtime(panelAppearDelay);

        // 3. Tampilkan panel dan mulai teks
        dialoguePanel.SetActive(true);
        index = 0;
        typingCoroutine = StartCoroutine(Type());
    }

    IEnumerator Type() {
        dialogueText.text = string.Empty;
        speakerNameText.text = lines[index].speakerName;

        foreach (char letter in lines[index].dialogue.ToCharArray()) {
            dialogueText.text += letter;
            yield return new WaitForSecondsRealtime(textDelay / 100f);
        }
    }

    void NextLine() {
        if (index < lines.Length - 1) {
            index++;
            typingCoroutine = StartCoroutine(Type());
        } else {
            dialoguePanel.SetActive(false);
            isPlaying = false;
            if (pausesGameplay) GamePause.Release(PauseReason.Dialogue);
            Action completed = onDialogueFinished;
            onDialogueFinished = null;
            completed?.Invoke();
        }
    }

    IEnumerator FirstWait(float seconds) {
        yield return new WaitForSecondsRealtime(seconds); 
        StartDialogue();
    }
}
