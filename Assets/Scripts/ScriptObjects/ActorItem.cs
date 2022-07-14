using UnityEngine;

namespace ScriptObjects
{
    [CreateAssetMenu(fileName = "New actor", menuName = "Actor", order = 0)]
    public class ActorItem : ScriptableObject
    {
        public new string name;
        public Sprite avatar;
    }
}