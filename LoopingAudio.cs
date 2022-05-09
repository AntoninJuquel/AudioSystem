using UnityEngine;

namespace AudioSystem
{
    [System.Serializable]
    public class LoopingAudio
    {
        [SerializeField] private AudioSource[] sources;
        [SerializeField] [Range(0, 1)] private float scale;
        [SerializeField] private bool music;
        private int _playingIndex;

        public void Play()
        {
            Play(Random.Range(0, sources.Length));
        }

        public void Play(int index)
        {
            index %= sources.Length;
            _playingIndex = index;
            if (music)
                AudioManager.PlayLoopingMusic(sources[index], scale);
            else
                AudioManager.PlayLoopingSound(sources[index], scale);
        }

        public void Stop()
        {
            if (music)
                AudioManager.StopLoopingMusic(sources[_playingIndex]);
            else
                AudioManager.StopLoopingSound(sources[_playingIndex]);
        }
    }
}