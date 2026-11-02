using System.Collections.Generic;
using System.Xml;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

public class HunterNPC : MonoBehaviour
{
    public StateMachine FSM { get; private set; }
    public SearchState SearchState { get; private set; }
    public AttackState AttackState { get; private set; }
    public GatherState GatherState { get; private set; }
    public FleeState FleeState { get; private set; }
    public ExitState ExitState { get; private set; }
    public DeadState DeadState { get; private set; }

    [Header("Componentes (Parcial 2)")]
    public PerceptionSystem Perception;

    [Header("Parámetros Base")]
    public float Speed = 5f;
    public float TBA = 3f;
    public float TimeSinceLastAttack;
    public float RangeAttackRadius = 10f;
    public float MeleeAttackRadius = 2f;

    [Header("Variables Parcial 2")]
    public int RequiredGathers = 3;
    public int CurrentGathers = 0;
    public float RespawnTime = 5f;
    public float SafeZoneWaitTime = 3f;

    [Header("Zonas del Mapa (Parcial 2)")]
    public Transform SpawnPoint; 
    public Transform HuntingZoneCenter; 
    public Transform ExitPoint; 

    [Header("Generación de Objetos")]
    public GameObject ObjectOfInterestPrefab;
    public List<GameObject> ActiveObjects = new List<GameObject>();

    [Header("Sistema de Vida y Combate")]
    public float Health = 100f;
    public GameObject ProjectilePrefab;

    [Header("Feedback Visual")]
    public TextMeshProUGUI StateText;

    public List<Node> CurrentPath = new List<Node>();
    public int PathIndex = 0;

    private void Awake()
    {
        FSM = new StateMachine();
        SearchState = new SearchState(this);
        AttackState = new AttackState(this);
        GatherState = new GatherState(this);
        FleeState = new FleeState(this);
        ExitState = new ExitState(this);
        DeadState = new DeadState(this);

        Perception = GetComponent<PerceptionSystem>();
        TimeSinceLastAttack = TBA;
    }

    private void Start()
    {
        transform.position = SpawnPoint.position;
        FSM.ChangeState(SearchState);
    }

    private void Update()
    {
        TimeSinceLastAttack += Time.deltaTime;
        FSM.Update();

        ActiveObjects.RemoveAll(item => item == null);

        if (StateText != null)
            StateText.text = $"State: {FSM.CurrentState.GetType().Name} \nGathers: {CurrentGathers}/{RequiredGathers}";
    }


    public void RequestPath(Vector3 targetPosition, bool evadeLobos = false)
    {
        Node startNode = Graph.Instance.GetClosestNode(transform.position, false);
        Node targetNode = Graph.Instance.GetClosestNode(targetPosition, false);

        if (startNode != null && targetNode != null)
        {
            CurrentPath = Pathfinding.FindPath(startNode, targetNode, evadeLobos);
            PathIndex = 0;
        }
    }

    public bool FollowPath()
    {
        if (CurrentPath == null || PathIndex >= CurrentPath.Count)
            return true;

        Vector3 currentWaypoint = CurrentPath[PathIndex].Position;

        transform.position = Vector3.MoveTowards(transform.position, currentWaypoint, Speed * Time.deltaTime);

        if (Vector3.Distance(transform.position, currentWaypoint) < 0.5f)
        {
            PathIndex++;
        }

        return false; 
    }


    public BoidAgent GetVisibleBoid(bool requiresDead)
    {
        BoidAgent closest = null;
        float minDistance = Mathf.Infinity;

        foreach (Transform target in Perception.VisibleTargets)
        {
            BoidAgent boid = target.GetComponent<BoidAgent>();
            if (boid != null && boid.IsDead == requiresDead)
            {
                float dist = Vector3.Distance(transform.position, boid.transform.position);
                if (dist < minDistance)
                {
                    minDistance = dist;
                    closest = boid;
                }
            }
        }
        return closest;
    }


    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, RangeAttackRadius);
        Gizmos.color = Color.black;
        Gizmos.DrawWireSphere(transform.position, MeleeAttackRadius);
        if (CurrentPath != null && CurrentPath.Count > 0)
        {
            Gizmos.color = Color.white;
            for (int i = PathIndex; i < CurrentPath.Count - 1; i++)
            {
                Gizmos.DrawLine(CurrentPath[i].Position, CurrentPath[i + 1].Position);
            }
        }
    }

    public void TakeDamage(float amount)
    {
        Health -= amount;
        if (Health <= 0 && FSM.CurrentState != DeadState)
        {
            Health = 0;
            FSM.ChangeState(DeadState); 
        }
    }

    public void Shoot(Vector3 targetPos)
    {
        if (ProjectilePrefab != null)
        {

            GameObject bullet = Instantiate(ProjectilePrefab, transform.position + Vector3.up * 0.5f, Quaternion.identity);
            bullet.transform.LookAt(targetPos + Vector3.up * 0.5f);
        }
    }
}