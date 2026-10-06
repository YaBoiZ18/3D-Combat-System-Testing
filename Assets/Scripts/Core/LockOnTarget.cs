using System.Collections.Generic;
using UnityEngine;

public class LockOnTarget : MonoBehaviour
{
    // NEW: every enabled target adds itself to this list, so the LockOnController
    // can loop over it instead of running a physics query every time.
    private static readonly List<LockOnTarget> activeTargets = new List<LockOnTarget>();
    public static IReadOnlyList<LockOnTarget> ActiveTargets => activeTargets;

    [SerializeField] private Transform lockPoint;

    public Transform LockPoint => lockPoint != null ? lockPoint : transform;

    // NEW: register / unregister automatically.
    // Disabling this component (e.g. when the enemy dies) removes it from the list
    // and the LockOnController will drop the lock by itself.
    private void OnEnable() => activeTargets.Add(this);
    private void OnDisable() => activeTargets.Remove(this);

    // CHANGED: removed the empty Start() and Update() methods.
}