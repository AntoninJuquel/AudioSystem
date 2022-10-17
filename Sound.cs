using System;
using UnityEngine;
using UnityEngine.Audio;
using System.Collections.Generic;

namespace AudioSystem
{
    [Serializable]
    public class Sound
    {
        public string name;
        public AudioClip[] clips;
        public AudioMixerGroup outputAudioMixerGroup;

        [Range(0f, 1f)] public float volume = 1f;
        [Range(0f, 1f)] public float spatialBlend;
        [Range(.1f, 3f)] public float pitch = 1f;
        public bool loop;
        [HideInInspector] public List<AudioSource> sources;
        public int index;
    }
}