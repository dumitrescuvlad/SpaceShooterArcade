using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerResourcesUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private SceneContext sceneContext;
    [SerializeField] private PlayerHealth playerHealth;
    [SerializeField] private ShootingScript shootingScript;

    [Header("Shield UI")]
    [SerializeField] private Slider shieldSlider;
    [SerializeField] private TMP_Text shieldText;

    [Header("Heat UI")]
    [SerializeField] private Slider heatSlider;
    [SerializeField] private TMP_Text heatText;

    private void Awake()
    {
        if (sceneContext == null)
            sceneContext = SceneContext.Instance;

        if (sceneContext != null)
        {
            if (playerHealth == null) playerHealth = sceneContext.PlayerHealth;
            if (shootingScript == null) shootingScript = sceneContext.PlayerShooting;
        }

        if (playerHealth == null)
            playerHealth = FindFirstObjectByType<PlayerHealth>();

        if (shootingScript == null)
            shootingScript = FindFirstObjectByType<ShootingScript>();
    }

    private void OnEnable()
    {
        if (playerHealth != null)
        {
            playerHealth.OnShieldChanged += HandleShieldChanged;
            HandleShieldChanged(playerHealth.CurrentShield, playerHealth.MaxShield);
        }

        if (shootingScript != null)
        {
            shootingScript.OnHeatChanged += HandleHeatChanged;
            HandleHeatChanged(shootingScript.CurrentHeat, shootingScript.MaxHeat, shootingScript.IsOverheated);
        }
    }

    private void OnDisable()
    {
        if (playerHealth != null)
            playerHealth.OnShieldChanged -= HandleShieldChanged;

        if (shootingScript != null)
            shootingScript.OnHeatChanged -= HandleHeatChanged;
    }

    private void HandleShieldChanged(int current, int max)
    {
        if (shieldSlider != null)
        {
            shieldSlider.maxValue = Mathf.Max(1, max);
            shieldSlider.value = Mathf.Clamp(current, 0, max);
        }

        if (shieldText != null)
            shieldText.text = $"Shield: {current}/{max}";
    }

    private void HandleHeatChanged(float current, float max, bool overheated)
    {
        if (heatSlider != null)
        {
            heatSlider.maxValue = Mathf.Max(1f, max);
            heatSlider.value = Mathf.Clamp(current, 0f, max);
        }

        if (heatText != null)
        {
            string status = overheated ? " (OVERHEATED)" : "";
            heatText.text = $"Heat: {Mathf.CeilToInt(current)}/{Mathf.CeilToInt(max)}{status}";
        }
    }
}
