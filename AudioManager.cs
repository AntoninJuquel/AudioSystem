/*
Simple Sound Manager (c) 2016 Digital Ruby, LLC
http://www.digitalruby.com

Source code may no longer be redistributed in source format. Using this in apps and games is fine.
*/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AudioSystem
{
    /// <summary>
    /// Do not add this script in the inspector. Just call the static methods from your own scripts or use the AudioSource extension methods.
    /// </summary>
    public class AudioManager : MonoBehaviour
    {
        private static int _persistTag;
        private static AudioManager _instance;
        private static readonly List<LoopingAudioSource> Music = new List<LoopingAudioSource>();
        private static readonly List<AudioSource> MusicOneShot = new List<AudioSource>();
        private static readonly List<LoopingAudioSource> Sounds = new List<LoopingAudioSource>();
        private static readonly HashSet<LoopingAudioSource> PersistedSounds = new HashSet<LoopingAudioSource>();
        private static readonly Dictionary<AudioClip, List<float>> SoundsOneShot = new Dictionary<AudioClip, List<float>>();
        private static float _soundVolume = 1.0f;
        private static float _musicVolume = 1.0f;
        private static bool _updated;

        /// <summary>
        /// Maximum number of the same audio clip to play at once
        /// </summary>
        public static int MaxDuplicateAudioClips = 4;

        /// <summary>
        /// Whether to stop sounds when a new level loads. Set to false for additive level loading.
        /// </summary>
        public static bool StopSoundsOnLevelLoad = true;

        private void Awake()
        {
            DontDestroyOnLoad(gameObject);
            _instance = this;
        }

        private void OnEnable()
        {
            UnityEngine.SceneManagement.SceneManager.sceneLoaded += SceneManagerSceneLoaded;
        }

        private void OnDisable()
        {
            UnityEngine.SceneManagement.SceneManager.sceneLoaded -= SceneManagerSceneLoaded;
        }

        private void Update()
        {
            _updated = true;

            for (var i = Sounds.Count - 1; i >= 0; i--)
            {
                if (Sounds[i].Update())
                {
                    Sounds.RemoveAt(i);
                }
            }

            for (var i = Music.Count - 1; i >= 0; i--)
            {
                var nullMusic = (Music[i] == null || Music[i].AudioSource == null);
                switch (nullMusic)
                {
                    case false when !Music[i].Update():
                        continue;
                    case false when Music[i].Tag != _persistTag:
                        Debug.LogWarning("Destroying persisted audio from previous scene: " + Music[i].AudioSource.gameObject.name);

                        // cleanup persisted audio from previous scenes
                        Destroy(Music[i].AudioSource.gameObject);
                        break;
                }

                Music.RemoveAt(i);
            }

            for (var i = MusicOneShot.Count - 1; i >= 0; i--)
            {
                if (!MusicOneShot[i].isPlaying)
                {
                    MusicOneShot.RemoveAt(i);
                }
            }
        }

        private void OnApplicationFocus(bool paused)
        {
            if (!PauseSoundsOnApplicationPause) return;
            if (paused)
            {
                ResumeAll();
            }
            else
            {
                PauseAll();
            }
        }

        private void StopLoopingListOnLevelLoad(IList<LoopingAudioSource> list)
        {
            for (var i = list.Count - 1; i >= 0; i--)
            {
                if (!list[i].Persist || !list[i].AudioSource.isPlaying)
                {
                    list.RemoveAt(i);
                }
            }
        }

        private void ClearPersistedSounds()
        {
            foreach (var s in PersistedSounds)
            {
                if (!s.AudioSource.isPlaying)
                {
                    GameObject.Destroy(s.AudioSource.gameObject);
                }
            }

            PersistedSounds.Clear();
        }

        private void SceneManagerSceneLoaded(UnityEngine.SceneManagement.Scene s, UnityEngine.SceneManagement.LoadSceneMode m)
        {
            // Just in case this is called a bunch of times, we put a check here
            if (!_updated || !StopSoundsOnLevelLoad) return;
            _persistTag++;

            _updated = false;

            Debug.LogWarningFormat("Reloaded level, new sound manager persist tag: {0}", _persistTag);

            StopNonLoopingSounds();
            StopLoopingListOnLevelLoad(Sounds);
            StopLoopingListOnLevelLoad(Music);
            SoundsOneShot.Clear();
            ClearPersistedSounds();
        }

        private static void UpdateSounds()
        {
            foreach (var s in Sounds)
            {
                s.TargetVolume = s.OriginalTargetVolume * _soundVolume;
            }
        }

        private static void UpdateMusic()
        {
            foreach (var s in Music)
            {
                if (!s.Stopping)
                {
                    s.TargetVolume = s.OriginalTargetVolume * _musicVolume;
                }
            }

            foreach (var s in MusicOneShot)
            {
                s.volume = _musicVolume;
            }
        }

        private static IEnumerator RemoveVolumeFromClip(AudioClip clip, float volume)
        {
            yield return new WaitForSeconds(clip.length);

            if (SoundsOneShot.TryGetValue(clip, out var volumes))
            {
                volumes.Remove(volume);
            }
        }

        private static void PlayLooping(AudioSource source, List<LoopingAudioSource> sources, float volumeScale, float fadeSeconds, bool persist, bool stopAll)
        {
            for (var i = sources.Count - 1; i >= 0; i--)
            {
                var s = sources[i];
                if (s.AudioSource == source)
                {
                    sources.RemoveAt(i);
                }

                if (stopAll)
                {
                    s.Stop();
                }
            }

            {
                var sourceGameObject = source.gameObject;
                sourceGameObject.SetActive(true);
                var s = new LoopingAudioSource(source, fadeSeconds, fadeSeconds, persist);
                s.Play(volumeScale, true);
                s.Tag = _persistTag;
                sources.Add(s);

                if (!persist) return;
                if (!sourceGameObject.name.StartsWith("PersistedBySoundManager-"))
                {
                    sourceGameObject.name = "PersistedBySoundManager-" + sourceGameObject.name + "-" + sourceGameObject.GetInstanceID();
                }

                sourceGameObject.transform.parent = null;
                DontDestroyOnLoad(sourceGameObject);
                PersistedSounds.Add(s);
            }
        }

        private static void StopLooping(AudioSource source, List<LoopingAudioSource> sources)
        {
            foreach (var s in sources)
            {
                if (s.AudioSource == source)
                {
                    s.Stop();
                    source = null;
                    break;
                }
            }

            if (source != null)
            {
                source.Stop();
            }
        }

        /// <summary>
        /// Play a sound once - sound volume will be affected by global sound volume
        /// </summary>
        /// <param name="source">Audio source</param>
        /// <param name="clip">Clip</param>
        public static void PlayOneShotSound(AudioSource source, AudioClip clip)
        {
            PlayOneShotSound(source, clip, 1.0f);
        }

        /// <summary>
        /// Play a sound once - sound volume will be affected by global sound volume
        /// </summary>
        /// <param name="source">Audio source</param>
        /// <param name="clip">Clip</param>
        /// <param name="volumeScale">Additional volume scale</param>
        public static void PlayOneShotSound(AudioSource source, AudioClip clip, float volumeScale)
        {
            List<float> volumes;
            if (!SoundsOneShot.TryGetValue(clip, out volumes))
            {
                volumes = new List<float>();
                SoundsOneShot[clip] = volumes;
            }
            else if (volumes.Count == MaxDuplicateAudioClips)
            {
                return;
            }

            var minVolume = float.MaxValue;
            var maxVolume = float.MinValue;
            foreach (var volume in volumes)
            {
                minVolume = Mathf.Min(minVolume, volume);
                maxVolume = Mathf.Max(maxVolume, volume);
            }

            var requestedVolume = (volumeScale * _soundVolume);
            if (maxVolume > 0.5f)
            {
                requestedVolume = (minVolume + maxVolume) / (float) (volumes.Count + 2);
            }
            // else requestedVolume can stay as is

            volumes.Add(requestedVolume);
            source.PlayOneShot(clip, requestedVolume);
            _instance.StartCoroutine(RemoveVolumeFromClip(clip, requestedVolume));
        }

        /// <summary>
        /// Play a looping sound - sound volume will be affected by global sound volume
        /// </summary>
        /// <param name="source">Audio source to play looping</param>
        /// <param name="volumeScale">Additional volume scale</param>
        /// <param name="fadeSeconds">Seconds to fade in and out</param>
        public static void PlayLoopingSound(AudioSource source, float volumeScale = 1.0f, float fadeSeconds = 1.0f)
        {
            PlayLooping(source, Sounds, volumeScale, fadeSeconds, false, false);
            UpdateSounds();
        }

        /// <summary>
        /// Play a music track once - music volume will be affected by the global music volume
        /// </summary>
        /// <param name="source">Audio source</param>
        /// <param name="clip">Clip</param>
        /// <param name="volumeScale">Additional volume scale</param>
        public static void PlayOneShotMusic(AudioSource source, AudioClip clip, float volumeScale = 1.0f)
        {
            var index = MusicOneShot.IndexOf(source);
            if (index >= 0)
            {
                MusicOneShot.RemoveAt(index);
            }

            source.PlayOneShot(clip, volumeScale * _musicVolume);
            MusicOneShot.Add(source);
        }

        /// <summary>
        /// Play a looping music track - music volume will be affected by the global music volume
        /// </summary>
        /// <param name="source">Audio source</param>
        /// <param name="volumeScale">Additional volume scale</param>
        /// <param name="fadeSeconds">Seconds to fade in and out</param>
        /// <param name="persist">Whether to persist the looping music between scene changes</param>
        public static void PlayLoopingMusic(AudioSource source, float volumeScale = 1.0f, float fadeSeconds = 1.0f, bool persist = false)
        {
            PlayLooping(source, Music, volumeScale, fadeSeconds, persist, true);
            UpdateMusic();
        }

        /// <summary>
        /// Stop looping a sound for an audio source
        /// </summary>
        /// <param name="source">Audio source to stop looping sound for</param>
        public static void StopLoopingSound(AudioSource source)
        {
            StopLooping(source, Sounds);
        }

        /// <summary>
        /// Stop looping music for an audio source
        /// </summary>
        /// <param name="source">Audio source to stop looping music for</param>
        public static void StopLoopingMusic(AudioSource source)
        {
            StopLooping(source, Music);
        }

        /// <summary>
        /// Stop all looping sounds, music, and music one shots. Non-looping sounds are not stopped.
        /// </summary>
        public static void StopAll()
        {
            StopAllLoopingSounds();
            StopNonLoopingSounds();
        }

        /// <summary>
        /// Stop all looping sounds and music. Music one shots and non-looping sounds are not stopped.
        /// </summary>
        public static void StopAllLoopingSounds()
        {
            foreach (var s in Sounds)
            {
                s.Stop();
            }

            foreach (var s in Music)
            {
                s.Stop();
            }
        }

        /// <summary>
        /// Stop all non-looping sounds. Looping sounds and looping music are not stopped.
        /// </summary>
        public static void StopNonLoopingSounds()
        {
            foreach (var s in MusicOneShot)
            {
                s.Stop();
            }
        }

        /// <summary>
        /// Pause all sounds
        /// </summary>
        public static void PauseAll()
        {
            foreach (var s in Sounds)
            {
                s.Pause();
            }

            foreach (var s in Music)
            {
                s.Pause();
            }
        }

        /// <summary>
        /// Unpause and resume all sounds
        /// </summary>
        public static void ResumeAll()
        {
            foreach (var s in Sounds)
            {
                s.Resume();
            }

            foreach (var s in Music)
            {
                s.Resume();
            }
        }

        /// <summary>
        /// Global music volume multiplier
        /// </summary>
        public static float MusicVolume
        {
            get => _musicVolume;
            set
            {
                _musicVolume = value;
                UpdateMusic();
            }
        }

        /// <summary>
        /// Global sound volume multiplier
        /// </summary>
        public static float SoundVolume
        {
            get => _soundVolume;
            set
            {
                _soundVolume = value;
                UpdateSounds();
            }
        }

        /// <summary>
        /// Whether to pause sounds when the application is paused and resume them when the application is activated.
        /// Player option "Run In Background" must be selected to enable this. Default is true.
        /// </summary>
        public static bool PauseSoundsOnApplicationPause { get; set; } = true;
    }
}