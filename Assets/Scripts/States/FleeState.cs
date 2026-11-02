using UnityEngine;

public class FleeState : IState
{
    private HunterNPC _hunter;
    private float _waitTimer = 0f;
    private bool _inSafeZone = false;

    public FleeState(HunterNPC hunter) { _hunter = hunter; }

    public void Enter()
    {
        _inSafeZone = false;
        _waitTimer = 0f;

        Node closestSafeNode = null;
        float minDist = Mathf.Infinity;

        foreach (Node node in Graph.Instance.AllNodes)
        {
            if (node.IsSafeZone)
            {
                float dist = Vector3.Distance(_hunter.transform.position, node.Position);
                if (dist < minDist)
                {
                    minDist = dist;
                    closestSafeNode = node;
                }
            }
        }

        if (closestSafeNode != null)
        {

            if (minDist < 2f)
            {
                _inSafeZone = true;
                _hunter.CurrentPath.Clear(); 
            }
            else
            {
                _hunter.RequestPath(closestSafeNode.Position);
            }
        }
    }

    public void Execute()
    {
        if (!_inSafeZone)
        {
            bool reached = _hunter.FollowPath();
            if (reached) _inSafeZone = true;
        }
        else
        {
            bool wolfStillVisible = false;
            foreach (Transform target in _hunter.Perception.VisibleTargets)
            {
                if (target.gameObject.layer == LayerMask.NameToLayer("Wolf"))
                {
                    wolfStillVisible = true;
                    break;
                }
            }

            if (wolfStillVisible)
            {
                _waitTimer = 0f;
            }
            else
            {
                _waitTimer += Time.deltaTime;
                if (_waitTimer >= _hunter.SafeZoneWaitTime)
                {
                    if (_hunter.CurrentGathers >= _hunter.RequiredGathers)
                        _hunter.FSM.ChangeState(_hunter.ExitState);
                    else
                        _hunter.FSM.ChangeState(_hunter.SearchState);
                }
            }
        }
    }

    public void Exit() { }
}