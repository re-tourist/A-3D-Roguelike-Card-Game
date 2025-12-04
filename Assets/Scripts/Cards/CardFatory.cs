using System;

public static class EffectFactory
{
    /// <summary>
    /// 根据 EffectInfo.type 创建对应的效果实例。
    /// 约定：
    /// - 数值一律在 Effect 类中通过 info.GetAmount(card.IsUpgraded) 获取
    /// - statusOrCardId 用来传递状态ID或卡牌ID（ApplyStatus / GenerateCard 等）
    /// </summary>
    public static CardEffect Create(EffectInfo info)
    {
        return info.type switch
        {
            // A. 直接数值
            EffectType.Damage                   => new Effect_Damage(info),
            EffectType.DamageAll                => new Effect_DamageAll(info),
            EffectType.MultiHitDamage           => new Effect_MultiHitDamage(info),
            EffectType.Block                    => new Effect_Block(info),
            EffectType.BlockNextTurn            => new Effect_BlockNextTurn(info),

            // B. 状态相关
            EffectType.ApplyStatus              => new Effect_ApplyStatus(info),
            EffectType.MultiplyStatus           => new Effect_MultiplyStatus(info),
            EffectType.GainStrength             => new Effect_GainStrength(info),
            EffectType.GainDexterity            => new Effect_GainDexterity(info),

            // C. 卡牌流动
            EffectType.DrawCards                => new Effect_DrawCards(info),
            EffectType.DiscardCards             => new Effect_DiscardCards(info),
            EffectType.DiscardHandThenDraw      => new Effect_DiscardHandThenDraw(info),
            EffectType.GenerateCard             => new Effect_GenerateCard(info),
            EffectType.RetainCardsThisTurn      => new Effect_RetainCardsThisTurn(info),

            // D. 能量 / 费用
            EffectType.GainEnergy               => new Effect_GainEnergy(info),
            EffectType.GainEnergyNextTurn       => new Effect_GainEnergyNextTurn(info),
            EffectType.SetHandCostThisTurn      => new Effect_SetHandCostThisTurn(info),
            EffectType.ReduceCardCostThisCombat => new Effect_ReduceCardCostThisCombat(info),
            EffectType.RefundEnergyIfCondition  => new Effect_RefundEnergyIfCondition(info),

            // E. 条件与连击
            EffectType.DamagePerCardPlayedThisTurn => new Effect_DamagePerCardPlayedThisTurn(info),
            EffectType.DamagePerStatusStack        => new Effect_DamagePerStatusStack(info),
            EffectType.DoubleNextTurnAttackDamage  => new Effect_DoubleNextTurnAttackDamage(info),
            EffectType.PlayNextSkillTwice          => new Effect_PlayNextSkillTwice(info),

            // F. 工具 / 控制
            EffectType.Scry                      => new Effect_Scry(info),
            EffectType.MoveCardBetweenPiles      => new Effect_MoveCardBetweenPiles(info),
            EffectType.ExhaustRandomCardFromHand => new Effect_ExhaustRandomCardFromHand(info),
            EffectType.CopyCardToHand            => new Effect_CopyCardToHand(info),

            _ => throw new Exception("未实现的 EffectType: " + info.type)
        };
    }
}
