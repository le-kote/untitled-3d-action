using System.Collections;
using Unity.VisualScripting;
using UnityEngine;

public class TimedDespawn : FancyBehaviour
{
    private Coroutine _coroutine;

    private IEnumerator DestroyTimer(float timer)
    {
        yield return new WaitForSeconds(timer);

        PoolHide();
        Destroy(this);
        yield break;
    }

    private void Cancel()
    {
        if (_coroutine != null)
            StopCoroutine(_coroutine);

        Destroy(this);
    }

    public static void SetTimer(GameObject target, float timer)
    {
        var comp = target.GetOrAddComponent<TimedDespawn>();
        comp._coroutine = comp.StartCoroutine(comp.DestroyTimer(timer));
    }

    public static void Cancel(GameObject target)
    {
        if (!target.TryGetComponent<TimedDespawn>(out var comp))
            return;

        comp.Cancel();
    }
}
