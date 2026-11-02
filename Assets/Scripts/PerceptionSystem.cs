using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PerceptionSystem : MonoBehaviour
{
    [Header("Configuración de Percepción")]
    public float ViewRadius = 10f;
    [Range(0, 360)]
    public float ViewAngle = 360f; 

    [Header("Filtros (Layers)")]
    public LayerMask TargetMask;  
    public LayerMask ObstacleMask; 

    [Header("Resultados en tiempo real")]
    public List<Transform> VisibleTargets = new List<Transform>();

    private void Start()
    {

        StartCoroutine(FindTargetsWithDelay(0.2f));
    }

    private IEnumerator FindTargetsWithDelay(float delay)
    {
        while (true)
        {
            yield return new WaitForSeconds(delay);
            FindVisibleTargets();
        }
    }

    private void FindVisibleTargets()
    {
        VisibleTargets.Clear();


        Collider[] targetsInViewRadius = Physics.OverlapSphere(transform.position, ViewRadius, TargetMask);

        foreach (Collider targetCollider in targetsInViewRadius)
        {
            Transform target = targetCollider.transform;
            Vector3 dirToTarget = (target.position - transform.position).normalized;

            if (Vector3.Angle(transform.forward, dirToTarget) < ViewAngle / 2)
            {
                float dstToTarget = Vector3.Distance(transform.position, target.position);

                if (!Physics.Raycast(transform.position, dirToTarget, dstToTarget, ObstacleMask))
                {
                    VisibleTargets.Add(target);
                }
            }
        }
    }

    public Vector3 DirFromAngle(float angleInDegrees, bool angleIsGlobal)
    {
        if (!angleIsGlobal)
            angleInDegrees += transform.eulerAngles.y;
        return new Vector3(Mathf.Sin(angleInDegrees * Mathf.Deg2Rad), 0, Mathf.Cos(angleInDegrees * Mathf.Deg2Rad));
    }

    private void OnDrawGizmos()
    {
 
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, ViewRadius);

        Vector3 viewAngleA = DirFromAngle(-ViewAngle / 2, false);
        Vector3 viewAngleB = DirFromAngle(ViewAngle / 2, false);
        Gizmos.DrawLine(transform.position, transform.position + viewAngleA * ViewRadius);
        Gizmos.DrawLine(transform.position, transform.position + viewAngleB * ViewRadius);

        Gizmos.color = Color.red;
        foreach (Transform visible in VisibleTargets)
        {
            Gizmos.DrawLine(transform.position, visible.position);
        }
    }
}