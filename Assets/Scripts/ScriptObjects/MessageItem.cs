using UnityEngine;

namespace ScriptObjects
{
    [CreateAssetMenu(fileName = "New Message", menuName = "Dialogue/Message", order = 2)]
    public class MessageItem : ScriptableObject
    {
        [TextArea]
        public string message;
        public DialogueEvent sequentialEvent = DialogueEvent.None;
    }
}