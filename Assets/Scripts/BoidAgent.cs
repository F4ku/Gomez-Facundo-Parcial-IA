using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoidAgent : MonoBehaviour
{
    [Header("Atributos")]
    public float MaxSpeed = 4f;
    public float MaxForce = 2f;
    public float Health = 100f;
    public bool IsDead { get; private set; } = false;

    [Header("Percepción")]
    public float CohesionAlignmentRadius = 5f;
    public float SeparationRadius = 2f;
    public float EvadeRadius = 15f;
    public float ObjectPerceptionRadius = 10f;

    [Header("Referencias")]
    public HunterNPC Hunter;

    private Vector3 _velocity;
    private MeshRenderer _renderer;
    private float _interactionTimer;

    private float _wanderTimer;
    private Vector3 _wanderTarget;

    private void Start()
    {
        _velocity = Random.insideUnitSphere * MaxSpeed;
        _velocity.y = 0;
        _renderer = GetComponent<MeshRenderer>();
        _wanderTarget = transform.position; 
    }

    private void Update()
    {
        if (IsDead) return;

        Vector3 acceleration = Vector3.zero;

        acceleration += ObstacleAvoidance() * 5f;

        if (Vector3.Distance(transform.position, Hunter.transform.position) < EvadeRadius)
        {
            acceleration += Evade(Hunter.transform.position) * 1.5f;
        }
        else
        {
            ObjectOfInterest closestObj = GetClosestObjectInRadius();

            if (closestObj != null)
            {
                acceleration += Arrive(closestObj.transform.position);
                InteractWithObject(closestObj);
            }
            else
            {
                Vector3 sep = Separation() * 1.5f;
                Vector3 ali = Alignment() * 1f;
                Vector3 coh = Cohesion() * 1f;

                if (sep == Vector3.zero && ali == Vector3.zero && coh == Vector3.zero)
                {
                    _wanderTimer -= Time.deltaTime;
                    if (_wanderTimer <= 0)
                    {
                        _wanderTimer = 1f;
                        _wanderTarget = transform.position + transform.forward * 3f + new Vector3(Random.Range(-4f, 4f), 0, Random.Range(-4f, 4f));
                    }
                    acceleration += Seek(_wanderTarget) * 0.8f;
                }
                else
                {
                    acceleration += sep + ali + coh;
                }
            }
        }

        if (Hunter != null && Hunter.HuntingZoneCenter != null)
        {
            if (Vector3.Distance(Hunter.HuntingZoneCenter.position, transform.position) > 15f)
            {
                acceleration += Seek(Hunter.HuntingZoneCenter.position) * 2f;
            }
        }

        acceleration.y = 0;
        _velocity += acceleration * Time.deltaTime;
        _velocity = Vector3.ClampMagnitude(_velocity, MaxSpeed);
        transform.position += _velocity * Time.deltaTime;

        if (_velocity != Vector3.zero)
        {
            transform.forward = Vector3.Slerp(transform.forward, _velocity.normalized, Time.deltaTime * 8f);
        }
    }

    public void TakeDamage(float amount)
    {
        if (IsDead) return;

        Health -= amount;
        if (Health <= 0)
        {
            Health = 0;
            Die();
        }
    }

    private ObjectOfInterest GetClosestObjectInRadius()
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, ObjectPerceptionRadius);
        ObjectOfInterest closest = null;
        float minDist = Mathf.Infinity;

        foreach (var hit in hits)
        {
            ObjectOfInterest obj = hit.GetComponent<ObjectOfInterest>();
            if (obj != null)
            {
                float dist = Vector3.Distance(transform.position, obj.transform.position);
                if (dist < minDist)
                {
                    minDist = dist;
                    closest = obj;
                }
            }
        }
        return closest;
    }

    private void InteractWithObject(ObjectOfInterest obj)
    {
        if (Vector3.Distance(transform.position, obj.transform.position) < 2f)
        {
            _interactionTimer += Time.deltaTime;
            if (_interactionTimer >= 1f)
            {
                obj.TakeDamage(10f);
                _interactionTimer = 0f;
            }
        }
    }

    private Vector3 Evade(Vector3 target)
    {
        Vector3 desiredVelocity = (transform.position - target).normalized * MaxSpeed;
        return Vector3.ClampMagnitude(desiredVelocity - _velocity, MaxForce);
    }

    private Vector3 Arrive(Vector3 target)
    {
        Vector3 desiredVelocity = target - transform.position;
        float distance = desiredVelocity.magnitude;

        if (distance < 1.5f)
        {
            desiredVelocity = desiredVelocity.normalized * MaxSpeed * (distance / 1.5f);
        }
        else
        {
            desiredVelocity = desiredVelocity.normalized * MaxSpeed;
        }

        return Vector3.ClampMagnitude(desiredVelocity - _velocity, MaxForce);
    }

    private Vector3 Seek(Vector3 target)
    {
        Vector3 desiredVelocity = (target - transform.position).normalized * MaxSpeed;
        return Vector3.ClampMagnitude(desiredVelocity - _velocity, MaxForce);
    }

    private Vector3 Separation()
    {
        Vector3 steering = Vector3.zero;
        int count = 0;
        Collider[] hits = Physics.OverlapSphere(transform.position, SeparationRadius);
        foreach (var hit in hits)
        {
            if (hit.gameObject != gameObject && hit.CompareTag("Boid"))
            {
                Vector3 diff = transform.position - hit.transform.position;
                steering += diff.normalized / diff.magnitude;
                count++;
            }
        }
        if (count > 0)
        {
            steering /= count;
            steering = steering.normalized * MaxSpeed;
            steering -= _velocity;
            steering = Vector3.ClampMagnitude(steering, MaxForce);
        }
        return steering;
    }

    private Vector3 Alignment()
    {
        Vector3 sum = Vector3.zero;
        int count = 0;
        Collider[] hits = Physics.OverlapSphere(transform.position, CohesionAlignmentRadius);
        foreach (var hit in hits)
        {
            if (hit.gameObject != gameObject && hit.CompareTag("Boid"))
            {
                sum += hit.GetComponent<BoidAgent>()._velocity;
                count++;
            }
        }
        if (count > 0)
        {
            sum /= count;
            sum = sum.normalized * MaxSpeed;
            Vector3 steer = sum - _velocity;
            return Vector3.ClampMagnitude(steer, MaxForce);
        }
        return Vector3.zero;
    }

    private Vector3 Cohesion()
    {
        Vector3 sum = Vector3.zero;
        int count = 0;
        Collider[] hits = Physics.OverlapSphere(transform.position, CohesionAlignmentRadius);
        foreach (var hit in hits)
        {
            if (hit.gameObject != gameObject && hit.CompareTag("Boid"))
            {
                sum += hit.transform.position;
                count++;
            }
        }
        if (count > 0)
        {
            sum /= count;
            return Arrive(sum);
        }
        return Vector3.zero;
    }

    public void Die()
    {
        IsDead = true;
        _renderer.material.color = Color.gray;
    }

    public void Collect()
    {
        GetComponent<MeshRenderer>().enabled = false;
        GetComponent<Collider>().enabled = false;
        StartCoroutine(RespawnRoutine());
    }

    private IEnumerator RespawnRoutine()
    {
        yield return new WaitForSeconds(5f);

        if (Hunter != null && Hunter.HuntingZoneCenter != null)
        {
            Vector3 randomOffset = new Vector3(Random.Range(-3f, 3f), 0, Random.Range(-3f, 3f));
            transform.position = Hunter.HuntingZoneCenter.position + randomOffset;
        }

        Health = 100f;
        IsDead = false;

        _renderer.material.color = Color.white;
        GetComponent<MeshRenderer>().enabled = true;
        GetComponent<Collider>().enabled = true;
    }

    private Vector3 ObstacleAvoidance()
    {
        Vector3 avoidForce = Vector3.zero;
        Vector3 origin = transform.position + Vector3.up * 0.5f;
        int obstacleLayerMask = LayerMask.GetMask("Obstacle"); 

        if (Physics.Raycast(origin, transform.forward, out RaycastHit hitC, 2.5f, obstacleLayerMask))
            avoidForce += hitC.normal * MaxSpeed;

        if (Physics.Raycast(origin, transform.forward + transform.right * 0.5f, out RaycastHit hitR, 1.5f, obstacleLayerMask))
            avoidForce += hitR.normal * MaxSpeed;

        if (Physics.Raycast(origin, transform.forward - transform.right * 0.5f, out RaycastHit hitL, 1.5f, obstacleLayerMask))
            avoidForce += hitL.normal * MaxSpeed;

        if (avoidForce != Vector3.zero)
        {
            return Vector3.ClampMagnitude(avoidForce - _velocity, MaxForce * 1.5f);
        }

        return Vector3.zero;
    }
}