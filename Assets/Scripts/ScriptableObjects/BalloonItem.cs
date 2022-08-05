using UnityEngine;

namespace ScriptableObjects
{
    [CreateAssetMenu(fileName = "New Balloon", menuName = "Balloon", order = 0)]
    public class BalloonItem : ScriptableObject
    {
        public Sprite[] balloonContent;
    }
}