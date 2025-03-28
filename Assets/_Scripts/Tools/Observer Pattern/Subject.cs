using System.Collections.Generic;
using UnityEngine;

public abstract class Subject : MonoBehaviour
{
    // Collection of all observers of this subject
    private List<IObserver> observers = new List<IObserver>();

    // add observer to the subject's collection
    void AddObserver(IObserver observer)
    {
        observers.Add(observer);
    }

    // remove observer from the subject's collection
    void RemoveObserver(IObserver observer)
    {
        observers.Remove(observer);
    }

    // notify each observer that an event has occured
    protected void NotifyObservers()
    {
        foreach(var observer in observers)
        {
            observer.OnNotify();
        }
    }

}
