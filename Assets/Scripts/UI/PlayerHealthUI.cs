using TMPro;
using UnityEngine;

public class PlayerHealthUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private SceneContext sceneContext;
    [SerializeField] private PlayerHealth playerHealth;
    [SerializeField] private TMP_Text hpText;

    private void Awake()
    {
        if (hpText == null)
            hpText = GetComponent<TMP_Text>();

        if (sceneContext == null)
            sceneContext = SceneContext.Instance;

        if (playerHealth == null && sceneContext != null)
            playerHealth = sceneContext.PlayerHealth;

        if (playerHealth == null)
            playerHealth = FindFirstObjectByType<PlayerHealth>();
    }

    private void OnEnable()
    {
        if (playerHealth != null)
            playerHealth.OnHealthChanged += HandleHealthChanged;

        if (playerHealth != null)
            HandleHealthChanged(playerHealth.CurrentHealth, playerHealth.MaxHealth);
    }

    private void OnDisable()
    {
        if (playerHealth != null)
            playerHealth.OnHealthChanged -= HandleHealthChanged;
    }

    private void HandleHealthChanged(int current, int max)
    {
        if (hpText == null) return;
        hpText.text = $"HP: {current}/{max}";
    }
}
