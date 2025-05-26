using UnityEngine;

public interface IInteractable {

    public GameObject InteractPrompt { get; set; }

    void Interact();

    void InteractAlternate();
    
}
