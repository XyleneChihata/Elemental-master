using System.Collections;
using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    [SerializeField] private int startingHealth = 10;
    [SerializeField] private GameObject deathVFXPrefab;
    [SerializeField] private float knockBackThrust = 2f;

    private int currentHealth;
    private Knockback knockback;
    private Flash flash;

    private Coroutine deathCheckRoutine;

    private void Awake()
    {
        flash = GetComponent<Flash>();
        knockback = GetComponent<Knockback>();
    }

    private void Start()
    {
        currentHealth = startingHealth;
    }

    public void TakeDamage(int damage)
    {
        currentHealth -= damage;
        currentHealth = Mathf.Max(currentHealth, 0);

        StartCoroutine(flash.FlashRoutine());

        if (deathCheckRoutine != null)
            StopCoroutine(deathCheckRoutine);

        deathCheckRoutine = StartCoroutine(CheckDetectDeathRoutine());
    }

    private IEnumerator CheckDetectDeathRoutine()
    {
        yield return new WaitForSeconds(flash.GetRestoreMatTime());
        DetectDeath();
    }

    public void DetectDeath()
    {
        if (currentHealth <= 0)
        {
            if (deathVFXPrefab != null)
            {
                Instantiate(deathVFXPrefab, transform.position, Quaternion.identity);
            }
            Destroy(gameObject);
        }
    }
}
