using UnityEngine;
using UnityEngine.InputSystem;
using InternshipProject.Interaction;

namespace InternshipProject.Player
{
    [RequireComponent(typeof(CharacterController))]
    public class PlayerController : MonoBehaviour
    {
        [Header("Movement")]
        [SerializeField] private float moveSpeed = 5f;
        [SerializeField] private float lookSensitivity = 1f;
        [SerializeField] private float gravity = -9.81f;

        [Header("Interaction")]
        [SerializeField] private float interactionRange = 5f;
        [SerializeField] private LayerMask interactionLayer;

        private CharacterController _controller;
        private Vector2 _moveInput;
        private Vector2 _lookInput;
        private float _verticalVelocity;
        private Transform _cameraTransform;
        private float _cameraPitch;

        // New Input System direct access
        private InputAction _moveAction;
        private InputAction _lookAction;
        private InputAction _interactAction;

        private void Awake()
        {
            _controller = GetComponent<CharacterController>();
            _cameraTransform = GetComponentInChildren<Camera>().transform;
            
            // Accessing actions directly from the global InputSystem.actions
            _moveAction = InputSystem.actions.FindAction("Move");
            _lookAction = InputSystem.actions.FindAction("Look");
            _interactAction = InputSystem.actions.FindAction("Interact");

            if (interactionLayer.value == 0) interactionLayer = ~0; 

            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        private void Update()
        {
            // Read values directly each frame for newest pattern
            _moveInput = _moveAction.ReadValue<Vector2>();
            _lookInput = _lookAction.ReadValue<Vector2>();

            if (_interactAction.WasPressedThisFrame())
            {
                Debug.Log("Interact Action Triggered via WasPressedThisFrame");
                PerformInteraction();
            }

            HandleRotation();
            HandleMovement();
            
            Debug.DrawRay(_cameraTransform.position, _cameraTransform.forward * interactionRange, Color.red);
        }

        private void HandleRotation()
        {
            transform.Rotate(Vector3.up * (_lookInput.x * lookSensitivity));
            _cameraPitch -= _lookInput.y * lookSensitivity;
            _cameraPitch = Mathf.Clamp(_cameraPitch, -89f, 89f);
            _cameraTransform.localRotation = Quaternion.Euler(_cameraPitch, 0, 0);
        }

        private void HandleMovement()
        {
            if (_controller.isGrounded && _verticalVelocity < 0) _verticalVelocity = -2f;

            Vector3 move = transform.right * _moveInput.x + transform.forward * _moveInput.y;
            _controller.Move(move * (moveSpeed * Time.deltaTime));

            _verticalVelocity += gravity * Time.deltaTime;
            _controller.Move(Vector3.up * (_verticalVelocity * Time.deltaTime));
        }

        private void PerformInteraction()
        {
            Ray ray = new Ray(_cameraTransform.position, _cameraTransform.forward);
            RaycastHit[] hits = Physics.RaycastAll(ray, interactionRange, interactionLayer);
            
            if (hits.Length > 0)
            {
                System.Array.Sort(hits, (a, b) => a.distance.CompareTo(b.distance));
                foreach (var hit in hits)
                {
                    if (hit.collider.gameObject == gameObject) continue;

                    if (hit.collider.TryGetComponent(out IInteractable interactable))
                    {
                        interactable.Interact();
                        return;
                    }
                }
            }
        }
    }
}

/* Optimization Note: Uses the latest InputSystem.actions API for direct action access. 
   Reduces boilerplate and avoids "SendMessages" overhead for cleaner code. */
