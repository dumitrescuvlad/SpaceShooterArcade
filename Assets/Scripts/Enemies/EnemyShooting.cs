using UnityEngine;

public class EnemyShooting : MonoBehaviour, IGameOverStoppable
{
    [Header("Shooting")]
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private float fireRate = 1.5f;
    [SerializeField] private float bulletSpeed = 14f;

    [Header("Range")]
    [SerializeField] private float shootRange = 12f;

    [Header("References")]
    [SerializeField] private Transform firePoint;
    [SerializeField] private SceneContext sceneContext;

    private float _nextFireTime;
    private Transform _player;
    private Collider2D _ownCollider;

    private void Awake()
    {
        if (sceneContext == null)
            sceneContext = SceneContext.Instance;

        _player = sceneContext != null ? sceneContext.PlayerTransform : null;

        if (_player == null)
            _player = GameObject.FindGameObjectWithTag(Tags.Player)?.transform;

        _ownCollider = GetComponent<Collider2D>();
    }

    private void OnEnable()
    {
        if (GameFlowController.Instance != null)
            GameFlowController.Instance.Register(this);
    }

    private void OnDisable()
    {
        if (GameFlowController.Instance != null)
            GameFlowController.Instance.Unregister(this);
    }

    private void Update()
    {
        if (_player == null && sceneContext != null)
            _player = sceneContext.PlayerTransform;

        if (_player == null)
        {
            _player = GameObject.FindGameObjectWithTag(Tags.Player)?.transform;
            if (_player == null) return;
        }

        float dist = Vector2.Distance(transform.position, _player.position);
        if (dist > shootRange) return;

        if (Time.time < _nextFireTime) return;

        Shoot();
        _nextFireTime = Time.time + fireRate;
    }

    private void Shoot()
    {
        if (bulletPrefab == null) return;
        if (firePoint == null) return;

        Vector2 direction = (_player.position - firePoint.position);
        if (direction.sqrMagnitude < 0.0001f) return;
        direction.Normalize();

        GameObject bullet = Instantiate(bulletPrefab, firePoint.position, Quaternion.identity);

        BulletController bulletController = bullet.GetComponent<BulletController>();
        if (bulletController != null)
        {
            bulletController.SetOwner(BulletController.BulletOwner.Enemy);
            bulletController.SetIgnoreCollider(_ownCollider);
            bulletController.Launch(direction, bulletSpeed);
        }
    }

    public void StopOnGameOver()
    {
        enabled = false;
    }
}
