using UnityEngine;

public class ObjectOfInterest : MonoBehaviour
{
    [Header("Atributos")]
    public float Health = 30f; 

    public void TakeDamage(float amount)
    {
        Health -= amount;

        if (Health <= 0)
        {
            Destroy(gameObject);
        }
    }
}