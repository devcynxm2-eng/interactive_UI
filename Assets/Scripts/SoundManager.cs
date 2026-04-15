using UnityEngine;

public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance;

    [Header("Audio Sources")]
    public AudioSource sfxSource;
    public AudioSource musicSource;

    [Header("Clips")]
    public AudioClip correctSound;
    public AudioClip wrongSound;
    public AudioClip buttonClickSound;
    public AudioClip backgroundMusic;

    [Header("Music Toggles")]
    public GameObject Togglefoground1;  
    public GameObject Togglefoback1;    

    [Header("SFX Toggles")]
    public GameObject Togglefoground2;  
    public GameObject Togglefoback2;    

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
        LoadSettings();
        PlayMusic();
    }

    // ───── SFX ─────
    public void PlayCorrect()
    {
        if (!sfxSource.mute)
            sfxSource.PlayOneShot(correctSound);
    }

    public void PlayWrong()
    {
        if (!sfxSource.mute)
            sfxSource.PlayOneShot(wrongSound);
    }

    public void PlayButtonClick()
    {
        if (!sfxSource.mute)
            sfxSource.PlayOneShot(buttonClickSound);
    }

    // ───── MUSIC ─────
    public void PlayMusic()
    {
        bool musicOn = PlayerPrefs.GetInt("music", 1) == 1;
        bool sfxOn = PlayerPrefs.GetInt("sfx", 1) == 1;

        musicSource.clip = backgroundMusic;
        musicSource.loop = true;

        // ✅ Music toggles
        Togglefoground1.SetActive(musicOn);
        Togglefoback1.SetActive(!musicOn);

        // ✅ SFX toggles
        Togglefoground2.SetActive(sfxOn);
        Togglefoback2.SetActive(!sfxOn);

        musicSource.mute = !musicOn;

        if (musicOn)
            musicSource.Play();
        else
            musicSource.Stop();
    }

    // ───── TOGGLES ─────
    public void SetMusicEnabled(bool isOn)
    {
        PlayerPrefs.SetInt("music", isOn ? 1 : 0);
        PlayerPrefs.Save();

        // ✅ Update music visuals only
        Togglefoground1.SetActive(isOn);
        Togglefoback1.SetActive(!isOn);

        musicSource.mute = !isOn;

        if (isOn)
        {
            if (!musicSource.isPlaying)
                musicSource.Play();
        }
        else
        {
            musicSource.Stop(); // ✅ was wrongly calling Play() when muting
        }
    }

    public void SetSFXEnabled(bool isOn)
    {
        PlayerPrefs.SetInt("sfx", isOn ? 1 : 0);
        PlayerPrefs.Save();

        sfxSource.mute = !isOn; // ✅ was completely missing

        // ✅ Update SFX visuals (was using wrong toggles 1 instead of 2)
        Togglefoground2.SetActive(isOn);
        Togglefoback2.SetActive(!isOn);
    }

    // ───── VOLUME ─────
    public void SetMusicVolume(float value)
    {
        musicSource.volume = value;
        PlayerPrefs.SetFloat("musicVol", value);
        PlayerPrefs.Save();
    }

    public void SetSFXVolume(float value)
    {
        sfxSource.volume = value;
        PlayerPrefs.SetFloat("sfxVol", value);
        PlayerPrefs.Save();
    }

    // ───── LOAD SETTINGS ─────
    void LoadSettings()
    {
        bool musicOn = PlayerPrefs.GetInt("music", 1) == 1;
        bool sfxOn = PlayerPrefs.GetInt("sfx", 1) == 1;
        float musicVol = PlayerPrefs.GetFloat("musicVol", 1f);
        float sfxVol = PlayerPrefs.GetFloat("sfxVol", 1f);

        musicSource.mute = !musicOn;
        sfxSource.mute = !sfxOn;
        musicSource.volume = musicVol;
        sfxSource.volume = sfxVol;
    }
}