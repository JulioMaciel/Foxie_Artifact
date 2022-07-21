using System.Collections;
using UnityEngine;

namespace ScriptableAnimations
{
    public class FarmerOpenDoor : MonoBehaviour
    {
        Quaternion openedDoorRotation;
        bool isOpeningDoor;
    
        void Start()
        {
            var rot = transform.localRotation;
            openedDoorRotation = Quaternion.Euler(new Vector3(rot.x, rot.y + 130, rot.z));
        }

        void Update()
        {
            if (!isOpeningDoor) return;
        
            transform.localRotation = Quaternion.Slerp(transform.localRotation, openedDoorRotation, Time.deltaTime);
            StartCoroutine(DestroySoon());
        }

        IEnumerator DestroySoon()
        {
            yield return new WaitForSeconds(5);
            Destroy(this);
        }

        void OnTriggerEnter(Collider col)
        {
            if (col.gameObject.name == "Farmer")
                isOpeningDoor = true;
        }
    }
}
