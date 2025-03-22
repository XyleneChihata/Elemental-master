using UnityEngine;

public class EnemyDamageSourceCharge : MonoBehaviour
{
    [SerializeField] private int damageAmount = 1;

    private void OnTriggerEnter2D(Collider2D other)
    {
        PlayerHealth targetHealth = other.gameObject.GetComponent<PlayerHealth>();
        targetHealth?.TakeDamage(damageAmount);
    }
}
