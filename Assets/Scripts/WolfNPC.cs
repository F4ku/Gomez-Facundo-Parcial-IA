using System.Collections.Generic;
using UnityEngine;

public class WolfNPC : MonoBehaviour
{
    [Header("Componentes")]
    public PerceptionSystem Perception;

    [Header("Parámetros")]
    public float Speed = 5.5f;
    public float AlertRadius = 15f;

    [Header("Restricciones de Mapa")]
    public Transform HuntingZoneCenter;

    private HunterNPC _hunterTarget;
    private List<Node> _currentPath = new List<Node>();
    private int _pathIndex = 0;
    private float _pathRefreshTimer = 0f;
    private float _attackCooldown = 0f;

    private void Awake()
    {
        if (Perception == null) Perception = GetComponent<PerceptionSystem>();
    }

    private void Update()
    {
        if (_attackCooldown > 0f) _attackCooldown -= Time.deltaTime;

        if (_hunterTarget != null)
        {
            Node hunterNode = Graph.Instance.GetClosestNode(_hunterTarget.transform.position, false);
            if (hunterNode != null && hunterNode.IsSafeZone && Vector3.Distance(_hunterTarget.transform.position, hunterNode.Position) < 4f)
            {
                _hunterTarget = null;
                _currentPath = null;
            }
        }


        if (_hunterTarget == null)
        {
            foreach (Transform visible in Perception.VisibleTargets)
            {
                HunterNPC hunter = visible.GetComponent<HunterNPC>();
                if (hunter != null)
                {
  
                    Node hunterNode = Graph.Instance.GetClosestNode(hunter.transform.position, false);
                    if (hunterNode != null && hunterNode.IsSafeZone && Vector3.Distance(hunter.transform.position, hunterNode.Position) < 4f)
                    {
                        continue; 
                    }

                    _hunterTarget = hunter;
                    AlertPack();
                    break;
                }
            }
        }

        if (_hunterTarget != null)
        {
            _pathRefreshTimer -= Time.deltaTime;
            if (_pathRefreshTimer <= 0)
            {
                _pathRefreshTimer = 1f;
                RequestPath(_hunterTarget.transform.position);
            }
            FollowPath();

            if (Vector3.Distance(transform.position, _hunterTarget.transform.position) < 2f)
            {
                if (_attackCooldown <= 0f)
                {
                    _hunterTarget.TakeDamage(25f);
                    _attackCooldown = 1.5f;
                }
            }
        }
        else
        {
            if (FollowPath())
            {
                Node randomNode = Graph.Instance.AllNodes[Random.Range(0, Graph.Instance.AllNodes.Count)];

                bool isHuntingZone = false;
                if (HuntingZoneCenter != null && Vector3.Distance(randomNode.Position, HuntingZoneCenter.position) < 15f)
                {
                    isHuntingZone = true;
                }

                bool isFarEnough = Vector3.Distance(transform.position, randomNode.Position) > 8f;

                if (!randomNode.IsSafeZone && !isHuntingZone && isFarEnough)
                {
                    RequestPath(randomNode.Position);
                }
            }
        }
    }

    public void ReceiveAlert(HunterNPC hunter)
    {
        _hunterTarget = hunter;
        RequestPath(_hunterTarget.transform.position);
    }

    private void AlertPack()
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, AlertRadius);
        foreach (Collider hit in hits)
        {
            WolfNPC wolf = hit.GetComponent<WolfNPC>();
            if (wolf != null && wolf != this)
            {
                wolf.ReceiveAlert(_hunterTarget);
            }
        }
    }

    private void RequestPath(Vector3 targetPos)
    {
        Node start = Graph.Instance.GetClosestNode(transform.position, true);
        Node target = Graph.Instance.GetClosestNode(targetPos, true);
        if (start != null && target != null)
        {
            _currentPath = Pathfinding.FindPath(start, target, true);
            _pathIndex = 0;
        }
    }

    private bool FollowPath()
    {
        if (_currentPath == null || _pathIndex >= _currentPath.Count) return true;

        Vector3 waypoint = _currentPath[_pathIndex].Position;
        transform.position = Vector3.MoveTowards(transform.position, waypoint, Speed * Time.deltaTime);

        if (waypoint != transform.position)
        {
            transform.forward = Vector3.Slerp(transform.forward, (waypoint - transform.position).normalized, Time.deltaTime * 10f);
        }

        if (Vector3.Distance(transform.position, waypoint) < 0.5f) _pathIndex++;
        return false;
    }
}