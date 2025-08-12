using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Renderer))]
public class SnapTest : MonoBehaviour
{
    [SerializeField] Camera cam;
    [SerializeField] float yOffset = 0.05f;

    // 👉 glisse ton mat "testmat" ici dans l’Inspector
    [SerializeField] Material overrideMaterial;
    [SerializeField] bool useSharedMaterial = true; // true = pas d'instance runtime

    Renderer rend;

    void Awake(){
        if (!cam) cam = Camera.main;

        // applique ton matériau si fourni
        rend = GetComponent<Renderer>();
        if (overrideMaterial && rend){
            if (useSharedMaterial) rend.sharedMaterial = overrideMaterial;
            else rend.material = overrideMaterial; // crée une instance runtime
        }
    }

    void Update(){
        if (Mouse.current?.leftButton.wasPressedThisFrame != true) return;

        Ray ray = cam.ScreenPointToRay(Mouse.current.position.ReadValue());
        if (Physics.Raycast(ray, out var hit, 2000f, ~0, QueryTriggerInteraction.Ignore)){
            var p = GridSystem.Instance ? GridSystem.Instance.SnapToGrid(hit.point) : hit.point;
            p.y = hit.point.y + yOffset;
            transform.position = p;

            Debug.Log($"✅ Plane hit: {hit.collider.name} | pos {p}");
        }
    }
}
