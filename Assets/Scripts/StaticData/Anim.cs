using UnityEngine;

namespace StaticData
{
    public struct AnimParam
    {
        public static int MoveSpeed = Animator.StringToHash("MoveSpeed");
        public static int Attack = Animator.StringToHash("Attack");
        public static int IdleEvent = Animator.StringToHash("IdleEvent");
    
        public struct Fox
        {
            public static int WalkMultiplier = Animator.StringToHash("walkMultiplier");
            public static int StandUp = Animator.StringToHash("StandUp");
            public static int Jump = Animator.StringToHash("Jump");
            public static int Sleep = Animator.StringToHash("Sleep");
        }

        public struct Goldie
        {
            public static int Bark = Animator.StringToHash("Bark");        
        }
    
        public struct Human
        {
            public static int Wave = Animator.StringToHash("Wave");
            public static int IsWorking = Animator.StringToHash("isWorking");
            public static int SitDown = Animator.StringToHash("SitDown");
            public static int StandUp = Animator.StringToHash("StandUp");
        }

        public struct Snake
        {
            public static int Die = Animator.StringToHash("Die");
        }

        public struct Car
        {
            public static int IsMovingForward = Animator.StringToHash("isMovingForward");
        }
    }

    public enum AnimClip
    {
        Idle,
        Wave
    }
}