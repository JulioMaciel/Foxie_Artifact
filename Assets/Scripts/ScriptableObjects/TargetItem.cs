using StaticData;
using UnityEngine;

namespace ScriptableObjects
{
    [CreateAssetMenu(fileName = "New target", menuName = "Target", order = 0)]
    public class TargetItem : ScriptableObject
    {
        public Target target;
        public string message;
    }
}