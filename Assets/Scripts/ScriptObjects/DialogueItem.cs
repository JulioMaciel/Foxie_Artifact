using System.Collections.Generic;
using UnityEngine;

namespace ScriptObjects
{
    [CreateAssetMenu(fileName = "New dialogue", menuName = "Dialogue", order = 0)]
    public class DialogueItem : ScriptableObject
    {
        public Dialogue dialogue;
        public ActorItem actorItem;
        public List<string> messages;
    }
}