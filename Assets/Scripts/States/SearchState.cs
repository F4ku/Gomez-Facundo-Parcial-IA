using UnityEngine;

public class SearchState : IState
{
    private HunterNPC _hunter;
    private float _spawnTimer = 0f;
    private bool _inHuntingZone = false;
    private float _wanderTimer = 0f;

    public SearchState(HunterNPC hunter) { _hunter = hunter; }

    public void Enter()
    {
        _inHuntingZone = false;
        _hunter.RequestPath(_hunter.HuntingZoneCenter.position);
    }

    public void Execute()
    {

        foreach (Transform visible in _hunter.Perception.VisibleTargets)
        {
            if (visible.gameObject.layer == LayerMask.NameToLayer("Wolf"))
            {
                _hunter.FSM.ChangeState(_hunter.FleeState);
                return;
            }
        }
        if (_hunter.GetVisibleBoid(requiresDead: true) != null)
        {
            _hunter.FSM.ChangeState(_hunter.GatherState);
            return;
        }

        if (_hunter.TimeSinceLastAttack >= _hunter.TBA && _hunter.GetVisibleBoid(requiresDead: false) != null)
        {
            _hunter.FSM.ChangeState(_hunter.AttackState);
            return;
        }


        bool reachedDestination = _hunter.FollowPath();

        if (reachedDestination)
        {
            _inHuntingZone = true;

            _wanderTimer -= Time.deltaTime;
            if (_wanderTimer <= 0)
            {
                _wanderTimer = 3f; 
                Vector3 randomTarget = _hunter.HuntingZoneCenter.position + new Vector3(Random.Range(-8f, 8f), 0, Random.Range(-8f, 8f));
                _hunter.RequestPath(randomTarget);
            }
        }

        if (_inHuntingZone)
        {
            _spawnTimer += Time.deltaTime;
            if (_spawnTimer >= 5f && _hunter.ActiveObjects.Count < 5)
            {
                _spawnTimer = 0;
                GameObject newObj = GameObject.Instantiate(_hunter.ObjectOfInterestPrefab, _hunter.transform.position, Quaternion.identity);
                _hunter.ActiveObjects.Add(newObj);
            }
        }
    }

    public void Exit() { }
}