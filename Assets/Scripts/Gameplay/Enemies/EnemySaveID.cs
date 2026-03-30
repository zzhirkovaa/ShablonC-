using UnityEngine;

public class EnemySaveId : MonoBehaviour
{
    [SerializeField] private string _id;

    public string Id => _id;
}