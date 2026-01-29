using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AbilityUpgrades : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private SceneContext sceneContext;
    [SerializeField] private ScoreManager scoreManager;
    [SerializeField] private ShootingScript shootingScript;
    [SerializeField] private PlayerController playerController;
    [SerializeField] private PlayerHealth playerHealth;

    [Header("Upgrade Amounts")]
    [SerializeField] private float damagePerPoint = 1f;
    [SerializeField] private float bulletSpeedPerPoint = 2f;
    [SerializeField] private float playerSpeedPerPoint = 0.5f;

    [Header("Freeze Ability")]
    [SerializeField] private float freezeDuration = 5f;

    // Levels (so you can display and verify)
    public int DamageLevel { get; private set; } = 1;
    public int BulletSpeedLevel { get; private set; } = 1;
    public int ShipSpeedLevel { get; private set; } = 1;

    public System.Action OnLevelsChanged;

    private Coroutine freezeRoutine;

    private void Awake()
    {
        if (sceneContext == null)
            sceneContext = SceneContext.Instance;

        if (sceneContext != null)
        {
            if (scoreManager == null) scoreManager = sceneContext.ScoreManager;
            if (shootingScript == null) shootingScript = sceneContext.PlayerShooting;
            if (playerHealth == null) playerHealth = sceneContext.PlayerHealth;
        }

        if (playerController == null)
            playerController = FindFirstObjectByType<PlayerController>();

        if (scoreManager == null)
            scoreManager = FindFirstObjectByType<ScoreManager>();

        if (shootingScript == null)
            shootingScript = FindFirstObjectByType<ShootingScript>();

        if (playerHealth == null)
            playerHealth = FindFirstObjectByType<PlayerHealth>();
    }

    private void OnEnable()
    {
        if (playerHealth != null)
            playerHealth.OnDied += HandlePlayerDied;
    }

    private void OnDisable()
    {
        if (playerHealth != null)
            playerHealth.OnDied -= HandlePlayerDied;
    }

    private void HandlePlayerDied(PlayerHealth _)
    {
        ResetAllUpgrades();
    }

    private void ResetAllUpgrades()
    {
        DamageLevel = 1;
        BulletSpeedLevel = 1;
        ShipSpeedLevel = 1;

        if (shootingScript != null)
            shootingScript.ResetUpgradeBonuses();

        if (playerController != null)
            playerController.ResetUpgradeBonuses();

        // If a freeze is active, stop it (we are in game over flow anyway)
        if (freezeRoutine != null)
        {
            StopCoroutine(freezeRoutine);
            freezeRoutine = null;
        }

        OnLevelsChanged?.Invoke();
    }

    // ===== Upgrade abilities =====

    public bool UpgradeDamage()
    {
        if (!TryConsumePoint()) return false;
        if (shootingScript == null) return false;

        shootingScript.AddDamageBonus(damagePerPoint);
        DamageLevel++;
        OnLevelsChanged?.Invoke();
        return true;
    }

    public bool UpgradeBulletSpeed()
    {
        if (!TryConsumePoint()) return false;
        if (shootingScript == null) return false;

        shootingScript.AddBulletSpeedBonus(bulletSpeedPerPoint);
        BulletSpeedLevel++;
        OnLevelsChanged?.Invoke();
        return true;
    }

    public bool UpgradePlayerSpeed()
    {
        if (!TryConsumePoint()) return false;
        if (playerController == null) return false;

        playerController.AddSpeedBonus(playerSpeedPerPoint);
        ShipSpeedLevel++;
        OnLevelsChanged?.Invoke();
        return true;
    }

    // ===== Freeze ability =====

    public bool FreezeEnemies()
    {
        if (!TryConsumePoint()) return false;

        if (freezeRoutine != null)
            StopCoroutine(freezeRoutine);

        freezeRoutine = StartCoroutine(FreezeEnemiesRoutine());
        return true;
    }

    private IEnumerator FreezeEnemiesRoutine()
    {
        float duration = Mathf.Max(0f, freezeDuration);

        var frozenControllers = new HashSet<Behaviour>();
        var frozenShooters = new HashSet<Behaviour>();
        var frozenBodies = new Dictionary<Rigidbody2D, bool>();

        float elapsed = 0f;
        const float scanInterval = 0.10f;

        while (elapsed < duration)
        {
            FreezeCurrentEnemies(frozenControllers, frozenShooters, frozenBodies);

            yield return new WaitForSecondsRealtime(scanInterval);
            elapsed += scanInterval;
        }

        foreach (var c in frozenControllers)
            if (c != null) c.enabled = true;

        foreach (var s in frozenShooters)
            if (s != null) s.enabled = true;

        foreach (var kvp in frozenBodies)
        {
            Rigidbody2D rb = kvp.Key;
            if (rb == null) continue;
            rb.simulated = kvp.Value;
        }

        freezeRoutine = null;
    }

    private void FreezeCurrentEnemies(
        HashSet<Behaviour> frozenControllers,
        HashSet<Behaviour> frozenShooters,
        Dictionary<Rigidbody2D, bool> frozenBodies)
    {
        EnemyController[] controllers = FindObjectsByType<EnemyController>(FindObjectsSortMode.None);
        for (int i = 0; i < controllers.Length; i++)
        {
            EnemyController c = controllers[i];
            if (c == null) continue;

            if (c.enabled)
            {
                c.enabled = false;
                frozenControllers.Add(c);
            }

            Rigidbody2D rb = c.GetComponent<Rigidbody2D>();
            if (rb != null && !frozenBodies.ContainsKey(rb))
            {
                frozenBodies.Add(rb, rb.simulated);
                rb.simulated = false;
            }
        }

        EnemyShooting[] shooters = FindObjectsByType<EnemyShooting>(FindObjectsSortMode.None);
        for (int i = 0; i < shooters.Length; i++)
        {
            EnemyShooting s = shooters[i];
            if (s == null) continue;

            if (s.enabled)
            {
                s.enabled = false;
                frozenShooters.Add(s);
            }

            Rigidbody2D rb = s.GetComponent<Rigidbody2D>();
            if (rb != null && !frozenBodies.ContainsKey(rb))
            {
                frozenBodies.Add(rb, rb.simulated);
                rb.simulated = false;
            }
        }
    }

    private bool TryConsumePoint()
    {
        if (scoreManager == null) return false;
        return scoreManager.ConsumeAbilityPoint();
    }
}
