using UnityEngine.Audio;
using System;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public Sound[] sounds;
    public Dialogue[] dialogues;

    void Awake()
    {
        foreach (Sound s in sounds)
        {
            if (s.location == null)
            {
                s.source = gameObject.AddComponent<AudioSource>();
            }
            else
            {
                s.source = s.location.AddComponent<AudioSource>();
            }
            if (s.mixerGroup != null)
            {
                s.source.outputAudioMixerGroup = s.mixerGroup;
            }
            else
            {
                Debug.LogWarning(s.name + ": Audio Mixer not found!");
            }

            s.source.clip = s.clip;
            s.source.volume = s.volume;
            s.source.pitch = s.pitch;
            s.source.loop = s.loop;
            s.source.spatialBlend = s.spatialBlend;
            s.source.minDistance = s.minDistance;
            s.source.maxDistance = s.maxDistance;
            s.source.rolloffMode = AudioRolloffMode.Linear;

            if (s.enabledOnStart)
            {
                s.source.Play();
            }

            //To Play: FindObjectOfType<AudioManager>.Play("AudioName");
        }
    }

    public void Play(string name)
    {
        Sound s = Array.Find(sounds, sound => sound.name == name);
        if (s == null)
        {
            Debug.LogWarning("Sound: " + name + " not found!");
            return;
        }
        s.source.Play();
    }

    public void PlayDialogue(string name)
    {
        Dialogue d = Array.Find(dialogues, dialogue => dialogue.name == name);
        if (d == null)
        {
            Debug.LogWarning("Sound: " + name + " not found!");
            return;
        }
        if (d.location == null)
        {
            d.source = gameObject.AddComponent<AudioSource>();
        }
        else
        {
            d.source = d.location.AddComponent<AudioSource>();
        }
        if (d.mixerGroup != null)
        {
            d.source.outputAudioMixerGroup = d.mixerGroup;
        }
        else
        {
            Debug.LogWarning(d.name + ": Audio Mixer not found!");
        }

        d.source.clip = d.clip;
        d.source.volume = d.volume;
        d.source.pitch = d.pitch;
        d.source.loop = d.loop;
        d.source.spatialBlend = d.spatialBlend;
        d.source.minDistance = d.minDistance;
        d.source.maxDistance = d.maxDistance;
        d.source.rolloffMode = AudioRolloffMode.Linear;
        d.source.Play();
    }
}
