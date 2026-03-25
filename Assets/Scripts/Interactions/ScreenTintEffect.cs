using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace Interactions
{
    /// <summary>
    /// Manages a URP post-processing Volume to apply a color tint overlay.
    /// Requires a Volume component on the same GameObject.
    /// </summary>
    [RequireComponent(typeof(Volume))]
    public class ScreenTintEffect : MonoBehaviour
    {
        private Volume _volume;
        private ColorAdjustments _colorAdjustments;

        private void Awake()
        {
            _volume = GetComponent<Volume>();
            _volume.isGlobal = true;
            _volume.priority = 100;

            if (_volume.profile == null)
                _volume.profile = ScriptableObject.CreateInstance<VolumeProfile>();

            if (!_volume.profile.TryGet(out _colorAdjustments))
                _colorAdjustments = _volume.profile.Add<ColorAdjustments>(true);

            _colorAdjustments.colorFilter.overrideState = false;
        }

        public void SetTint(Color color)
        {
            _colorAdjustments.colorFilter.overrideState = true;
            _colorAdjustments.colorFilter.value = color;
        }

        public void ClearTint()
        {
            _colorAdjustments.colorFilter.overrideState = false;
        }
    }
}
