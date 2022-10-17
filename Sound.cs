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

        [SerializeField] [Range(0f, 1f)] private float defaultVolume = 1f, volume = 1f;
        [SerializeField] [Range(0f, 1f)] private float spatialBlend;
        [SerializeField] [Range(.1f, 3f)] private float pitch = 1f;
        [SerializeField] private float defaultBlendTime;
        [SerializeField] private bool loop;

        private MonoBehaviour _caller;
        private AudioSource[] _sources;

        private int _currentSoundIndex;
        private Coroutine _playingCoroutine, _pitchRoutine;
        private Coroutine[] _stoppingCoroutines;

        public int Count => clips.Length;
        private bool SoundPlaying => _playingCoroutine != null;

        public void Initialize(MonoBehaviour caller)
        {
            _caller = caller;
            _sources = new AudioSource[Count];
            _stoppingCoroutines = new Coroutine[Count];

            for (var i = 0; i < Count; i++)
            {
                _sources[i] = _caller.gameObject.AddComponent<AudioSource>();
                _sources[i].clip = clips[i];
                _sources[i].volume = 0f;
                _sources[i].pitch = pitch;
                _sources[i].loop = loop;
                _sources[i].spatialBlend = spatialBlend;
                _sources[i].outputAudioMixerGroup = outputAudioMixerGroup;
            }
        }

        private IEnumerator PlayRoutine(int index, float blendTime)
        {
            var timer = 0f;
            _currentSoundIndex = index;
            _sources[index].Play();

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
                _caller.StopCoroutine(_stoppingCoroutines[index]);
                _stoppingCoroutines[index] = null;
            }

            if (SoundPlaying)
            {
                _caller.StopCoroutine(_playingCoroutine);
                if (!switchingSound) return;
                _stoppingCoroutines[_currentSoundIndex] = _caller.StartCoroutine(StopRoutine(blendTime));
                _playingCoroutine = _caller.StartCoroutine(PlayRoutine(index, blendTime));
            }
            else
            {
                _playingCoroutine = _caller.StartCoroutine(PlayRoutine(index, blendTime));
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

            _caller.StopCoroutine(_playingCoroutine);
            _playingCoroutine = null;
            _stoppingCoroutines[_currentSoundIndex] = _caller.StartCoroutine(StopRoutine(blendTime));
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
                _caller.StopCoroutine(_pitchRoutine);
                _pitchRoutine = null;
            }

            _pitchRoutine = _caller.StartCoroutine(PitchRoutine(targetPitch, blendTime));
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