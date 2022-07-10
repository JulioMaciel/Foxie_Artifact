using UnityEngine;

public class FarmerOpenDoor : MonoBehaviour
{
    Quaternion openedDoorRotation;
    bool isOpeningDoor;
    
    void Start()
    {
        var rot = transform.rotation;
        openedDoorRotation = Quaternion.Euler(new Vector3(rot.x, rot.y + 130, rot.z));
    }

    void Update()
    {
        if (!isOpeningDoor) return;
        
        transform.rotation = Quaternion.Lerp(transform.rotation, openedDoorRotation, Time.deltaTime);
        
        if (transform.rotation.normalized != openedDoorRotation.normalized)
            Destroy(this);
    }

    void OnCollisionEnter(Collision col)
    {
        if (col.gameObject.name == "Farmer")
            isOpeningDoor = true;
    }
}
