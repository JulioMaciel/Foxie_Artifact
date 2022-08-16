using System;
using ScriptableObjects;
using StaticData;
using TMPro;
using UnityEngine;
using Image = UnityEngine.UI.Image;

namespace Managers
{
    public class DialogueManager : MonoBehaviour
    {
        [SerializeField] RectTransform dialogueCanvas;
        [SerializeField] Image avatarImage;
        [SerializeField] TextMeshProUGUI authorTMP;
        [SerializeField] TextMeshProUGUI messageTMP;

        DialogueItem currentDialogueItem;
        MessageItem currentMessageItem;
        int currentMessageIndex;
    
        public event Action<EventToTrigger> OnEventToTrigger;
    
        public static DialogueManager Instance;
        void Awake() => Instance = this;

        public void StartDialogue(DialogueItem dialogueItem)
        {
            currentMessageIndex = 0;
            currentDialogueItem = dialogueItem;
            currentMessageItem = currentDialogueItem.messages[currentMessageIndex];
            dialogueCanvas.gameObject.SetActive(true);
            avatarImage.sprite = currentDialogueItem.actorItem.avatar;
            authorTMP.text = currentDialogueItem.actorItem.name; 
            messageTMP.text = currentMessageItem.message;
        }

        public void ProceedDialogue()
        {
            if (currentMessageItem.sequentialEventToTrigger != EventToTrigger.None)
                OnEventToTrigger?.Invoke(currentMessageItem.sequentialEventToTrigger);
        
            currentMessageIndex++;

            if (currentDialogueItem.messages.Count >= currentMessageIndex + 1)
            {
                currentMessageItem = currentDialogueItem.messages[currentMessageIndex];
                messageTMP.text = currentMessageItem.message;
            }
            else
                dialogueCanvas.gameObject.SetActive(false);
        }
    }
}
