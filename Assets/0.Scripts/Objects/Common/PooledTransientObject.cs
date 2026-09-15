using UnityEngine;

public class PooledTransientObject : PooledObject
{
    [SerializeField] float maxTime = 1.0f;

    void OnEnable()
    {
        Invoke(nameof(Enqueue), maxTime);
    }

    protected override void OnEnqueue()
    {
        base.OnEnqueue();
        CancelInvoke(nameof(Enqueue));
    }
}
