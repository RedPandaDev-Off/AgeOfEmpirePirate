using UnityEngine;

[ExecuteAlways]
public class RuntimeGridOverlay : MonoBehaviour {
    public float cellSize = 2f;
    public float halfSize = 50f;
    public float y = 0.02f;
    public Color lineColor = new(0f,1f,1f,0.7f);

    Material mat;

    void OnEnable(){
        if (mat == null){
            mat = new Material(Shader.Find("Universal Render Pipeline/Unlit"));
            mat.SetColor("_BaseColor", lineColor);
        }
    }

    void OnRenderObject(){
        if (mat == null) return;
        mat.SetPass(0);
        GL.PushMatrix();
        GL.MultMatrix(transform.localToWorldMatrix);
        GL.Begin(GL.LINES);
        GL.Color(lineColor);

        float s = halfSize;
        for (float x = -s; x <= s; x += cellSize){ GL.Vertex3(x, y, -s); GL.Vertex3(x, y, s); }
        for (float z = -s; z <= s; z += cellSize){ GL.Vertex3(-s, y, z); GL.Vertex3(s, y, z); }

        GL.End();
        GL.PopMatrix();
    }
}
