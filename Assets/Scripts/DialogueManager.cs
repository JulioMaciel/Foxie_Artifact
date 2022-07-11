using System;
using ScriptObjects;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using Image = UnityEngine.UI.Image;

public class DialogueManager : MonoBehaviour
{
    [SerializeField] GameObject Canvas;
    [SerializeField] Image AvatarImage;
    [SerializeField] TextMeshProUGUI AuthorTMP;
    [SerializeField] TextMeshProUGUI MessageTMP;

    Dialogue currentDialogue;
    int currentMessageIndex;
    
    public static DialogueManager instance;
    
    public event Action<int> OnEndMessage;
    
    void Awake() => instance = this;

    public void StartDialogue(Dialogue dialogue)
    {
        currentDialogue = dialogue;
        Canvas.SetActive(true);
        AvatarImage.sprite = dialogue.actor.avatar;
        AuthorTMP.text = dialogue.actor.name; 
        MessageTMP.text = dialogue.messages[currentMessageIndex];
    }

    public void ProceedDialogue()
    {
        OnEndMessage?.Invoke(currentMessageIndex);
        currentMessageIndex++;

        if (currentDialogue.messages.Count - 1 >= currentMessageIndex)
            MessageTMP.text = currentDialogue.messages[currentMessageIndex];
        else
            Canvas.SetActive(false);
    }
}
