using UnityEngine;

namespace InternshipProject.Interaction
{
    public class InteractableObject : MonoBehaviour, IInteractable
    {
        [SerializeField] private string interactText = "Interact";
        [SerializeField] private Material interactMaterial;
        
        private Renderer _renderer;
        private Material _originalMaterial;
        private bool _isInteracted = false;

        private void Awake()
        {
            _renderer = GetComponent<Renderer>();
            if (_renderer != null)
            {
                _originalMaterial = _renderer.sharedMaterial;
            }
        }

        public string GetInteractText() => interactText;

        public void Interact()
        {
            _isInteracted = !_isInteracted;
            Debug.Log($"Interacted with {gameObject.name}. State: {_isInteracted}");
            
            if (_renderer != null && interactMaterial != null)
            {
                _renderer.material = _isInteracted ? interactMaterial : _originalMaterial;
            }
        }
    }
}

/* Optimization Note: Toggles between materials. 
   In production with many objects, use MaterialPropertyBlocks to avoid material instantiation on .material access. */
