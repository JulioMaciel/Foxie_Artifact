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
    int currentMessageIndex;
    
    public static DialogueManager Instance;
    
    public event Action<Dialogue, int> OnEndMessage;
    
    void Awake() => Instance = this;

    public void StartDialogue(DialogueItem dialogueItem)
    {
        currentDialogueItem = dialogueItem;
        canvas.SetActive(true);
        avatarImage.sprite = dialogueItem.actorItem.avatar;
        authorTMP.text = dialogueItem.actorItem.name; 
        messageTMP.text = dialogueItem.messages[currentMessageIndex];
    }

    public void ProceedDialogue()
    {
        OnEndMessage?.Invoke(currentDialogueItem.dialogue, currentMessageIndex);
        currentMessageIndex++;

        if (currentDialogueItem.messages.Count - 1 >= currentMessageIndex)
            messageTMP.text = currentDialogueItem.messages[currentMessageIndex];
        else
            canvas.SetActive(false);
    }
}
