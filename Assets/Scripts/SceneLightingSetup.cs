using UnityEngine;

[DefaultExecutionOrder(-999)]
public class SceneLightingSetup : MonoBehaviour
{
    [SerializeField] private Material _skyboxMaterial;

    private void Awake()
    {
        if (_skyboxMaterial != null)
            RenderSettings.skybox = _skyboxMaterial;
    }
}
