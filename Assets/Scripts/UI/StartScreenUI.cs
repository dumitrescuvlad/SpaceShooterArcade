using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class StartScreenUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject startPanel;
    [SerializeField] private Button playButton;
    [SerializeField] private TMP_Text lastScoreText;

    [Header("Hide While On Start Screen")]
    [Tooltip("HUD root or specific HUD objects to disable while the Start Panel is shown (Score/HP/Shield/Heat).")]
    [SerializeField] private GameObject[] uiToHideOnStart;

    [Tooltip("Player object to hide while on the Start Screen.")]
    [SerializeField] private GameObject playerToHide;

    private const string LAST_SCORE_KEY = "LAST_SCORE";

    private void Awake()
    {
        if (startPanel != null)
            startPanel.SetActive(true);

        if (playButton != null)
            playButton.onClick.AddListener(OnPlayClicked);

        int lastScore = PlayerPrefs.GetInt(LAST_SCORE_KEY, 0);
        if (lastScoreText != null)
            lastScoreText.text = $"Last Score: {lastScore}";

        SetStartScreenHiddenState(true);

        Time.timeScale = 0f;
    }

    private void OnPlayClicked()
    {
        Time.timeScale = 1f;

        SetStartScreenHiddenState(false);

        if (startPanel != null)
            startPanel.SetActive(false);
    }

    private void SetStartScreenHiddenState(bool isStartScreenActive)
    {
        if (uiToHideOnStart != null)
        {
            for (int i = 0; i < uiToHideOnStart.Length; i++)
            {
                if (uiToHideOnStart[i] != null)
                    uiToHideOnStart[i].SetActive(!isStartScreenActive);
            }
        }

        if (playerToHide != null)
            playerToHide.SetActive(!isStartScreenActive);
    }
}
