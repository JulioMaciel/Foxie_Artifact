using UnityEngine;

namespace ScriptObjects
{
    [CreateAssetMenu(fileName = "New Actor", menuName = "Dialogue/Actor", order = 1)]
    public class ActorItem : ScriptableObject
    {
        public new string name;
        public Sprite avatar;
    }
}