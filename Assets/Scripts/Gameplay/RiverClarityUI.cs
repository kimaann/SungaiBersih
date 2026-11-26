
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class RiverClarityUI : MonoBehaviour
{
    [Header("Referensi")]
    [SerializeField] private BoatTrashCollector trashCollector;
    [SerializeField] private TMP_Text clarityPercentText;
    [SerializeField] private Image clarityBarFill;

    [Header("Pengaturan Kejernihan")]
    [SerializeField] private float startingClarity = 50f;
    [SerializeField] private float correctIncrease = 10f;
    [SerializeField] private float wrongDecrease = 5f;

    public float CurrentClarity { get; private set; }

    private void Awake()
    {
        CurrentClarity = Mathf.Clamp(
            startingClarity, 0f, 100f
        );

        UpdateUI();
    }

    private void OnEnable()
    {
        if (trashCollector != null)
        {
            trashCollector.OnClassificationAnswered
                += HandleClassificationAnswered;
        }
    }

    private void OnDisable()
    {
        if (trashCollector != null)
        {
            trashCollector.OnClassificationAnswered
                -= HandleClassificationAnswered;
        }
    }

    private void HandleClassificationAnswered(
        bool isCorrect,
        TrashCategory selectedCategory,
        TrashCategory correctCategory)
    {
        if (isCorrect)
        {
            CurrentClarity += correctIncrease;
            Debug.Log("Jawaban benar! Kejernihan +10%");
        }
        else
        {
            CurrentClarity -= wrongDecrease;
            Debug.Log("Jawaban salah! Kejernihan -5%");
        }

        CurrentClarity = Mathf.Clamp(
            CurrentClarity, 0f, 100f
        );

        UpdateUI();
    }

    private void UpdateUI()
    {
        if (clarityPercentText != null)
        {
            clarityPercentText.text =
                Mathf.RoundToInt(CurrentClarity) + "%";
        }

        if (clarityBarFill != null)
        {
            clarityBarFill.fillAmount =
                CurrentClarity / 100f;
        }
    }
}
