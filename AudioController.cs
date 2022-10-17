using System.Collections.Generic;
using UnityEngine;

namespace AudioSystem
{
    public class AudioController : MonoBehaviour
    {
        [SerializeField] private Sound[] sounds;
        private readonly Dictionary<string, Sound> _sounds = new();

#if UNITY_EDITOR
        private bool _firstDeserialization = true;
        private int _arrayLength = 0;

        private void OnValidate()
        {
            if (_firstDeserialization)
            {
                _arrayLength = sounds.Length;
                _firstDeserialization = false;
            }
            else
            {
                if (sounds.Length == _arrayLength) return;
                if (sounds.Length > _arrayLength)
                {
                    for (var i = _arrayLength; i < sounds.Length; i++)
                        sounds[i] = new Sound();
                }

                _arrayLength = sounds.Length;
            }
        }
#endif
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