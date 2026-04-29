using UnityEngine;
using System.Collections.Generic;

public class VialVFXContainer : MonoBehaviour
{
    [SerializeField] private Transform effectAnchor;
    [SerializeField] private float activationDistance = 2.5f;
    [SerializeField] private float simulationSpeed = 0.5f;
    [SerializeField] private int maxParticles = 8;
    [SerializeField] private string effectTag = "vial_effects";

    private List<ParticleSystem> targetSystems = new List<ParticleSystem>();
    private Transform playerTransform;

    private void Awake()
    {
        if (effectAnchor != null)
        {
            RefreshSystems();
        }
    }

    public void InjectEffect(GameObject vfxPrefab)
    {
        if (vfxPrefab == null || effectAnchor == null) return;

        GameObject instance = Instantiate(vfxPrefab, effectAnchor);
        instance.transform.localPosition = Vector3.zero;
        instance.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);
        
        RefreshSystems();
    }

    public void RefreshSystems()
    {
        targetSystems.Clear();
        if (effectAnchor == null) return;

        ParticleSystem[] allSystems = effectAnchor.GetComponentsInChildren<ParticleSystem>(true);
        
        foreach (var ps in allSystems)
        {
            // Note: We no longer gate by tag here to allow the Pad to find all systems
            targetSystems.Add(ps);
        }
    }
}
