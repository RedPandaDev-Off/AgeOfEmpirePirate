using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

[ExecuteAlways]
[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class RuntimeGridMesh : MonoBehaviour
{
    [Header("Grid")]
    public float cellSize = 2f;
    public float halfSize = 50f;     // demi-taille (=> grille de -half..+half)
    public float y = 0.02f;

    [Header("Appearance")]
    public Color color = new(0f, 1f, 1f, 0.75f);
    public Material material;        // facultatif (URP/Unlit auto sinon)

    Mesh mesh;

    void OnEnable()
    {
        var mf = GetComponent<MeshFilter>();
        var mr = GetComponent<MeshRenderer>();

        if (material == null)
        {
            material = new Material(Shader.Find("Universal Render Pipeline/Unlit"));
        }
        if (material.HasProperty("_BaseColor"))
            material.SetColor("_BaseColor", color);
        mr.sharedMaterial = material;

        if (mesh == null)
        {
            mesh = new Mesh { name = "GridMesh" };
            mesh.hideFlags = HideFlags.DontSave;
        }

        // ⚠️ Assigner sharedMesh UNIQUEMENT ici (pas en OnValidate)
        mf.sharedMesh = mesh;

        RebuildGeometry();
    }

#if UNITY_EDITOR
    void OnValidate()
    {
        // Évite toute assignation de sharedMesh pendant OnValidate
        // On décale la reconstruction après la phase de validation
        if (!isActiveAndEnabled) return;

        EditorApplication.delayCall += () =>
        {
            if (this == null) return;

            var mr = GetComponent<MeshRenderer>();
            if (material == null)
                material = new Material(Shader.Find("Universal Render Pipeline/Unlit"));
            if (material.HasProperty("_BaseColor"))
                material.SetColor("_BaseColor", color);
            mr.sharedMaterial = material;

            if (mesh == null)
            {
                mesh = new Mesh { name = "GridMesh" };
                mesh.hideFlags = HideFlags.DontSave;

                // Assigner le mesh après la validation (sécurisé)
                var mf = GetComponent<MeshFilter>();
                if (mf) mf.sharedMesh = mesh;
            }

            RebuildGeometry();
            EditorUtility.SetDirty(this);
        };
    }
#endif

    void OnDisable()
    {
        // Nettoyage du mesh si nécessaire
        if (mesh != null)
        {
#if UNITY_EDITOR
            if (!Application.isPlaying) DestroyImmediate(mesh);
            else Destroy(mesh);
#else
            Destroy(mesh);
#endif
            mesh = null;
        }
    }

    void RebuildGeometry()
    {
        if (mesh == null) return;

        float s = Mathf.Max(0f, halfSize);
        float step = Mathf.Max(0.0001f, cellSize);

        var verts = new List<Vector3>();
        var indices = new List<int>();
        int idx = 0;

        for (float x = -s; x <= s + 0.0001f; x += step)
        {
            verts.Add(new Vector3(x, y, -s));
            verts.Add(new Vector3(x, y,  s));
            indices.Add(idx++); indices.Add(idx++);
        }
        for (float z = -s; z <= s + 0.0001f; z += step)
        {
            verts.Add(new Vector3(-s, y, z));
            verts.Add(new Vector3( s, y, z));
            indices.Add(idx++); indices.Add(idx++);
        }

        mesh.Clear();
        if (verts.Count > 65000) mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
        mesh.SetVertices(verts);
        mesh.SetIndices(indices, MeshTopology.Lines, 0);
        mesh.RecalculateBounds();
    }
}
