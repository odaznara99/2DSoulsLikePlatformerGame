using UnityEngine;
using UnityEngine.UI;

public class AudioSettingsUI : MonoBehaviour
{
    public Slider musicSlider;
    public Slider sfxSlider;
    public Toggle musicToggle;
    public Toggle sfxToggle;

    void Start()
    {
        musicSlider.value = AudioManager.Instance.musicSource.volume;
        sfxSlider.value = AudioManager.Instance.sfxSource.volume;

        musicToggle.isOn = AudioManager.Instance.musicEnabled;
        sfxToggle.isOn = AudioManager.Instance.sfxEnabled;

        musicSlider.onValueChanged.AddListener(SetMusicVolume);
        sfxSlider.onValueChanged.AddListener(SetSFXVolume);
        musicToggle.onValueChanged.AddListener(ToggleMusic);
        sfxToggle.onValueChanged.AddListener(ToggleSFX);
    }



    void SetMusicVolume(float volume)
    {
        AudioManager.Instance.SetMusicVolume(volume);
        PlayerPrefs.SetFloat(PlayerPrefKeys.MusicVolume, volume);
    }

    void SetSFXVolume(float volume)
    {
        AudioManager.Instance.SetSFXVolume(volume);
        PlayerPrefs.SetFloat(PlayerPrefKeys.SFXVolume, volume);
    }

    void ToggleMusic(bool isOn)
    {
        AudioManager.Instance.ToggleMusic(isOn);
        PlayerPrefs.SetInt(PlayerPrefKeys.MusicEnabled, isOn ? 1 : 0);
    }

    void ToggleSFX(bool isOn)
    {
        AudioManager.Instance.ToggleSFX(isOn);
        PlayerPrefs.SetInt(PlayerPrefKeys.SFXEnabled, isOn ? 1 : 0);
    }



}
