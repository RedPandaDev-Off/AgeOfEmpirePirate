using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Renderer))]
public class CubeMove : MonoBehaviour
{
    [SerializeField] Camera cam;
    [SerializeField] bool fitToCell = true;      // ajuste X/Z à la taille d'une case
    [SerializeField, Min(0f)] float extraY = 0.01f;

    // (optionnel) ton matériau perso
    [SerializeField] Material overrideMaterial;
    [SerializeField] bool useSharedMaterial = true;

    Renderer rend;

    void Awake(){
        if (!cam) cam = Camera.main;
        rend = GetComponent<Renderer>();

        if (overrideMaterial && rend){
            if (useSharedMaterial) rend.sharedMaterial = overrideMaterial;
            else rend.material = overrideMaterial;
        }

        if (fitToCell && GridSystem.Instance) FitToGrid();
    }

    void OnValidate(){
        if (fitToCell && GridSystem.Instance) FitToGrid();
    }

    void FitToGrid(){
        float c = GridSystem.Instance.cellSize;
        var s = transform.localScale;
        s.x = c; s.z = c;              // empreinte au sol = 1 cellule
        transform.localScale = s;
    }

    void Update(){
        if (Mouse.current?.leftButton.wasPressedThisFrame != true) return;

        Ray ray = cam.ScreenPointToRay(Mouse.current.position.ReadValue());
        if (Physics.Raycast(ray, out var hit, 2000f, ~0, QueryTriggerInteraction.Ignore)){
            // ➜ centre de cellule (plus possible d’être sur la ligne)
            Vector3 p = GridSystem.Instance
                ? GridSystem.Instance.SnapToCellCenter(hit.point)
                : hit.point;

            // pose le cube sur la surface
            float halfH = rend ? rend.bounds.extents.y : 0.5f;
            p.y = hit.point.y + halfH + extraY;

            transform.position = p;
            Debug.Log($"📦 Moved to cell center {p} (hit {hit.collider.name})");
        }
    }
}
