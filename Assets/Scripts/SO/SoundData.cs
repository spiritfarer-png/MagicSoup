using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "AudioData", menuName = "ScriptableObject/声音数据", order = 0)]
public class SoundData : ScriptableObject
{
    public string soundID;
    public AudioClip clip;
    [Range(0f, 1f)] public float volume = 1f;

    [Header("音调随机)")]
    public bool randomizePitch = false;
    public Vector2 pitchRange = new Vector2(0.9f, 1.1f);

    [Header("播放间隔")]
    public float cooldown = 0.05f; // 防止同一帧/极短时间内重复触发
    [HideInInspector] public float lastPlayTime;//上一次播放时间
}
