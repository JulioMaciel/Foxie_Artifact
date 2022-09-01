using UnityEngine;

namespace ScriptableObjects
{
    [CreateAssetMenu(fileName = "New Intro Background", menuName = "Intro/Background", order = 1)]
    public class IntroBackgroundItem : ScriptableObject
    {
        public Sprite image;
    }
}