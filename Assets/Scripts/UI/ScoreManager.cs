using System;
using TMPro;
using UnityEngine;

public class ScoreManager : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private TMP_Text scoreText;

    private int score;
    private int kills;
    private int abilityPoints;

    [Tooltip("How many kills are required to gain 1 ability point.")]
    [SerializeField] private int killsPerAbilityPoint = 5;

    public int Score => score;
    public int Kills => kills;
    public int AbilityPoints => abilityPoints;

    public event Action<int> OnScoreChanged;
    public event Action<int> OnAbilityPointsChanged;

    private void Awake()
    {
        ResetScore();
    }

    public void AddKill()
    {
        kills++;

        // Keep existing behavior: 1 kill = 1 score
        AddScore(1);

        TryGrantAbilityPointFromKills();
    }

    public void AddScore(int amount)
    {
        if (amount <= 0) return;

        score += amount;
        UpdateUI();
        OnScoreChanged?.Invoke(score);
    }

    public void ResetScore()
    {
        score = 0;
        kills = 0;
        abilityPoints = 0;

        UpdateUI();
        OnScoreChanged?.Invoke(score);
        OnAbilityPointsChanged?.Invoke(abilityPoints);
    }

    private void TryGrantAbilityPointFromKills()
    {
        int threshold = Mathf.Max(1, killsPerAbilityPoint);

        // Grant 1 point every X kills (5, 10, 15, ...)
        if (kills % threshold == 0)
        {
            abilityPoints++;
            OnAbilityPointsChanged?.Invoke(abilityPoints);
        }
    }

    public bool ConsumeAbilityPoint()
    {
        if (abilityPoints <= 0) return false;

        abilityPoints--;
        OnAbilityPointsChanged?.Invoke(abilityPoints);
        return true;
    }

    private void UpdateUI()
    {
        if (scoreText != null)
            scoreText.text = $"Score: {score}";
    }
}
