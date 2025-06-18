using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GameController : MonoBehaviour
{
    public static GameController Instance { get; private set; }
    [field: SerializeField] public Board Board { get; set; }
    [field: SerializeField] public List<Card> GameCards { get; set; } = new List<Card>();
    [field: SerializeField] public List<DeckCardPosition> CardPositions { get; set; } = new List<DeckCardPosition>();
    public Action<int, int> OnSlotUpdated { get; set; }
    public Action<LayerMask> OnRaycastUpdated { get; set; }

    /// <summary>
    /// First method that will be called when a script is enabled. Only called once.
    /// </summary>
    private void Awake()
    {
        Instance = this;
    }

    /// <summary>
    /// Start is called on the frame when a script is enabled just before any of the Update methods are called the first time. Only called once.
    /// </summary>
    private void Start()
    {
        UpdateScore();
        Board.Slots.Shuffle();
        GameCards.Shuffle();

        if (DataController.Instance.SettingData.RandomRule)
        {
            GameCards.ForEach(card => card.CardData = CardDatabase.Instance.GetRandomElement());
        }
        else
        {
            List<Card> blueCards = GameCards.Where(card => card.Team == Team.BLUE).ToList();

            for (int i = 0; i < blueCards.Count; i++)
            {
                blueCards[i].CardData = CardDatabase.Instance.FindElementById(Player.Instance.CurrentDeck.CardIds[i]);
            }

            GameCards.Except(blueCards).ToList().ForEach(card => card.CardData = CardDatabase.Instance.GetRandomElement());
        }

        TurnController.Instance.ChangeState(new StartMatchState());
    }

    /// <summary>
    /// Update the score when a slot is updated.
    /// </summary>
    public void UpdateScore()
    {
        if (OnSlotUpdated != null)
        {
            int blueScore = GameCards.Where(card => card.Team == Team.BLUE).Count();
            OnSlotUpdated(blueScore, GameCards.Count - blueScore);
        }
    }

    /// <summary>
    /// Update the raycast physics when it is updated.
    /// </summary>
    /// <param name="layer">The layer which we will interact.</param>
    public void UpdateRaycastPhysics(LayerMask layer)
    {
        if (OnRaycastUpdated != null)
        {
            OnRaycastUpdated(layer);
        }
    }
}
