using UnityEngine;

public class CameraScript : MonoBehaviour
{
    public Camera gameCamera;
    public Transform target;

    public Vector3 offset = new Vector3(0f, 0f, -10f);
    public float followSmoothTime = 0.15f;
    private Vector3 followVelocity = Vector3.zero;

    public float minZoom = 10f;
    public float maxZoom = 21f;
    public float zoomSpeed = 10f;
    public float zoomSmoothTime = 0.1f;
    private float zoomVelocity;

    private bool _frozen;
    private Vector3 _frozenPosition;

    void Awake()
    {
        if (!gameCamera) gameCamera = GetComponent<Camera>();
        if (!target)
        {
            if (transform.parent != null) target = transform.parent;
            if (!target)
            {
                var p = GameObject.FindGameObjectWithTag(Tags.Player);
                if (p) target = p.transform;
            }
        }

        if (gameCamera && !gameCamera.orthographic)
            gameCamera.orthographic = true;
    }

    void LateUpdate()
    {
        if (_frozen)
        {
            transform.position = _frozenPosition;
            return;
        }

        FollowTargetWithDelay();
        HandleZoom();
    }

    public void FreezeAtCurrentPosition()
    {
        _frozenPosition = transform.position;
        _frozen = true;
    }

    public void Unfreeze()
    {
        _frozen = false;
    }

    private void FollowTargetWithDelay()
    {
        if (!target) return;

        Vector3 desired = target.position + offset;
        transform.position = Vector3.SmoothDamp(
            transform.position,
            desired,
            ref followVelocity,
            followSmoothTime
        );
    }

    private void HandleZoom()
    {
        if (!gameCamera) return;

        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (Mathf.Abs(scroll) > Mathf.Epsilon)
        {
            float targetSize = gameCamera.orthographicSize - scroll * zoomSpeed;
            targetSize = Mathf.Clamp(targetSize, minZoom, maxZoom);

            gameCamera.orthographicSize = Mathf.SmoothDamp(
                gameCamera.orthographicSize,
                targetSize,
                ref zoomVelocity,
                zoomSmoothTime
            );
        }
    }
}
