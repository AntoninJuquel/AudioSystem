using UnityEngine;

namespace AudioSystem
{
    /// <summary>
    /// Sound manager extension methods
    /// </summary>
    public static class AudioSourceExtensions
    {
        /// <summary>
        /// Play an audio clip once using the global sound volume as a multiplier
        /// </summary>
        /// <param name="source">AudioSource</param>
        /// <param name="clip">Clip</param>
        public static void PlayOneShotSoundManaged(this AudioSource source, AudioClip clip)
        {
            AudioManager.PlayOneShotSound(source, clip, 1.0f);
        }

        /// <summary>
        /// Play an audio clip once using the global sound volume as a multiplier
        /// </summary>
        /// <param name="source">AudioSource</param>
        /// <param name="clip">Clip</param>
        /// <param name="volumeScale">Additional volume scale</param>
        public static void PlayOneShotSoundManaged(this AudioSource source, AudioClip clip, float volumeScale)
        {
            AudioManager.PlayOneShotSound(source, clip, volumeScale);
        }

        /// <summary>
        /// Play an audio clip once using the global music volume as a multiplier
        /// </summary>
        /// <param name="source">AudioSource</param>
        /// <param name="clip">Clip</param>
        public static void PlayOneShotMusicManaged(this AudioSource source, AudioClip clip)
        {
            AudioManager.PlayOneShotMusic(source, clip);
        }

        /// <summary>
        /// Play an audio clip once using the global music volume as a multiplier
        /// </summary>
        /// <param name="source">AudioSource</param>
        /// <param name="clip">Clip</param>
        /// <param name="volumeScale">Additional volume scale</param>
        public static void PlayOneShotMusicManaged(this AudioSource source, AudioClip clip, float volumeScale)
        {
            AudioManager.PlayOneShotMusic(source, clip, volumeScale);
        }

        /// <summary>
        /// Play a sound and loop it until stopped, using the global sound volume as a modifier
        /// </summary>
        /// <param name="source">Audio source to play</param>
        public static void PlayLoopingSoundManaged(this AudioSource source)
        {
            AudioManager.PlayLoopingSound(source);
        }

        /// <summary>
        /// Play a sound and loop it until stopped, using the global sound volume as a modifier
        /// </summary>
        /// <param name="source">Audio source to play</param>
        /// <param name="volumeScale">Additional volume scale</param>
        /// <param name="fadeSeconds">The number of seconds to fade in and out</param>
        public static void PlayLoopingSoundManaged(this AudioSource source, float volumeScale, float fadeSeconds)
        {
            AudioManager.PlayLoopingSound(source, volumeScale, fadeSeconds);
        }

        /// <summary>
        /// Play a music track and loop it until stopped, using the global music volume as a modifier
        /// </summary>
        /// <param name="source">Audio source to play</param>
        public static void PlayLoopingMusicManaged(this AudioSource source)
        {
            AudioManager.PlayLoopingMusic(source);
        }

        /// <summary>
        /// Play a music track and loop it until stopped, using the global music volume as a modifier
        /// </summary>
        /// <param name="source">Audio source to play</param>
        /// <param name="volumeScale">Additional volume scale</param>
        /// <param name="fadeSeconds">The number of seconds to fade in and out</param>
        /// <param name="persist">Whether to persist the looping music between scene changes</param>
        public static void PlayLoopingMusicManaged(this AudioSource source, float volumeScale, float fadeSeconds, bool persist)
        {
            AudioManager.PlayLoopingMusic(source, volumeScale, fadeSeconds, persist);
        }

        /// <summary>
        /// Stop a looping sound
        /// </summary>
        /// <param name="source">AudioSource to stop</param>
        public static void StopLoopingSoundManaged(this AudioSource source)
        {
            AudioManager.StopLoopingSound(source);
        }

        /// <summary>
        /// Stop a looping music track
        /// </summary>
        /// <param name="source">AudioSource to stop</param>
        public static void StopLoopingMusicManaged(this AudioSource source)
        {
            AudioManager.StopLoopingMusic(source);
        }
    }
}