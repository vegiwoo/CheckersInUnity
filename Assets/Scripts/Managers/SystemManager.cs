using Checkers.Interfaces;
using Checkers.Managers;
using UnityEngine;

public class SystemManager : MonoBehaviour
{
    ISubject gameManager;
    IObserver observerManager;

    private void Awake()
    {
        gameManager = gameObject.AddComponent<GameManager>();
        observerManager = new ObserverManager();
    }

    private void OnEnable()
    {
        gameManager.Attach(observerManager);
    }

    private void OnDisable()
    {
        gameManager.Detach(observerManager);
    }
}