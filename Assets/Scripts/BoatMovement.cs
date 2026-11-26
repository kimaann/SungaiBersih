using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class BoatMovement : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 7f;
    public float autoForwardSpeed = 4f;
    public float acceleration = 20f;
    public float deceleration = 28f;

    [Header("Slow Motion")]
    [Range(0.01f, 1f)]
    public float slowMotionMultiplier = 0.15f;
    private bool isSlowMotion = false;

    [Header("Knockback")]
    [SerializeField] private float knockbackRecovery = 8f;
    [SerializeField] private float maxKnockbackSpeed = 9f;

    private Rigidbody rb;
    private Vector3 inputDirection;
    private Vector3 movementVelocity;
    private Vector3 externalVelocity;
    private Transform visualTransform;

    public float CurrentSpeedMultiplier => isSlowMotion ? slowMotionMultiplier : 1f;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        visualTransform = transform.Find("Boatvisual");

        Transform colliderTransform = transform.Find("BoatCollider");
        if (colliderTransform != null && colliderTransform.TryGetComponent(out MeshRenderer mr))
            mr.enabled = false;

        rb.useGravity = false;
        rb.isKinematic = false;
        rb.interpolation = RigidbodyInterpolation.Interpolate;
        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        rb.constraints =
            RigidbodyConstraints.FreezePositionY |
            RigidbodyConstraints.FreezeRotation;

#if UNITY_6000_0_OR_NEWER
        Vector3 initialVelocity = rb.linearVelocity;
#else
        Vector3 initialVelocity = rb.velocity;
#endif

        movementVelocity = new Vector3(
            initialVelocity.x,
            0f,
            initialVelocity.z
        );
    }

    // Dipanggil oleh script lain untuk mengaktifkan atau mematikan slow motion.
    public void SetSlowMotion(bool active)
    {
        isSlowMotion = active;
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

        if (visualTransform != null)
        {
            // shortcut: sudut tilt tetap (-12 roll, 4 pitch), pindah ke SerializeField jika perlu tuning per kapal
            Quaternion targetRot = Quaternion.Euler(z * 4f, 0f, -x * 12f - externalVelocity.x * 1.5f);
            visualTransform.localRotation = Quaternion.Slerp(visualTransform.localRotation, targetRot, 10f * Time.deltaTime);
        }
    }

    private void FixedUpdate()
    {
        float speedMultiplier = isSlowMotion
            ? slowMotionMultiplier
            : 1f;

        // Gerakan manual WASD dan gerakan otomatis ke depan.
        Vector3 manualVelocity = inputDirection * moveSpeed;
        Vector3 automaticVelocity = Vector3.forward * autoForwardSpeed;

        Vector3 targetVelocity =
            (manualVelocity + automaticVelocity) * speedMultiplier;

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

        rate *= speedMultiplier;

        // Perubahan kecepatan dari kontrol pemain.
        movementVelocity = Vector3.MoveTowards(
            movementVelocity,
            targetVelocity,
            rate * Time.fixedDeltaTime
        );

        // Knockback berkurang secara bertahap.
        externalVelocity = Vector3.MoveTowards(
            externalVelocity,
            Vector3.zero,
            knockbackRecovery * speedMultiplier * Time.fixedDeltaTime
        );

        // Gabungkan gerakan kapal dan knockback.
        Vector3 finalVelocity =
            movementVelocity + externalVelocity * speedMultiplier;

#if UNITY_6000_0_OR_NEWER
        rb.linearVelocity = new Vector3(
            finalVelocity.x,
            0f,
            finalVelocity.z
        );
#else
        rb.velocity = new Vector3(
            finalVelocity.x,
            0f,
            finalVelocity.z
        );
#endif
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
