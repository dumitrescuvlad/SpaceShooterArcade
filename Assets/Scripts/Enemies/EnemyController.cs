using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class EnemyController : MonoBehaviour
{
    public enum Mode { Chase, Orbit }

    public Transform target;

    public float maxSpeed = 7f;
    public float acceleration = 20f;

    public float orbitRadius = 4f;
    public float orbitSpeed = 5f;
    public float radialTightness = 4f;
    public bool clockwise = false;

    public float enterOrbitDistance = 5.0f;
    public float exitOrbitDistance = 6.0f;

    public float spriteForwardOffsetDeg = 90f;

    public float minSeparation = 0.3f;

    public bool respectKnockback = true;

    public Mode mode = Mode.Chase;
    private Rigidbody2D rb;
    private Vector2 velocity;
    private EnemyHealth _health;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        if (!target)
        {
            var p = GameObject.FindGameObjectWithTag(Tags.Player);
            if (p) target = p.transform;
        }
        rb.gravityScale = 0f;
        rb.freezeRotation = true;

        _health = GetComponent<EnemyHealth>();
    }

    void FixedUpdate()
    {
        if (!target) return;

        if (respectKnockback && _health != null && _health.IsInKnockback)
            return;

        float dt = Time.fixedDeltaTime;
        Vector2 pos = rb.position;
        Vector2 toTarget = (Vector2)target.position - pos;
        float dist = toTarget.magnitude;

        if (mode == Mode.Chase && dist <= enterOrbitDistance) mode = Mode.Orbit;
        else if (mode == Mode.Orbit && dist >= exitOrbitDistance) mode = Mode.Chase;

        Vector2 desiredVel;

        if (mode == Mode.Chase)
        {
            Vector2 dir = toTarget.sqrMagnitude > 1e-6f ? toTarget.normalized : Vector2.zero;
            desiredVel = dir * maxSpeed;
        }
        else
        {
            Vector2 r = pos - (Vector2)target.position;
            float rMag = r.magnitude;

            if (rMag < minSeparation)
            {
                Vector2 pushOut = (r.sqrMagnitude > 1e-6f ? r.normalized : Random.insideUnitCircle.normalized) * orbitSpeed;
                desiredVel = pushOut;
            }
            else
            {
                Vector2 radialDir = r / Mathf.Max(rMag, 1e-6f);
                Vector2 tangent = clockwise
                    ? new Vector2(radialDir.y, -radialDir.x)
                    : new Vector2(-radialDir.y, radialDir.x);

                float radialError = rMag - orbitRadius;

                Vector2 tangentialVel = tangent * orbitSpeed;
                Vector2 radialCorrection = -radialDir * (radialTightness * radialError);

                desiredVel = tangentialVel + radialCorrection;

                if (desiredVel.magnitude > maxSpeed)
                    desiredVel = desiredVel.normalized * maxSpeed;
            }
        }

        velocity = Vector2.MoveTowards(velocity, desiredVel, acceleration * dt);
        rb.MovePosition(pos + velocity * dt);
        RotateTowards(target.position);
    }

    private void RotateTowards(Vector3 worldPoint)
    {
        Vector2 dir = (Vector2)(worldPoint - transform.position);
        if (dir.sqrMagnitude < 1e-8f) return;

        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg - spriteForwardOffsetDeg;
        rb.MoveRotation(angle);
    }
}
