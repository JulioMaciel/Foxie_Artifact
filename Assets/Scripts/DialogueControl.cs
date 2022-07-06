using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;

public class DialogueControl : MonoBehaviour, IPointerClickHandler
{
    public void OnPointerClick(PointerEventData eventData)
    {
        DialogueManager.instance.ProceedDialogue();
    }
}