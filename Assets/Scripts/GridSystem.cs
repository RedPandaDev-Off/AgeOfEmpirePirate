using UnityEngine;

public class GridSystem : MonoBehaviour
{
    public static GridSystem Instance;

    [Header("Grille logique")]
    [Min(0.0001f)] public float cellSize = 2f;
    public Vector3 origin = Vector3.zero;     // coin bas-gauche logique (0,0)

    [Header("Sync avec la grille visuelle (facultatif)")]
    public Transform gridVisual;              // l'objet "Grid" qui affiche la grille (RuntimeGridMesh)
    public float gridVisualHalfSize = 50f;    // même valeur que halfSize du RuntimeGridMesh
    public bool syncOriginWithVisual = true;  // aligne origin automatiquement

    [Header("Gizmos (éditeur)")]
    public float halfSize = 50f;              // demi-taille dessinée
    public float gizmoY = 0.02f;              // anti z-fighting
    public Color gizmoColor = new(0f, 1f, 1f, 0.6f);

    void Awake() => Instance = this;

    void LateUpdate()
    {
        if (syncOriginWithVisual && gridVisual)
            origin = gridVisual.position - new Vector3(gridVisualHalfSize, 0f, gridVisualHalfSize);
    }

    // --- conversions ---
    public Vector2Int WorldToCell(Vector3 w)
    {
        Vector3 p = w - origin;
        int i = Mathf.FloorToInt(p.x / cellSize);
        int j = Mathf.FloorToInt(p.z / cellSize);
        return new Vector2Int(i, j);
    }

    public Vector3 CellCenterToWorld(int i, int j, float y = 0f)
    {
        return origin + new Vector3((i + 0.5f) * cellSize, y, (j + 0.5f) * cellSize);
    }

    // --- snaps ---
    public Vector3 SnapToCellCenter(Vector3 w)
    {
        var c = WorldToCell(w);
        return CellCenterToWorld(c.x, c.y, w.y);
    }
    public Vector3 SnapToGrid(Vector3 w) => SnapToCellCenter(w); // compat

    // --- utilitaires ---
    public Vector3 ClampToBounds(Vector3 pos)
    {
        float minX = origin.x;
        float maxX = origin.x + 2f * halfSize;
        float minZ = origin.z;
        float maxZ = origin.z + 2f * halfSize;
        pos.x = Mathf.Clamp(pos.x, minX, maxX);
        pos.z = Mathf.Clamp(pos.z, minZ, maxZ);
        return pos;
    }

    // --- gizmos ---
    void OnDrawGizmos()
    {
        Gizmos.color = gizmoColor;
        float s = halfSize, step = Mathf.Max(0.0001f, cellSize), y = gizmoY;
        Vector3 o = origin;

        for (float x = -s; x <= s + 0.0001f; x += step)
            Gizmos.DrawLine(o + new Vector3(x, y, -s), o + new Vector3(x, y,  s));

        for (float z = -s; z <= s + 0.0001f; z += step)
            Gizmos.DrawLine(o + new Vector3(-s, y, z), o + new Vector3( s, y, z));
    }
}
