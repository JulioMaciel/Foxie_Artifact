using System;
using UnityEngine;

public class AttackSnakeEvent : MonoBehaviour
{
    [SerializeField] GameObject attackSnakeCanvas;
    
    public static AttackSnakeEvent Instance;
    void Awake() => Instance = this;

    void OnEnable()
    {
        var handler = attackSnakeCanvas.GetComponent<AttackSnakeHandler>();
        handler.OnPlayerApproachInput += OnPlayerApproachInput;
    }

    void OnPlayerApproachInput(float obj)
    {
        throw new NotImplementedException();
    }

    public void StartEvent()
    {
        attackSnakeCanvas.SetActive(true);
        // posiciona player
        // posisiona snake
    }
    
    // react when player move > 0
    // qual a relação do user input pro alarm level?
    // send stress alarm level...
    
    // player tem que chegar sorrateiramente por traz,
        // se for visto, a cobra ataca se perto        
    // se falhar, recomeça
}