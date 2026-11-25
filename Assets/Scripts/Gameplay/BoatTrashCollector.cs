
using System;
using System.Collections;
using UnityEngine;

[RequireComponent(typeof(BoatMovement))]
[RequireComponent(typeof(Rigidbody))]
public class BoatTrashCollector : MonoBehaviour
{
    [Header("Durasi Klasifikasi")]
    [SerializeField] private float classificationDuration = 5f;

    private BoatMovement boatMovement;
    private Coroutine classificationRoutine;

    private bool isClassifying = false;

    public string CurrentTrashName { get; private set; }

    public TrashCategory CurrentTrashCategory { get; private set; }

    public bool IsClassifying => isClassifying;

    public float RemainingTime { get; private set; }

    public float ClassificationDuration => classificationDuration;

    // Event ketika sampah berhasil diambil
    public event Action<string, TrashCategory> OnTrashCollected;

    // Event ketika player menjawab
    public event Action<bool, TrashCategory, TrashCategory>
        OnClassificationAnswered;

    // Event ketika waktu habis
    public event Action<TrashCategory> OnClassificationTimeout;

    void Awake()
    {
        boatMovement = GetComponent<BoatMovement>();
    }

    
void OnTriggerEnter(Collider other)
{
    Debug.Log("TRIGGER TERDETEKSI: " + other.name);

    if (isClassifying)
    {
        Debug.Log("Klasifikasi masih berlangsung.");
        return;
    }

    TrashItem trashItem =
        other.GetComponentInParent<TrashItem>();

    if (trashItem == null)
    {
        Debug.LogWarning(
            "Collider terdeteksi, tetapi TrashItem tidak ditemukan: "
            + other.name
        );
        return;
    }

    CurrentTrashName = trashItem.TrashDisplayName;
    CurrentTrashCategory = trashItem.Category;

    isClassifying = true;

    boatMovement.SetSlowMotion(true);

    Destroy(trashItem.gameObject);

    Debug.Log(
        "Sampah diambil: " + CurrentTrashName +
        " | Kategori: " + CurrentTrashCategory
    );

    RemainingTime = classificationDuration;

    classificationRoutine =
        StartCoroutine(ClassificationTimer());

    OnTrashCollected?.Invoke(
        CurrentTrashName,
        CurrentTrashCategory
    );
}


    IEnumerator ClassificationTimer()
    {
        while (RemainingTime > 0f)
        {
            RemainingTime -= Time.unscaledDeltaTime;

            RemainingTime = Mathf.Max(0f, RemainingTime);

            yield return null;
        }

        classificationRoutine = null;

        if (!isClassifying) yield break;

        Debug.Log("Waktu klasifikasi habis!");

        // Beritahu sistem bahwa player tidak menjawab
        OnClassificationTimeout?.Invoke(
            CurrentTrashCategory
        );

        FinishClassification();
    }

    // Dipanggil ketika player memilih salah satu kategori
    public void SubmitAnswer(TrashCategory selectedCategory)
    {
        if (!isClassifying) return;

        bool isCorrect =
            selectedCategory == CurrentTrashCategory;

        if (isCorrect)
        {
            Debug.Log("JAWABAN BENAR! Kejernihan +10%");
        }
        else
        {
            Debug.Log("JAWABAN SALAH! Kejernihan -5%");
        }

        // Kirim hasil jawaban ke sistem lain
        OnClassificationAnswered?.Invoke(
            isCorrect,
            selectedCategory,
            CurrentTrashCategory
        );

        FinishClassification();
    }

    private void FinishClassification()
    {
        if (classificationRoutine != null)
        {
            StopCoroutine(classificationRoutine);
            classificationRoutine = null;
        }

        isClassifying = false;

        RemainingTime = 0f;

        // Kembalikan kecepatan kapal
        boatMovement.SetSlowMotion(false);

        CurrentTrashName = "";
    }
}
