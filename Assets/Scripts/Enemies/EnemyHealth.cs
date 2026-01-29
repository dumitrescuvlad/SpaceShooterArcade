using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
public class EnemyHealth : MonoBehaviour
{
    [Header("Health")]
    public int maxHealth = 3;
    [SerializeField] private int currentHealth;

    [Header("Appearance")]
    [Tooltip("If enabled, the enemy gets a random tint color on spawn.")]
    [SerializeField] private bool randomizeColorOnSpawn = true;

    [Tooltip("Possible spawn colors (Red, Blue, Green, Purple, Pink).")]
    [SerializeField] private Color[] spawnColors =
    {
        new Color(1f, 0.2f, 0.2f, 1f),   // Red
        new Color(0.2f, 0.45f, 1f, 1f),  // Blue
        new Color(0.2f, 1f, 0.4f, 1f),   // Green
        new Color(0.7f, 0.3f, 1f, 1f),   // Purple
        new Color(1f, 0.35f, 0.75f, 1f)  // Pink
    };

    [Header("Hit Flash")]
    public float hitFlashDuration = 0.08f;
    public Color flashColor = Color.white;

    [Header("Knockback")]
    public float knockbackForce = 2f;
    public float knockbackDuration = 0.20f;
    public float knockbackDrag = 4f;

    [Header("References")]
    [SerializeField] private SceneContext sceneContext;

    public bool IsInKnockback { get; private set; }

    private Rigidbody2D rb;
    private SpriteRenderer[] renderers;
    private Color[] originalColors;

    private Coroutine knockbackCoroutine;
    private Coroutine flashCoroutine;

    private ScoreManager scoreManager;
    private bool isDead;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();

        if (rb.bodyType != RigidbodyType2D.Dynamic)
            rb.bodyType = RigidbodyType2D.Dynamic;

        currentHealth = Mathf.Max(1, maxHealth);

        renderers = GetComponentsInChildren<SpriteRenderer>(false);

        if (randomizeColorOnSpawn && spawnColors != null && spawnColors.Length > 0 && renderers.Length > 0)
        {
            Color chosen = spawnColors[Random.Range(0, spawnColors.Length)];
            for (int i = 0; i < renderers.Length; i++)
            {
                if (renderers[i] != null)
                    renderers[i].color = chosen;
            }
        }

        originalColors = new Color[renderers.Length];
        for (int i = 0; i < renderers.Length; i++)
            originalColors[i] = renderers[i].color;

        if (sceneContext == null)
            sceneContext = SceneContext.Instance;

        if (sceneContext != null)
            scoreManager = sceneContext.ScoreManager;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag(Tags.PlayerBullet))
        {
            int dmg = 1;
            if (other.TryGetComponent<BulletController>(out var bullet))
                dmg = bullet.Damage;

            TakeHit(dmg, other.transform.position);
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.collider.CompareTag(Tags.PlayerBullet))
        {
            int dmg = 1;
            if (collision.collider.TryGetComponent<BulletController>(out var bullet))
                dmg = bullet.Damage;

            TakeHit(dmg, collision.collider.transform.position);
        }
    }

    public void TakeHit(int damage, Vector2 hitFromPosition)
    {
        if (isDead) return;

        currentHealth -= Mathf.Max(1, damage);

        StartKnockback(hitFromPosition);
        StartHitFlash();

        if (currentHealth <= 0)
            Die();
    }

    private void Die()
    {
        if (isDead) return;
        isDead = true;

        if (scoreManager == null && SceneContext.Instance != null)
            scoreManager = SceneContext.Instance.ScoreManager;

        if (scoreManager != null)
            scoreManager.AddKill();

        Destroy(gameObject);
    }

    private void StartKnockback(Vector2 fromPos)
    {
        Vector2 dir = (Vector2)transform.position - fromPos;
        if (dir.sqrMagnitude < 1e-6f) dir = Random.insideUnitCircle;
        dir.Normalize();

        if (knockbackCoroutine != null)
            StopCoroutine(knockbackCoroutine);

        knockbackCoroutine = StartCoroutine(KnockbackRoutine(dir));
    }

    private IEnumerator KnockbackRoutine(Vector2 dir)
    {
        IsInKnockback = true;

        float prevDrag = rb.linearDamping;
        rb.linearVelocity = Vector2.zero;
        rb.linearDamping = knockbackDrag;
        rb.AddForce(dir * knockbackForce, ForceMode2D.Impulse);

        yield return new WaitForSeconds(knockbackDuration);

        rb.linearDamping = prevDrag;
        IsInKnockback = false;

        knockbackCoroutine = null;
    }

    private void StartHitFlash()
    {
        if (flashCoroutine != null)
            StopCoroutine(flashCoroutine);

        flashCoroutine = StartCoroutine(HitFlashRoutine());
    }

    private IEnumerator HitFlashRoutine()
    {
        for (int i = 0; i < renderers.Length; i++)
            if (renderers[i] != null) renderers[i].color = flashColor;

        yield return new WaitForSeconds(hitFlashDuration);

        for (int i = 0; i < renderers.Length; i++)
            if (renderers[i] != null) renderers[i].color = originalColors[i];

        flashCoroutine = null;
    }
}
