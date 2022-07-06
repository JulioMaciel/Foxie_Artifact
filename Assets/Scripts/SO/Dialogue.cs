using System.Collections.Generic;
using UnityEngine;

namespace ScriptObjects
{
    [CreateAssetMenu(fileName = "New dialogue", menuName = "Dialogue", order = 0)]
    public class Dialogue : ScriptableObject
    {
        public Actor actor;
        public List<string> messages;
    }
}