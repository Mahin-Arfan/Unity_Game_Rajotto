using UnityEngine.Audio;
using UnityEngine;

[System.Serializable]
public class Sound
{
    public bool enabledOnStart;

    public string name;

    public AudioClip clip;

    public AudioMixerGroup mixerGroup;

    public GameObject location;

    [Range(0f, 1f)]
    public float volume;
    [Range(-3f, 3f)]
    public float pitch;
    [Range(0f, 1f)]
    [Tooltip("0: 2D, 1: 3D")]
    public float spatialBlend;
    public float minDistance = 1f;
    public float maxDistance = 100f;

    public bool loop;

    [HideInInspector]
    public AudioSource source;
}

[System.Serializable]
public class Dialogue
{
    public bool enabledOnStart;

    public string name;

    public AudioClip clip;

    public AudioMixerGroup mixerGroup;

    public GameObject location;

    [Range(0f, 1f)]
    public float volume;
    [Range(-3f, 3f)]
    public float pitch;
    [Range(0f, 1f)]
    [Tooltip("0: 2D, 1: 3D")]
    public float spatialBlend;
    public float minDistance = 1f;
    public float maxDistance = 100f;

    public bool loop;

    [HideInInspector]
    public AudioSource source;
}