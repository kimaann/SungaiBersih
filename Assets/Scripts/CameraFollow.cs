
using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [Header("Target")]
    public Transform target;
    public Rigidbody boatRb;

    [Header("Camera Position")]
    public Vector3 offset = new Vector3(0f, 13f, -15f);
    public float lookAhead = 7f;

    [Header("Smooth Follow")]
    [Range(0.01f, 0.5f)]
    public float smoothTime = 0.08f;

    [Header("Subtle Parallax")]
    public float sideLag = 0.3f;
    public float forwardLag = 0.15f;
    public float lagSmoothness = 10f;
    public float referenceSpeed = 7f;

    private Vector3 followVelocity;
    private Vector3 currentLag;

    void Start()
    {
        if (target == null)
        {
            Debug.LogError("Camera target belum diisi!");
            enabled = false;
            return;
        }

        if (boatRb == null)
            boatRb = target.GetComponent<Rigidbody>();

        transform.position = target.position + offset;

        // Tentukan sudut kamera satu kali.
        // Sudut tetap agar tidak bergoyang tiap frame.
        Vector3 lookDirection =
            Vector3.forward * lookAhead - offset;

        transform.rotation = Quaternion.LookRotation(
            lookDirection,
            Vector3.up
        );
    }

    void LateUpdate()
    {
        if (target == null) return;

        float dt = Time.deltaTime;
        if (dt <= 0f) return;

        Vector3 velocity = Vector3.zero;

        if (boatRb != null)
        {
#if UNITY_6000_0_OR_NEWER
            velocity = boatRb.linearVelocity;
#else
            velocity = boatRb.velocity;
#endif
        }

        float speed = Mathf.Max(referenceSpeed, 0.1f);

        Vector3 desiredLag = new Vector3(
            -Mathf.Clamp(velocity.x / speed, -1f, 1f)
                * sideLag,
            0f,
            -Mathf.Clamp(velocity.z / speed, -1f, 1f)
                * forwardLag
        );

        float blend = 1f - Mathf.Exp(
            -lagSmoothness * dt
        );

        currentLag = Vector3.Lerp(
            currentLag,
            desiredLag,
            blend
        );

        Vector3 desiredPosition =
            target.position + offset + currentLag;

        transform.position = Vector3.SmoothDamp(
            transform.position,
            desiredPosition,
            ref followVelocity,
            smoothTime,
            Mathf.Infinity,
            dt
        );
    }
}
