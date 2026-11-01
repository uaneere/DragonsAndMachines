using UnityEngine;
using UnityEngine.InputSystem;

namespace DragonsAndMachines.Unity.Presentation
{
    public sealed class IsoCameraController : MonoBehaviour
    {
        public Vector3 Target;
        public float Distance = 28f;
        public float MinDistance = 10f;
        public float MaxDistance = 55f;
        public float MoveSpeed = 18f;
        public float ZoomSpeed = 4f;

        private Vector3 _lastMouse;

        public void Focus(Vector3 target)
        {
            Target = target;
            Apply();
        }

        private void LateUpdate()
        {
            var keyboard = Keyboard.current;
            var mouse = Mouse.current;
            var move = Vector3.zero;
            if (keyboard != null)
            {
                if (keyboard.wKey.isPressed || keyboard.upArrowKey.isPressed) move += new Vector3(1f, 0f, 1f);
                if (keyboard.sKey.isPressed || keyboard.downArrowKey.isPressed) move += new Vector3(-1f, 0f, -1f);
                if (keyboard.aKey.isPressed || keyboard.leftArrowKey.isPressed) move += new Vector3(-1f, 0f, 1f);
                if (keyboard.dKey.isPressed || keyboard.rightArrowKey.isPressed) move += new Vector3(1f, 0f, -1f);
            }

            if (move.sqrMagnitude > 0f)
                Target += move.normalized * (MoveSpeed * Time.deltaTime);

            if (mouse != null)
            {
                var scroll = mouse.scroll.ReadValue().y;
                if (Mathf.Abs(scroll) > 0.01f)
                    Distance = Mathf.Clamp(Distance - scroll * 0.02f * ZoomSpeed, MinDistance, MaxDistance);

                if (mouse.middleButton.isPressed)
                {
                    var pos = mouse.position.ReadValue();
                    if (mouse.middleButton.wasPressedThisFrame)
                        _lastMouse = pos;
                    var delta = (Vector3)pos - _lastMouse;
                    Target += new Vector3(-delta.x, 0f, -delta.y) * 0.04f;
                    _lastMouse = pos;
                }
            }

            Apply();
        }

        private void Apply()
        {
            transform.rotation = Quaternion.Euler(35.264f, 45f, 0f);
            transform.position = Target - transform.forward * Distance;
            var cam = GetComponent<Camera>();
            if (cam != null)
            {
                cam.orthographic = true;
                cam.orthographicSize = Distance * 0.35f;
            }
        }
    }
}
