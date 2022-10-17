using UnityEngine;
using UnityEngine.UI;

namespace AudioSystem
{
    [RequireComponent(typeof(Slider))]
    public class AudioSlider : MonoBehaviour
    {
        [SerializeField] private string groupName;
        private Slider _slider;

        private void Awake()
        {
            _slider = GetComponent<Slider>();
            _slider.minValue = 0.0001f;
            _slider.maxValue = 1f;
            _slider.value = PlayerPrefs.GetFloat(name, 1f);
        }

        private void Start()
        {
            SetVolume(_slider.value);
        }

        private void OnEnable()
        {
            _slider.onValueChanged.AddListener(SetVolume);
        }

        private void OnDisable()
        {
            _slider.onValueChanged.RemoveListener(SetVolume);
        }

        private void SetVolume(float value)
        {
            AudioManager.Instance.SetMixerValue(groupName, value);
            PlayerPrefs.SetFloat(name, value);
        }
    }
}