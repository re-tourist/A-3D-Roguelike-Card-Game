using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CardTestRunner : MonoBehaviour
{
    public CardData cardToTest;
    public EnemyCombatant dummyEnemy;
    public PlayerCombatant dummyPlayer;
    public int simulateTurns = 5;
    public float turnInterval = 0.5f;
    public string poisonStatusId = "精神侵蚀";

    private void Start()
    {
        BattleContext.Player = dummyPlayer;
        BattleContext.Enemies = new List<EnemyCombatant> { dummyEnemy };

        var instance = new CardInstance(cardToTest, upgraded: false);
        CardExecutor.UseCard(instance, new List<Combatant> { dummyEnemy });

        StartCoroutine(SimulateTurns());
    }

    IEnumerator SimulateTurns()
    {
        for (int t = 1; t <= simulateTurns; t++)
        {
            Debug.Log($"—— 开始第 {t} 回合 ——");

            if (BattleContext.Player != null)
                BattleContext.Player.OnTurnStart();

            foreach (var e in BattleContext.Enemies)
            {
                if (e == null) continue;
                int stacks = e.GetStatusStacks(poisonStatusId);
                if (stacks > 0)
                {
                    int hpBefore = e.currentHP;
                    e.currentHP = hpBefore - stacks;
                    Debug.Log($"[精神侵蚀] {e.displayName} 层数 {stacks}，扣除 {stacks} 生命，HP {hpBefore} → {e.currentHP}");
                    e.AddStatus(poisonStatusId, -1);
                    Debug.Log($"[精神侵蚀] 层数减少 1 → {e.GetStatusStacks(poisonStatusId)}");
                }
                else
                {
                    Debug.Log($"[精神侵蚀] {e.displayName} 当前无层数");
                }
            }

            if (BattleContext.Player != null)
                BattleContext.Player.OnTurnEnd();

            Debug.Log("—— 回合结束 ——");
            yield return new WaitForSeconds(turnInterval);
        }

        Debug.Log("多回合测试结束");
    }
}
