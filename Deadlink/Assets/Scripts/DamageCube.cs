using UnityEngine;

public class DamageCube : MonoBehaviour
{
    public float damageAmount = 25f;
    public bool destroyAfterHit = false;

    private void OnTriggerEnter(Collider other)
    {
        PlayerHealth player = other.GetComponent<PlayerHealth>();
        if (player != null)
        {
            player.TakeDamage(damageAmount);

            if (destroyAfterHit)
                Destroy(gameObject);
        }
    }
}
