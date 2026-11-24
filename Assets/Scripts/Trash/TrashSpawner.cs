
using System.Collections.Generic;
using UnityEngine;

public class TrashSpawner : MonoBehaviour
{
    [Header("Prefab Sampah")]
    [SerializeField] private TrashItem[] trashPrefabs;

    [Header("Jumlah Sampah")]
    [SerializeField] private int spawnCount = 25;

    [Header("Area Spawn")]
    [SerializeField] private Vector2 spawnAreaSize =
        new Vector2(16f, 180f);

    [SerializeField] private float spawnHeight = 0.07f;

    [Header("Jarak Antar Sampah")]
    [SerializeField] private float minDistance = 3f;

    [SerializeField] private int maxAttemptsPerTrash = 30;

    private readonly List<Vector3> spawnedPositions =
        new List<Vector3>();

    private void Start()
    {
        SpawnTrash();
    }

    public void SpawnTrash()
    {
        if (trashPrefabs == null || trashPrefabs.Length == 0)
        {
            Debug.LogWarning(
                "TrashSpawner: Belum ada prefab sampah!"
            );
            return;
        }

        spawnedPositions.Clear();

        int spawnedCount = 0;
        int attempts = 0;

        int maxAttempts =
            spawnCount * maxAttemptsPerTrash;

        while (spawnedCount < spawnCount &&
               attempts < maxAttempts)
        {
            attempts++;

            TrashItem prefab = trashPrefabs[
                Random.Range(0, trashPrefabs.Length)
            ];

            if (prefab == null)
                continue;

            Vector3 randomPosition = new Vector3(
                transform.position.x +
                Random.Range(
                    -spawnAreaSize.x / 2f,
                    spawnAreaSize.x / 2f
                ),

                spawnHeight,

                transform.position.z +
                Random.Range(
                    -spawnAreaSize.y / 2f,
                    spawnAreaSize.y / 2f
                )
            );

            bool tooClose = false;

            foreach (Vector3 existingPosition
                     in spawnedPositions)
            {
                Vector2 currentXZ = new Vector2(
                    randomPosition.x,
                    randomPosition.z
                );

                Vector2 existingXZ = new Vector2(
                    existingPosition.x,
                    existingPosition.z
                );

                if ((currentXZ - existingXZ).sqrMagnitude <
                    minDistance * minDistance)
                {
                    tooClose = true;
                    break;
                }
            }

            if (tooClose)
                continue;

            Quaternion randomRotation =
                Quaternion.Euler(
                    0f,
                    Random.Range(0f, 360f),
                    0f
                );

            Instantiate(
                prefab,
                randomPosition,
                randomRotation,
                transform
            );

            spawnedPositions.Add(randomPosition);
            spawnedCount++;
        }

        Debug.Log(
            $"Berhasil spawn {spawnedCount} dari {spawnCount} sampah."
        );
    }
}
