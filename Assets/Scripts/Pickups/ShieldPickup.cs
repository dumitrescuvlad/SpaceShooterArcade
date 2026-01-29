using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class ShieldPickup : MonoBehaviour
{
    [Header("Pickup Settings")]
    [SerializeField] private int shieldValue = 1;

    [Tooltip("Seconds before the pickup despawns automatically.")]
    [SerializeField] private float lifeTime = 20f;

    private bool _consumed;

    private void Awake()
    {
        var col = GetComponent<Collider2D>();
        col.isTrigger = true;
    }

    private void Start()
    {
        if (lifeTime > 0f)
            Destroy(gameObject, lifeTime);
    }

    public void TryConsume(PlayerHealth player)
    {
        if (_consumed) return;
        if (player == null) return;

        bool gained = player.TryAddShield(shieldValue);
        if (!gained) return;

        _consumed = true;
        Destroy(gameObject);
    }
}
