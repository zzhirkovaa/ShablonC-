using UnityEngine;

public sealed class BossStaticWorldEffect : MonoBehaviour
{
    private Vector3 _position;
    private Quaternion _rotation;
    private bool _initialized;

    public void Initialize(Vector3 position, Quaternion rotation)
    {
        _position = position;
        _rotation = rotation;
        _initialized = true;
        transform.SetPositionAndRotation(_position, _rotation);
    }

    private void LateUpdate()
    {
        if (_initialized)
            transform.SetPositionAndRotation(_position, _rotation);
    }
}
