using UnityEngine;

namespace Interactions
{
    public class AnvilSurface : MonoBehaviour
    {
        [SerializeField] private AlchemyZone _alchemyZone;
        [SerializeField] private float requiredImpactForce = 3f;

        private void OnCollisionEnter(Collision collision)
        {
            // Verify the impacting object: Weapon tag or Elven_Hammer name
            if (collision.gameObject.CompareTag("Weapon") || collision.gameObject.name.Contains("Elven_Hammer"))
            {
                // Calculate the physical impact force
                float impactForce = collision.relativeVelocity.magnitude;
                
                if (impactForce >= requiredImpactForce)
                {
                    if (_alchemyZone != null && _alchemyZone.IsPrepped)
                    {
                        _alchemyZone.ExecuteReaction();
                    }
                    else if (_alchemyZone == null)
                    {
                        Debug.LogWarning("AnvilSurface: AlchemyZone is not assigned!");
                    }
                }
            }
        }
    }
}
