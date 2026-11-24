using UnityEditor;
using UnityEngine;
using Interactions;

namespace Interactions.Editor
{
    public static class VFXPopulator
    {
        [MenuItem("Tools/Populate Vial VFX")]
        public static void Populate()
        {
            var vials = Object.FindObjectsByType<ItemInstance>(FindObjectsSortMode.None);
            int count = 0;

            foreach (var vial in vials)
            {
                if (vial.Definition == null || vial.Definition.EffectPrefab == null) 
                    continue;

                var container = vial.GetComponent<VialVFXContainer>();
                if (container == null) 
                    continue;

                // Reach into the anchor to clear old effects (avoiding overcrowding/duplicates)
                // We use serializedObject to access the private effectAnchor field
                var so = new SerializedObject(container);
                var anchorProp = so.FindProperty("effectAnchor");
                if (anchorProp != null && anchorProp.objectReferenceValue is Transform anchor)
                {
                    for (int i = anchor.childCount - 1; i >= 0; i--)
                    {
                        Object.DestroyImmediate(anchor.GetChild(i).gameObject);
                    }
                }

                // Inject the effect via the container logic (now with fixed 0.02f scale)
                container.InjectEffect(vial.Definition.EffectPrefab);
                
                // Final pass to ensure all systems are dormant and correctly scaled
                var systems = vial.GetComponentsInChildren<ParticleSystem>(true);
                foreach(var ps in systems)
                {
                    var main = ps.main;
                    main.playOnAwake = false;
                    main.scalingMode = ParticleSystemScalingMode.Hierarchy;
                    ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
                }

                var audioSources = vial.GetComponentsInChildren<AudioSource>(true);
                foreach(var source in audioSources)
                {
                    source.playOnAwake = false;
                    source.Stop();
                }

                count++;
            }

            Debug.Log($"[VFXPopulator] ✓ Successfully injected VFX into {count} vials.");
        }
    }
}
