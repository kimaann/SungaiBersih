
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class BoatMovement : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 7f;
    public float acceleration = 20f;
    public float deceleration = 28f;

    [Header("Slow Motion")]
    [Range(0.01f, 1f)]
    public float slowMotionMultiplier = 0.15f;

    private bool isSlowMotion = false;

    private Rigidbody rb;
    private Vector3 inputDirection;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();

        rb.useGravity = false;
        rb.isKinematic = false;

        rb.interpolation =
            RigidbodyInterpolation.Interpolate;

        rb.collisionDetectionMode =
            CollisionDetectionMode.ContinuousDynamic;

        rb.constraints =
            RigidbodyConstraints.FreezePositionY |
            RigidbodyConstraints.FreezeRotation;
    }

    // Dipanggil oleh script lain untuk mengatur slow motion
    public void SetSlowMotion(bool active)
    {
        isSlowMotion = active;
    }

    void Update()
    {
        float x = 0f;
        float z = 0f;

        if (Input.GetKey(KeyCode.W)) z += 1f;
        if (Input.GetKey(KeyCode.S)) z -= 1f;
        if (Input.GetKey(KeyCode.A)) x -= 1f;
        if (Input.GetKey(KeyCode.D)) x += 1f;

        inputDirection = Vector3.ClampMagnitude(
            new Vector3(x, 0f, z),
            1f
        );
    }

    void FixedUpdate()
    {
        float speedMultiplier = isSlowMotion
            ? slowMotionMultiplier
            : 1f;

        Vector3 targetVelocity =
            inputDirection * moveSpeed * speedMultiplier;

#if UNITY_6000_0_OR_NEWER
        Vector3 velocity = rb.linearVelocity;
#else
        Vector3 velocity = rb.velocity;
#endif

        Vector3 currentVelocity = new Vector3(
            velocity.x, 0f, velocity.z
        );

        float rate = (
            inputDirection.sqrMagnitude > 0f
            ? acceleration
            : deceleration
        ) * speedMultiplier;

        Vector3 newVelocity = Vector3.MoveTowards(
            currentVelocity,
            targetVelocity,
            rate * Time.fixedDeltaTime
        );

#if UNITY_6000_0_OR_NEWER
        rb.linearVelocity = newVelocity;
#else
        rb.velocity = newVelocity;
#endif
    }
}
