using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class RiverFinishController : MonoBehaviour
{
    [Header("REFERENSI UTAMA")]
    [SerializeField] private Transform boat;
    [SerializeField] private BoatMovement boatMovement;
    [SerializeField] private Rigidbody boatRigidbody;
    [SerializeField] private BoatHealth boatHealth;
    [SerializeField] private RiverClarityUI claritySystem;
    [SerializeField] private EnvironmentLooper environmentLooper;

    [Header("LOKASI FINISH")]
    [SerializeField] private string finalSegmentName = "EnvironmentSegment_03";
    [SerializeField] private string riverObjectName = "River";
    [Tooltip("Jarak garis finish dari ujung River pada EnvironmentSegment_03.")]
    [SerializeField] private float finishOffsetFromRiverEnd = 6f;

    [Header("PROGRESS PERJALANAN")]
    [SerializeField] private Image journeyBarFill;
    [SerializeField] private TMP_Text journeyPercentText;
    [SerializeField] private TMP_Text journeyStatusText;

    [Header("BATAS BINTANG BERDASARKAN KEJERNIHAN SAAT FINISH")]
    [Range(1f, 100f)]
    [SerializeField] private float oneStarMinClarity = 40f;

    [Range(1f, 100f)]
    [SerializeField] private float twoStarMinClarity = 60f;

    [Range(1f, 100f)]
    [SerializeField] private float threeStarMinClarity = 80f;

    [Header("FINISH PANEL")]
    [SerializeField] private GameObject finishPanel;
    [SerializeField] private TMP_Text finishTitleText;
    [SerializeField] private TMP_Text finishDescriptionText;
    [SerializeField] private TMP_Text starResultText;

    [Header("GAME OVER PANEL")]
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private TMP_Text gameOverTitleText;
    [SerializeField] private TMP_Text gameOverDescriptionText;

    [Header("PENGATURAN AKHIR")]
    [SerializeField] private bool pauseOnResult = true;

    private float startZ;
    private float finishZ;
    private float furthestZ;

    private bool isInitialized;
    private bool gameFinished;

    public bool GameFinished => gameFinished;
    public float FinishZ => finishZ;

    private void Awake()
    {
        Time.timeScale = 1f;

        if (boatMovement == null)
            boatMovement = FindFirstObjectByType<BoatMovement>();

        if (boat == null && boatMovement != null)
            boat = boatMovement.transform;

        if (boat != null)
        {
            if (boatMovement == null)
                boatMovement = boat.GetComponent<BoatMovement>();

            if (boatRigidbody == null)
                boatRigidbody = boat.GetComponent<Rigidbody>();

            if (boatHealth == null)
                boatHealth = boat.GetComponent<BoatHealth>();
        }

        if (boatHealth == null)
            boatHealth = FindFirstObjectByType<BoatHealth>();

        if (claritySystem == null)
            claritySystem = FindFirstObjectByType<RiverClarityUI>();

        if (environmentLooper == null)
            environmentLooper = FindFirstObjectByType<EnvironmentLooper>();

        // Pastikan EnvironmentLooper tidak memindahkan ulang segment.
        // Ini dilakukan di Awake agar berlaku sebelum EnvironmentLooper.Start().
        if (environmentLooper != null)
        {
            environmentLooper.finiteCourseMode = true;
            environmentLooper.loopForward = false;
            environmentLooper.loopBackward = false;
        }

        if (journeyBarFill == null)
        {
            GameObject fillObject = GameObject.Find("PerjalananBarFill");

            if (fillObject != null)
                journeyBarFill = fillObject.GetComponent<Image>();
        }

        if (finishPanel != null)
            finishPanel.SetActive(false);

        if (gameOverPanel != null)
            gameOverPanel.SetActive(false);
    }

    private IEnumerator Start()
    {
        // Tunggu EnvironmentLooper menyusun Environment 01, 02, dan 03.
        yield return null;

        InitializeCourse();
    }

    private void InitializeCourse()
    {
        if (boat == null || boatMovement == null ||
            boatHealth == null || claritySystem == null ||
            environmentLooper == null)
        {
            Debug.LogError(
                "RiverFinishController: Referensi Boat, BoatMovement, " +
                "BoatHealth, RiverClarityUI, atau EnvironmentLooper belum ditemukan.",
                this
            );

            enabled = false;
            return;
        }

        environmentLooper.finiteCourseMode = true;
        environmentLooper.loopForward = false;
        environmentLooper.loopBackward = false;

        Transform finalSegment = FindFinalSegment();

        if (finalSegment == null)
        {
            Debug.LogError(
                "RiverFinishController: " + finalSegmentName +
                " tidak ditemukan pada EnvironmentLooper.",
                this
            );

            enabled = false;
            return;
        }

        Renderer finalRiverRenderer = FindRiverRenderer(finalSegment);

        if (finalRiverRenderer == null)
        {
            Debug.LogError(
                "RiverFinishController: Renderer River pada " +
                finalSegmentName + " tidak ditemukan.",
                this
            );

            enabled = false;
            return;
        }

        startZ = boat.position.z;
        furthestZ = startZ;

        // Finish ditempatkan di ujung River EnvironmentSegment_03.
        finishZ = finalRiverRenderer.bounds.max.z -
                  Mathf.Max(0f, finishOffsetFromRiverEnd);

        if (finishZ <= startZ)
        {
            Debug.LogError(
                "RiverFinishController: Posisi finish berada di belakang Boat. " +
                "Periksa posisi Boat dan EnvironmentSegment_03.",
                this
            );

            enabled = false;
            return;
        }

        if (journeyBarFill != null)
        {
            journeyBarFill.type = Image.Type.Filled;
            journeyBarFill.fillMethod = Image.FillMethod.Horizontal;
            journeyBarFill.fillOrigin = (int)Image.OriginHorizontal.Left;
            journeyBarFill.fillAmount = 0f;
        }

        if (journeyPercentText != null)
            journeyPercentText.text = "0%";

        if (journeyStatusText != null)
            journeyStatusText.text = "MENUJU FINISH - ENVIRONMENT 03";

        isInitialized = true;

        Debug.Log(
            "RIVER FINISH READY | Start Z: " + startZ +
            " | Finish Z: " + finishZ +
            " | Final Segment: " + finalSegment.name
        );
    }

    private Transform FindFinalSegment()
    {
        if (environmentLooper == null ||
            environmentLooper.segments == null)
            return null;

        foreach (Transform segment in environmentLooper.segments)
        {
            if (segment != null && segment.name == finalSegmentName)
                return segment;
        }

        // Fallback: gunakan segment terakhir pada array EnvironmentLooper.
        if (environmentLooper.segments.Length > 0)
            return environmentLooper.segments[
                environmentLooper.segments.Length - 1
            ];

        return null;
    }

    private Renderer FindRiverRenderer(Transform segment)
    {
        if (segment == null)
            return null;

        Transform[] allChildren =
            segment.GetComponentsInChildren<Transform>(true);

        foreach (Transform child in allChildren)
        {
            if (child.name != riverObjectName)
                continue;

            Renderer renderer = child.GetComponent<Renderer>();

            if (renderer == null)
                renderer = child.GetComponentInChildren<Renderer>(true);

            if (renderer != null)
                return renderer;
        }

        return null;
    }

    private void Update()
    {
        if (!isInitialized || gameFinished)
            return;

        // Kondisi kalah pertama: HP kapal habis.
        if (boatHealth.CurrentHealth <= 0)
        {
            ShowGameOver("HP kapal habis sebelum mencapai garis finish.");
            return;
        }

        float clarity = claritySystem.CurrentClarity;

        // Kondisi kalah kedua: kejernihan sungai mencapai 0%.
        if (clarity <= 0f)
        {
            ShowGameOver("Kejernihan sungai mencapai 0%. Sungai kembali tercemar.");
            return;
        }

        furthestZ = Mathf.Max(furthestZ, boat.position.z);

        float totalDistance = finishZ - startZ;
        float travelledDistance = furthestZ - startZ;

        float progress = Mathf.Clamp01(
            travelledDistance / totalDistance
        );

        UpdateJourneyUI(progress);

        // Pemain baru finish ketika Boat benar-benar mencapai ujung Environment 03.
        if (boat.position.z >= finishZ)
        {
            int stars = CalculateStars(clarity);
            ShowFinish(stars, clarity);
        }
    }

    private int CalculateStars(float clarity)
    {
        if (clarity >= threeStarMinClarity)
            return 3;

        if (clarity >= twoStarMinClarity)
            return 2;

        if (clarity >= oneStarMinClarity)
            return 1;

        return 0;
    }

    private void UpdateJourneyUI(float progress)
    {
        if (journeyBarFill != null)
            journeyBarFill.fillAmount = progress;

        if (journeyPercentText != null)
            journeyPercentText.text =
                Mathf.RoundToInt(progress * 100f) + "%";
    }

    private void ShowFinish(int stars, float finalClarity)
    {
        if (gameFinished)
            return;

        gameFinished = true;
        StopBoat();

        if (gameOverPanel != null)
            gameOverPanel.SetActive(false);

        if (finishPanel != null)
            finishPanel.SetActive(true);

        if (finishTitleText != null)
            finishTitleText.text = "LEVEL COMPLETED!";

        string description =
            "Kejernihan akhir sungai: " +
            Mathf.RoundToInt(finalClarity) +
            "%\nKamu berhasil mencapai garis finish Environment 03.";

        if (finishDescriptionText != null)
            finishDescriptionText.text = description;

        string starsVisual = BuildStarVisual(stars);

        if (starResultText != null)
        {
            starResultText.text =
                starsVisual + "\n" + stars + "/3 BINTANG";

            starResultText.color =
                new Color32(255, 196, 55, 255);
        }
        else if (finishDescriptionText != null)
        {
            finishDescriptionText.text =
                starsVisual + "\n" +
                stars + "/3 BINTANG\n\n" + description;
        }

        Debug.Log(
            "LEVEL COMPLETED | Clarity: " +
            finalClarity.ToString("0") +
            "% | Stars: " + stars + "/3"
        );

        if (pauseOnResult)
            Time.timeScale = 0f;
    }

    private string BuildStarVisual(int stars)
    {
        string result = "";

        for (int i = 0; i < 3; i++)
            result += i < stars ? "★" : "☆";

        return result;
    }

    public void NotifyBoatHealthDepleted()
    {
        ShowGameOver("HP kapal habis sebelum mencapai garis finish.");
    }

    private void ShowGameOver(string reason)
    {
        if (gameFinished)
            return;

        gameFinished = true;
        StopBoat();

        if (finishPanel != null)
            finishPanel.SetActive(false);

        if (gameOverPanel != null)
            gameOverPanel.SetActive(true);

        if (gameOverTitleText != null)
            gameOverTitleText.text = "GAME OVER";

        if (gameOverDescriptionText != null)
            gameOverDescriptionText.text =
                reason + "\nCoba lagi dan selesaikan perjalanan.";

        Debug.LogWarning("GAME OVER: " + reason);

        if (pauseOnResult)
            Time.timeScale = 0f;
    }

    private void StopBoat()
    {
        if (boatMovement != null)
            boatMovement.enabled = false;

        if (boat != null)
        {
            BoatTrashCollector collector =
                boat.GetComponent<BoatTrashCollector>();

            if (collector != null)
                collector.enabled = false;
        }

        if (boatRigidbody != null)
        {
#if UNITY_6000_0_OR_NEWER
            boatRigidbody.linearVelocity = Vector3.zero;
#else
            boatRigidbody.velocity = Vector3.zero;
#endif

            boatRigidbody.angularVelocity = Vector3.zero;
            boatRigidbody.isKinematic = true;
        }
    }

    public void RestartGame()
    {
        Time.timeScale = 1f;

        SceneManager.LoadScene(
            SceneManager.GetActiveScene().buildIndex
        );
    }

    private void OnDestroy()
    {
        Time.timeScale = 1f;
    }
}
