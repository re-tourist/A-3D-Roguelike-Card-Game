using System.Collections.Generic;
using UnityEngine;

public class BattleManager : MonoBehaviour
{
    public static BattleManager Instance { get; private set; }

    public int HandCount => hand.Count;
    public bool HasDiscardedThisTurn { get; private set; }
    public int AttackCardsPlayedThisTurn { get; private set; }
    public bool NextSkillPlaysTwice { get; set; }

    readonly List<string> hand = new List<string>();
    readonly List<string> drawPile = new List<string>();
    readonly List<string> discardPile = new List<string>();

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void DrawCards(int count)
    {
        for (int i = 0; i < count; i++)
        {
            if (drawPile.Count > 0)
            {
                var id = drawPile[drawPile.Count - 1];
                drawPile.RemoveAt(drawPile.Count - 1);
                hand.Add(id);
            }
            else
            {
                hand.Add("DebugCard");
            }
        }
    }

    public void DiscardCards(int count)
    {
        int n = Mathf.Min(count, hand.Count);
        for (int i = 0; i < n; i++)
        {
            var id = hand[hand.Count - 1];
            hand.RemoveAt(hand.Count - 1);
            discardPile.Add(id);
        }
        HasDiscardedThisTurn = n > 0;
    }

    public void DiscardAllCards()
    {
        while (hand.Count > 0)
        {
            var id = hand[hand.Count - 1];
            hand.RemoveAt(hand.Count - 1);
            discardPile.Add(id);
        }
        HasDiscardedThisTurn = true;
    }

    public void AddCardToHand(string cardId)
    {
        hand.Add(cardId);
    }

    public void RetainCards(int count) { }

    public void SetAllHandCardCost(int cost) { }

    public void ReduceCostOfRandomCard(int delta) { }

    public void Scry(int count) { }

    public void MoveCardBetweenPiles(string cardId) { }

    public void ExhaustRandomCard()
    {
        if (hand.Count == 0) return;
        hand.RemoveAt(hand.Count - 1);
    }

    public void CopyCardToHand(string cardId)
    {
        hand.Add(cardId);
    }
}

