using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameEventRaiser : MonoBehaviour
{
    public GameEvent Event;
    public bool RaiseOnAwake = false;

    private void Awake()
    {
        if (!RaiseOnAwake)
        {
            return;
        }
        Raise();
    }

    public void Raise()
    {
        Event.Raise();
    }
}