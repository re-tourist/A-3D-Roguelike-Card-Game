using System.Collections.Generic;
using UnityEngine;

public static class CardExecutor
{
    /// <summary>
    /// 使用一张卡牌。targets 用于 SingleEnemy 类型；AllEnemies 情况下由具体 Effect 自己决定如何处理。
    /// </summary>
    public static void UseCard(CardInstance card, List<Combatant> targets)
    {
        foreach (var info in card.Data.effects)
        {
            CardEffect effect = EffectFactory.Create(info);

            switch (info.target)
            {
                case CardTarget.Self:
                    // 直接对玩家自身生效
                    effect.Execute(card, BattleContext.Player);
                    break;

                case CardTarget.SingleEnemy:
                    // 默认选第一个目标；未来可以做“选敌人 UI”之后把选中的传进来
                    Combatant first = (targets != null && targets.Count > 0) ? targets[0] : null;
                    effect.Execute(card, first);
                    break;

                case CardTarget.AllEnemies:
                    // 这里不要再遍历敌人列表，以免和 Effect_DamageAll 等重复
                    // 具体遍历逻辑交给具体的 Effect（例如内部使用 BattleContext.Enemies）
                    effect.Execute(card, null);
                    break;

                case CardTarget.None:
                default:
                    // 完全不依赖目标的效果（抽牌、加能量等）
                    effect.Execute(card, null);
                    break;
            }
        }
    }

    /// <summary>
    /// 单目标简易重载，方便调用。
    /// </summary>
    public static void UseCard(CardInstance card, Combatant singleTarget)
    {
        var list = singleTarget != null ? new List<Combatant> { singleTarget } : new List<Combatant>();
        UseCard(card, list);
    }
}
