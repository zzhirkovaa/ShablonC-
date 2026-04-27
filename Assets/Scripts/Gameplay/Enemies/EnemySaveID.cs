using System.Collections.Generic;
using UnityEngine;

public class EnemySaveId : MonoBehaviour
{
    private static readonly List<EnemySaveId> RegisteredInstancesInternal = new();

    [SerializeField] private string _id;

    public string Id => _id;
    public static IReadOnlyList<EnemySaveId> RegisteredInstances => RegisteredInstancesInternal;

    private void Awake()
    {
        Register();
    }

    private void OnEnable()
    {
        Register();
    }

    private void OnDestroy()
    {
        RegisteredInstancesInternal.Remove(this);
    }

    private void Register()
    {
        if (!RegisteredInstancesInternal.Contains(this))
            RegisteredInstancesInternal.Add(this);
    }
}
