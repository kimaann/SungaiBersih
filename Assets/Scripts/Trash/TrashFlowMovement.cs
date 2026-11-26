
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class TrashFlowMovement : MonoBehaviour
{
    [Header("ARUS SUNGAI")]
    [SerializeField] private Vector3 flowDirection =
        Vector3.forward;

    [SerializeField] private float currentSpeed = 1.2f;

    private Rigidbody rb;
    private Vector3 normalizedFlowDirection;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();

        // Sampah tidak dipengaruhi gravitasi
        rb.useGravity = false;
        rb.isKinematic = true;

        rb.interpolation =
            RigidbodyInterpolation.Interpolate;

        // Kunci posisi vertikal dan seluruh rotasi
        rb.constraints =
            RigidbodyConstraints.FreezePositionY |
            RigidbodyConstraints.FreezeRotation;

        // Pastikan arah arus hanya pada sumbu horizontal
        flowDirection.y = 0f;

        if (flowDirection.sqrMagnitude < 0.001f)
            flowDirection = Vector3.forward;

        normalizedFlowDirection =
            flowDirection.normalized;

        currentSpeed = Mathf.Max(0f, currentSpeed);
    }

    private void FixedUpdate()
    {
        if (currentSpeed <= 0f)
            return;

        // Gerakan lurus mengikuti arus sungai
        Vector3 movement =
            normalizedFlowDirection *
            currentSpeed *
            Time.fixedDeltaTime;

        rb.MovePosition(rb.position + movement);
    }
}
