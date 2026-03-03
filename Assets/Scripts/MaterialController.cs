using DefaultNamespace;
using UnityEngine;

public class MaterialController : MonoBehaviour
{
    [Header("Tags (müssen zu euren GameObjects passen)")]
    [SerializeField] private string wallTag = "Wall";
    [SerializeField] private string floorTag = "Floor";
    [SerializeField] private string ballTag = "PlayerBall";

    [Header("Standard-Materialien (optional)")]
    [SerializeField] private Material defaultWallMaterial;
    [SerializeField] private Material defaultFloorMaterial;
    [SerializeField] private Material defaultBallMaterial;

    [Header("Floor Scale (optional)")]
    [SerializeField] private Vector3 floorScale = new Vector3(1f, 0.1f, 1f);


    [Header("Materialien")]
    [SerializeField] private Material stahl;
    [SerializeField] private Material holz;
    [SerializeField] private Material gummi;
    [SerializeField] private Material holzboden;


    public void HandleMatChange(CollisionMaterial ball, CollisionMaterial wall, CollisionMaterial ground)
    {
        switch (ball)
        {
            case CollisionMaterial.STAHL:SetBallMaterial(stahl);break;
            case CollisionMaterial.HOLZ: SetBallMaterial(holz); break;
            case CollisionMaterial.GUMMI: SetBallMaterial(gummi); break;
            default: break;
        }

        switch (wall)
        {
            case CollisionMaterial.STAHL:SetWallMaterial(stahl);break;
            case CollisionMaterial.HOLZ: SetWallMaterial(holz); break;
            case CollisionMaterial.GUMMI: SetWallMaterial(gummi); break;
            default: SetWallMaterial(holz); break;
        }

        switch (ground)
        {
            case CollisionMaterial.STAHL: SetFloorMaterial(stahl); break;
            case CollisionMaterial.HOLZ: SetFloorMaterial(holzboden); break;
            case CollisionMaterial.GUMMI: SetFloorMaterial(gummi); break;
            default: SetFloorMaterial(holz); break;
        }
    }

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
            if (r != null) r.sharedMaterial = wallMat;
        }
        Debug.Log($"Walls ({walls.Length}) mit Tag '{wallTag}' auf Material '{wallMat.name}' gesetzt.");
    }

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

    public void SetBallMaterial(Material ballMat)
    {
        if (ballMat == null)
        {
            Debug.LogWarning("SetBallMaterial: ballMat ist null.");
            return;
        }

        var balls = GameObject.FindWithTag(ballTag);
        var r = balls.GetComponent<Renderer>();
        if( r != null )r.sharedMaterial = ballMat;

        Debug.Log($"Ball auf Material '{ballMat.name}' gesetzt.");
    }

    [ContextMenu("Apply Materials")]
    public void SetMaterials(Material wallMat, Material floorMat)
    {
        if (wallMat != null) SetWallMaterial(wallMat);
        if (floorMat != null) SetFloorMaterial(floorMat);
    }

    [ContextMenu("Apply Default Materials")]
    public void ApplyDefaultMaterials()
    {
        SetMaterials(defaultWallMaterial, defaultFloorMaterial);
    }
}