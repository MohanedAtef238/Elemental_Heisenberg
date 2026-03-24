using UnityEngine;

namespace Interactions
{
    /// <summary>
    /// this tints the skybox to a target color when two labelled objects
    /// are smashed together. Doesnt revert as of now.
    /// We aim to replace or extend with SpawnEffect, SoundEffect, etc. following the same pattern.
    /// </summary>
    [CreateAssetMenu(fileName = "SkyboxColorEffect", menuName = "Interactions/Effects/Skybox Color")]
    public class SkyboxColorEffect : InteractionEffect
    {
        [SerializeField] private Color _targetColor = Color.cyan;
        [SerializeField] private string _colorProperty = "_SkyTint";

        // NonSerialized = reset on domain reload; safe to store runtime-only state here.
        [System.NonSerialized] private Material _runtimeSkybox;
        [System.NonSerialized] private Color _originalColor;

        public override void Apply(GameObject objectA, GameObject objectB)
        {
            if (RenderSettings.skybox == null) return;

            if (_runtimeSkybox == null)
            {
                _originalColor = RenderSettings.skybox.GetColor(_colorProperty);
                _runtimeSkybox = new Material(RenderSettings.skybox);
                RenderSettings.skybox = _runtimeSkybox;
            }

            _runtimeSkybox.SetColor(_colorProperty, _targetColor);
            DynamicGI.UpdateEnvironment();
        }

        public override void Revert()
        {
            if (_runtimeSkybox == null) return;
            _runtimeSkybox.SetColor(_colorProperty, _originalColor);
            DynamicGI.UpdateEnvironment();
        }
    }
}
