using System;
using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    [Header("Health")]
    public int maxHealth = 10;
    private int currentHealth;

    [Header("Shield")]
    public int maxShield = 5;
    private int currentShield;

    [Tooltip("Seconds after taking damage before shield can regenerate.")]
    public float shieldRegenDelayAfterDamage = 1.5f;

    [Tooltip("Seconds after firing before shield can regenerate.")]
    public float shieldRegenDelayAfterShot = 0.75f;

    [Tooltip("Shield points regenerated per second.")]
    public float shieldRegenPerSecond = 1.0f;

    [Header("Damage")]
    public int enemyBulletDamage = 1;

    [Header("Death")]
    [Tooltip("How many seconds after death before the whole game freezes.")]
    public float freezeAfterDeathSeconds = 3f;

    [Header("References")]
    [Tooltip("Optional. If not assigned, the script will try SceneContext, then fallback to scene search.")]
    public ShootingScript shootingScript;

    [Header("Death animation")]
    public float fallGravityScale = 10f;
    public float fallDownVelocity = -10f;
    public float deathAngularVelocity = -360f;

    public int CurrentHealth => currentHealth;
    public int MaxHealth => maxHealth;

    public int CurrentShield => currentShield;
    public int MaxShield => maxShield;

    public bool IsDead => isDead;

    public event Action<int, int> OnHealthChanged;
    public event Action<int, int> OnShieldChanged;

    public event Action<PlayerHealth> OnDied;

    private bool isDead;
    private Rigidbody2D rb;

    private float lastDamageTime;
    private float shieldFloat;

    private void Awake()
    {
        currentHealth = Mathf.Max(1, maxHealth);
        currentShield = Mathf.Max(0, maxShield);
        shieldFloat = currentShield;

        rb = GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.gravityScale = 0f;
            rb.freezeRotation = true;
        }

        if (shootingScript == null)
        {
            var ctx = SceneContext.Instance;
            if (ctx != null)
                shootingScript = ctx.PlayerShooting;
        }

        if (shootingScript == null)
            shootingScript = FindFirstObjectByType<ShootingScript>();

        OnHealthChanged?.Invoke(currentHealth, maxHealth);
        OnShieldChanged?.Invoke(currentShield, maxShield);
    }

    private void Update()
    {
        if (isDead) return;
        RegenerateShield();
    }

    private void RegenerateShield()
    {
        if (maxShield <= 0) return;
        if (currentShield >= maxShield) return;

        if (Time.time - lastDamageTime < shieldRegenDelayAfterDamage)
            return;

        if (shootingScript != null)
        {
            if (Time.time - shootingScript.LastShotTime < shieldRegenDelayAfterShot)
                return;
        }

        shieldFloat += shieldRegenPerSecond * Time.deltaTime;

        int newShieldInt = Mathf.Clamp(Mathf.FloorToInt(shieldFloat), 0, maxShield);
        if (newShieldInt != currentShield)
        {
            currentShield = newShieldInt;
            OnShieldChanged?.Invoke(currentShield, maxShield);
        }
    }

    public bool TryAddShield(int amount)
    {
        if (isDead) return false;
        if (maxShield <= 0) return false;
        if (amount <= 0) return false;
        if (currentShield >= maxShield) return false;

        int newShield = Mathf.Clamp(currentShield + amount, 0, maxShield);
        if (newShield == currentShield) return false;

        currentShield = newShield;
        shieldFloat = currentShield;
        OnShieldChanged?.Invoke(currentShield, maxShield);
        return true;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (isDead) return;

        if (other.CompareTag(Tags.EnemyBullet))
        {
            TakeDamage(enemyBulletDamage);
            Destroy(other.gameObject);
            return;
        }

        if (other.CompareTag(Tags.ShieldPickup))
        {
            if (other.TryGetComponent<ShieldPickup>(out var pickup))
                pickup.TryConsume(this);
        }
    }

    private void TakeDamage(int amount)
    {
        if (amount <= 0 || isDead) return;

        lastDamageTime = Time.time;
        int remaining = amount;

        if (currentShield > 0)
        {
            int absorbed = Mathf.Min(currentShield, remaining);
            currentShield -= absorbed;
            remaining -= absorbed;

            shieldFloat = currentShield;
            OnShieldChanged?.Invoke(currentShield, maxShield);
        }

        if (remaining > 0)
        {
            currentHealth = Mathf.Max(0, currentHealth - remaining);
            OnHealthChanged?.Invoke(currentHealth, maxHealth);

            if (currentHealth == 0)
                Die();
        }
    }

    private void Die()
    {
        if (isDead) return;
        isDead = true;

        var controller = GetComponent<PlayerController>();
        if (controller != null) controller.enabled = false;

        if (shootingScript != null) shootingScript.enabled = false;

        if (rb != null)
        {
            rb.freezeRotation = false;
            rb.gravityScale = fallGravityScale;
            rb.linearVelocity = new Vector2(0f, fallDownVelocity);
            rb.angularVelocity = deathAngularVelocity;
        }

        OnDied?.Invoke(this);
    }
}
