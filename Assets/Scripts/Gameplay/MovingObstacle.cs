
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class MovingObstacle : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private Vector3 direction = Vector3.right;
    [SerializeField] private float speed = 2f;
    [SerializeField] private float travelDistance = 10f;

    private Rigidbody rb;
    private Vector3 startPosition;
    private float elapsed;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();

        rb.useGravity = false;
        rb.isKinematic = true;
        rb.interpolation = RigidbodyInterpolation.Interpolate;

        startPosition = rb.position;

        if (direction.sqrMagnitude < 0.001f)
            direction = Vector3.right;

        direction.Normalize();

        speed = Mathf.Max(0.1f, speed);
        travelDistance = Mathf.Max(0.1f, travelDistance);
    }

    private void FixedUpdate()
    {
        elapsed += Time.fixedDeltaTime;

        float distance = Mathf.PingPong(
            elapsed * speed,
            travelDistance
        );

        rb.MovePosition(
            startPosition + direction * distance
        );
    }
}
