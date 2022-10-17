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
            Instance = this;
            base.Awake();
        }

        public bool SetMixerValue(string groupName, float value)
        {
            if (mixer) return mixer.SetFloat(groupName, Mathf.Log10(value) * 20f);
            Debug.LogError("No audio mixer set in audio manager", gameObject);
            return false;
        }
    }
}