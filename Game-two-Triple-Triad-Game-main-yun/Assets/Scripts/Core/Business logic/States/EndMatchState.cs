using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Localization.Settings;

public class EndMatchState : BaseState
{
    private GameController gameController;
    private float timeToShow = 0f;
    private bool draw = false;

    /// <summary>
    /// Called when a state start.
    /// </summary>
    public override void Enter()
    {
        gameController = GameController.Instance;
        timeToShow = 0f;
        draw = false;
        gameController.UpdateRaycastPhysics(1 << LayerMask.NameToLayer("Nothing"));
        turnController.TurnIndicator.color = new CustomColorAttribute("#00000000").HexadecimalToRGBColor();
        turnController.UpdateTimer();

        int blueScore = gameController.GameCards.Where(card => card.Team == Team.BLUE).Count();

        if (blueScore > gameController.GameCards.Count / 2)
        {
            turnController.UpdateTurnWindow(GenericAttribute.GetAttribute<CustomColorAttribute>(Team.BLUE).HexadecimalToRGBColor(), LocalizationSettings.StringDatabase.GetLocalizedString("Game", "BlueWin"));
            string idCardUnlocked = UnlockEnemyCard();

            if (idCardUnlocked != string.Empty)
            {
                Player.Instance.UnlockedIdCards.Add(idCardUnlocked);
                DataController.Instance.SavePlayerData();
            }

        }
        else if (blueScore < gameController.GameCards.Count / 2)
        {
            turnController.UpdateTurnWindow(GenericAttribute.GetAttribute<CustomColorAttribute>(Team.RED).HexadecimalToRGBColor(), LocalizationSettings.StringDatabase.GetLocalizedString("Game", "RedWin"));
        }
        else
        {
            draw = true;
            turnController.UpdateTurnWindow(new CustomColorAttribute("#131313").HexadecimalToRGBColor(), LocalizationSettings.StringDatabase.GetLocalizedString("Game", "Draw"));
        }
    }

    /// <summary>
    /// Called when a state finish.
    /// </summary>
    public override void Exit()
    {
        gameController.GameCards.Where(card => card.Placed).ToList().ForEach(card =>
        {
            Transform cardPosition = GameController.Instance.CardPositions.Where(position =>
                                                                                position.Team == card.Team
                                                                                && position.transform.childCount == 0)
                                                                            .FirstOrDefault().transform;
            card.CardData.Power = card.OriginalPower;
            card.UpdatePowerText(Power.NORMAL);
            card.Placed = false;
            card.startPosition = cardPosition.transform.position;
            card.transform.SetParent(cardPosition);
            card.transform.position = card.startPosition;
        });

        gameController.Board.Slots.ForEach(slot => slot.Occupied = false);
    }

    /// <summary>
    /// Called by the state every frame.
    /// </summary>
    public override void UpdateLogic()
    {
        timeToShow += Time.deltaTime;

        if (timeToShow > 1.5f)
        {
            turnController.UpdateTurnVisibility(1f);
        }

        if (timeToShow > 4f)
        {
            if (draw && DataController.Instance.SettingData.SuddenDeathRule)
            {
                turnController.ChangeState(new StartMatchState());
            }
            else
            {
                turnController.UpdateTurnVisibility(1f, true);
            }
        }
    }

    /// <summary>
    /// Unlock a random card from the database that the player have not obtained.
    /// </summary>
    /// <returns>The id of the card unlocked.</returns>
    public string UnlockRandomCard()
    {

        string cardId = string.Empty;
        List<CardData> card = CardDatabase.Instance.GetAllElement().Where(card => !Player.Instance.UnlockedIdCards.Contains(card.Id)).ToList();

        if (card.Count != 0)
        {
            cardId = card[new System.Random().Next(card.Count)].Id;
        }

        return cardId;
    }

    /// <summary>
    /// Unlock a random card from the enemy deck that the player have not obtained.
    /// </summary>
    /// <returns>The id of the card unlocked.</returns>
    public string UnlockEnemyCard()
    {
        string cardId = string.Empty;
        List<Card> card = GameController.Instance.GameCards.Where(card => card.Team == Team.RED && !Player.Instance.UnlockedIdCards.Contains(card.CardData.Id)).ToList();

        if (card.Count != 0)
        {
            cardId = card[new System.Random().Next(card.Count)].CardData.Id;
        }

        return cardId;
    }
}
