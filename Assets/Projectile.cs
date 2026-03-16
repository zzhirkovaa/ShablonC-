using UnityEngine;

public class Projectile : MonoBehaviour
{
    public float speed = 10f;
    public float damageAmount = 10f; 
    public float lifetime = 5f;

    void Start()
    {
        Destroy(gameObject, lifetime);
    }

    void Update()
    {
        transform.Translate(Vector3.forward * speed * Time.deltaTime);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent<IDamageable>(out var victim))
        {
            DamageInfo info = new DamageInfo(damageAmount, DamageType.Magical);
            victim.TakeDamage(info);

            Destroy(gameObject);
        }
    }
}