using UnityEngine;

namespace Interactions
{
    /// <summary>
    /// Activates a vial's particle and audio effects when the vial is physically
    /// placed on this pad (Paper_02). Only responds to GameObjects tagged "Vial".
    /// Combinations are NOT triggered here — only visual/audio effects.
    ///
    /// Requirements:
    ///   - This GameObject's BoxCollider must have isTrigger = true.
    ///   - Vial GameObjects must be tagged "Vial".
    ///   - The Interactable layer must NOT collide with the Player layer
    ///     (configured in Edit > Project Settings > Physics).
    /// </summary>
    public class VialEffectPad : MonoBehaviour
    {
        private void OnTriggerEnter(Collider other)
        {
            // Strictly validate: only "Vial"-tagged objects activate the pad.
            if (!other.CompareTag("Vial"))
                return;

            ActivateEffects(other, play: true);
        }

        private void OnTriggerExit(Collider other)
        {
            // Mirror exit: only deactivate for "Vial"-tagged objects.
            if (!other.CompareTag("Vial"))
                return;

            ActivateEffects(other, play: false);
        }

        /// <summary>
        /// Walks to the root of the vial hierarchy to find all particle systems
        /// and audio sources, then plays or stops them.
        /// </summary>
        private static void ActivateEffects(Collider vialCollider, bool play)
        {
            // Target only the specific vial that triggered the pad.
            // We look for the VialVFXContainer to ensure we have the correct object.
            var container = vialCollider.GetComponentInParent<VialVFXContainer>();
            if (container == null) return;

            // Use the centralized visibility method for particle systems
            container.SetEffectVisibility(play);

            Transform vialTransform = container.transform;
            AudioSource[] sources = vialTransform.GetComponentsInChildren<AudioSource>(true);
            foreach (var source in sources)
            {
                if (play)
                    source.Play();
                else
                    source.Stop();
            }
        }
    }
}
