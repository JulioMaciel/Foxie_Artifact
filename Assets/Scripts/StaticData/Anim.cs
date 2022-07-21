using UnityEngine;

namespace StaticData
{
    public static class AnimParam
    {
        public static int MoveSpeed = Animator.StringToHash("MoveSpeed");
        public static int Attack = Animator.StringToHash("Attack");
        public static int IdleEvent = Animator.StringToHash("IdleEvent");
    
        public static class Fox
        {
            public static int WalkMultiplier = Animator.StringToHash("walkMultiplier");
            public static int StandUp = Animator.StringToHash("StandUp");
            public static int Jump = Animator.StringToHash("Jump");
            public static int Sleep = Animator.StringToHash("Sleep");
        }

        public static class Goldie
        {
            public static int Bark = Animator.StringToHash("Bark");        
        }
    
        public static class Human
        {
            public static int Wave = Animator.StringToHash("Wave");
            public static int isWorking = Animator.StringToHash("isWorking");
        }

        public static class Snake
        {
            public static int Die = Animator.StringToHash("Die");
        }
    }

    public enum AnimClip
    {
        Idle,
        Wave
    }
}