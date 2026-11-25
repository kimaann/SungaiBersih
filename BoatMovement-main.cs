
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class BoatMovement : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 7f;
    public float autoForwardSpeed = 4f;
    public float acceleration = 20f;
    public float deceleration = 28f;

    [Header("Knockback")]
    [SerializeField] private float knockbackRecovery = 8f;
    [SerializeField] private float maxKnockbackSpeed = 9f;

    private Rigidbody rb;

    private Vector3 inputDirection;
    private Vector3 movementVelocity;
    private Vector3 externalVelocity;

    private void Awake()
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

        Vector3 initialVelocity = rb.linearVelocity;

        movementVelocity = new Vector3(
            initialVelocity.x,
            0f,
            initialVelocity.z
        );
    }

    private void Update()
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

    private void FixedUpdate()
    {
        // Input manual WASD
        Vector3 manualVelocity =
            inputDirection * moveSpeed;

        // Kecepatan otomatis ke depan
        Vector3 automaticVelocity =
            Vector3.forward * autoForwardSpeed;

        // Gabungkan keduanya
        Vector3 targetVelocity =
            manualVelocity + automaticVelocity;

#if UNITY_6000_0_OR_NEWER
        Vector3 velocity = rb.linearVelocity;
#else
        Vector3 velocity = rb.velocity;
#endif

        Vector3 currentVelocity = new Vector3(
            velocity.x,
            0f,
            velocity.z
        );

        float rate =
            targetVelocity.magnitude > currentVelocity.magnitude
            ? acceleration
            : deceleration;

        // Kecepatan dari kontrol pemain
        movementVelocity = Vector3.MoveTowards(
            movementVelocity,
            targetVelocity,
            rate * Time.fixedDeltaTime
        );

        // Knockback berkurang secara bertahap
        externalVelocity = Vector3.MoveTowards(
            externalVelocity,
            Vector3.zero,
            knockbackRecovery * Time.fixedDeltaTime
        );

        // Gabungkan kedua jenis kecepatan
        Vector3 finalVelocity =
            movementVelocity + externalVelocity;

        rb.linearVelocity = new Vector3(
            finalVelocity.x,
            0f,
            finalVelocity.z
        );
    }

    public void ApplyKnockback(Vector3 velocityChange)
    {
        velocityChange.y = 0f;

        externalVelocity += velocityChange;

        externalVelocity = Vector3.ClampMagnitude(
            externalVelocity,
            maxKnockbackSpeed
        );
    }
}
