using UnityEngine;

namespace Interactions
{
    /// <summary>
    /// Manages global skybox changes.
    /// Assign to any Manager GameObject and reference from InteractionCoordinator.
    /// </summary>
    public class AtmosphereController : MonoBehaviour
    {
        private Material _defaultSkybox;

        private void Awake()
        {
            _defaultSkybox = RenderSettings.skybox;
        }

        /// <summary>
        /// Swaps the global skybox to the specified material.
        /// </summary>
        public void SetSkybox(Material skybox)
        {
            if (skybox == null) return;
            RenderSettings.skybox = skybox;
            DynamicGI.UpdateEnvironment();
        }

        /// <summary>
        /// Restores the original skybox captured on Awake.
        /// </summary>
        public void ResetSkybox()
        {
            RenderSettings.skybox = _defaultSkybox;
            DynamicGI.UpdateEnvironment();
        }
    }
}
