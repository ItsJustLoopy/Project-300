using UnityEngine;
using System.Collections.Generic;

public class AudioManager : Singleton<AudioManager>
{
    [Header("Audio Sources")]
    [SerializeField] private AudioSource musicSource;
    [SerializeField] private AudioSource sfxSource;
    
    [Header("Background Music")]
    [SerializeField] private AudioClip backgroundMusic;
    [SerializeField] [Range(0f, 1f)] private float musicVolume = 0.5f;
    
    [Header("Sound Effects")]
    [SerializeField] private AudioClip playerMoveSound;
    [SerializeField] private AudioClip blockPushSound;
    [SerializeField] private AudioClip blockCombineSound;
    [SerializeField] private AudioClip elevatorSound;
    [SerializeField] [Range(0f, 1f)] private float sfxVolume = 0.7f;
    
    protected override void Awake()
    {
        base.Awake();
        
        if (musicSource == null)
        {
            GameObject musicObj = new GameObject("MusicSource");
            musicObj.transform.SetParent(transform);
            musicSource = musicObj.AddComponent<AudioSource>();
            musicSource.loop = true;
            musicSource.playOnAwake = false;
        }
        
        if (sfxSource == null)
        {
            GameObject sfxObj = new GameObject("SFXSource");
            sfxObj.transform.SetParent(transform);
            sfxSource = sfxObj.AddComponent<AudioSource>();
            sfxSource.loop = false;
            sfxSource.playOnAwake = false;
        }
        
        musicSource.volume = musicVolume;
        sfxSource.volume = sfxVolume;
    }
    
    private void Start()
    {
        PlayBackgroundMusic();
    }
    
    public void PlayBackgroundMusic()
    {
        if (backgroundMusic != null && !musicSource.isPlaying)
        {
            musicSource.clip = backgroundMusic;
            musicSource.Play();
            Debug.Log("Background music started");
        }
    }
    
    public void StopBackgroundMusic()
    {
        if (musicSource.isPlaying)
        {
            musicSource.Stop();
        }
    }
    
    public void PlayPlayerMoveSound()
    {
        PlaySFX(playerMoveSound);
    }
    
    public void PlayBlockPushSound()
    {
        PlaySFX(blockPushSound);
    }
    
    public void PlayBlockCombineSound()
    {
        PlaySFX(blockCombineSound);
    }
    
    public void PlayElevatorSound()
    {
        PlaySFX(elevatorSound);
    }
    
    private void PlaySFX(AudioClip clip)
    {
        if (clip != null)
        {
            sfxSource.PlayOneShot(clip, sfxVolume);
        }
    }
    
    public void SetMusicVolume(float volume)
    {
        musicVolume = Mathf.Clamp01(volume);
        musicSource.volume = musicVolume;
    }
    
    public void SetSFXVolume(float volume)
    {
        sfxVolume = Mathf.Clamp01(volume);
        sfxSource.volume = sfxVolume;
    }
    
    public void ToggleMusic(bool enabled)
    {
        if (enabled)
        {
            PlayBackgroundMusic();
        }
        else
        {
            StopBackgroundMusic();
        }
    }
}