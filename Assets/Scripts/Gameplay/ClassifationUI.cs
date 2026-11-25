
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ClassificationUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject classificationPanel;
    [SerializeField] private TMP_Text trashNameText;
    [SerializeField] private Image timerBarFill;

    [Header("Classification Buttons")]
    [SerializeField] private Button btnOrganik;
    [SerializeField] private Button btnAnorganik;
    [SerializeField] private Button btnB3;

    [Header("Game References")]
    [SerializeField] private BoatTrashCollector boatTrashCollector;

    void Awake()
    {
        if (classificationPanel != null)
            classificationPanel.SetActive(false);

        if (timerBarFill != null)
            timerBarFill.fillAmount = 1f;
    }

    void OnEnable()
    {
        if (boatTrashCollector != null)
        {
            boatTrashCollector.OnTrashCollected += ShowPanel;
            boatTrashCollector.OnClassificationAnswered += HidePanel;
            boatTrashCollector.OnClassificationTimeout += HidePanel;
        }

        if (btnOrganik != null)
            btnOrganik.onClick.AddListener(ChooseOrganik);

        if (btnAnorganik != null)
            btnAnorganik.onClick.AddListener(ChooseAnorganik);

        if (btnB3 != null)
            btnB3.onClick.AddListener(ChooseB3);
    }

    void OnDisable()
    {
        if (boatTrashCollector != null)
        {
            boatTrashCollector.OnTrashCollected -= ShowPanel;
            boatTrashCollector.OnClassificationAnswered -= HidePanel;
            boatTrashCollector.OnClassificationTimeout -= HidePanel;
        }

        if (btnOrganik != null)
            btnOrganik.onClick.RemoveListener(ChooseOrganik);

        if (btnAnorganik != null)
            btnAnorganik.onClick.RemoveListener(ChooseAnorganik);

        if (btnB3 != null)
            btnB3.onClick.RemoveListener(ChooseB3);
    }

    void Update()
    {
        if (boatTrashCollector == null ||
            !boatTrashCollector.IsClassifying)
            return;

        // Update timer bar
        float progress =
            boatTrashCollector.RemainingTime /
            boatTrashCollector.ClassificationDuration;

        timerBarFill.fillAmount = Mathf.Clamp01(progress);

        // Keyboard input
        if (Input.GetKeyDown(KeyCode.Alpha1) ||
            Input.GetKeyDown(KeyCode.Keypad1))
        {
            ChooseOrganik();
        }

        if (Input.GetKeyDown(KeyCode.Alpha2) ||
            Input.GetKeyDown(KeyCode.Keypad2))
        {
            ChooseAnorganik();
        }

        if (Input.GetKeyDown(KeyCode.Alpha3) ||
            Input.GetKeyDown(KeyCode.Keypad3))
        {
            ChooseB3();
        }
    }

    void ShowPanel(string trashName, TrashCategory category)
    {
        trashNameText.text = trashName;
        timerBarFill.fillAmount = 1f;
        classificationPanel.SetActive(true);
    }

    void HidePanel(TrashCategory category)
    {
        classificationPanel.SetActive(false);
    }

    void HidePanel(
        bool isCorrect,
        TrashCategory selectedCategory,
        TrashCategory correctCategory)
    {
        classificationPanel.SetActive(false);
    }

    // Button Organik
    public void ChooseOrganik()
    {
        SubmitAnswer(TrashCategory.Organik);
    }

    // Button Anorganik
    public void ChooseAnorganik()
    {
        SubmitAnswer(TrashCategory.Anorganik);
    }

    // Button B3
    public void ChooseB3()
    {
        SubmitAnswer(TrashCategory.B3);
    }

    void SubmitAnswer(TrashCategory category)
    {
        if (boatTrashCollector == null ||
            !boatTrashCollector.IsClassifying)
            return;

        boatTrashCollector.SubmitAnswer(category);
    }
}
