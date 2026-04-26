using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("BGM")]
    public AudioClip bgmClip;

    [Header("Cauldron ambient")]
    public AudioClip bubbleClip;

    [Header("Lose SFX")]
    public AudioClip p1LoseClip;
    public AudioClip p2LoseClip;

    [Header("Score SFX")]
    public AudioClip scoreUpClip;
    public AudioClip scoreDownClip;

    [Header("Recipe spin SFX")]
    public AudioClip spinClip;

    [Header("Cauldron switch SFX")]
    public AudioClip switchClip;

    [Header("Ambient creature SFX")]
    public AudioClip catIdleClip;
    public Vector2   catIdleInterval  = new Vector2(6f, 14f);

    public AudioClip crowIdleClip;
    public Vector2   crowIdleInterval = new Vector2(8f, 18f);

    private const int SfxPoolSize = 8;

    private AudioSource   _bgmSource;
    private AudioSource   _bubbleSource;
    private AudioSource[] _sfxPool;
    private int           _sfxPoolIndex;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;

        _bgmSource      = gameObject.AddComponent<AudioSource>();
        _bgmSource.loop = true;
        _bubbleSource      = gameObject.AddComponent<AudioSource>();
        _bubbleSource.loop = true;

        _sfxPool = new AudioSource[SfxPoolSize];
        for (int i = 0; i < SfxPoolSize; i++)
            _sfxPool[i] = gameObject.AddComponent<AudioSource>();
    }

    private void Start()
    {
        if (bgmClip    != null) { _bgmSource.clip    = bgmClip;    _bgmSource.Play();    }
        if (bubbleClip != null) { _bubbleSource.clip = bubbleClip; _bubbleSource.Play(); }

        if (catIdleClip  != null) StartCoroutine(RandomIdleLoop(catIdleClip,  catIdleInterval));
        if (crowIdleClip != null) StartCoroutine(RandomIdleLoop(crowIdleClip, crowIdleInterval));
    }

    private System.Collections.IEnumerator RandomIdleLoop(AudioClip clip, Vector2 interval)
    {
        while (true)
        {
            yield return new WaitForSeconds(Random.Range(interval.x, interval.y));
            PlaySFX(clip);
        }
    }

    public void RestartBGM()
    {
        if (_bgmSource.clip != null) { _bgmSource.Stop(); _bgmSource.Play(); }
    }

    public void PlaySFX(AudioClip clip)
    {
        if (clip == null) return;

        // find a free source; fall back to round-robin if all are busy
        for (int i = 0; i < SfxPoolSize; i++)
        {
            int idx = (_sfxPoolIndex + i) % SfxPoolSize;
            if (!_sfxPool[idx].isPlaying)
            {
                _sfxPool[idx].PlayOneShot(clip);
                _sfxPoolIndex = (idx + 1) % SfxPoolSize;
                return;
            }
        }
        _sfxPool[_sfxPoolIndex].PlayOneShot(clip);
        _sfxPoolIndex = (_sfxPoolIndex + 1) % SfxPoolSize;
    }

    public void PlayScore(int delta)
    {
        if (delta > 0) PlaySFX(scoreUpClip);
        else if (delta < 0) PlaySFX(scoreDownClip);
    }

    public void PlaySpin()   => PlaySFX(spinClip);
    public void PlaySwitch() => PlaySFX(switchClip);
}
