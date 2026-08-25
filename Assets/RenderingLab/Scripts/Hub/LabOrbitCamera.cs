using UnityEngine;
using UnityEngine.Rendering;

namespace RenderingLab
{
    /// <summary>
    /// Orbit camera for the lab. RMB drag / WASD optional. Keeps the character in frame.
    /// </summary>
    public class LabOrbitCamera : MonoBehaviour
    {
        public Transform target;
        public Vector3 offset = new Vector3(0, 1.4f, 0);
        public float distance = 4.4f;
        public float yaw = -8f;
        public float pitch = 8f;
        public float sensitivity = 140f;
        public float zoomSpeed = 4f;

        Vector3 _lastMouse;

        void LateUpdate()
        {
            if (target == null)
            {
                var slot = FindFirstObjectByType<CharacterSlot>();
                if (slot != null) target = slot.transform;
            }

            if (Input.GetMouseButtonDown(1))
                _lastMouse = Input.mousePosition;
            if (Input.GetMouseButton(1))
            {
                Vector3 delta = Input.mousePosition - _lastMouse;
                _lastMouse = Input.mousePosition;
                yaw += delta.x * sensitivity * 0.02f * Time.deltaTime * 60f;
                pitch -= delta.y * sensitivity * 0.02f * Time.deltaTime * 60f;
                pitch = Mathf.Clamp(pitch, -10f, 55f);
            }

            distance = Mathf.Clamp(distance - Input.mouseScrollDelta.y * zoomSpeed * 0.15f, 1.6f, 10f);
            Vector3 pivot = target != null ? target.position + offset : offset;
            Quaternion rot = Quaternion.Euler(pitch, yaw, 0f);
            transform.SetPositionAndRotation(pivot + rot * new Vector3(0, 0, -distance), rot);
        }
    }
}
