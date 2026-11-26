using UnityEngine;

public class EnvironmentLooper : MonoBehaviour
{
    [Header("References")]
    public Transform boat;
    public Transform riverReference;
    public Transform[] segments;

    [Header("Settings")]
    public bool arrangeSegmentsOnStart = true;
    public bool loopForward = true;
    public bool loopBackward = true;

    [Header("Course Mode")]
    [Tooltip("Aktifkan untuk level dengan 3 environment dan finish tetap di EnvironmentSegment_03.")]
    public bool finiteCourseMode = true;

    private float segmentLength;
    private bool isReady;

    public float SegmentLength => segmentLength;

    private void Start()
    {
        if (boat == null)
        {
            Debug.LogError("EnvironmentLooper: Boat belum diisi!");
            enabled = false;
            return;
        }

        if (segments == null || segments.Length < 2)
        {
            Debug.LogError("EnvironmentLooper: Minimal harus ada 2 Environment Segments!");
            enabled = false;
            return;
        }

        foreach (Transform segment in segments)
        {
            if (segment == null)
            {
                Debug.LogError("EnvironmentLooper: Ada Segment yang belum diisi!");
                enabled = false;
                return;
            }
        }

        if (riverReference == null)
        {
            Debug.LogError("EnvironmentLooper: River Reference belum diisi!");
            enabled = false;
            return;
        }

        Renderer riverRenderer = riverReference.GetComponent<Renderer>();

        if (riverRenderer == null)
            riverRenderer = riverReference.GetComponentInChildren<Renderer>(true);

        if (riverRenderer == null)
        {
            Debug.LogError("EnvironmentLooper: Renderer River tidak ditemukan!");
            enabled = false;
            return;
        }

        segmentLength = riverRenderer.bounds.size.z;

        if (segmentLength <= 0.01f)
        {
            Debug.LogError("EnvironmentLooper: Panjang Segment tidak valid!");
            enabled = false;
            return;
        }

        // Susun segment secara berurutan: 01, 02, lalu 03.
        if (arrangeSegmentsOnStart)
        {
            Vector3 firstSegmentPosition = segments[0].position;

            for (int i = 1; i < segments.Length; i++)
            {
                segments[i].position =
                    firstSegmentPosition +
                    Vector3.forward * segmentLength * i;
            }
        }

        // Untuk level Sungai Bersih, jangan daur ulang segment.
        // Pemain harus benar-benar melewati EnvironmentSegment_03.
        if (finiteCourseMode)
        {
            loopForward = false;
            loopBackward = false;

            Debug.Log(
                "EnvironmentLooper: FINITE COURSE aktif. " +
                "Environment 01 -> 02 -> 03 -> Finish."
            );
        }

        isReady = true;

        Debug.Log(
            "Environment Loop siap. Panjang Segment: " +
            segmentLength
        );
    }

    private void LateUpdate()
    {
        if (!isReady || finiteCourseMode)
            return;

        float boatZ = boat.position.z;

        if (loopForward)
        {
            int safety = 0;

            while (safety < 100)
            {
                int minIndex = GetMinSegmentIndex();
                int maxIndex = GetMaxSegmentIndex();

                if (boatZ <=
                    segments[minIndex].position.z + segmentLength)
                    break;

                segments[minIndex].position =
                    segments[maxIndex].position +
                    Vector3.forward * segmentLength;

                foreach (MovingObstacle obstacle in
                         segments[minIndex].GetComponentsInChildren<MovingObstacle>(true))
                {
                    obstacle.ResetObstacle();
                }

                safety++;
            }
        }

        if (loopBackward)
        {
            int safety = 0;

            while (safety < 100)
            {
                int minIndex = GetMinSegmentIndex();
                int maxIndex = GetMaxSegmentIndex();

                if (boatZ >= segments[minIndex].position.z)
                    break;

                segments[maxIndex].position =
                    segments[minIndex].position -
                    Vector3.forward * segmentLength;

                safety++;
            }
        }
    }

    private int GetMinSegmentIndex()
    {
        int index = 0;

        for (int i = 1; i < segments.Length; i++)
        {
            if (segments[i].position.z < segments[index].position.z)
                index = i;
        }

        return index;
    }

    private int GetMaxSegmentIndex()
    {
        int index = 0;

        for (int i = 1; i < segments.Length; i++)
        {
            if (segments[i].position.z > segments[index].position.z)
                index = i;
        }

        return index;
    }
}
