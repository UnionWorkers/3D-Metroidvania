using UnityEngine;
using System;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    [SerializeField] private SoundsPresetSO musicPreset, sfxPreset;
    [SerializeField] private bool playPreset = false;
    public Sound[] musicSounds, sfxSounds;
    public AudioSource musicSource, sfxSource;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        PlayMusic("ThisLevelTheme");
    }

    public void PlayMusic(string name)
    {
        Sound s = null;
     
        if (playPreset)
        {
            s = musicPreset.GetSoundByName(name);
        }
        else
        {
            s = Array.Find(musicSounds, x => x.name == name);
        }

        if (s == null)
        {
            Debug.Log("Sound Not Found");
        }

        else
        {
            musicSource.clip = s.clip;
            musicSource.Play();
        }
    }

    public void PlaySFX(string name)
    {
        Sound s = null;
        if (playPreset)
        {
            s = sfxPreset.GetSoundByName(name);
        }
        else
        {
            s = Array.Find(sfxSounds, x => x.name == name);
        }

        if (s == null)
        {
            Debug.Log("Sound Not Found");
        }

        else
        {
            sfxSource.PlayOneShot(s.clip);
        }
    }
}
