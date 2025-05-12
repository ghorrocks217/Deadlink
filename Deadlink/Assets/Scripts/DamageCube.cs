using UnityEngine;

public class DamageCube : MonoBehaviour
{
    public int damageAmount = 10;

    void OnCollisionEnter(Collision collision)
    {
        PlayerHealth health = collision.collider.GetComponent<PlayerHealth>();
        if (health != null)
        {
            health.TakeDamage(damageAmount);
        }
    }
}
