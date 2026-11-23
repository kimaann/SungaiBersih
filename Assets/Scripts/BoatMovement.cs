// coba
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class BoatMovement : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 7f;
    public float acceleration = 20f;
    public float deceleration = 28f;

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
        Vector3 targetVelocity =
            inputDirection * moveSpeed;

#if UNITY_6000_0_OR_NEWER
        Vector3 velocity = rb.linearVelocity;
#else
        Vector3 velocity = rb.velocity;
#endif

        Vector3 currentVelocity = new Vector3(
            velocity.x, 0f, velocity.z
        );

        float rate = inputDirection.sqrMagnitude > 0f
            ? acceleration
            : deceleration;

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
