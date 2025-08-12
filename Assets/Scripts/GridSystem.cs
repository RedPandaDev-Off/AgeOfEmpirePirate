using UnityEngine;

public class GridSystem : MonoBehaviour
{
    public static GridSystem Instance;
    public float cellSize = 2f;

    void Awake()
    {
        Instance = this;
    }

    public Vector3 SnapToGrid(Vector3 worldPosition)
    {
        float x = Mathf.Round(worldPosition.x / cellSize) * cellSize;
        float z = Mathf.Round(worldPosition.z / cellSize) * cellSize;
        return new Vector3(x, worldPosition.y, z);
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.gray;
        for (float x = -100f; x <= 100f; x += cellSize)
            Gizmos.DrawLine(new Vector3(x, 0f, -100f), new Vector3(x, 0f, 100f));

        for (float z = -100f; z <= 100f; z += cellSize)
            Gizmos.DrawLine(new Vector3(-100f, 0f, z), new Vector3(100f, 0f, z));
    }
}
