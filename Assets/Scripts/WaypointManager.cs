using System;
using System.Collections;
using TMPro;
using UnityEngine;

public class WaypointManager : MonoBehaviour
{
    [SerializeField] GameObject waypoint3DUI;
    [SerializeField] GameObject canvasWaypoint;
    [SerializeField] GameObject player;

    WaypointHandler waypointHandler;
    TextMeshProUGUI waypointText;
    Transform target;
    
    public static WaypointManager Instance;
    
    void Awake()
    {
        Instance = this;
        waypointHandler = waypoint3DUI.GetComponentInChildren<WaypointHandler>();
        waypointText = canvasWaypoint.GetComponentInChildren<TextMeshProUGUI>();
    }

    public void SetNewTarget(Transform newTarget, string text)
    {
        waypoint3DUI.SetActive(true);
        canvasWaypoint.SetActive(true);
        
        waypointHandler.SetTarget(newTarget);
        waypointText.text = text;
        target = newTarget;

        StartCoroutine(EraseMessage());
        StartCoroutine(CheckIfApproachedTarget());
    }

    IEnumerator EraseMessage()
    {
        yield return new WaitForSeconds(5);
        waypointText.text = string.Empty;
    }

    IEnumerator CheckIfApproachedTarget()
    {
        while (!player.transform.position.IsCloseEnough(target.position, 2f))
            yield return null;
        
        waypoint3DUI.SetActive(false);
        canvasWaypoint.SetActive(false);
    } 
}
