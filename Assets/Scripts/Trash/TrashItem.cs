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
    [SerializeField] private string trashDisplayName = "Styrofoam";

    [SerializeField] private TrashCategory category =
        TrashCategory.Anorganik;

    public string TrashDisplayName => trashDisplayName;
    public TrashCategory Category => category;
}
