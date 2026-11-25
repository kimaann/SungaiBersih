
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class BoatCollisionResponse : MonoBehaviour
{
    [Header("Boat Knockback")]
    [SerializeField] private float boatKnockbackStrength = 4f;
    [Header("Rock Side Shift")]
    [SerializeField] private float rockSideShiftStrength = 5.5f;

    [Header("Wood Physics")]
    [SerializeField] private float logImpulse = 9f;
    [SerializeField] private float logTorque = 14f;
    [SerializeField] private float logSpin = 4f;

    [Header("Collision Settings")]
    [SerializeField] private float responseCooldown = 0.35f;

    private Rigidbody boatRb;
    private BoatMovement boatMovement;

    private float nextResponseTime;

    private void Awake()
    {
        boatRb = GetComponent<Rigidbody>();
        boatMovement = GetComponent<BoatMovement>();
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (!collision.collider.CompareTag("Obstacle"))
            return;

        if (Time.time < nextResponseTime)
            return;

        if (collision.contactCount == 0)
            return;

        nextResponseTime = Time.time + responseCooldown;

        ContactPoint contact = collision.GetContact(0);

        // Identifikasi jenis obstacle
        MovingObstacle movingObstacle =
            collision.collider.GetComponentInParent<MovingObstacle>();

        // berarti obstacle ini merupakan batu atau obstacle statis.
        if (movingObstacle == null)
        {
            ApplyRockSideShift(collision.collider);
            return;
        }

        if (!movingObstacle.enabled)
            return;

        // Respons kapal untuk kayu tetap menggunakan
        // mekanisme sebelumnya.
        Vector3 boatDirection =
            boatRb.worldCenterOfMass - contact.point;

        boatDirection.y = 0f;

        if (boatDirection.sqrMagnitude < 0.001f)
            boatDirection = -transform.forward;

        boatDirection.Normalize();

        if (boatMovement != null)
        {
            boatMovement.ApplyKnockback(
                boatDirection * boatKnockbackStrength
            );
        }

        // Ambil Rigidbody milik kayu
        Rigidbody logRb =
            movingObstacle.GetComponent<Rigidbody>();

        // Jika tidak ditemukan, ambil dari collider
        if (logRb == null)
            logRb = collision.rigidbody;

        if (logRb == null)
            return;

        if (logRb == null)
            return;

        // Hentikan gerakan otomatis
        movingObstacle.enabled = false;

        // Aktifkan fisika dinamis
        logRb.isKinematic = false;
        logRb.useGravity = false;

        // Kunci posisi vertikal saja.
        // Semua rotasi dibebaskan.
        logRb.constraints =
            RigidbodyConstraints.FreezePositionY;

        // Hapus kecepatan lama
        logRb.linearVelocity = Vector3.zero;
        logRb.angularVelocity = Vector3.zero;

        logRb.maxAngularVelocity = 15f;

        // Arah lemparan kayu:
        // berlawanan dengan arah arus permainan (+Z).
        Vector3 logDirection = Vector3.back;

        // Berikan impuls ke belakang
        logRb.AddForce(
            logDirection * logImpulse,
            ForceMode.Impulse
        );

        // Sumbu panjang cylinder Unity adalah local Y
        Vector3 logAxis = logRb.transform.up;

        // Buat kayu terguling pada sumbu tegak lurus
        Vector3 tumbleAxis =
            Vector3.Cross(logAxis, logDirection);

        if (tumbleAxis.sqrMagnitude < 0.001f)
        {
            tumbleAxis =
                Vector3.Cross(Vector3.up, logDirection);
        }

        if (tumbleAxis.sqrMagnitude < 0.001f)
            tumbleAxis = Vector3.right;

        tumbleAxis.Normalize();

        // Terguling dan berputar
        Vector3 totalTorque =
            tumbleAxis * logTorque +
            logAxis.normalized * logSpin;

        logRb.AddTorque(
            totalTorque,
            ForceMode.Impulse
        );

        logRb.WakeUp();
    }


    private void ApplyRockSideShift(Collider rockCollider)
    {
        // Posisi kapal dan batu pada sumbu X
        float offsetX =
            boatRb.worldCenterOfMass.x -
            rockCollider.bounds.center.x;

        // Tentukan arah menjauhi batu
        float sideDirection;

        if (Mathf.Abs(offsetX) > 0.05f)
        {
            sideDirection = Mathf.Sign(offsetX);
        }
        else
        {
            // Jika posisi X hampir sejajar,
            // gunakan arah kiri sebagai pilihan awal.
            sideDirection = -1f;
        }

        // Dorongan hanya pada sumbu horizontal X
        Vector3 sideShift =
            Vector3.right *
            sideDirection *
            rockSideShiftStrength;

        if (boatMovement != null)
        {
            boatMovement.ApplyKnockback(sideShift);
        }

        Debug.Log(
            "Rock Collision: Boat shifted " +
            (sideDirection < 0 ? "LEFT" : "RIGHT")
        );
    }

}
