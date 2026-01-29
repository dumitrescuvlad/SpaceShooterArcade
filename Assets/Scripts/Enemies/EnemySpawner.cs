using System.Collections;
using UnityEngine;

public class EnemySpawner : MonoBehaviour, IGameOverStoppable
{
    [Header("References")]
    [SerializeField] private SceneContext sceneContext;

    public Transform player;
    public GameObject enemyPrefab;

    [Header("Spawn Position (2D around player)")]
    public float minDistanceFromPlayer = 25f;
    public float maxDistanceFromPlayer = 60f;
    public float repositionEverySeconds = 5f;

    [Tooltip("If enabled, the spawner keeps its current Y when repositioning (useful for side-scrollers). For top-down 2D, keep this OFF.")]
    public bool keepOriginalY = false;

    [Tooltip("Legacy field kept to avoid breaking Inspector data. Not used in 2D top-down spawning.")]
    public LayerMask groundLayer = ~0;

    [Header("Spawn Timing")]
    public float initialSpawnInterval = 4f;
    public float minSpawnInterval = 0.75f;
    public float timeToReachMinInterval = 300f;
    public int maxActiveEnemies = 0;

    [Header("Spawn Safety")]
    [Tooltip("Random delay (seconds) before the spawner starts creating enemies.")]
    [SerializeField] private Vector2 initialSpawnDelayRange = new Vector2(1f, 2f);

    private float _startTime;
    private int _activeEnemies = 0;

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

        _startTime = Time.time;
        StartCoroutine(MoveLoop());
        StartCoroutine(DelayedSpawnStart());
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

    private IEnumerator DelayedSpawnStart()
    {
        float min = Mathf.Min(initialSpawnDelayRange.x, initialSpawnDelayRange.y);
        float max = Mathf.Max(initialSpawnDelayRange.x, initialSpawnDelayRange.y);

        float delay = Random.Range(min, max);
        yield return new WaitForSeconds(delay);

        if (enabled)
            StartCoroutine(SpawnLoop());
    }

    private IEnumerator MoveLoop()
    {
        var wait = new WaitForSeconds(repositionEverySeconds);

        while (enabled)
        {
            if (player == null && sceneContext != null)
                player = sceneContext.PlayerTransform;

            if (player != null)
            {
                Vector3 newPos = GetRandomFarPosition2D(player.position);
                transform.position = newPos;
            }

            yield return wait;
        }
    }

    private IEnumerator SpawnLoop()
    {
        while (enabled)
        {
            float t = Mathf.Clamp01((Time.time - _startTime) / Mathf.Max(0.0001f, timeToReachMinInterval));
            float currentInterval = Mathf.Lerp(initialSpawnInterval, minSpawnInterval, t);

            if (enemyPrefab != null && (maxActiveEnemies <= 0 || _activeEnemies < maxActiveEnemies))
            {
                SpawnEnemy();
            }

            yield return new WaitForSeconds(currentInterval);
        }
    }

    private void SpawnEnemy()
    {
        var go = Instantiate(enemyPrefab, transform.position, Quaternion.identity);
        _activeEnemies++;

        var tracker = go.AddComponent<OnDestroyNotifier>();
        tracker.onDestroyed += () => _activeEnemies = Mathf.Max(0, _activeEnemies - 1);
    }

    private Vector3 GetRandomFarPosition2D(Vector3 center)
    {
        if (maxDistanceFromPlayer <= minDistanceFromPlayer)
            maxDistanceFromPlayer = minDistanceFromPlayer + 1f;

        float angle = Random.Range(0f, Mathf.PI * 2f);

        float safeMin = Mathf.Max(3f, minDistanceFromPlayer);
        float dist = Random.Range(safeMin, maxDistanceFromPlayer);

        Vector3 offset = new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0f) * dist;
        Vector3 candidate = center + offset;

        if (keepOriginalY)
            candidate.y = transform.position.y;

        candidate.z = transform.position.z;
        return candidate;
    }

    private class OnDestroyNotifier : MonoBehaviour
    {
        public System.Action onDestroyed;
        private void OnDestroy() => onDestroyed?.Invoke();
    }
}
