using UnityEngine;
using System.Collections.Generic;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    [System.Serializable]
    public class AudioSourceSettings
    {
        public AudioSource musicSource;
        public AudioSource sfxSource;
        public AudioSource fightMusicSource;
    }

    [System.Serializable]
    public class AudioClipSettings
    {
        public List<AudioClip> musicClips;
        public List<AudioClip> sfxClips;
    }

    [Header("Audio Sources")]
    public AudioSourceSettings audioSources = new AudioSourceSettings();

    [Header("Audio Clips")]
    public AudioClipSettings audioClips = new AudioClipSettings();

    private Dictionary<string, AudioClip> musicDict = new Dictionary<string, AudioClip>();
    private Dictionary<string, AudioClip> sfxDict   = new Dictionary<string, AudioClip>();

    private float musicVolume = 1f;
    private float sfxVolume   = 1f;

    public bool musicEnabled = true;
    public bool sfxEnabled   = true;

    /// <summary>
    /// Enforces singleton behaviour, populates lookup dictionaries, and loads saved audio settings.
    /// </summary>
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        foreach (var clip in audioClips.musicClips)
            if (!musicDict.ContainsKey(clip.name))
                musicDict.Add(clip.name, clip);

        foreach (var clip in audioClips.sfxClips)
            if (!sfxDict.ContainsKey(clip.name))
                sfxDict.Add(clip.name, clip);

        LoadAudioSettings();
    }

    /// <summary>
    /// Stops any currently playing fight music when the scene starts.
    /// </summary>
    private void Start()
    {
        StopFightMusic();
    }

    /// <summary>
    /// Stops any currently playing music, then plays the named clip on the music source.
    /// </summary>
    /// <param name="clipName">The name of the music clip to play.</param>
    /// <param name="loop">Whether the clip should loop.</param>
    public void PlayMusic(string clipName, bool loop = true)
    {
        audioSources.musicSource.Stop();

        if (musicDict.TryGetValue(clipName, out AudioClip clip))
        {
            audioSources.musicSource.clip = clip;
            audioSources.musicSource.loop = loop;
            audioSources.musicSource.Play();
        }
        else
        {
            Debug.LogWarning($"Music '{clipName}' not found.");
        }
    }

    /// <summary>
    /// Starts playback of the fight music source.
    /// </summary>
    public void PlayFightMusic()
    {
        audioSources.fightMusicSource.Play();
    }

    /// <summary>
    /// Stops playback of the fight music source.
    /// </summary>
    public void StopFightMusic()
    {
        audioSources.fightMusicSource.Stop();
    }

    /// <summary>
    /// Stops playback of the main music source.
    /// </summary>
    public void StopMusic()
    {
        audioSources.musicSource.Stop();
    }

    /// <summary>
    /// Plays the named SFX clip as a one-shot on the SFX source.
    /// </summary>
    /// <param name="clipName">The name of the SFX clip to play.</param>
    public void PlaySFX(string clipName)
    {
        if (sfxDict.TryGetValue(clipName, out AudioClip clip))
        {
            audioSources.sfxSource.PlayOneShot(clip);
        }
        else
        {
            Debug.LogWarning($"SFX '{clipName}' not found.");
        }
    }

    /// <summary>
    /// Plays the provided audio clip as a one-shot on the SFX source.
    /// </summary>
    /// <param name="audioClip">The clip to play.</param>
    public void PlaySFX(AudioClip audioClip)
    {
        audioSources.sfxSource.PlayOneShot(audioClip);
    }

    /// <summary>
    /// Sets the music volume and applies it immediately, respecting the mute toggle.
    /// </summary>
    /// <param name="volume">Volume level between 0 and 1.</param>
    public void SetMusicVolume(float volume)
    {
        musicVolume = volume;
        audioSources.musicSource.volume = musicEnabled ? musicVolume : 0f;
    }

    /// <summary>
    /// Sets the SFX volume and applies it immediately, respecting the mute toggle.
    /// </summary>
    /// <param name="volume">Volume level between 0 and 1.</param>
    public void SetSFXVolume(float volume)
    {
        sfxVolume = volume;
        audioSources.sfxSource.volume = sfxEnabled ? sfxVolume : 0f;
    }

    /// <summary>
    /// Enables or disables music playback by setting the music source volume.
    /// </summary>
    /// <param name="isOn">True to enable music; false to mute it.</param>
    public void ToggleMusic(bool isOn)
    {
        musicEnabled = isOn;
        audioSources.musicSource.volume = isOn ? musicVolume : 0f;
    }

    /// <summary>
    /// Enables or disables SFX playback by setting the SFX source volume.
    /// </summary>
    /// <param name="isOn">True to enable SFX; false to mute it.</param>
    public void ToggleSFX(bool isOn)
    {
        sfxEnabled = isOn;
        audioSources.sfxSource.volume = isOn ? sfxVolume : 0f;
    }

    /// <summary>
    /// Loads saved volume levels and enabled states from PlayerPrefs and applies them.
    /// </summary>
    private void LoadAudioSettings()
    {
        musicVolume  = PlayerPrefs.GetFloat(PlayerPrefKeys.MusicVolume, 1f);
        sfxVolume    = PlayerPrefs.GetFloat(PlayerPrefKeys.SFXVolume, 1f);
        musicEnabled = PlayerPrefs.GetInt(PlayerPrefKeys.MusicEnabled, 1) == 1;
        sfxEnabled   = PlayerPrefs.GetInt(PlayerPrefKeys.SFXEnabled, 1) == 1;

        SetMusicVolume(musicVolume);
        SetSFXVolume(sfxVolume);
        ToggleMusic(musicEnabled);
        ToggleSFX(sfxEnabled);
    }
}
