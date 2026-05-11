using UnityEngine;

public interface ICameraPlanarBasisProvider
{
    Vector3 ForwardOnPlane { get; }
    Vector3 RightOnPlane { get; }
}
