using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))]
public class BulletController : MonoBehaviour
{
    public enum BulletOwner
    {
        Player,
        Enemy
    }

    [Header("Owner")]
    [SerializeField] private BulletOwner owner = BulletOwner.Player;

    [Header("Movement")]
    [SerializeField] private float speed = 12f;

    [Header("Lifetime")]
    [SerializeField] private float lifeTime = 2f;

    [Header("Visual")]
    [Tooltip("Rotation offset applied so the sprite faces the travel direction.")]
    [SerializeField] private float spriteForwardOffsetDeg = -90f;

    [Header("Collision")]
    [Tooltip("If set, the bullet will ignore collisions with this collider.")]
    [SerializeField] private Collider2D ignoreCollider;

    [Header("Damage")]
    [SerializeField] private int damage = 1;

    public int Damage => damage;
    public BulletOwner Owner => owner;

    private Rigidbody2D rb;
    private Collider2D col;
    private bool _launched;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<Collider2D>();

        rb.gravityScale = 0f;
        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        rb.interpolation = RigidbodyInterpolation2D.Interpolate;

        col.isTrigger = true;
    }

    private void OnEnable()
    {
        _launched = false;

        rb.linearVelocity = Vector2.zero;
        rb.angularVelocity = 0f;

        if (ignoreCollider != null && col != null)
            Physics2D.IgnoreCollision(col, ignoreCollider, true);
    }

    public void SetOwner(BulletOwner newOwner)
    {
        owner = newOwner;
    }

    public void SetIgnoreCollider(Collider2D colliderToIgnore)
    {
        ignoreCollider = colliderToIgnore;

        if (ignoreCollider != null && col != null)
            Physics2D.IgnoreCollision(col, ignoreCollider, true);
    }

    public void SetDamage(int newDamage)
    {
        damage = Mathf.Max(1, newDamage);
    }

    public void Launch(Vector2 direction, float? overrideSpeed = null, float? overrideLifetime = null)
    {
        if (overrideSpeed.HasValue) speed = overrideSpeed.Value;
        if (overrideLifetime.HasValue) lifeTime = overrideLifetime.Value;

        if (direction.sqrMagnitude < 0.0001f)
            direction = Vector2.right;

        direction.Normalize();

        rb.linearVelocity = Vector2.zero;
        rb.angularVelocity = 0f;
        rb.linearVelocity = direction * Mathf.Max(0f, speed);

        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0f, 0f, angle + spriteForwardOffsetDeg);

        if (lifeTime > 0f)
            Destroy(gameObject, lifeTime);

        _launched = true;
    }

    private void Start()
    {
        if (_launched) return;
        Launch(transform.right);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other == null) return;

        if (ignoreCollider != null && other == ignoreCollider)
            return;

        if (other.CompareTag(Tags.ShieldPickup))
            return;

        if (owner == BulletOwner.Player && other.CompareTag(Tags.Player))
            return;

        Destroy(gameObject);
    }
}
