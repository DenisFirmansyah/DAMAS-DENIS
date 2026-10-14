using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    [SerializeField] private GameObject mainMenu;
    [SerializeField] private GameObject levelMenu;
    [SerializeField] private GameObject optionsMenu;
    [SerializeField] private GameObject creditsMenu;

    bool isFirstTime = true;

    public void PlayGame() {
        if (isFirstTime) {
            isFirstTime = false;
            SceneManager.LoadScene(1);
        } else {
            levelMenu.SetActive(true);
        }
    }

    public void Options() {
        mainMenu.SetActive(false);
        optionsMenu.SetActive(true);
    }

    public void Credits() {
        mainMenu.SetActive(false);
        creditsMenu.SetActive(true);
    }

    public void Back() {
        mainMenu.SetActive(true);
        optionsMenu.SetActive(false);
        creditsMenu.SetActive(false);
    }

    public void ExitGame() {
        Application.Quit();
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #endif
    }
}
