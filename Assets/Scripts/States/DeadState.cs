using UnityEngine;

public class DeadState : IState
{
    private HunterNPC _hunter;
    private float _timer = 0f;

    public DeadState(HunterNPC hunter) { _hunter = hunter; }

    public void Enter()
    {
        _timer = 0f;
        _hunter.CurrentPath.Clear(); 
    }

    public void Execute()
    {
        _timer += Time.deltaTime;
        if (_timer >= _hunter.RespawnTime)
        {
            _hunter.Health = 100f;
            _hunter.TimeSinceLastAttack = _hunter.TBA;

            _hunter.transform.position = _hunter.SpawnPoint.position;
            _hunter.Perception.VisibleTargets.Clear();

            _hunter.FSM.ChangeState(_hunter.SearchState);
        }
    }

    public void Exit() { }
}