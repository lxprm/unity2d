using System.Linq;
using UnityEngine;

public class PlayerTurnState : BaseState
{
    /// <summary>
    /// Called when a state start.
    /// </summary>
    public override void Enter()
    {
        turnController.UpdateTimer();
        turnController.TurnEnded = false;
        turnController.TeamTurn = Team.BLUE;
        turnController.TurnIndicator.transform.position = new Vector3(Mathf.Abs(turnController.TurnIndicator.transform.position.x),
                                                                    turnController.TurnIndicator.transform.position.y,
                                                                    turnController.TurnIndicator.transform.position.z);
        GameController.Instance.UpdateRaycastPhysics(1 << LayerMask.NameToLayer("PlayerCard"));
    }

    /// <summary>
    /// Called when a state finish.
    /// </summary>
    public override void Exit()
    {
        GameController.Instance.UpdateRaycastPhysics(1 << LayerMask.NameToLayer("Nothing"));
    }

    /// <summary>
    /// Called by the state every frame.
    /// </summary>
    public override void UpdateLogic()
    {
        turnController.TurnTimer -= Time.deltaTime;
        turnController.UpdateTimerProgress();

        if (turnController.TurnTimer <= 0f)
        {
            BasicAI(Team.BLUE);
            if(!cardSelected.Dragging) cardSelected.UpdateOrderInLayer(300);
            slotSelected.PlaceCard(cardSelected);
        }

        if (turnController.TurnEnded)
        {
            if (GameController.Instance.Board.Slots.Any(slot => !slot.Occupied))
            {
                turnController.ChangeState(new EnemyTurnState());
            }
            else
            {
                turnController.ChangeState(new EndMatchState());
            }
        }
    }
}
