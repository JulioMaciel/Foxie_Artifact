using System;
using UnityEngine;
using UnityEngine.UI;

public class AttackSnakeHandler : MonoBehaviour
{
    [SerializeField] Slider stressTotalSlider;
    [SerializeField] Slider stressSideSlider;
    [SerializeField] RectTransform stressLevelSideBar;

    // float currentStress;
    // float totalStress;
    //float buttonPosition;

    // bool isUpdatingButtonPosition;
    // bool isUpdatingCurrentStress;
    // bool isUpdatingTotalStress;

    public Action<float> OnPlayerApproachInput;

    void OnEnable()
    {
        // must subscribe to current and total stress changes from AttackSnakeEvent 
        stressSideSlider.onValueChanged.AddListener(ReactToPlayerInput);
    }

    // void Update()
    // {
    //     if (isUpdatingCurrentStress) UpdateStressLevelSideBar();
    //     if (isUpdatingTotalStress) UpdateTotalStressTopBar();
    // }

    // bool IsPlayerMovingButton()
    // {
    //     if (!Input.GetMouseButton(0)) return false;
    //     
    //     var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
    //     Physics.Raycast(ray, out var hit);
    //
    //     return hit.transform.gameObject == attackSnakeButton.transform.gameObject;
    // }

    void ReactToPlayerInput(float value)
    {
        OnPlayerApproachInput?.Invoke(value);
    }

    public void UpdateTotalStressTopBar(float totalStress)
    {
        var scaledTotalStress = totalStress / 100;
        //stressTopBarCurrent.localScale = new Vector3(scaledTotalStress, 1f, 1f);
        stressTotalSlider.value = scaledTotalStress;
    }

    public void UpdateStressLevelSideBar(float currentStress)
    {
        var scaledCurrentStress = currentStress / 100;
        stressLevelSideBar.localScale = new Vector3(1f, scaledCurrentStress, 1f);
    }
    
    void OnDisable()
    {
        stressSideSlider.onValueChanged.RemoveListener(ReactToPlayerInput);
    }
}
