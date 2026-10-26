using UnityEngine;
using UnityEngine.UI;
using TMPro;

public sealed class OptionsScreen : UIScreen
{
    [Header("Audio")]
    [SerializeField] private Slider masterSlider;
    [SerializeField] private TextMeshProUGUI masterTextSlider;
    [SerializeField] private Slider musicSlider;
    [SerializeField] private TextMeshProUGUI musicTextSlider;
    [SerializeField] private Slider sfxSlider;
    [SerializeField] private TextMeshProUGUI sfxTextSlider;

    protected override void OnShowComplete() => Refresh();

    protected override void OnEnable() { base.OnEnable(); Refresh(); }
    
    private void Refresh() {
        var s = GameSettings.Instance; if (s == null) return;
        masterSlider.SetValueWithoutNotify(s.MasterVolume); masterTextSlider.text = Mathf.RoundToInt(s.MasterVolume*100).ToString();
        musicSlider.SetValueWithoutNotify(s.MusicVolume); musicTextSlider.text = Mathf.RoundToInt(s.MusicVolume*100).ToString();
        sfxSlider.SetValueWithoutNotify(s.SFXVolume); sfxTextSlider.text = Mathf.RoundToInt(s.SFXVolume*100).ToString();
    }

    public void SetMasterVolume(float value)
    {
        value = Mathf.Clamp01(value);
        GameSettings.Instance?.SetMasterVolume(value);
        masterTextSlider.text = Mathf.RoundToInt(value * 100).ToString();
    }

    public void SetMusicVolume(float value)
    {
        value = Mathf.Clamp01(value);
        GameSettings.Instance?.SetMusicVolume(value);
        musicTextSlider.text = Mathf.RoundToInt(value * 100).ToString();
    }

    public void SetSFXVolume(float value)
    {
        value = Mathf.Clamp01(value);
        GameSettings.Instance?.SetSFXVolume(value);
        sfxTextSlider.text = Mathf.RoundToInt(value * 100).ToString();
    }

    public void Back()
    {
        if (MainMenuController.Instance != null)
            MainMenuController.Instance.BackToMainMenu();
        else
            Debug.LogWarning("Cannot navigate back: MainMenuController.Instance is missing.", this);
    }
}
