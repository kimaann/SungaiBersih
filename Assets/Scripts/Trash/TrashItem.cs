using UnityEngine;

public enum TrashCategory
{
    Organik,
    Anorganik,
    B3
}

public class TrashItem : MonoBehaviour
{
    [Header("Informasi Sampah")]
    [SerializeField] private string trashDisplayName = "";

    [SerializeField] private TrashCategory category =
        TrashCategory.Organik;

    public string TrashDisplayName => trashDisplayName;
    public TrashCategory Category => category;
}
