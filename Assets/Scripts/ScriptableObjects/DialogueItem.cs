using System.Collections.Generic;
using UnityEngine;

namespace ScriptableObjects
{
    [CreateAssetMenu(fileName = "New dialogue", menuName = "Dialogue/Dialogue", order = 0)]
    public class DialogueItem : ScriptableObject
    {
        //public Dialogue dialogue;
        public ActorItem actorItem;
        public List<MessageItem> messages;
    }
}