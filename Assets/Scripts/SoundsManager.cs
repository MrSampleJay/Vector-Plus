using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class SoundsManager : MonoBehaviour
{
    private static string PREFAB_PATH = "Other/SoundsManager";

    [Header("Audio Sources")]
    [Tooltip("Used as the first SFX channel; additional channels are created at runtime.")]
    public AudioSource AudioSourceMain;              // SFX channel #0 (pooled)
    public AudioSource AudioSourceBackground;        // Music / Background (single channel)

    [Header("SFX Channel Pool")]
    [Min(1)] public int initialSfxChannels = 11;      // includes AudioSourceMain if assigned
    [Min(1)] public int maxSfxChannels = 16;
    public bool autoExpandPool = true;

    [Header("Defaults")]
    [Range(0f, 1f)] public float defaultSfxVolume = 1f;
    [Range(0f, 1f)] public float defaultMusicVolume = 1f;

    private static SoundsManager _instance;
    public static SoundsManager Instance
    {
        get
        {
            if (_instance == null)
            {
                var prefab = Resources.Load<GameObject>(PREFAB_PATH);
                var obj = Instantiate(prefab);
                obj.name = "SoundsManager";
                DontDestroyOnLoad(obj);
                _instance = obj.GetComponent<SoundsManager>();
            }
            return _instance;
        }
    }

    private readonly List<AudioSource> _sfxSources = new List<AudioSource>();
    private Transform _sfxContainer;

    private void Awake()
    {
        // Singleton guard for scene-dropped instances
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }
        _instance = this;
        DontDestroyOnLoad(gameObject);

        BuildPool();
        Init();
    }

    private void BuildPool()
    {
        // Container object (keeps Hierarchy tidy)
        _sfxContainer = new GameObject("SFX_Channels").transform;
        _sfxContainer.SetParent(transform, false);

        // Use AudioSourceMain as first SFX channel if provided
        if (AudioSourceMain != null)
        {
            PrepareSfxSource(AudioSourceMain);
            _sfxSources.Add(AudioSourceMain);
        }

        // Create extra channels up to initialSfxChannels
        int need = Mathf.Max(1, initialSfxChannels) - _sfxSources.Count;
        for (int i = 0; i < need; i++)
        {
            _sfxSources.Add(CreateSfxSource());
        }

        // Default volumes
        if (AudioSourceBackground != null)
            AudioSourceBackground.volume = defaultMusicVolume;
        foreach (var s in _sfxSources) s.volume = defaultSfxVolume;
    }

    private AudioSource CreateSfxSource()
    {
        var go = new GameObject($"SFX_Channel_{_sfxSources.Count}");
        go.transform.SetParent(_sfxContainer, false);
        var src = go.AddComponent<AudioSource>();
        PrepareSfxSource(src);
        return src;
    }

    private void PrepareSfxSource(AudioSource src)
    {
        src.playOnAwake = false;
        src.volume = AudioSourceMain.volume;
        src.loop = false;
        src.spatialBlend = 0f; // 2D by default
        src.dopplerLevel = 0f;
    }

    private AudioSource GetFreeSfxSource()
    {
        // Prefer an idle source
        var idle = _sfxSources.FirstOrDefault(s => !s.isPlaying);
        if (idle != null) return idle;

        // Expand if allowed and under cap
        if (autoExpandPool && _sfxSources.Count < Mathf.Max(1, maxSfxChannels))
        {
            var extra = CreateSfxSource();
            _sfxSources.Add(extra);
            return extra;
        }

        // Steal the one that’s closest to finishing (least remaining time)
        // Fallback if all are busy and expansion is not allowed or maxed
        AudioSource candidate = _sfxSources[0];
        float minRemaining = Remaining(candidate);
        for (int i = 1; i < _sfxSources.Count; i++)
        {
            float rem = Remaining(_sfxSources[i]);
            if (rem < minRemaining)
            {
                minRemaining = rem;
                candidate = _sfxSources[i];
            }
        }
        candidate.Stop(); // preempt
        return candidate;

        float Remaining(AudioSource s)
        {
            if (s.clip == null || !s.isPlaying) return 0f;
            return Mathf.Max(0f, s.clip.length - s.time);
        }
    }

    public void Init() { /* reserved for future setup */ }

    // ======== Global Controls ========

    public void SetMusicPause(bool state)
    {
        if (AudioSourceBackground != null) AudioSourceBackground.mute = state;
    }

    public void SetSoundsPause(bool state)
    {
        foreach (var s in _sfxSources) s.mute = state;
    }

    public void PauseAll(bool state)
    {
        if (state)
        {
            if (AudioSourceBackground != null) AudioSourceBackground.Pause();
            foreach (var s in _sfxSources) s.Pause();
        }
        else
        {
            if (AudioSourceBackground != null) AudioSourceBackground.UnPause();
            foreach (var s in _sfxSources) s.UnPause();
        }
    }

    public void SetSoundLevel(float value)
    {
        foreach (var s in _sfxSources) s.volume = value;
    }

    public void SetMusicLevel(float value)
    {
        if (AudioSourceBackground != null) AudioSourceBackground.volume = value;
    }

    // ======== Music / Background ========

    public void StopBackground()
    {
        if (AudioSourceBackground != null) AudioSourceBackground.Stop();
    }

    public void PlayBackground(MusicType musicType) => PlayBackground(musicType.ToString());

    public void PlayBackground(string musicName, bool loop = true)
    {
        if (AudioSourceBackground == null)
        {
            Debug.LogWarning("Background AudioSource is not assigned.");
            return;
        }
        AudioSourceBackground.Stop();
        StartCoroutine(ResourcesLoader.LoadMusicClipAsync(musicName, loaded =>
        {
            if (loaded == null)
            {
                Debug.LogError("Music clip not found " + musicName);
                return;
            }
            AudioSourceBackground.clip = loaded;
            AudioSourceBackground.loop = loop;
            AudioSourceBackground.Play();
        }));
    }

    // ======== SFX: Single / Parallel ========

    public void PlaySounds(params SoundType[] soundTypes)
    {
        string[] names = soundTypes.Select(s => s.ToString()).ToArray();
        PlaySounds(names);
    }

    /// <summary>Plays all sounds in parallel on free channels.</summary>
    public void PlaySounds(params string[] soundNames)
    {
        foreach (var name in soundNames)
        {
            StartCoroutine(ResourcesLoader.LoadAudioClipAsync(name, clip =>
            {
                if (clip == null)
                {
                    Debug.LogError("Clip not found " + name);
                    return;
                }
                var src = GetFreeSfxSource();
                src.pitch = 1f;
                src.spatialBlend = 0f;
                src.PlayOneShot(clip);
            }));

        }
    }

    /// <summary>Play one sound once with custom volume and optional pitch.</summary>
    public void PlaySoundsOnce(string soundName, float volume, float pitch = 1f)
    {
        StartCoroutine(ResourcesLoader.LoadAudioClipAsync(soundName, clip =>
        {
            if (clip == null)
            {
                Debug.LogError("Clip not found " + soundName);
                return;
            }
            var src = GetFreeSfxSource();
            src.pitch = pitch;
            src.spatialBlend = 0f;
            src.PlayOneShot(clip, Mathf.Clamp01(volume));
        }));
    }

    /// <summary>3D variant at a world position.</summary>
    public void PlaySoundAt(string soundName, Vector3 position, float volume = 1f, float pitch = 1f, float spatialBlend = 1f)
    {
        StartCoroutine(ResourcesLoader.LoadAudioClipAsync(soundName, clip =>
        {
            if (clip == null)
            {
                Debug.LogError("Clip not found " + soundName);
                return;
            }
            var src = GetFreeSfxSource();
            src.transform.position = position;
            src.pitch = pitch;
            src.spatialBlend = Mathf.Clamp01(spatialBlend);
            src.minDistance = 1f;
            src.maxDistance = 25f;
            src.rolloffMode = AudioRolloffMode.Linear;
            src.PlayOneShot(clip, Mathf.Clamp01(volume));
        }));        
    }

    // ======== SFX: Sequential ========

    /// <summary>Plays the provided sounds sequentially on a dedicated channel.</summary>
    public void PlaySoundsSequential(params string[] soundNames)
    {
        StartCoroutine(PlaySequentially(soundNames));
    }

    /// <summary>Plays SoundTypes sequentially.</summary>
    public void PlaySoundsSequential(params SoundType[] soundTypes)
    {
        string[] names = soundTypes.Select(s => s.ToString()).ToArray();
        StartCoroutine(PlaySequentially(names));
    }

    private IEnumerator PlaySequentially(params string[] soundNames)
    {
        var src = GetFreeSfxSource();
        src.pitch = 1f;
        src.spatialBlend = 0f;

        foreach (var name in soundNames)
        {
            AudioClip clip = null;

            yield return ResourcesLoader.LoadAudioClipAsync(name, loaded => clip = loaded);

            if (clip == null)
            {
                Debug.LogError("Clip not found " + name);
                continue;
            }

            src.clip = clip;
            src.Play();

            // Wait for the clip to finish
            while (src != null && src.isPlaying)
                yield return null;
        }
    }

    // ======== Utilities ========

    public void StopAllSfx()
    {
        foreach (var s in _sfxSources) s.Stop();
    }

    public bool IsAnySfxPlaying()
    {
        return _sfxSources.Any(s => s.isPlaying);
    }
}