using UnityEngine;

public class Weapon : MonoBehaviour
{
    public float attackRange = 2f;
    public float attackRadius = 0.5f;  // For spherecast
    public int damage = 20;
    public float attackCooldown = 0.8f;
    private float lastAttackTime;

    public LayerMask enemyLayers;
    public Transform attackPoint;  // Empty child object at weapon tip or player front

    void Update()
    {
        if (Input.GetMouseButtonDown(0) && Time.time > lastAttackTime + attackCooldown)
        {
            Attack();
            lastAttackTime = Time.time;
        }
    }

    void Attack()
    {
        // Option 1: Spherecast in front of attackPoint
        RaycastHit[] hits = Physics.SphereCastAll(attackPoint.position, attackRadius, attackPoint.forward, attackRange, enemyLayers);

        foreach (var hit in hits)
        {
            Enemy enemy = hit.collider.GetComponent<Enemy>();
            if (enemy != null)
            {
                enemy.TakeDamage(damage);
            }
        }

        // Optional: add attack animation, sound, effects here
    }

    private void OnDrawGizmosSelected()
    {
        if (attackPoint == null) return;

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(attackPoint.position + attackPoint.forward * attackRange, attackRadius);
    }
}
