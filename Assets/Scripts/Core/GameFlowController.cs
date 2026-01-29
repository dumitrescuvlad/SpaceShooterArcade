using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameFlowController : MonoBehaviour
{
    public static GameFlowController Instance { get; private set; }

    [Header("References")]
    [SerializeField] private SceneContext sceneContext;

    [SerializeField] private PlayerHealth playerHealth;
    [SerializeField] private GameOverUI gameOverUI;
    [SerializeField] private CameraScript cameraScript;

    [Header("Freeze")]
    [Tooltip("If not set, will use playerHealth.freezeAfterDeathSeconds.")]
    [SerializeField] private float freezeAfterDeathSecondsOverride = -1f;

    private readonly List<IGameOverStoppable> _stoppables = new();
    private bool _handledDeath;

    private void Awake()
    {
        if (FindFirstObjectByType<StartScreenUI>() == null)
            Time.timeScale = 1f;

        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        if (sceneContext == null)
            sceneContext = SceneContext.Instance;

        if (sceneContext != null)
        {
            if (playerHealth == null) playerHealth = sceneContext.PlayerHealth;
            if (gameOverUI == null) gameOverUI = sceneContext.GameOverUI;
            if (cameraScript == null) cameraScript = sceneContext.CameraScript;
        }

        // Hard fallback only if SceneContext is not present (keeps scene resilient).
        if (playerHealth == null)
            playerHealth = FindFirstObjectByType<PlayerHealth>();
        if (gameOverUI == null)
            gameOverUI = FindFirstObjectByType<GameOverUI>();
        if (cameraScript == null)
            cameraScript = FindFirstObjectByType<CameraScript>();
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

    public void Register(IGameOverStoppable stoppable)
    {
        if (stoppable == null) return;
        if (_stoppables.Contains(stoppable)) return;
        _stoppables.Add(stoppable);
    }

    public void Unregister(IGameOverStoppable stoppable)
    {
        if (stoppable == null) return;
        _stoppables.Remove(stoppable);
    }

    private void HandlePlayerDied(PlayerHealth ph)
    {
        if (_handledDeath) return;
        _handledDeath = true;

        StopRegisteredGameplaySystems();

        if (cameraScript != null)
            cameraScript.FreezeAtCurrentPosition();

        if (gameOverUI != null)
            gameOverUI.Show();

        float delay = freezeAfterDeathSecondsOverride >= 0f
            ? freezeAfterDeathSecondsOverride
            : (ph != null ? ph.freezeAfterDeathSeconds : 3f);

        StartCoroutine(FreezeAfterDelay(delay));
    }

    private void StopRegisteredGameplaySystems()
    {
        var snapshot = _stoppables.ToArray();
        for (int i = 0; i < snapshot.Length; i++)
        {
            if (snapshot[i] == null) continue;
            snapshot[i].StopOnGameOver();
        }
    }

    private IEnumerator FreezeAfterDelay(float delaySeconds)
    {
        float delay = Mathf.Max(0f, delaySeconds);
        if (delay > 0f)
            yield return new WaitForSecondsRealtime(delay);

        Time.timeScale = 0f;
    }
}
