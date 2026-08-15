using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
public static AudioManager Instance { get; private set; }

    [Header("音乐库")]
    [SerializeField] 
    private List<SoundData> soundDatabase;
    private Dictionary<string, SoundData> soundDict = new Dictionary<string, SoundData>();

    [Header("BGM控制 (俩个用于淡入淡出)")]
    private AudioSource bgmSourceA;
    private AudioSource bgmSourceB;
    private bool isSourceAPlaying = false;
    private Coroutine bgmFadeCoroutine; //用于淡入淡出协程的引用

    [Header("音效对象池")]
    [SerializeField] private int sfxPoolSize = 10;
    private List<AudioSource> sfxPool = new List<AudioSource>();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            Initialize();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Initialize()
    {
        foreach (var data in soundDatabase)
        {
            if (data != null && !soundDict.ContainsKey(data.soundID))
                soundDict.Add(data.soundID, data);
        }

        bgmSourceA = gameObject.AddComponent<AudioSource>();
        bgmSourceB = gameObject.AddComponent<AudioSource>();
        bgmSourceA.loop = true;
        bgmSourceB.loop = true;

        GameObject poolRoot = new GameObject("SFX_Pool");
        poolRoot.transform.SetParent(transform);
        for (int i = 0; i < sfxPoolSize; i++)
        {
            AudioSource source = poolRoot.AddComponent<AudioSource>();
            source.playOnAwake = false;
            sfxPool.Add(source);
        }
    }

    #region SFX 音效播放

    public void PlaySFX(string soundID)
    {
        if (!soundDict.TryGetValue(soundID, out SoundData data))
        {
            Debug.LogWarning($"[AudioManager] 未找到音效: {soundID}");
            return;
        }
        // 冷却限制检测
        if (Time.time - data.lastPlayTime < data.cooldown) return;
        data.lastPlayTime = Time.time;

        AudioSource source = GetAvailableSFXSource();
        if (source == null) return;

            source.spatialBlend = 0f; // 2D (UI / 全局)

        source.clip = data.clip;
        source.pitch = data.randomizePitch 
            ? Random.Range(data.pitchRange.x, data.pitchRange.y) 
            : 1f;

        source.Play();
    }

    private AudioSource GetAvailableSFXSource()
    {
        // 优先查找空闲的 Source
        foreach (var source in sfxPool)
        {
            if (!source.isPlaying) return source;
        }

        // 池子已满时：动态扩容或复用播放时间最长的 Source
        AudioSource newSource = transform.GetChild(0).gameObject.AddComponent<AudioSource>();
        newSource.playOnAwake = false;
        sfxPool.Add(newSource);
        return newSource;
    }

    #endregion

    #region BGM 音乐播放与平滑过渡

    public void PlayBGM(AudioClip clip, float fadeDuration = 1.0f)
    {
        if (clip == null) return;

        AudioSource activeSource = isSourceAPlaying ? bgmSourceA : bgmSourceB;
        AudioSource newSource = isSourceAPlaying ? bgmSourceB : bgmSourceA;

        // 如果已经在播放相同曲目则忽略
        if (activeSource.isPlaying && activeSource.clip == clip) return;

        if (bgmFadeCoroutine != null) StopCoroutine(bgmFadeCoroutine);
        bgmFadeCoroutine = StartCoroutine(CrossfadeBGM(activeSource, newSource, clip, fadeDuration));

        isSourceAPlaying = !isSourceAPlaying;
    }

    private IEnumerator CrossfadeBGM(AudioSource fromSource, AudioSource toSource, AudioClip newClip, float duration)
    {
        toSource.clip = newClip;
        toSource.volume = 0f;
        toSource.Play();

        float timer = 0f;

        float fromInitialVolume = fromSource.isPlaying ? fromSource.volume : 0f;

        while (timer < duration)
        {
            timer += Time.deltaTime;
            float t = timer / duration;

            if (fromSource.isPlaying)
                fromSource.volume = Mathf.Lerp(fromInitialVolume, 0f, t);

            toSource.volume = Mathf.Lerp(0f, 1, t);
            yield return null;
        }

        fromSource.Stop();
        fromSource.volume = 0f;
        toSource.volume = 1;
    }

    #endregion
}
