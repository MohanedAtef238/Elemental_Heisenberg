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

    private void Start()
    {
        // Try to find the XR camera
        if (Camera.main != null)
        {
            playerTransform = Camera.main.transform;
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
            if (ps.CompareTag(effectTag))
            {
                var main = ps.main;
                main.maxParticles = maxParticles;
                main.simulationSpeed = simulationSpeed;
                targetSystems.Add(ps);
            }
        }
    }

    private void Update()
    {
        if (targetSystems.Count == 0) return;

        // If player transform is lost, try to re-acquire (useful for VR scene loading)
        if (playerTransform == null && Camera.main != null)
        {
            playerTransform = Camera.main.transform;
        }

        if (playerTransform == null) return;

        float dist = Vector3.Distance(transform.position, playerTransform.position);
        bool shouldBeActive = dist <= activationDistance;

        foreach (var ps in targetSystems)
        {
            if (shouldBeActive && !ps.isPlaying)
            {
                ps.Play();
            }
            else if (!shouldBeActive && ps.isPlaying)
            {
                ps.Pause();
            }
        }
    }
}
