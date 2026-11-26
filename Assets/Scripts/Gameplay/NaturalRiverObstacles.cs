using UnityEngine;

public class NaturalRiverObstacles : MonoBehaviour
{
    [Header("Custom 3D Assets (Drag Prefab Stones / Wood Logs ke sini)")]
    [Tooltip("Masukkan prefab dari asset Stones (Pixelcloud). Jika kosong, otomatis memakai batu yang sudah ada di scene.")]
    public GameObject[] rockPrefabs;

    [Tooltip("Masukkan prefab dari asset Awesome Free Scans - Wood Logs. Jika kosong, otomatis memakai kayu yang sudah ada di scene.")]
    public GameObject[] logPrefabs;

    [Header("Pengaturan Susunan Natural")]
    [Tooltip("Jumlah gelombang/titik rintangan di setiap EnvironmentSegment (panjang 220m).")]
    [SerializeField] private int wavesPerSegment = 9;

    [Tooltip("Lebar setengah sungai dari tengah ke tepi.")]
    [SerializeField] private float riverHalfWidth = 9.2f;

    [Tooltip("Lebar jalur aman minimal yang berkelok untuk dilewati kapal.")]
    [SerializeField] private float safeChannelHalfWidth = 3.2f;

    [SerializeField] private int randomSeed = 42;

    private void Start()
    {
        EnvironmentLooper looper = GetComponent<EnvironmentLooper>();
        if (looper == null)
            looper = FindFirstObjectByType<EnvironmentLooper>();

        if (looper == null || looper.segments == null || looper.segments.Length == 0)
            return;

        AutoLoadInstalledAssetsIfEmpty();

        Random.State oldState = Random.state;
        Random.InitState(randomSeed);

        for (int s = 0; s < looper.segments.Length; s++)
        {
            Transform segment = looper.segments[s];
            if (segment != null)
                PopulateSegment(segment, s);
        }

        Random.state = oldState;
    }

    private void PopulateSegment(Transform segment, int segmentIndex)
    {
        Transform templateRock = segment.Find("Rock_01");
        Transform templateLog = segment.Find("Log_01");

        if (templateRock == null || templateLog == null)
            return;

        ReplaceVisualWithCustomPrefab(templateRock, rockPrefabs);
        ReplaceVisualWithCustomPrefab(templateLog, logPrefabs);

        // Sembunyikan template asli di Z=-75/-60 supaya tidak menghalangi titik start kapal di Segment 01
        templateRock.gameObject.SetActive(false);
        templateLog.gameObject.SetActive(false);

        Transform container = new GameObject("NaturalObstacles_Container").transform;
        container.SetParent(segment, false);

        // Segment 01 mulai dari Z = -48 (di depan kapal yang spawn di Z = -85.6)
        float zStart = segmentIndex == 0 ? -48f : -92f;
        float zEnd = 92f;
        float stepZ = (zEnd - zStart) / Mathf.Max(1, wavesPerSegment);

        for (int i = 0; i < wavesPerSegment; i++)
        {
            float baseZ = zStart + i * stepZ + Random.Range(-3.5f, 3.5f);

            // Jalur aman berkelok (meandering S-curve) supaya sungai tidak lurus monoton
            float phase = (segmentIndex * wavesPerSegment + i) * 0.85f;
            float safeCenterX = Mathf.Sin(phase) * 4.2f;

            // 1. Gugusan Batu Tepian Kiri & Kanan (variasi ukuran tiap cluster: raksasa, sedang, kerikil)
            if (Random.value > 0.15f)
                SpawnRockCluster(container, templateRock.gameObject, -riverHalfWidth + Random.Range(0.3f, 1.8f), baseZ + Random.Range(-5f, 5f), Random.Range(1, 5), Random.Range(0.75f, 1.45f));

            if (Random.value > 0.15f)
                SpawnRockCluster(container, templateRock.gameObject, riverHalfWidth - Random.Range(0.3f, 1.8f), baseZ + Random.Range(-5f, 5f), Random.Range(1, 5), Random.Range(0.75f, 1.45f));

            // 2. Gugusan Batu Tengah / Sisi Berlawanan dari Jalur Aman (membuat efek jeram berkelok)
            float midRockX = safeCenterX > 0f
                ? Random.Range(-riverHalfWidth + 2.5f, safeCenterX - safeChannelHalfWidth)
                : Random.Range(safeCenterX + safeChannelHalfWidth, riverHalfWidth - 2.5f);

            SpawnRockCluster(container, templateRock.gameObject, midRockX, baseZ, Random.Range(1, 4), Random.Range(0.65f, 1.35f));

            // 3. Kayu Gelondongan (Wood Logs) di sela-sela antar gugusan batu
            if (i % 2 == 1)
            {
                float logZ = baseZ + stepZ * 0.45f;
                bool isMovingLog = (i + segmentIndex) % 3 != 0;
                SpawnLog(container, templateLog.gameObject, safeCenterX, logZ, isMovingLog);
            }
        }
    }

    private void SpawnRockCluster(Transform parent, GameObject templateRock, float centerX, float centerZ, int count, float clusterScaleMultiplier)
    {
        for (int i = 0; i < count; i++)
        {
            Vector3 offset = i == 0
                ? Vector3.zero
                : new Vector3(
                    Random.Range(-2.4f, 2.4f),
                    0f,
                    Random.Range(-2.8f, 2.8f)
                );

            float posX = Mathf.Clamp(centerX + offset.x, -riverHalfWidth, riverHalfWidth);
            float posZ = centerZ + offset.z;

            // Kontras ukuran tinggi: boulder besar (2.4x-3.4x), batu sedang (1.1x-1.8x), batu kecil/ceper (0.45x-0.85x)
            float baseTier = i == 0
                ? Random.Range(1.8f, 2.8f)
                : (i == 1 ? Random.Range(0.95f, 1.55f) : Random.Range(0.42f, 0.82f));

            float scaleFactor = baseTier * clusterScaleMultiplier;
            Vector3 scale = new Vector3(
                scaleFactor * Random.Range(0.85f, 1.35f),
                scaleFactor * Random.Range(0.55f, 1.05f),
                scaleFactor * Random.Range(0.85f, 1.35f)
            );

            GameObject rock = Instantiate(templateRock, parent);
            rock.name = $"NaturalRock_{i}";
            rock.tag = "Obstacle";

            // Batu besar muncul lebih tinggi di atas air, batu kecil agak tenggelam
            float posY = Mathf.Lerp(-0.08f, 0.35f, Mathf.InverseLerp(0.4f, 3.2f, scaleFactor));
            rock.transform.localPosition = new Vector3(posX, posY, posZ);
            rock.transform.localRotation = Quaternion.Euler(
                Random.Range(-12f, 12f),
                Random.Range(0f, 360f),
                Random.Range(-12f, 12f)
            );
            rock.transform.localScale = scale;

            ReplaceVisualWithCustomPrefab(rock.transform, rockPrefabs, preserveOriginalProportions: true);
            rock.SetActive(true);
        }
    }

    private void SpawnLog(Transform parent, GameObject templateLog, float safeCenterX, float posZ, bool isMoving)
    {
        GameObject log = Instantiate(templateLog, parent);
        log.name = isMoving ? "NaturalLog_Moving" : "NaturalLog_Snag";
        log.tag = "Obstacle";

        float lengthScale = Random.Range(2.2f, 3.3f);
        float thickness = Random.Range(0.38f, 0.52f);
        log.transform.localScale = new Vector3(thickness, lengthScale, thickness);

        MovingObstacle mover = log.GetComponent<MovingObstacle>();

        if (isMoving && mover != null)
        {
            float startX = Random.Range(-5.5f, -1.5f);
            float travel = Random.Range(5f, 9f);
            float spd = Random.Range(1.4f, 2.6f);

            log.transform.localPosition = new Vector3(startX, 0.38f, posZ);
            // Sedikit miring alami terhadap arus sungai
            log.transform.localRotation = Quaternion.Euler(Random.Range(-15f, 15f), 0f, 90f);

            mover.Configure(Vector3.right, spd, travel);
        }
        else
        {
            // Kayu hanyut statis / tersangkut miring di tepi arus seperti foto referensi 2
            if (mover != null)
                Destroy(mover);

            float snagX = safeCenterX > 0f
                ? Random.Range(-riverHalfWidth + 2f, safeCenterX - safeChannelHalfWidth - 0.5f)
                : Random.Range(safeCenterX + safeChannelHalfWidth + 0.5f, riverHalfWidth - 2f);

            log.transform.localPosition = new Vector3(snagX, 0.32f, posZ);
            log.transform.localRotation = Quaternion.Euler(0f, Random.Range(-35f, 35f), 90f);
        }

        ReplaceVisualWithCustomPrefab(log.transform, logPrefabs);
        log.SetActive(true);
    }

    private void ReplaceVisualWithCustomPrefab(Transform target, GameObject[] customPrefabs, bool preserveOriginalProportions = false)
    {
        if (customPrefabs == null || customPrefabs.Length == 0)
            return;

        GameObject chosen = customPrefabs[Random.Range(0, customPrefabs.Length)];
        if (chosen == null)
            return;

        // Hapus visual custom lama jika ada dari template
        Transform oldVisual = target.Find("CustomAssetVisual");
        if (oldVisual != null)
            Destroy(oldVisual.gameObject);

        // Matikan MeshRenderer bawaan (sphere/cylinder) tapi biarkan Collider & Rigidbody utuh
        if (target.TryGetComponent(out MeshRenderer baseRenderer))
            baseRenderer.enabled = false;

        GameObject visual = Instantiate(chosen, target);
        visual.name = "CustomAssetVisual";
        visual.transform.localPosition = Vector3.zero;
        visual.transform.localRotation = Quaternion.identity;
        visual.transform.localScale = Vector3.one;

        // Matikan collider pada child visual agar tidak bentrok ganda dengan collider utama
        foreach (Collider col in visual.GetComponentsInChildren<Collider>(true))
            col.enabled = false;

        FitVisualToTargetCollider(target, visual.transform, preserveOriginalProportions);
    }

    private void FitVisualToTargetCollider(Transform target, Transform visual, bool preserveOriginalProportions)
    {
        MeshFilter mf = visual.GetComponentInChildren<MeshFilter>();
        if (mf == null || mf.sharedMesh == null)
            return;

        Bounds meshBounds = mf.sharedMesh.bounds;
        if (meshBounds.size.sqrMagnitude < 0.00001f)
            return;

        // Samakan ukuran visual dengan ukuran primitif asli (Sphere = 1x1x1, Cylinder Unity = 1x2x1)
        Vector3 targetLocalSize = Vector3.one;
        if (target.TryGetComponent(out BoxCollider box))
            targetLocalSize = box.size;
        else if (target.TryGetComponent(out SphereCollider sphere))
            targetLocalSize = Vector3.one * (sphere.radius * 2f);

        if (preserveOriginalProportions)
        {
            // Pertahankan bentuk asli tiap jenis batu (ceper, bulat, lonjong) tanpa dipaksa jadi kotak 1x1x1
            float maxMeshAxis = Mathf.Max(meshBounds.size.x, Mathf.Max(meshBounds.size.y, meshBounds.size.z));
            float uniformFit = targetLocalSize.x / Mathf.Max(0.001f, maxMeshAxis);
            visual.localScale = Vector3.one * uniformFit;
        }
        else
        {
            float scaleX = targetLocalSize.x / Mathf.Max(0.001f, meshBounds.size.x);
            float scaleY = targetLocalSize.y / Mathf.Max(0.001f, meshBounds.size.y);
            float scaleZ = targetLocalSize.z / Mathf.Max(0.001f, meshBounds.size.z);
            visual.localScale = new Vector3(scaleX, scaleY, scaleZ);
        }

        visual.localPosition = -Vector3.Scale(meshBounds.center, visual.localScale);
    }

    private void AutoLoadInstalledAssetsIfEmpty()
    {
#if UNITY_EDITOR
        if (rockPrefabs == null || rockPrefabs.Length == 0)
        {
            string[] stonePaths =
            {
                "Assets/Free/Stones/Mesh/Stone_1.fbx",
                "Assets/Free/Stones/Mesh/Stone_2.fbx",
                "Assets/Free/Stones/Mesh/Stone_3.fbx",
                "Assets/Free/Stones/Mesh/Stone_4.fbx",
                "Assets/Free/Stones/Mesh/Stone_5.fbx"
            };
            rockPrefabs = LoadAssetsAtPaths(stonePaths);
        }

        if (logPrefabs == null || logPrefabs.Length == 0)
        {
            string[] logPaths =
            {
                "Assets/AwesomeFreeScans/WoodLogs/Prefabs/LowEndPC/SM_AFS_Log03_LowEndPC.prefab",
                "Assets/AwesomeFreeScans/WoodLogs/Prefabs/LowEndPC/SM_AFS_Log04_LowEndPC.prefab",
                "Assets/AwesomeFreeScans/WoodLogs/Prefabs/LowEndPC/SM_AFS_Log05_LowEndPC.prefab",
                "Assets/AwesomeFreeScans/WoodLogs/Prefabs/LowEndPC/SM_AFS_Log06_LowEndPC.prefab",
                "Assets/AwesomeFreeScans/WoodLogs/Prefabs/LowEndPC/SM_AFS_Log22_LowEndPC.prefab",
                "Assets/AwesomeFreeScans/WoodLogs/Prefabs/LowEndPC/SM_AFS_Log25_LowEndPC.prefab"
            };
            logPrefabs = LoadAssetsAtPaths(logPaths);
        }
#endif
    }

#if UNITY_EDITOR
    private static GameObject[] LoadAssetsAtPaths(string[] paths)
    {
        var list = new System.Collections.Generic.List<GameObject>();
        foreach (string path in paths)
        {
            GameObject asset = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (asset != null)
                list.Add(asset);
        }
        return list.ToArray();
    }
#endif
}
