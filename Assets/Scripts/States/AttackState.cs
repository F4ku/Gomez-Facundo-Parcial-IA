using UnityEngine;

public class AttackState : IState
{
    private HunterNPC _hunter;
    private BoidAgent _target;

    public AttackState(HunterNPC hunter) { _hunter = hunter; }

    public void Enter()
    {
        _target = null;
    }

    public void Execute()
    {

        _target = _hunter.GetVisibleBoid(requiresDead: false);

        if (_target == null)
        {

            _hunter.FSM.ChangeState(_hunter.SearchState);
            return;
        }

        float distance = Vector3.Distance(_hunter.transform.position, _target.transform.position);

        if (distance <= _hunter.MeleeAttackRadius)
        {
            PerformAttack("Melee");
        }
        else if (distance <= _hunter.RangeAttackRadius)
        {
            PerformAttack("Ranged");
        }
        else
        {

            _hunter.transform.position = Vector3.MoveTowards(_hunter.transform.position, _target.transform.position, _hunter.Speed * Time.deltaTime);
        }
    }

    private void PerformAttack(string type)
    {
        Debug.Log($"Ataque {type} exitoso al Boid!");

        _hunter.Shoot(_target.transform.position);

        _target.TakeDamage(100f);

        _hunter.TimeSinceLastAttack = 0f; 
        _hunter.FSM.ChangeState(_hunter.SearchState);
    }

    public void Exit() { }
}