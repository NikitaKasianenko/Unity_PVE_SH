using System;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

public abstract class Subject : MonoBehaviour
{
    private List<IObserver> _observers = new List<IObserver>();

    void AddObserver(IObserver observer)
    {
        _observers.Add(observer);
    }

    void RemoveObsrver(IObserver observer)
    {
        _observers.Remove(observer);
    }

    protected void NotifyObservers(string action)
    {
        _observers.ForEach((_observer)=>
        {
            _observer.OnNotify(action);
        });
    }
}
