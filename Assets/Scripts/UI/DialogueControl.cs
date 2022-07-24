using Managers;
using UnityEngine;
using UnityEngine.EventSystems;

namespace UI
{
    public class DialogueControl : MonoBehaviour, IPointerClickHandler
    {
        public void OnPointerClick(PointerEventData eventData) => DialogueManager.Instance.ProceedDialogue();
    }
}
