using TMPro;
using UnityEngine;

public class AbilityHotkeys : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private ScoreManager scoreManager;
    [SerializeField] private AbilityUpgrades abilityUpgrades;

    [Header("UI")]
    [SerializeField] private TMP_Text abilityInfoText;

    [Header("Keys")]
    [SerializeField] private KeyCode damageKey = KeyCode.Alpha1;
    [SerializeField] private KeyCode bulletSpeedKey = KeyCode.Alpha2;
    [SerializeField] private KeyCode shipSpeedKey = KeyCode.Alpha3;

    // Support both top-row 4 and numpad 4
    [SerializeField] private KeyCode freezeKeyPrimary = KeyCode.Alpha4;
    [SerializeField] private KeyCode freezeKeySecondary = KeyCode.Keypad4;

    private void Awake()
    {
        if (scoreManager == null)
            scoreManager = FindFirstObjectByType<ScoreManager>();

        if (abilityUpgrades == null)
            abilityUpgrades = FindFirstObjectByType<AbilityUpgrades>();

        RefreshText();
    }

    private void OnEnable()
    {
        if (scoreManager != null)
            scoreManager.OnAbilityPointsChanged += HandleAbilityPointsChanged;

        if (abilityUpgrades != null)
            abilityUpgrades.OnLevelsChanged += HandleLevelsChanged;

        RefreshText();
    }

    private void OnDisable()
    {
        if (scoreManager != null)
            scoreManager.OnAbilityPointsChanged -= HandleAbilityPointsChanged;

        if (abilityUpgrades != null)
            abilityUpgrades.OnLevelsChanged -= HandleLevelsChanged;
    }

    private void Update()
    {
        if (scoreManager == null || abilityUpgrades == null)
            return;

        if (Input.GetKeyDown(damageKey))
            abilityUpgrades.UpgradeDamage();

        if (Input.GetKeyDown(bulletSpeedKey))
            abilityUpgrades.UpgradeBulletSpeed();

        if (Input.GetKeyDown(shipSpeedKey))
            abilityUpgrades.UpgradePlayerSpeed();

        if (Input.GetKeyDown(freezeKeyPrimary) || Input.GetKeyDown(freezeKeySecondary))
            abilityUpgrades.FreezeEnemies();
    }

    private void HandleAbilityPointsChanged(int _)
    {
        RefreshText();
    }

    private void HandleLevelsChanged()
    {
        RefreshText();
    }

    private void RefreshText()
    {
        if (abilityInfoText == null || scoreManager == null || abilityUpgrades == null)
            return;

        abilityInfoText.text =
            $"AP: {scoreManager.AbilityPoints}  |  " +
            $"DMG Lv {abilityUpgrades.DamageLevel}  |  " +
            $"SPD Lv {abilityUpgrades.BulletSpeedLevel}  |  " +
            $"SHIP Lv {abilityUpgrades.ShipSpeedLevel}";
    }
}
