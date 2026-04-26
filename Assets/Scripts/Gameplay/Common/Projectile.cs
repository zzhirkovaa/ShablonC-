using UnityEngine;

public class Projectile : MonoBehaviour
{
    public float speed = 15f;
    public float damageAmount = 25f;
    public float lifetime = 5f;

    [HideInInspector] public GameObject owner;

    private int _roomBoundsLayer;

    private void Awake()
    {
        _roomBoundsLayer = LayerMask.NameToLayer("RoomBounds");
    }

    private void Start()
    {
        Destroy(gameObject, lifetime);
    }

    private void Update()
    {
        transform.Translate(Vector3.forward * speed * Time.deltaTime);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (owner != null && other.gameObject == owner)
            return;

        if (_roomBoundsLayer != -1 && other.gameObject.layer == _roomBoundsLayer)
            return;

        if (other.TryGetComponent<IDamageable>(out var victim))
        {
            DamageInfo info = new DamageInfo(damageAmount, DamageType.Magical, owner);
            victim.TakeDamage(info);
            Destroy(gameObject);
            return;
        }

        if (!other.isTrigger)
        {
            Destroy(gameObject);
        }
    }
}
