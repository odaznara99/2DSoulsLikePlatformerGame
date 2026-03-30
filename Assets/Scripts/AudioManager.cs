using UnityEngine;
using System.Collections.Generic;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    [Header("Audio Sources")]
    public AudioSource musicSource;
    public AudioSource sfxSource;
    public AudioSource fightMusicSource;

    [Header("Audio Clips")]
    public List<AudioClip> musicClips;
    public List<AudioClip> sfxClips;

    private Dictionary<string, AudioClip> musicDict = new Dictionary<string, AudioClip>();
    private Dictionary<string, AudioClip> sfxDict = new Dictionary<string, AudioClip>();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        foreach (var clip in musicClips)
            if (!musicDict.ContainsKey(clip.name))
                musicDict.Add(clip.name, clip);

        foreach (var clip in sfxClips)
            if (!sfxDict.ContainsKey(clip.name))
                sfxDict.Add(clip.name, clip);

        LoadAudioSettings();
    }

    public void PlayMusic(string clipName, bool loop = true)
    {
        try
        {
            musicSource.Stop();

            if (musicDict.TryGetValue(clipName, out AudioClip clip))
            {
                musicSource.clip = clip;
                musicSource.loop = loop;
                musicSource.Play();
            }
            else
            {
                Debug.LogWarning($"Music '{clipName}' not found.");
            }
        }
        catch
        (System.Exception ex)
        {
            Debug.LogWarning($"[Audio Manager]Error playing music '{clipName}': {ex.Message}");
        }
    }

    public void PlayFightMusic() {   
        fightMusicSource.Play();
    }

    public void StopFightMusic() {
        fightMusicSource.Stop();
    }

    public void StopMusic()
    {
        musicSource.Stop();
    }

    public void PlaySFX(string clipName)
    {
        if (sfxDict.TryGetValue(clipName, out AudioClip clip))
        {
            sfxSource.PlayOneShot(clip);
        }
        else
        {
            Debug.LogWarning($"SFX '{clipName}' not found.");
        }
    }

    public void PlaySFX(AudioClip audioClip)
    {
        sfxSource.PlayOneShot(audioClip);
    }

    private float musicVolume = 1f;
    private float sfxVolume = 1f;

    public bool musicEnabled = true;
    public bool sfxEnabled = true;

    public void SetMusicVolume(float volume)
    {
        musicVolume = volume;
        musicSource.volume = musicEnabled ? musicVolume : 0f;
    }

    public void SetSFXVolume(float volume)
    {
        sfxVolume = volume;
        sfxSource.volume = sfxEnabled ? sfxVolume : 0f;
    }

    public void ToggleMusic(bool isOn)
    {
        musicEnabled = isOn;
        musicSource.volume = isOn ? musicVolume : 0f;
    }

    public void ToggleSFX(bool isOn)
    {
        sfxEnabled = isOn;
        sfxSource.volume = isOn ? sfxVolume : 0f;
    }

    private void LoadAudioSettings()
    {
        musicVolume = PlayerPrefs.GetFloat(PlayerPrefKeys.MusicVolume, 1f);
        sfxVolume = PlayerPrefs.GetFloat(PlayerPrefKeys.SFXVolume, 1f);
        musicEnabled = PlayerPrefs.GetInt(PlayerPrefKeys.MusicEnabled, 1) == 1;
        sfxEnabled = PlayerPrefs.GetInt(PlayerPrefKeys.SFXEnabled, 1) == 1;

        SetMusicVolume(musicVolume);
        SetSFXVolume(sfxVolume);
        ToggleMusic(musicEnabled);
        ToggleSFX(sfxEnabled);
    }

    private void Start()
    {
        StopFightMusic();
    }


}
