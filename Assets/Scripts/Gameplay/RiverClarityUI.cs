using System;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class RiverClarityUI : MonoBehaviour
{
    [Header("Referensi UI")]
    [SerializeField] private BoatTrashCollector trashCollector;
    [SerializeField] private TMP_Text clarityPercentText;
    [SerializeField] private Image clarityBarFill;

    [Header("Referensi Air")]
    [Tooltip("Masukkan Mesh Renderer dari River pada setiap EnvironmentSegment.")]
    [SerializeField] private Renderer[] waterRenderers;

    [Header("Auto Setup Environment")]
    [SerializeField] private string segmentNamePrefix = "EnvironmentSegment_";
    [SerializeField] private string riverObjectName = "River";

    [Tooltip("Nama property warna pada Shader Graph. Script juga mencoba versi dengan awalan underscore.")]
    [SerializeField] private string deepColorProperty = "_DeepColor";
    [SerializeField] private string shallowColorProperty = "_ShallowColor";

    [Header("Warna Air - RGB 0 sampai 255")]
    [SerializeField] private Color muddyDeepColor = new Color32(18, 25, 20, 255);
    [SerializeField] private Color muddyShallowColor = new Color32(43, 55, 35, 255);
    [SerializeField] private Color clearDeepColor = new Color32(34, 112, 128, 255);
    [SerializeField] private Color clearShallowColor = new Color32(158, 222, 216, 255);

    [Header("Warna Horizon Air")]
    [SerializeField] private string horizonColorProperty = "_HorizonColor";
    [SerializeField] private Color muddyHorizonColor = new Color32(30, 42, 34, 255);
    [SerializeField] private Color clearHorizonColor = new Color32(145, 210, 204, 255);
    
    [Header("Pengaturan Kejernihan")]
    [SerializeField, Range(0f, 100f)] private float startingClarity = 50f;
    [SerializeField] private float correctIncrease = 10f;
    [SerializeField] private float wrongDecrease = 5f;

    [Header("Akhir Permainan")]
    [SerializeField] private Transform boat;
    [SerializeField] private BoatMovement boatMovement;
    [SerializeField] private Rigidbody boatRigidbody;
    [SerializeField] private GameObject gameEndPanel;
    [SerializeField] private TMP_Text gameEndText;
    [SerializeField] private bool pauseGameOnEnd = true;

    public float CurrentClarity { get; private set; }

    private readonly List<Material> runtimeWaterMaterials = new List<Material>();
    private bool gameEnded;
    private bool propertyWarningShown;

    private void Awake()
    {
        Time.timeScale = 1f;

        if (gameEndPanel != null)
            gameEndPanel.SetActive(false);

        if (boat != null)
        {
            if (boatMovement == null)
                boatMovement = boat.GetComponent<BoatMovement>();

            if (boatRigidbody == null)
                boatRigidbody = boat.GetComponent<Rigidbody>();
        }

        CurrentClarity = Mathf.Clamp(startingClarity, 0f, 100f);

        // Deteksi River secara otomatis.
        AutoFindWaterRenderers();

        // Siapkan material untuk perubahan warna.
        SetupWaterMaterials();

        UpdateUI();
        CheckGameEnd();
    }

    private void OnEnable()
    {
        if (trashCollector != null)
            trashCollector.OnClassificationAnswered += HandleClassificationAnswered;
    }

    private void OnDisable()
    {
        if (trashCollector != null)
            trashCollector.OnClassificationAnswered -= HandleClassificationAnswered;
    }

    private void AutoFindWaterRenderers()
    {
        List<Renderer> foundRenderers = new List<Renderer>();

        GameObject[] roots =
            SceneManager.GetActiveScene().GetRootGameObjects();

        foreach (GameObject root in roots)
        {
            Transform[] allObjects =
                root.GetComponentsInChildren<Transform>(true);

            foreach (Transform segment in allObjects)
            {
                if (!segment.name.StartsWith(
                    segmentNamePrefix,
                    StringComparison.OrdinalIgnoreCase))
                    continue;

                Transform river = FindRiverChild(segment);

                if (river == null)
                {
                    Debug.LogWarning(
                        "River tidak ditemukan pada " + segment.name
                    );
                    continue;
                }

                Renderer riverRenderer =
                    river.GetComponent<Renderer>();

                if (riverRenderer == null)
                {
                    riverRenderer =
                        river.GetComponentInChildren<Renderer>(true);
                }

                if (riverRenderer == null)
                {
                    Debug.LogWarning(
                        "Mesh Renderer tidak ditemukan pada " + segment.name
                    );
                    continue;
                }

                if (!foundRenderers.Contains(riverRenderer))
                {
                    foundRenderers.Add(riverRenderer);

                    Debug.Log(
                        "River berhasil ditemukan: " + segment.name
                    );
                }
            }
        }

        if (foundRenderers.Count > 0)
        {
            waterRenderers = foundRenderers.ToArray();

            Debug.Log(
                "AUTO SETUP BERHASIL: " +
                waterRenderers.Length + " River terhubung."
            );
        }
        else
        {
            Debug.LogError(
                "AUTO SETUP GAGAL: Tidak ada River yang ditemukan."
            );
        }
    }

    private Transform FindRiverChild(Transform parent)
    {
        foreach (Transform child in parent)
        {
            if (string.Equals(
                child.name,
                riverObjectName,
                StringComparison.OrdinalIgnoreCase))
            {
                return child;
            }

            Transform result = FindRiverChild(child);

            if (result != null)
                return result;
        }

        return null;
    }

    private void SetupWaterMaterials()
    {
        runtimeWaterMaterials.Clear();

        if (waterRenderers == null || waterRenderers.Length == 0)
        {
            Debug.LogWarning("RiverClarityUI: Water Renderers belum diisi.", this);
            return;
        }

        foreach (Renderer waterRenderer in waterRenderers)
        {
            if (waterRenderer == null || waterRenderer.sharedMaterial == null)
                continue;

            // Buat material runtime agar asset material asli tidak berubah.
            Material runtimeMaterial = new Material(waterRenderer.sharedMaterial);
            runtimeMaterial.name = waterRenderer.sharedMaterial.name + " (Runtime Clarity)";
            waterRenderer.sharedMaterial = runtimeMaterial;
            runtimeWaterMaterials.Add(runtimeMaterial);
        }

        if (runtimeWaterMaterials.Count == 0)
            Debug.LogWarning("RiverClarityUI: Tidak menemukan material pada Water Renderers.", this);
    }

    private void HandleClassificationAnswered(
        bool isCorrect,
        TrashCategory selectedCategory,
        TrashCategory correctCategory)
    {
        if (gameEnded)
            return;

        if (isCorrect)
        {
            CurrentClarity += correctIncrease;
            Debug.Log("Jawaban benar! Kejernihan sungai meningkat.");
        }
        else
        {
            CurrentClarity -= wrongDecrease;
            Debug.Log("Jawaban salah! Kejernihan sungai menurun.");
        }

        CurrentClarity = Mathf.Clamp(CurrentClarity, 0f, 100f);
        UpdateUI();
        CheckGameEnd();
    }

    private void UpdateUI()
    {
        if (clarityPercentText != null)
            clarityPercentText.text = Mathf.RoundToInt(CurrentClarity) + "%";

        if (clarityBarFill != null)
            clarityBarFill.fillAmount = CurrentClarity / 100f;

        UpdateWaterColors();
    }

    private void UpdateWaterColors()
    {
        if (runtimeWaterMaterials.Count == 0)
            return;

        // Persentase kejernihan asli.
        float rawClarity =
            Mathf.Clamp01(CurrentClarity / 100f);

        // Kurva non-linear.
        // Membuat air tetap gelap pada 50%.
        // Pada 100%, warna mencapai kondisi jernih sepenuhnya.
        float clarity01 = Mathf.Pow(rawClarity, 2.2f);

        // Hitung warna berdasarkan kejernihan.
        Color currentDeepColor = Color.Lerp(
            muddyDeepColor,
            clearDeepColor,
            clarity01
        );

        Color currentShallowColor = Color.Lerp(
            muddyShallowColor,
            clearShallowColor,
            clarity01
        );

        Color currentHorizonColor = Color.Lerp(
            muddyHorizonColor,
            clearHorizonColor,
            clarity01
        );

        bool allDeepPropertiesFound = true;
        bool allShallowPropertiesFound = true;
        bool allHorizonPropertiesFound = true;

        foreach (Material waterMaterial in runtimeWaterMaterials)
        {
            if (waterMaterial == null)
                continue;

            allDeepPropertiesFound &= TrySetColor(
                waterMaterial,
                deepColorProperty,
                currentDeepColor
            );

            allShallowPropertiesFound &= TrySetColor(
                waterMaterial,
                shallowColorProperty,
                currentShallowColor
            );

            allHorizonPropertiesFound &= TrySetColor(
                waterMaterial,
                horizonColorProperty,
                currentHorizonColor
            );
        }

        if ((!allDeepPropertiesFound ||
            !allShallowPropertiesFound ||
            !allHorizonPropertiesFound) &&
            !propertyWarningShown)
        {
            Debug.LogWarning(
                "RiverClarityUI: Ada property warna yang tidak ditemukan. " +
                "Periksa DeepColor, ShallowColor, dan HorizonColor.",
                this
            );

            propertyWarningShown = true;
        }
    }


    private bool TrySetColor(Material material, string propertyName, Color color)
    {
        if (material == null || string.IsNullOrWhiteSpace(propertyName))
            return false;

        string property = propertyName.Trim();

        if (material.HasProperty(property))
        {
            material.SetColor(property, color);
            return true;
        }

        // Coba nama property dengan/tanpa awalan underscore.
        string alternative = property.StartsWith("_")
            ? property.Substring(1)
            : "_" + property;

        if (material.HasProperty(alternative))
        {
            material.SetColor(alternative, color);
            return true;
        }

        return false;
    }

    private void CheckGameEnd()
    {
        // Kondisi menang dan kalah sekarang
        // ditangani oleh RiverFinishController.
        // Mencapai 100% bukan berarti permainan selesai.
    }

    private void EndGame(bool playerWon)
    {
        if (gameEnded)
            return;

        gameEnded = true;

        if (boatMovement != null)
            boatMovement.enabled = false;

        if (boatRigidbody != null)
        {
            boatRigidbody.linearVelocity = Vector3.zero;
            boatRigidbody.angularVelocity = Vector3.zero;
            boatRigidbody.isKinematic = true;
        }

        if (gameEndPanel != null)
            gameEndPanel.SetActive(true);

        if (gameEndText != null)
        {
            gameEndText.text = playerWon
                ? "MISI SELESAI!\nSungai berhasil dibersihkan!"
                : "GAME OVER\nSungai terlalu tercemar!";
        }

        Debug.Log(playerWon
            ? "GAME COMPLETED: Kejernihan sungai mencapai 100%."
            : "GAME OVER: Kejernihan sungai mencapai 0%.");

        if (pauseGameOnEnd)
            Time.timeScale = 0f;
    }

    private void OnDestroy()
    {
        foreach (Material runtimeMaterial in runtimeWaterMaterials)
        {
            if (runtimeMaterial != null)
                Destroy(runtimeMaterial);
        }

        if (gameEnded && pauseGameOnEnd)
            Time.timeScale = 1f;
    }
}
