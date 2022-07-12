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

    Dialogue currentDialogue;
    int currentMessageIndex;
    
    public static DialogueManager Instance;
    
    public event Action<Quest, int> OnEndMessage;
    
    void Awake() => Instance = this;

    public void StartDialogue(Dialogue dialogue)
    {
        currentDialogue = dialogue;
        canvas.SetActive(true);
        avatarImage.sprite = dialogue.actor.avatar;
        authorTMP.text = dialogue.actor.name; 
        messageTMP.text = dialogue.messages[currentMessageIndex];
    }

    public void ProceedDialogue()
    {
        OnEndMessage?.Invoke(currentDialogue.quest, currentMessageIndex);
        currentMessageIndex++;

        if (currentDialogue.messages.Count - 1 >= currentMessageIndex)
            messageTMP.text = currentDialogue.messages[currentMessageIndex];
        else
            canvas.SetActive(false);
    }
}
