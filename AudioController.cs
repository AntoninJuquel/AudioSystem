using System.Collections.Generic;
using UnityEngine;

namespace AudioSystem
{
    public class AudioController : MonoBehaviour
    {
        [SerializeField] private Sound[] sounds;
        private readonly Dictionary<string, Sound> _sounds = new();

        protected virtual void Awake()
        {
            foreach (var sound in sounds)
            {
                sound.Initialize(this);
                _sounds.Add(sound.Name, sound);
            }
        }

        public void Play(string soundName)
        {
            _sounds[soundName].Play();
        }

        public void Play(string soundName, int index)
        {
            _sounds[soundName].Play(index);
        }

        public void Play(string soundName, float blendTime)
        {
            _sounds[soundName].Play(blendTime);
        }

        public void Play(string soundName, int index, float blendTime)
        {
            _sounds[soundName].Play(index, blendTime);
        }

        public void Stop(string soundName)
        {
            _sounds[soundName].Stop();
        }

        public void Stop(string soundName, float blendTime)
        {
            _sounds[soundName].Stop(blendTime);
        }

        public void SetPitch(string soundName, float targetPitch)
        {
            _sounds[soundName].SetPitch(targetPitch);
        }

        public void SetPitch(string soundName, float targetPitch, float blendTime)
        {
            _sounds[soundName].SetPitch(targetPitch, blendTime);
        }

        public void ResetPitch(string soundName)
        {
            _sounds[soundName].ResetPitch();
        }
    }
}