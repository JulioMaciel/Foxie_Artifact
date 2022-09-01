using UnityEngine;

namespace ScriptableObjects
{
    [CreateAssetMenu(fileName = "New Intro Text", menuName = "Intro/Text", order = 0)]
    public class IntroTextItem : ScriptableObject
    {
        public string text;
        public IntroTextItem next;
        public IntroBackgroundItem backgroundItem;
    }
}