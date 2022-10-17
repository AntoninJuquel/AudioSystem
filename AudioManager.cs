namespace AudioSystem
{
    public class AudioManager : AudioController
    {
        public static AudioManager Instance;

        protected override void Awake()
        {
            base.Awake();
            Instance = this;
        }
    }
}