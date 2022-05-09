using UnityEngine;

namespace AudioSystem
{
    [System.Serializable]
    public class OneShotAudio
    {
        [SerializeField] private AudioClip[] clips;
        [SerializeField] private AudioSource source;
        [SerializeField] [Range(0, 1)] private float scale;
        [SerializeField] private bool music;

        public void Play()
        {
            Play(Random.Range(0, clips.Length));
        }

        public void Play(int index)
        {
            index %= clips.Length;
            if (music)
                AudioManager.PlayOneShotMusic(source, clips[index], scale);
            else
                AudioManager.PlayOneShotSound(source, clips[index], scale);
        }
    }
}