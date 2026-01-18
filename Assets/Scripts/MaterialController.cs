
using UnityEngine;

public class MaterialController : MonoBehaviour
{
    [Header("Tags (müssen zu euren GameObjects passen)")]
    [SerializeField] private string wallTag = "Wall";
    [SerializeField] private string floorTag = "Floor";

    [Header("Standard-Materialien (optional)")]
    [SerializeField] private Material defaultWallMaterial;
    [SerializeField] private Material defaultFloorMaterial;

    [Header("Floor Scale (optional)")]
    [SerializeField] private Vector3 floorScale = new Vector3(1f, 0.1f, 1f);

    // ============================
    // Öffentliche API-Methoden
    // ============================

    /// <summary>
    /// Setzt das Material für alle Wände mit dem angegebenen Tag.
    /// </summary>
    public void SetWallMaterial(Material wallMat)
    {
        if (wallMat == null)
        {
            Debug.LogWarning("SetWallMaterial: wallMat ist null.");
            return;
        }

        var walls = GameObject.FindGameObjectsWithTag(wallTag);
        foreach (var w in walls)
        {
            var r = w.GetComponent<Renderer>();
            if (r != null) r.sharedMaterial = wallMat; // sharedMaterial => keine Kopie
        }
        Debug.Log($"Walls ({walls.Length}) mit Tag '{wallTag}' auf Material '{wallMat.name}' gesetzt.");
    }

    /// <summary>
    /// Setzt das Material für alle Floors mit dem angegebenen Tag.
    /// </summary>
    public void SetFloorMaterial(Material floorMat)
    {
        if (floorMat == null)
        {
            Debug.LogWarning("SetFloorMaterial: floorMat ist null.");
            return;
        }

        var floors = GameObject.FindGameObjectsWithTag(floorTag);
        foreach (var f in floors)
        {
            var r = f.GetComponent<Renderer>();
            if (r != null) r.sharedMaterial = floorMat;
        }
        Debug.Log($"Floors ({floors.Length}) mit Tag '{floorTag}' auf Material '{floorMat.name}' gesetzt.");
    }

    /// <summary>
    /// Setzt Material für Walls und Floors in einem Schritt.
    /// Übergib null, um einen Typ unverändert zu lassen.
    /// </summary>
    [ContextMenu("Apply Materials")]
    public void SetMaterials(Material wallMat, Material floorMat)
    {
        if (wallMat != null) SetWallMaterial(wallMat);
        if (floorMat != null) SetFloorMaterial(floorMat);
    }

    // ============================
    // 
    // ============================

    [ContextMenu("Apply Default Materials")]
    public void ApplyDefaultMaterials()
    {
        SetMaterials(defaultWallMaterial, defaultFloorMaterial);
    }
}
