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
        private GameObject _activeWeatherInstance;

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
        /// Spawns a global weather prefab.
        /// </summary>
        public void SetWeather(GameObject weatherPrefab)
        {
            ClearWeather();
            if (weatherPrefab == null) return;
            
            _activeWeatherInstance = Instantiate(weatherPrefab);
        }

        /// <summary>
        /// Destroys the active weather effect.
        /// </summary>
        public void ClearWeather()
        {
            if (_activeWeatherInstance != null)
            {
                Destroy(_activeWeatherInstance);
                _activeWeatherInstance = null;
            }
        }

        /// <summary>
        /// Restores the original skybox captured on Awake and clears weather.
        /// </summary>
        public void ResetSkybox()
        {
            RenderSettings.skybox = _defaultSkybox;
            DynamicGI.UpdateEnvironment();
            ClearWeather();
        }
    }
}
