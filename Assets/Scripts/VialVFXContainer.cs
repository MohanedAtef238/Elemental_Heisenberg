using UnityEngine;
using System.Collections.Generic;

public class VialVFXContainer : MonoBehaviour
{
    [SerializeField] private Transform effectAnchor;
    [SerializeField] private float activationDistance = 2.5f;
    [SerializeField] private float simulationSpeed = 0.5f;
    [SerializeField] private int maxParticles = 8;
    [SerializeField] private string effectTag = "vial_effects";

    [SerializeField] private float vfxScale = 0.02f;
    [SerializeField] private float intensityMultiplier = 2.0f;

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
        instance.transform.localScale = Vector3.one * vfxScale;

        // Ensure all particle systems in the effect respect the transform scale
        // and apply intensity multiplier to emission
        foreach (var ps in instance.GetComponentsInChildren<ParticleSystem>(true))
        {
            var main = ps.main;
            main.scalingMode = ParticleSystemScalingMode.Hierarchy;
            
            // Increase emission intensity
            var emission = ps.emission;
            emission.rateOverTimeMultiplier *= intensityMultiplier;
            
            // Adjust bursts for one-shot effects
            ParticleSystem.Burst[] bursts = new ParticleSystem.Burst[emission.burstCount];
            emission.GetBursts(bursts);
            for (int i = 0; i < bursts.Length; i++)
            {
                bursts[i].minCount = (short)(bursts[i].minCount * intensityMultiplier);
                bursts[i].maxCount = (short)(bursts[i].maxCount * intensityMultiplier);
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
