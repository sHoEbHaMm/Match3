using UnityEngine;

public interface IObserver
{
    // Subject uses this method to communicate with the observers
    public void OnNotify();
}
