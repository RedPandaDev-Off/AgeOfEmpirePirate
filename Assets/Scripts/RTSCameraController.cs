using UnityEngine;
using UnityEngine.InputSystem;

public class RTSCameraController : MonoBehaviour
{
    [Header("Quality of life")]
    public bool smartZoom = true;
    public bool doubleClickCenter = true;
    public float doubleClickWindow = 0.25f;
    float lastClickTime;

    [Header("Height")]
    public float rigBaseHeight = 15f;
    public bool lockBaseHeight = true;

    [Header("Rig refs")]
    public Transform pitchPivot;   // enfant de RTS_Rig
    public Camera cam;             // Main Camera (enfant de pitchPivot)

    [Header("Framing")]
    [Range(20,85)] public float pitch = 55f;
    public float startDistance = 22f;
    public float minDistance = 8f;
    public float maxDistance = 60f;
    public float zoomSpeed = 4f;
    public float zoomSmoothing = 12f;

    [Header("Pan")]
    public float panSpeed = 20f;
    public float fastMultiplier = 2f;
    public bool edgePan = true;
    public int edgeThickness = 12;

    [Header("Rotate")]
    public bool allowRotate = true;
    public float rotateSpeed = 90f;
    public float mouseRotateSensitivity = 0.2f;

    [Header("Bounds")]
    public bool clamp = true;
    public float minX = -100, maxX = 100, minZ = -100, maxZ = 100;

    float targetDistance, currentDistance;

    void Awake()
    {
        if (!cam) cam = Camera.main;
        if (!pitchPivot)
            Debug.LogError("[RTSCamera] Assigne PitchPivot (enfant du rig).");
        if (cam && cam.transform.parent != pitchPivot)
            Debug.LogWarning("[RTSCamera] La Camera devrait être enfant de PitchPivot.");

        SetPitch(pitch);
        targetDistance = currentDistance = Mathf.Clamp(startDistance, minDistance, maxDistance);
        UpdateCameraLocal(true);

        if (lockBaseHeight) {
            var p = transform.position; p.y = rigBaseHeight; transform.position = p;
        }
    }

    void Update()
    {
        if (Mouse.current == null || Keyboard.current == null || cam == null || pitchPivot == null)
            return;

        float dt = Time.unscaledDeltaTime;

        // --- double-clic pour centrer ---
        if (doubleClickCenter && Mouse.current.leftButton.wasPressedThisFrame) {
            if (Time.unscaledTime - lastClickTime <= doubleClickWindow) CenterToMouse();
            lastClickTime = Time.unscaledTime;
        }

        // --- PAN (ZQSD + edge) ---
        Vector2 move = Vector2.zero;
        if (Keyboard.current.wKey.isPressed || Keyboard.current.zKey.isPressed) move.y += 1; // AZERTY: Z
        if (Keyboard.current.sKey.isPressed) move.y -= 1;
        if (Keyboard.current.aKey.isPressed || Keyboard.current.qKey.isPressed) move.x -= 1; // AZERTY: Q
        if (Keyboard.current.dKey.isPressed) move.x += 1;

        if (edgePan && Application.isFocused) {
            Vector2 mp = Mouse.current.position.ReadValue();
            if (mp.x <= edgeThickness) move.x -= 1;
            else if (mp.x >= Screen.width - edgeThickness) move.x += 1;
            if (mp.y <= edgeThickness) move.y -= 1;
            else if (mp.y >= Screen.height - edgeThickness) move.y += 1;
        }

        move = move.normalized;
        float spd = panSpeed * (Keyboard.current.leftShiftKey.isPressed ? fastMultiplier : 1f);

        Vector3 fwd = new Vector3(transform.forward.x, 0f, transform.forward.z).normalized;
        Vector3 right = new Vector3(transform.right.x, 0f, transform.right.z).normalized;
        transform.position += (right * move.x + fwd * move.y) * spd * dt;

        // --- ROTATION (Q/E + drag RMB) ---
        if (allowRotate) {
            if (Keyboard.current.qKey.isPressed) transform.Rotate(0f, -rotateSpeed * dt, 0f);
            if (Keyboard.current.eKey.isPressed) transform.Rotate(0f,  rotateSpeed * dt, 0f);

            if (Mouse.current.rightButton.isPressed) {
                Vector2 d = Mouse.current.delta.ReadValue();
                transform.Rotate(0f, d.x * mouseRotateSensitivity, 0f);

                float newPitch = Mathf.Clamp(pitch - d.y * mouseRotateSensitivity, 20f, 85f);
                if (Mathf.Abs(newPitch - pitch) > 0.01f) { pitch = newPitch; SetPitch(pitch); }
            }
        }

        // --- ZOOM (molette) ---
        float scroll = Mouse.current.scroll.ReadValue().y;
        if (Mathf.Abs(scroll) > 0.01f)
            targetDistance = Mathf.Clamp(targetDistance - scroll * zoomSpeed * 0.1f, minDistance, maxDistance);

        // lissage
        currentDistance = Mathf.Lerp(currentDistance, targetDistance, 1f - Mathf.Exp(-zoomSmoothing * dt));
        UpdateCameraLocal(false);

        // smart zoom vers la souris
        if (smartZoom && Mathf.Abs(scroll) > 0.01f) AnchorZoomToMouse();

        // --- CLAMP ---
        if (clamp) {
            Vector3 p = transform.position;
            p.x = Mathf.Clamp(p.x, minX, maxX);
            p.z = Mathf.Clamp(p.z, minZ, maxZ);
            transform.position = p;
        }

        // --- LOCK HAUTEUR (toujours en dernier) ---
        if (lockBaseHeight) {
            var p = transform.position; p.y = rigBaseHeight; transform.position = p;
        }
    }

    // helpers
    void SetPitch(float deg) {
        Vector3 e = pitchPivot.localEulerAngles; e.x = deg; pitchPivot.localEulerAngles = e;
    }

    void UpdateCameraLocal(bool snap) {
        var t = cam.transform;
        t.localPosition = new Vector3(0f, 0f, -currentDistance);
        t.localRotation = Quaternion.identity;
    }

    void CenterToMouse() {
        if (TryRaycastMouse(out var hit)) {
            Vector3 p = transform.position;
            p.x = clamp ? Mathf.Clamp(hit.point.x, minX, maxX) : hit.point.x;
            p.z = clamp ? Mathf.Clamp(hit.point.z, minZ, maxZ) : hit.point.z;
            transform.position = p;
        }
    }

    void AnchorZoomToMouse() {
        if (!TryRaycastMouse(out var h1)) return;
        Ray r2 = cam.ScreenPointToRay(Mouse.current.position.ReadValue());
        if (!Physics.Raycast(r2, out var h2, 5000f, ~0, QueryTriggerInteraction.Ignore)) return;
        Vector3 delta = h1.point - h2.point;
        transform.position += new Vector3(delta.x, 0f, delta.z);
    }

    bool TryRaycastMouse(out RaycastHit hit) {
        Ray ray = cam.ScreenPointToRay(Mouse.current.position.ReadValue());
        return Physics.Raycast(ray, out hit, 5000f, ~0, QueryTriggerInteraction.Ignore);
    }
}
