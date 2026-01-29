using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public float maxSpeed = 8f;
    public float acceleration = 18f;
    public float deceleration = 10f;

    public float spriteForwardOffsetDeg = 90f;

    public SpriteRenderer player;
    public Rigidbody2D rb;

    [Header("Aim")]
    [Tooltip("Optional. If not set, Camera.main will be cached once on Awake.")]
    [SerializeField] private Camera aimCamera;

    private Vector2 velocity;

    private float speedBonus = 0f;

    void Reset()
    {
        rb = GetComponent<Rigidbody2D>();
        player = GetComponentInChildren<SpriteRenderer>();
    }

    private void Awake()
    {
        if (aimCamera == null)
            aimCamera = Camera.main;
    }

    void Update()
    {
        Vector2 input = new Vector2(
            Input.GetAxisRaw("Horizontal"),
            Input.GetAxisRaw("Vertical")
        ).normalized;

        if (input.sqrMagnitude > 0.0001f)
        {
            velocity += input * acceleration * Time.deltaTime;
        }
        else
        {
            velocity = Vector2.MoveTowards(velocity, Vector2.zero, deceleration * Time.deltaTime);
        }

        float effectiveMaxSpeed = Mathf.Max(0f, maxSpeed + speedBonus);

        if (velocity.magnitude > effectiveMaxSpeed)
            velocity = velocity.normalized * effectiveMaxSpeed;

        Vector2 newPos = (Vector2)transform.position + velocity * Time.deltaTime;
        if (rb != null)
            rb.MovePosition(newPos);
        else
            transform.position = newPos;

        RotateTowardsMouse();
    }

    private void RotateTowardsMouse()
    {
        if (aimCamera == null)
        {
            aimCamera = Camera.main;
            if (aimCamera == null) return;
        }

        Vector3 mouseWorld = aimCamera.ScreenToWorldPoint(Input.mousePosition);
        Vector2 dir = (Vector2)(mouseWorld - transform.position);

        if (dir.sqrMagnitude < 0.0001f) return;

        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg - spriteForwardOffsetDeg;
        transform.rotation = Quaternion.Euler(0f, 0f, angle);
    }

    // ===== Ability hook (logic only) =====
    public void AddSpeedBonus(float amount)
    {
        speedBonus += Mathf.Max(0f, amount);
    }

    public void ResetUpgradeBonuses()
    {
        speedBonus = 0f;
    }
}
