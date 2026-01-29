using UnityEngine;

public abstract class GameOverStoppableBehaviour : MonoBehaviour, IGameOverStoppable
{
    protected virtual void OnEnable()
    {
        if (GameFlowController.Instance != null)
            GameFlowController.Instance.Register(this);
    }

    protected virtual void OnDisable()
    {
        if (GameFlowController.Instance != null)
            GameFlowController.Instance.Unregister(this);
    }

    public virtual void StopOnGameOver()
    {
        StopAllCoroutines();
        enabled = false;
    }
}
