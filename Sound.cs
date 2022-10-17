using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Audio;

namespace AudioSystem
{
    [Serializable]
    public class Sound
    {
        [field: SerializeField] public string Name { get; private set; }
        [SerializeField] private AudioClip[] clips;
        [SerializeField] private AudioMixerGroup outputAudioMixerGroup;
        [SerializeField] private bool playOnAwake, loop;
        [SerializeField] [Range(0f, 1f)] private float volume = 1f;
        [SerializeField] [Range(.1f, 3f)] private float pitch = 1f;
        [SerializeField] [Range(-1f, 1f)] private float stereoPan;
        [SerializeField] [Range(0f, 1f)] private float spatialBlend;
        [SerializeField] private float defaultBlendTime;

        [SerializeField] [Range(0f, 5f)] private float dopplerLevel = 1;
        [SerializeField] [Range(0f, 360f)] private float spread;
        [SerializeField] private AudioRolloffMode volumeRolloff = AudioRolloffMode.Logarithmic;
        [SerializeField] private float minDistance = 1f, maxDistance = 500f;

        private AudioController _audioController;
        private AudioSource[] _sources;

        private int _currentSoundIndex;
        private Coroutine _playingCoroutine, _pitchRoutine;
        private Coroutine[] _stoppingCoroutines;

        public int Count => clips.Length;
        private bool SoundPlaying => _playingCoroutine != null;

        public void Initialize(AudioController audioController)
        {
            if (Count == 0)
            {
                Debug.LogError($"No clips on {audioController.name}", audioController.gameObject);
                return;
            }

            if (Name == string.Empty)
            {
                Debug.LogError($"No name for {this} on {audioController.name}", audioController.gameObject);
                return;
            }

            _audioController = audioController;
            _sources = new AudioSource[Count];
            _stoppingCoroutines = new Coroutine[Count];

            for (var i = 0; i < Count; i++)
            {
                _sources[i] = _audioController.gameObject.AddComponent<AudioSource>();
                _sources[i].clip = clips[i];
                _sources[i].outputAudioMixerGroup = outputAudioMixerGroup;
                _sources[i].playOnAwake = false;
                _sources[i].loop = loop;
                _sources[i].volume = 0f;
                _sources[i].pitch = pitch;
                _sources[i].panStereo = stereoPan;
                _sources[i].spatialBlend = spatialBlend;
                _sources[i].dopplerLevel = dopplerLevel;
                _sources[i].spread = spread;
                _sources[i].rolloffMode = volumeRolloff;
                _sources[i].minDistance = minDistance;
                _sources[i].maxDistance = maxDistance;
            }

            if (playOnAwake)
            {
                Play(0);
            }
        }

        private IEnumerator PlayRoutine(int index, float blendTime)
        {
            var timer = 0f;
            _currentSoundIndex = index;
            _sources[index].Play();

            if (blendTime == 0)
            {
                _sources[index].volume = volume;
            }

            while (timer < blendTime)
            {
                timer += Time.deltaTime;
                var t = timer / blendTime;
                var v = Mathf.Lerp(0f, volume, t);
                _sources[index].volume = v;
                yield return null;
            }
        }

        private IEnumerator StopRoutine(float blendTime)
        {
            var timer = 0f;
            var index = _currentSoundIndex;
            var sourceVolume = _sources[index].volume;

            if (blendTime == 0)
            {
                _sources[index].volume = 0f;
            }

            while (timer < blendTime)
            {
                timer += Time.deltaTime;
                var t = timer / blendTime;
                var v = Mathf.Lerp(sourceVolume, 0f, t);
                _sources[index].volume = v;
                yield return null;
            }

            _sources[index].Stop();
            _stoppingCoroutines[index] = null;
        }

        private IEnumerator PitchRoutine(float targetPitch, float blendTime)
        {
            var timer = 0f;
            var currentPitch = _sources[_currentSoundIndex].pitch;

            if (blendTime == 0)
            {
                _sources[_currentSoundIndex].pitch = targetPitch;
            }

            while (timer < blendTime)
            {
                timer += Time.deltaTime;
                var t = timer / blendTime;
                var p = Mathf.Lerp(currentPitch, targetPitch, t);
                foreach (var source in _sources)
                {
                    source.pitch = p;
                }

                yield return null;
            }

            _pitchRoutine = null;
        }

        #region Play

        public void Play(int index, float blendTime)
        {
            index %= Count;
            var switchingSound = index != _currentSoundIndex;
            var soundIsStopping = _stoppingCoroutines[index] != null;

            if (soundIsStopping)
            {
                _audioController.StopCoroutine(_stoppingCoroutines[index]);
                _stoppingCoroutines[index] = null;
            }

            if (SoundPlaying)
            {
                _audioController.StopCoroutine(_playingCoroutine);
                if (!switchingSound) return;
                _stoppingCoroutines[_currentSoundIndex] = _audioController.StartCoroutine(StopRoutine(blendTime));
                _playingCoroutine = _audioController.StartCoroutine(PlayRoutine(index, blendTime));
            }
            else
            {
                _playingCoroutine = _audioController.StartCoroutine(PlayRoutine(index, blendTime));
            }
        }

        public void Play(int index)
        {
            Play(index, defaultBlendTime);
        }

        public void Play(float blendTime)
        {
            Play(UnityEngine.Random.Range(0, Count), blendTime);
        }

        public void Play()
        {
            Play(UnityEngine.Random.Range(0, Count), defaultBlendTime);
        }

        #endregion

        #region Stop

        public void Stop(float blendTime)
        {
            if (!SoundPlaying) return;

            _audioController.StopCoroutine(_playingCoroutine);
            _playingCoroutine = null;
            _stoppingCoroutines[_currentSoundIndex] = _audioController.StartCoroutine(StopRoutine(blendTime));
        }

        public void Stop()
        {
            Stop(defaultBlendTime);
        }

        #endregion

        #region Pitch

        public void SetPitch(float targetPitch, float blendTime)
        {
            if (_pitchRoutine != null)
            {
                _audioController.StopCoroutine(_pitchRoutine);
                _pitchRoutine = null;
            }

            _pitchRoutine = _audioController.StartCoroutine(PitchRoutine(targetPitch, blendTime));
        }

        public void SetPitch(float targetPitch)
        {
            SetPitch(targetPitch, defaultBlendTime);
        }

        public void ResetPitch()
        {
            SetPitch(pitch);
        }

        #endregion
    }
}