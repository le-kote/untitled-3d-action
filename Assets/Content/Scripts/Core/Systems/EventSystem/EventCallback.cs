using UnityEngine;

public delegate T EventCallback<T>(ref T eventData) where T : struct;
