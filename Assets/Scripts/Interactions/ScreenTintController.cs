using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace Interactions
{
    /// <summary>
    /// Manages the URP post-processing screen color tint.
    /// Requires a Volume component on the same GameObject.
    /// Reference from InteractionCoordinator to control tint separately from the skybox.
    /// </summary>
    [RequireComponent(typeof(Volume))]
    public class ScreenTintController : MonoBehaviour
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

            // Disabled by default — only activates when a reaction fires
            _colorAdjustments.colorFilter.overrideState = false;
        }

        /// <summary>
        /// Applies a color tint over the screen via post-processing.
        /// </summary>
        public void SetTint(Color color)
        {
            if (_colorAdjustments == null) return;
            _colorAdjustments.colorFilter.overrideState = true;
            _colorAdjustments.colorFilter.value = color;
        }

        /// <summary>
        /// Clears the tint, restoring the default color filter state.
        /// </summary>
        public void ClearTint()
        {
            if (_colorAdjustments == null) return;
            _colorAdjustments.colorFilter.overrideState = false;
        }
    }
}
