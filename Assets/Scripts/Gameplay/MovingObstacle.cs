
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class MovingObstacle : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private Vector3 direction = Vector3.right;
    [SerializeField] private float speed = 2f;
    [SerializeField] private float travelDistance = 10f;

    private Rigidbody rb;
    private Vector3 initialLocalPosition;
    private Quaternion initialLocalRotation;
    private float elapsed;
    private BoatMovement boatMovement;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        boatMovement = FindFirstObjectByType<BoatMovement>();

        initialLocalPosition = transform.localPosition;
        initialLocalRotation = transform.localRotation;

        if (direction.sqrMagnitude < 0.001f)
            direction = Vector3.right;

        direction.Normalize();

        speed = Mathf.Max(0.1f, speed);
        travelDistance = Mathf.Max(0.1f, travelDistance);

        ResetObstacle();
    }

    public void ResetObstacle()
    {
        enabled = true;
        elapsed = 0f;

        if (rb != null)
        {
            if (!rb.isKinematic)
            {
                rb.linearVelocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
            }
            rb.useGravity = false;
            rb.isKinematic = true;
            rb.interpolation = RigidbodyInterpolation.Interpolate;
        }

        transform.localPosition = initialLocalPosition;
        transform.localRotation = initialLocalRotation;
    }

    private void FixedUpdate()
    {
        float multiplier = boatMovement != null ? boatMovement.CurrentSpeedMultiplier : 1f;
        elapsed += Time.fixedDeltaTime * multiplier;

        float distance = Mathf.PingPong(
            elapsed * speed,
            travelDistance
        );

        Vector3 localPos = initialLocalPosition + direction * distance;
        Vector3 worldPos = transform.parent != null
            ? transform.parent.TransformPoint(localPos)
            : localPos;

        rb.MovePosition(worldPos);
    }
}
