using UnityEngine;
using UnityEngine.Audio;

namespace AudioSystem
{
    public class AudioManager : AudioController
    {
        [SerializeField] private AudioMixer mixer;
        public static AudioManager Instance;

        protected override void Awake()
        {
            base.Awake();
            Instance = this;
        }

        public void SetMixerValue(string groupName, float value)
        {
            mixer.SetFloat(groupName, Mathf.Log10(value) * 20f);
        }
    }
}