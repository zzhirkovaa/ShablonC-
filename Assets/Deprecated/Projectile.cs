using UnityEngine;

public class Projectile : MonoBehaviour
{
    public float speed = 15f;
    public float damageAmount = 25f;
    public float lifetime = 5f;

    [HideInInspector] public GameObject owner; 

    void Start() => Destroy(gameObject, lifetime);

    void Update() => transform.Translate(Vector3.forward * speed * Time.deltaTime);

    private void OnTriggerEnter(Collider other)
    {
        if (owner != null && other.gameObject == owner) return;
        if (other.TryGetComponent<IDamageable>(out var victim))
        {
            DamageInfo info = new DamageInfo(damageAmount, DamageType.Magical);
            victim.TakeDamage(info);
            Destroy(gameObject);
        }
        else if (other.gameObject.layer == LayerMask.NameToLayer("Default"))
        {
            Destroy(gameObject); 
        }
    }
}