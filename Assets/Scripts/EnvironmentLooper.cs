
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

    private float segmentLength;
    private bool isReady;

    void Start()
    {
        // Validasi Boat
        if (boat == null)
        {
            Debug.LogError("EnvironmentLooper: Boat belum diisi!");
            enabled = false;
            return;
        }

        // Validasi Segments
        if (segments == null || segments.Length < 2)
        {
            Debug.LogError("Minimal harus ada 2 Environment Segments!");
            enabled = false;
            return;
        }

        foreach (Transform segment in segments)
        {
            if (segment == null)
            {
                Debug.LogError("Ada Segment yang belum diisi!");
                enabled = false;
                return;
            }
        }

        // Cari Renderer River
        if (riverReference == null)
        {
            Debug.LogError("River Reference belum diisi!");
            enabled = false;
            return;
        }

        Renderer riverRenderer =
            riverReference.GetComponent<Renderer>();

        if (riverRenderer == null)
        {
            riverRenderer =
                riverReference.GetComponentInChildren<Renderer>();
        }

        if (riverRenderer == null)
        {
            Debug.LogError("Renderer River tidak ditemukan!");
            enabled = false;
            return;
        }

        // Hitung panjang River berdasarkan ukuran aslinya
        segmentLength = riverRenderer.bounds.size.z;

        if (segmentLength <= 0.01f)
        {
            Debug.LogError("Panjang River tidak valid!");
            enabled = false;
            return;
        }

        // Susun semua segment secara berurutan
        if (arrangeSegmentsOnStart)
        {
            Vector3 startPosition = segments[0].position;

            for (int i = 1; i < segments.Length; i++)
            {
                segments[i].position =
                    startPosition +
                    Vector3.forward * segmentLength * i;
            }
        }

        isReady = true;

        Debug.Log(
            "Environment Loop aktif. Panjang Segment: " +
            segmentLength
        );
    }

    void LateUpdate()
    {
        if (!isReady) return;

        float boatZ = boat.position.z;

        // Loop ke depan
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

                foreach (MovingObstacle obs in segments[minIndex].GetComponentsInChildren<MovingObstacle>(true))
                    obs.ResetObstacle();

                safety++;
            }
        }

        // Loop ke belakang
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

    int GetMinSegmentIndex()
    {
        int index = 0;

        for (int i = 1; i < segments.Length; i++)
        {
            if (segments[i].position.z <
                segments[index].position.z)
            {
                index = i;
            }
        }

        return index;
    }

    int GetMaxSegmentIndex()
    {
        int index = 0;

        for (int i = 1; i < segments.Length; i++)
        {
            if (segments[i].position.z >
                segments[index].position.z)
            {
                index = i;
            }
        }

        return index;
    }
}
