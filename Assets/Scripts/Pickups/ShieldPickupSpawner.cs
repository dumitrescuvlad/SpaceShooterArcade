using UnityEngine;

public class ShieldPickupSpawner : MonoBehaviour, IGameOverStoppable
{
    [Header("References")]
    [SerializeField] private SceneContext sceneContext;
    [SerializeField] private Transform player;
    [SerializeField] private GameObject shieldPickupPrefab;

    [Header("Spawn Timing")]
    [SerializeField] private float spawnInterval = 6f;
    [SerializeField] private int maxActivePickups = 3;

    [Header("Spawn Range (around player)")]
    [SerializeField] private float minDistanceFromPlayer = 6f;
    [SerializeField] private float maxDistanceFromPlayer = 16f;

    private float _nextSpawnTime;
    private int _activePickups;

    private void Awake()
    {
        if (sceneContext == null)
            sceneContext = SceneContext.Instance;

        if (player == null && sceneContext != null)
            player = sceneContext.PlayerTransform;

        if (player == null)
        {
            var p = GameObject.FindGameObjectWithTag(Tags.Player);
            if (p != null) player = p.transform;
        }
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

    public void StopOnGameOver()
    {
        enabled = false;
    }

    private void Update()
    {
        if (player == null && sceneContext != null)
            player = sceneContext.PlayerTransform;

        if (player == null) return;
        if (shieldPickupPrefab == null) return;

        if (Time.time < _nextSpawnTime) return;

        if (maxActivePickups > 0 && _activePickups >= maxActivePickups)
        {
            _nextSpawnTime = Time.time + spawnInterval;
            return;
        }

        SpawnOne();
        _nextSpawnTime = Time.time + spawnInterval;
    }

    private void SpawnOne()
    {
        Vector3 pos = GetRandomPositionAroundPlayer(player.position);
        var go = Instantiate(shieldPickupPrefab, pos, Quaternion.identity);

        _activePickups++;

        var tracker = go.AddComponent<OnDestroyNotifier>();
        tracker.onDestroyed += () => _activePickups = Mathf.Max(0, _activePickups - 1);
    }

    private Vector3 GetRandomPositionAroundPlayer(Vector3 center)
    {
        if (maxDistanceFromPlayer <= minDistanceFromPlayer)
            maxDistanceFromPlayer = minDistanceFromPlayer + 1f;

        float angle = Random.Range(0f, Mathf.PI * 2f);
        float dist = Random.Range(minDistanceFromPlayer, maxDistanceFromPlayer);

        Vector3 offset = new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0f) * dist;
        return center + offset;
    }

    private class OnDestroyNotifier : MonoBehaviour
    {
        public System.Action onDestroyed;
        private void OnDestroy() => onDestroyed?.Invoke();
    }
}
