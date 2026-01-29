using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameOverUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private Button retryButton;
    [SerializeField] private ScoreManager scoreManager;

    private const string LAST_SCORE_KEY = "LAST_SCORE";

    private void Awake()
    {
        if (gameOverPanel != null)
            gameOverPanel.SetActive(false);

        if (retryButton != null)
            retryButton.onClick.AddListener(Retry);
    }

    public void Show()
    {
        if (gameOverPanel != null)
            gameOverPanel.SetActive(true);
    }

    public void Hide()
    {
        if (gameOverPanel != null)
            gameOverPanel.SetActive(false);
    }

    public void Retry()
    {
        // Save score BEFORE restarting
        if (scoreManager == null)
            scoreManager = FindFirstObjectByType<ScoreManager>();

        if (scoreManager != null)
            PlayerPrefs.SetInt(LAST_SCORE_KEY, scoreManager.Score);

        PlayerPrefs.Save();

        // Ensure time resumes
        Time.timeScale = 1f;

        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}
