using UnityEngine;

public delegate void EventCallback<T>(ref T eventData) where T : struct;
