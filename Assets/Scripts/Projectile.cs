using UnityEngine;

public class Projectile : MonoBehaviour
{
    public float Speed = 15f;
    public float Lifetime = 3f;
    public float Damage = 100f; 

    private void Start()
    {
        Destroy(gameObject, Lifetime); 
    }

    private void Update()
    {
        transform.position += transform.forward * Speed * Time.deltaTime;
    }

    private void OnTriggerEnter(Collider other)
    {

        BoidAgent boid = other.GetComponent<BoidAgent>();
        if (boid != null)
        {
            boid.TakeDamage(Damage);
            Destroy(gameObject);
        }

        else if (other.gameObject.layer == LayerMask.NameToLayer("Obstacle"))
        {
            Destroy(gameObject);
        }
    }
}