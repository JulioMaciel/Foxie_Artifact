using UnityEngine;

namespace Managers
{
    public class Entity : MonoBehaviour
    {
        public GameObject player;
        public GameObject goldie;
        public GameObject farmer;
        public GameObject snake;
        public GameObject boringCar;
        public GameObject boringDriver;
        public GameObject boringPassenger;
        
        public static Entity Instance;
        void Awake() => Instance = this;
    }
}