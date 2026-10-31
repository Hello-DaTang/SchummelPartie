using HarmonyLib;
using MelonLoader;
using UnityEngine;
using Object = UnityEngine.Object;

namespace SchummelPartie.module;

public static class LocalFreeCamera
{
    private static Camera _source;
    private static Camera _camera;
    private static float _yaw;
    private static float _pitch;

    public static bool Active => _camera != null;

    public static void Toggle()
    {
        if (Active) Stop();
        else Start();
    }

    public static void Start()
    {
        if (Active || ModuleManager.IsSuspended) return;
        _source = FindSource();
        if (_source == null)
        {
            MelonLogger.Warning("[Free Camera] No active game camera found.");
            return;
        }

        var cameraObject = new GameObject("SchummelPartie.LocalFreeCamera") { hideFlags = HideFlags.DontSave };
        _camera = cameraObject.AddComponent<Camera>();
        _camera.CopyFrom(_source);
        _camera.transform.SetPositionAndRotation(_source.transform.position, _source.transform.rotation);
        // Keep the source and every gameplay component running. The untagged extra camera
        // renders after the source, while Camera.main still points at the game's camera.
        _camera.depth = _source.depth + 0.1f;
        if (_camera.clearFlags == CameraClearFlags.Depth || _camera.clearFlags == CameraClearFlags.Nothing)
            _camera.clearFlags = CameraClearFlags.Skybox;
        _camera.enabled = true;
        _yaw = _camera.transform.eulerAngles.y;
        _pitch = Mathf.DeltaAngle(0, _camera.transform.eulerAngles.x);
        MelonLogger.Msg("[Free Camera] Local view enabled; gameplay continues normally.");
    }

    public static void Stop()
    {
        if (_camera != null)
        {
            _camera.enabled = false;
            _camera.gameObject.SetActive(false);
            Object.Destroy(_camera.gameObject);
        }
        _camera = null;
        _source = null;
    }

    public static void OnUpdate()
    {
        if (ModuleManager.IsSuspended)
        {
            Stop();
            return;
        }
        if (Input.GetKeyDown(KeyCode.F11)) Toggle();
        if (!Active) return;
        if (_source == null || !_source.isActiveAndEnabled)
        {
            Stop();
            return;
        }

        if (Input.GetMouseButton(1))
        {
            _yaw += Input.GetAxisRaw("Mouse X") * 2f;
            _pitch = Mathf.Clamp(_pitch - Input.GetAxisRaw("Mouse Y") * 2f, -89f, 89f);
            _camera.transform.rotation = Quaternion.Euler(_pitch, _yaw, 0);
        }
        var movement = Vector3.zero;
        if (Input.GetKey(KeyCode.W)) movement += _camera.transform.forward;
        if (Input.GetKey(KeyCode.S)) movement -= _camera.transform.forward;
        if (Input.GetKey(KeyCode.D)) movement += _camera.transform.right;
        if (Input.GetKey(KeyCode.A)) movement -= _camera.transform.right;
        if (Input.GetKey(KeyCode.E)) movement += Vector3.up;
        if (Input.GetKey(KeyCode.Q)) movement -= Vector3.up;
        var speed = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift) ? 90f : 30f;
        _camera.transform.position += Vector3.ClampMagnitude(movement, 1f) * speed * Mathf.Min(Time.unscaledDeltaTime, 0.1f);
    }

    private static Camera FindSource()
    {
        if (IsGameCamera(Camera.main)) return Camera.main;
        if (GameManager.Minigame != null && IsGameCamera(GameManager.Minigame.MinigameCamera))
            return GameManager.Minigame.MinigameCamera;
        Camera result = null;
        foreach (var camera in Camera.allCameras)
            if (IsGameCamera(camera) && (result == null || camera.depth < result.depth))
                result = camera;
        return result;
    }

    private static bool IsGameCamera(Camera camera)
    {
        return camera != null && camera != _camera && camera.isActiveAndEnabled &&
               camera.cameraType == CameraType.Game && camera.targetTexture == null && camera.cullingMask != 0;
    }
}

// The stock F11 handler disables a serialized list of components. Our independent
// camera must never stop the host or minigame logic, even when Debug mode is on.
[HarmonyPatch(typeof(NoClipCam), "Update")]
public static class NoClipCamPatch
{
    [HarmonyPrefix]
    internal static bool Prefix() => false;
}
