using UnityEngine;

public class SceneContext : MonoBehaviour
{
    public static SceneContext Instance { get; private set; }

    [Header("Core References")]
    [SerializeField] private PlayerHealth playerHealth;
    [SerializeField] private ShootingScript playerShooting;
    [SerializeField] private CameraScript cameraScript;
    [SerializeField] private GameOverUI gameOverUI;
    [SerializeField] private ScoreManager scoreManager;

    public PlayerHealth PlayerHealth => playerHealth;
    public ShootingScript PlayerShooting => playerShooting;
    public CameraScript CameraScript => cameraScript;
    public GameOverUI GameOverUI => gameOverUI;
    public ScoreManager ScoreManager => scoreManager;

    public Transform PlayerTransform => playerHealth != null ? playerHealth.transform : null;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        AutoWireIfMissing();
    }

    private void OnValidate()
    {
        if (!Application.isPlaying)
            AutoWireIfMissing();
    }

    private void AutoWireIfMissing()
    {
        if (playerHealth == null)
            playerHealth = FindFirstObjectByType<PlayerHealth>();

        if (playerShooting == null)
            playerShooting = FindFirstObjectByType<ShootingScript>();

        if (cameraScript == null)
            cameraScript = FindFirstObjectByType<CameraScript>();

        if (gameOverUI == null)
            gameOverUI = FindFirstObjectByType<GameOverUI>();

        if (scoreManager == null)
            scoreManager = FindFirstObjectByType<ScoreManager>();
    }
}
