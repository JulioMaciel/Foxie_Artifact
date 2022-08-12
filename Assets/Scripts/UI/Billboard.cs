using UnityEngine;

namespace UI
{
    public class Billboard : MonoBehaviour
    {
        Camera mainCamera;
    
        void Start()
        {
            mainCamera = Camera.main;
        }

        void LateUpdate()
        {
            //transform.rotation = Quaternion.Euler(0, mainCamera.transform.rotation.y, 0);
            transform.LookAt(transform.position + mainCamera.transform.forward);
        }
    }
}
