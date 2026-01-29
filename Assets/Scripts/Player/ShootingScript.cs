using UnityEngine;

public class ShootingScript : MonoBehaviour
{
    [Header("Projectile")]
    public GameObject bulletPrefab;
    public Transform firePoint;
    public float bulletSpeed = 14f;

    [Header("Damage")]
    [SerializeField] private int baseDamage = 1;

    [Header("Aim")]
    [Tooltip("If enabled, bullets aim at the mouse position. If disabled, uses firePoint axis.")]
    public bool aimAtMouse = true;

    [Tooltip("Optional. If not set, Camera.main will be cached once on Awake.")]
    [SerializeField] private Camera aimCamera;

    public ForwardAxis forwardAxis = ForwardAxis.Right;
    public enum ForwardAxis { Right, Up }

    [Header("Fire")]
    public float fireRate = 6f;
    public bool autoFire = true;
    public KeyCode fireKey = KeyCode.Mouse0;

    [Header("Overheat")]
    public float maxHeat = 100f;
    public float heatPerShot = 12f;
    public float coolPerSecond = 25f;

    [Tooltip("If overheated, must cool below this fraction of maxHeat to resume shooting.")]
    [Range(0.0f, 1.0f)]
    public float resumeHeatFraction = 0.35f;

    public bool IsOverheated => _isOverheated;
    public float CurrentHeat => _currentHeat;
    public float MaxHeat => maxHeat;

    public float LastShotTime { get; private set; } = -9999f;

    public System.Action<float, float, bool> OnHeatChanged;

    private float _nextFireTime = 0f;
    private float _currentHeat = 0f;
    private bool _isOverheated = false;

    private Collider2D _ownCollider;

    private float _damageBonus = 0f;
    private float _bulletSpeedBonus = 0f;

    private void Awake()
    {
        if (aimCamera == null)
            aimCamera = Camera.main;

        _ownCollider = GetComponent<Collider2D>();
    }

    private void Update()
    {
        bool wantsToFire = autoFire ? Input.GetKey(fireKey) : Input.GetKeyDown(fireKey);

        CooldownHeat(wantsToFire);

        if (_isOverheated)
            return;

        if (wantsToFire && Time.time >= _nextFireTime)
        {
            Shoot();
            _nextFireTime = Time.time + (1f / Mathf.Max(0.0001f, fireRate));
        }
    }

    private void CooldownHeat(bool wantsToFire)
    {
        float prevHeat = _currentHeat;

        if (!wantsToFire || _isOverheated)
            _currentHeat = Mathf.Max(0f, _currentHeat - coolPerSecond * Time.deltaTime);

        if (_isOverheated)
        {
            float resumeHeat = Mathf.Clamp01(resumeHeatFraction) * Mathf.Max(1f, maxHeat);
            if (_currentHeat <= resumeHeat)
                _isOverheated = false;
        }

        if (!Mathf.Approximately(prevHeat, _currentHeat))
            OnHeatChanged?.Invoke(_currentHeat, maxHeat, _isOverheated);
    }

    private void Shoot()
    {
        if (bulletPrefab == null) return;

        Transform fp = firePoint != null ? firePoint : transform;

        _currentHeat += heatPerShot;
        if (_currentHeat >= maxHeat)
        {
            _currentHeat = maxHeat;
            _isOverheated = true;
        }

        OnHeatChanged?.Invoke(_currentHeat, maxHeat, _isOverheated);

        Vector2 dir = GetShotDirection(fp);
        bool launched = LaunchBullet(fp.position, fp.rotation, dir);

        if (launched)
            LastShotTime = Time.time;
    }

    private Vector2 GetShotDirection(Transform fp)
    {
        if (aimAtMouse)
        {
            if (aimCamera == null)
                aimCamera = Camera.main;

            if (aimCamera != null)
            {
                Vector3 mouseWorld = aimCamera.ScreenToWorldPoint(Input.mousePosition);
                Vector2 dir = (Vector2)(mouseWorld - fp.position);
                if (dir.sqrMagnitude > 0.0001f)
                    return dir.normalized;
            }
        }

        Vector2 axisDir = (forwardAxis == ForwardAxis.Right) ? (Vector2)fp.right : (Vector2)fp.up;
        if (axisDir.sqrMagnitude < 0.0001f) axisDir = Vector2.right;
        return axisDir.normalized;
    }

    private bool LaunchBullet(Vector3 pos, Quaternion rot, Vector2 direction)
    {
        GameObject bullet = Instantiate(bulletPrefab, pos, rot);

        if (bullet.TryGetComponent<BulletController>(out var bulletController))
        {
            bulletController.SetOwner(BulletController.BulletOwner.Player);
            bulletController.SetIgnoreCollider(_ownCollider);

            int finalDamage = Mathf.Max(1, baseDamage + Mathf.RoundToInt(_damageBonus));
            bulletController.SetDamage(finalDamage);

            float finalSpeed = Mathf.Max(0f, bulletSpeed + _bulletSpeedBonus);
            bulletController.Launch(direction, finalSpeed);

            return true;
        }

        if (bullet.TryGetComponent<Rigidbody2D>(out var rb))
        {
            rb.linearVelocity = Vector2.zero;
            rb.angularVelocity = 0f;

            float finalSpeed = Mathf.Max(0f, bulletSpeed + _bulletSpeedBonus);
            rb.linearVelocity = direction.normalized * finalSpeed;
            return true;
        }

        return false;
    }


    public void AddDamageBonus(float amount)
    {
        _damageBonus += Mathf.Max(0f, amount);
    }

    public void AddBulletSpeedBonus(float amount)
    {
        _bulletSpeedBonus += Mathf.Max(0f, amount);
    }

    public void ResetUpgradeBonuses()
    {
        _damageBonus = 0f;
        _bulletSpeedBonus = 0f;
    }
}
