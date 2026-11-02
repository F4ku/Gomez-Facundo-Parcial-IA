using UnityEngine;

public class ExitState : IState
{
    private HunterNPC _hunter;

    public ExitState(HunterNPC hunter) { _hunter = hunter; }

    public void Enter()
    {
        _hunter.RequestPath(_hunter.ExitPoint.position);
    }

    public void Execute()
    {
        bool reachedExit = _hunter.FollowPath();

        if (reachedExit)
        {
            Debug.Log("¡Cazador escapó exitosamente! Reiniciando ciclo...");

            _hunter.CurrentGathers = 0;
            _hunter.Health = 100f;
            _hunter.TimeSinceLastAttack = _hunter.TBA;

            _hunter.CurrentPath.Clear(); 
            _hunter.Perception.VisibleTargets.Clear(); 

            _hunter.transform.position = _hunter.SpawnPoint.position;
            _hunter.FSM.ChangeState(_hunter.SearchState);
        }
    }

    public void Exit() { }
}