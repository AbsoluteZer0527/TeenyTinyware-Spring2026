using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("BGM")]
    public AudioClip bgmClip;

    [Header("Lose SFX")]
    public AudioClip p1LoseClip;
    public AudioClip p2LoseClip;

    private AudioSource _bgmSource;
    private AudioSource _sfxSource;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        _bgmSource = gameObject.AddComponent<AudioSource>();
        _bgmSource.loop = true;
        _sfxSource = gameObject.AddComponent<AudioSource>();
    }

    private void Start()
    {
        if (bgmClip != null) { _bgmSource.clip = bgmClip; _bgmSource.Play(); }
    }

    public void PlaySFX(AudioClip clip)
    {
        if (clip != null) _sfxSource.PlayOneShot(clip);
    }
}
