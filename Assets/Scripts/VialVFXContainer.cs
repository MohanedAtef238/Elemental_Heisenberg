using UnityEngine;
using System.Collections.Generic;

public class VialVFXContainer : MonoBehaviour
{
    [SerializeField] private Transform effectAnchor;

    [Header("Scaling & Positioning")]
    [SerializeField] private float vfxScale = 0.35f; // Increased from 0.12f to engulf the vial
    [SerializeField] private float baseIntensityMultiplier = 5.0f; // Increased from 3.5f
    [SerializeField] private float shapeScaleMultiplier = 4.0f; // Increased from 1.8f to spread outside the glass
    [SerializeField] private float particleSizeMultiplier = 2.0f; // New: make individual particles larger
    [SerializeField] private Vector3 vfxOffset = new Vector3(0, 0.05f, 0); // Position inside the vial

    private List<ParticleSystem> targetSystems = new List<ParticleSystem>();
    private Transform playerTransform;

    private void Awake()
    {
        if (effectAnchor != null)
        {
            RefreshSystems();
            // Start with effects hidden/stopped
            SetEffectVisibility(false);
        }
    }

    /// <summary>
    /// Centralized control for vial VFX visibility.
    /// </summary>
    public void SetEffectVisibility(bool visible)
    {
        if (targetSystems.Count == 0 && effectAnchor != null)
            RefreshSystems();

        foreach (var ps in targetSystems)
        {
            if (visible)
            {
                if (!ps.isPlaying) ps.Play();
            }
            else
            {
                ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            }
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
            main.startSizeMultiplier *= particleSizeMultiplier;
            
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
            targetSystems.Add(ps);
        }
    }
}
