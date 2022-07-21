using System;
using ScriptObjects;
using TMPro;
using UnityEngine;
using Image = UnityEngine.UI.Image;

public class DialogueManager : MonoBehaviour
{
    [SerializeField] GameObject canvas;
    [SerializeField] Image avatarImage;
    [SerializeField] TextMeshProUGUI authorTMP;
    [SerializeField] TextMeshProUGUI messageTMP;

    DialogueItem currentDialogueItem;
    MessageItem currentMessageItem;
    int currentMessageIndex;
    
    public event Action<DialogueEvent> OnDialogueEvent;
    
    public static DialogueManager Instance;
    void Awake() => Instance = this;

    public void StartDialogue(DialogueItem dialogueItem)
    {
        currentMessageIndex = 0;
        currentDialogueItem = dialogueItem;
        currentMessageItem = currentDialogueItem.messages[currentMessageIndex];
        canvas.SetActive(true);
        avatarImage.sprite = currentDialogueItem.actorItem.avatar;
        authorTMP.text = currentDialogueItem.actorItem.name; 
        messageTMP.text = currentMessageItem.message;
    }

    public void ProceedDialogue()
    {
        if (currentMessageItem.sequentialEvent != DialogueEvent.None)
            OnDialogueEvent?.Invoke(currentMessageItem.sequentialEvent);
        
        currentMessageIndex++;

        if (currentDialogueItem.messages.Count >= currentMessageIndex + 1)
        {
            currentMessageItem = currentDialogueItem.messages[currentMessageIndex];
            messageTMP.text = currentMessageItem.message;
        }
        else
            canvas.SetActive(false);
    }
}
