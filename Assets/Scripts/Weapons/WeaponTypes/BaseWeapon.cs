using UnityEngine;

public abstract class BaseWeapon : ScriptableObject
{
    public float DelayBeforeAttack;
    public float AttackRate;
    public bool Automatic;
    public bool StopAttacksOnButtonUp;
}
