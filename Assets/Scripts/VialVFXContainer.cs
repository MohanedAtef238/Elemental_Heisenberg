using UnityEngine;
using System.Collections.Generic;

public class VialVFXContainer : MonoBehaviour
{
    [SerializeField] private Transform effectAnchor;
    [SerializeField] private float activationDistance = 2.5f;
    [SerializeField] private float simulationSpeed = 0.5f;
    [SerializeField] private int maxParticles = 8;
    [SerializeField] private string effectTag = "vial_effects";

    [Header("Scaling & Positioning")]
    [SerializeField] private float vfxScale = 0.04f; // Slightly smaller default
    [SerializeField] private float baseIntensityMultiplier = 1.2f; // Lowered from 2.0
    [SerializeField] private float shapeScaleMultiplier = 1.2f; // Lowered from 1.5
    [SerializeField] private Vector3 vfxOffset = new Vector3(0, 0.05f, 0); // Position inside the vial

    private List<ParticleSystem> targetSystems = new List<ParticleSystem>();
    private Transform playerTransform;

    private void Awake()
    {
        if (effectAnchor != null)
        {
            RefreshSystems();
        }
    }

    public void InjectEffect(GameObject vfxPrefab, float intensityMultiplier = 1.0f)
    {
        if (vfxPrefab == null) return;
        
        // Fallback to self if no anchor is defined
        Transform anchor = effectAnchor != null ? effectAnchor : transform;

        GameObject instance = Instantiate(vfxPrefab, anchor);
        instance.transform.localPosition = vfxOffset;
        instance.transform.localScale = Vector3.one * vfxScale;

        float finalIntensity = baseIntensityMultiplier * intensityMultiplier;

        // Ensure all particle systems in the effect respect the transform scale
        // and apply intensity multiplier to emission
        foreach (var ps in instance.GetComponentsInChildren<ParticleSystem>(true))
        {
            var main = ps.main;
            main.scalingMode = ParticleSystemScalingMode.Hierarchy;
            
            // Increase the "Area" of the effect by scaling the shape module
            var shape = ps.shape;
            if (shape.enabled)
            {
                shape.scale *= shapeScaleMultiplier;
            }

            // Increase emission intensity
            var emission = ps.emission;
            emission.rateOverTimeMultiplier *= finalIntensity;
            
            // Adjust bursts for one-shot effects
            ParticleSystem.Burst[] bursts = new ParticleSystem.Burst[emission.burstCount];
            emission.GetBursts(bursts);
            for (int i = 0; i < bursts.Length; i++)
            {
                bursts[i].minCount = (short)(bursts[i].minCount * finalIntensity);
                bursts[i].maxCount = (short)(bursts[i].maxCount * finalIntensity);
            }
            emission.SetBursts(bursts);
        }
        
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
