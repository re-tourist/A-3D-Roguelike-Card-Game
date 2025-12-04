using UnityEngine;

/// --------------------------
/// 基类：所有效果继承它
/// --------------------------
/// 约定：
/// - 数值一律通过 info.GetAmount(card.IsUpgraded) 获取
/// - 字符串 ID 一律用 info.statusOrCardId（状态ID / 卡牌ID等）
/// --------------------------
public abstract class CardEffect
{
    protected EffectInfo info;

    protected CardEffect(EffectInfo info)
    {
        this.info = info;
    }

    public abstract void Execute(CardInstance card, object rawTarget);
}


/// --------------------------
/// A 区：直接数值类
/// --------------------------

public class Effect_Damage : CardEffect
{
    public Effect_Damage(EffectInfo info) : base(info) { }

    public override void Execute(CardInstance card, object rawTarget)
    {
        EnemyCombatant enemy = rawTarget as EnemyCombatant;
        if (enemy == null) return;

        int dmg = info.GetAmount(card.IsUpgraded);   // 后续可加入力量修正
        enemy.TakeDamage(dmg);
    }
}

public class Effect_DamageAll : CardEffect
{
    public Effect_DamageAll(EffectInfo info) : base(info) { }

    public override void Execute(CardInstance card, object rawTarget)
    {
        int dmg = info.GetAmount(card.IsUpgraded);

        foreach (var e in BattleContext.Enemies)
            e.TakeDamage(dmg);
    }
}

public class Effect_MultiHitDamage : CardEffect
{
    public Effect_MultiHitDamage(EffectInfo info) : base(info) { }

    public override void Execute(CardInstance card, object rawTarget)
    {
        EnemyCombatant enemy = rawTarget as EnemyCombatant;
        if (enemy == null) return;

        int perHitDamage = info.GetAmount(card.IsUpgraded);
        enemy.TakeDamage(perHitDamage);
    }
}

public class Effect_Block : CardEffect
{
    public Effect_Block(EffectInfo info) : base(info) { }

    public override void Execute(CardInstance card, object rawTarget)
    {
        int block = info.GetAmount(card.IsUpgraded);
        BattleContext.Player.GainBlock(block);
    }
}

public class Effect_BlockNextTurn : CardEffect
{
    public Effect_BlockNextTurn(EffectInfo info) : base(info) { }

    public override void Execute(CardInstance card, object rawTarget)
    {
        int block = info.GetAmount(card.IsUpgraded);
        // 下回合获得格挡 → 放入 Player 的“下回合效果列表”
        BattleContext.Player.AddNextTurnBlock(block);
    }
}


/// --------------------------
/// B 区：状态相关
/// --------------------------
public class Effect_ApplyStatus : CardEffect
{
    public Effect_ApplyStatus(EffectInfo info) : base(info) { }

    public override void Execute(CardInstance card, object rawTarget)
    {
        Combatant target = rawTarget as Combatant;
        if (target == null) return;

        int stacks = info.GetAmount(card.IsUpgraded);
        string statusId = info.statusOrCardId;  // e.g. "精神负担", "心理动摇"

        target.AddStatus(statusId, stacks);
    }
}

public class Effect_MultiplyStatus : CardEffect
{
    public Effect_MultiplyStatus(EffectInfo info) : base(info) { }

    public override void Execute(CardInstance card, object rawTarget)
    {
        Combatant target = rawTarget as Combatant;
        if (target == null) return;

        int factor = info.GetAmount(card.IsUpgraded); // 2 = 翻倍, 3 = 三倍...
        string statusId = info.statusOrCardId;

        target.MultiplyStatus(statusId, factor);
    }
}

public class Effect_GainStrength : CardEffect
{
    public Effect_GainStrength(EffectInfo info) : base(info) { }

    public override void Execute(CardInstance card, object rawTarget)
    {
        int amount = info.GetAmount(card.IsUpgraded);
        BattleContext.Player.AddStatus("力量", amount);
    }
}

public class Effect_GainDexterity : CardEffect
{
    public Effect_GainDexterity(EffectInfo info) : base(info) { }

    public override void Execute(CardInstance card, object rawTarget)
    {
        int amount = info.GetAmount(card.IsUpgraded);
        BattleContext.Player.AddStatus("敏捷", amount);
    }
}


/// --------------------------
/// C 区：卡牌流动（抽、弃、生成）
/// --------------------------

public class Effect_DrawCards : CardEffect
{
    public Effect_DrawCards(EffectInfo info) : base(info) { }

    public override void Execute(CardInstance card, object rawTarget)
    {
        int count = info.GetAmount(card.IsUpgraded);
        BattleManager.Instance.DrawCards(count);
    }
}

public class Effect_DiscardCards : CardEffect
{
    public Effect_DiscardCards(EffectInfo info) : base(info) { }

    public override void Execute(CardInstance card, object rawTarget)
    {
        int count = info.GetAmount(card.IsUpgraded);
        // 这里可以以后扩展：随机弃牌 / 选择弃牌
        BattleManager.Instance.DiscardCards(count);
    }
}

public class Effect_DiscardHandThenDraw : CardEffect
{
    public Effect_DiscardHandThenDraw(EffectInfo info) : base(info) { }

    public override void Execute(CardInstance card, object rawTarget)
    {
        int count = BattleManager.Instance.HandCount;
        BattleManager.Instance.DiscardAllCards();
        BattleManager.Instance.DrawCards(count);
    }
}

public class Effect_GenerateCard : CardEffect
{
    public Effect_GenerateCard(EffectInfo info) : base(info) { }

    public override void Execute(CardInstance card, object rawTarget)
    {
        int count = info.GetAmount(card.IsUpgraded);
        string cardId = info.statusOrCardId;  // 在这里用作 cardId

        for (int i = 0; i < count; i++)
            BattleManager.Instance.AddCardToHand(cardId);
    }
}

public class Effect_RetainCardsThisTurn : CardEffect
{
    public Effect_RetainCardsThisTurn(EffectInfo info) : base(info) { }

    public override void Execute(CardInstance card, object rawTarget)
    {
        int count = info.GetAmount(card.IsUpgraded);
        BattleManager.Instance.RetainCards(count);
    }
}


/// --------------------------
/// D 区：能量/费用
/// --------------------------
public class Effect_GainEnergy : CardEffect
{
    public Effect_GainEnergy(EffectInfo info) : base(info) { }

    public override void Execute(CardInstance card, object rawTarget)
    {
        int energy = info.GetAmount(card.IsUpgraded);
        BattleContext.Player.GainEnergy(energy);
    }
}

public class Effect_GainEnergyNextTurn : CardEffect
{
    public Effect_GainEnergyNextTurn(EffectInfo info) : base(info) { }

    public override void Execute(CardInstance card, object rawTarget)
    {
        int energy = info.GetAmount(card.IsUpgraded);
        BattleContext.Player.AddNextTurnEnergy(energy);
    }
}

public class Effect_SetHandCostThisTurn : CardEffect
{
    public Effect_SetHandCostThisTurn(EffectInfo info) : base(info) { }

    public override void Execute(CardInstance card, object rawTarget)
    {
        int cost = info.GetAmount(card.IsUpgraded);
        BattleManager.Instance.SetAllHandCardCost(cost);
    }
}

public class Effect_ReduceCardCostThisCombat : CardEffect
{
    public Effect_ReduceCardCostThisCombat(EffectInfo info) : base(info) { }

    public override void Execute(CardInstance card, object rawTarget)
    {
        int delta = info.GetAmount(card.IsUpgraded);
        // 这里你后面可以做“选一张牌降费”的交互；目前先随便找一张
        BattleManager.Instance.ReduceCostOfRandomCard(delta);
    }
}

public class Effect_RefundEnergyIfCondition : CardEffect
{
    public Effect_RefundEnergyIfCondition(EffectInfo info) : base(info) { }

    public override void Execute(CardInstance card, object rawTarget)
    {
        int energy = info.GetAmount(card.IsUpgraded);

        if (BattleManager.Instance.HasDiscardedThisTurn)
            BattleContext.Player.GainEnergy(energy);
    }
}


/// --------------------------
/// E 区：条件类 / 连击类
/// --------------------------

public class Effect_DamagePerCardPlayedThisTurn : CardEffect
{
    public Effect_DamagePerCardPlayedThisTurn(EffectInfo info) : base(info) { }

    public override void Execute(CardInstance card, object rawTarget)
    {
        EnemyCombatant enemy = rawTarget as EnemyCombatant;
        if (enemy == null) return;

        int dmgPer = info.GetAmount(card.IsUpgraded);
        int times = BattleManager.Instance.AttackCardsPlayedThisTurn;

        for (int i = 0; i < times; i++)
            enemy.TakeDamage(dmgPer);
    }
}

public class Effect_DamagePerStatusStack : CardEffect
{
    public Effect_DamagePerStatusStack(EffectInfo info) : base(info) { }

    public override void Execute(CardInstance card, object rawTarget)
    {
        EnemyCombatant enemy = rawTarget as EnemyCombatant;
        if (enemy == null) return;

        int dmgPerStack = info.GetAmount(card.IsUpgraded);
        string statusId = info.statusOrCardId;

        int stacks = enemy.GetStatusStacks(statusId);
        enemy.TakeDamage(stacks * dmgPerStack);
    }
}

public class Effect_DoubleNextTurnAttackDamage : CardEffect
{
    public Effect_DoubleNextTurnAttackDamage(EffectInfo info) : base(info) { }

    public override void Execute(CardInstance card, object rawTarget)
    {
        BattleContext.Player.DoubleNextTurnAttackDamage = true;
    }
}

public class Effect_PlayNextSkillTwice : CardEffect
{
    public Effect_PlayNextSkillTwice(EffectInfo info) : base(info) { }

    public override void Execute(CardInstance card, object rawTarget)
    {
        BattleManager.Instance.NextSkillPlaysTwice = true;
    }
}


/// --------------------------
/// F 区：工具类（非核心）
/// --------------------------

public class Effect_Scry : CardEffect
{
    public Effect_Scry(EffectInfo info) : base(info) { }

    public override void Execute(CardInstance card, object rawTarget)
    {
        int count = info.GetAmount(card.IsUpgraded);
        BattleManager.Instance.Scry(count);
    }
}

public class Effect_MoveCardBetweenPiles : CardEffect
{
    public Effect_MoveCardBetweenPiles(EffectInfo info) : base(info) { }

    public override void Execute(CardInstance card, object rawTarget)
    {
        string cardId = info.statusOrCardId;
        BattleManager.Instance.MoveCardBetweenPiles(cardId);
    }
}

public class Effect_ExhaustRandomCardFromHand : CardEffect
{
    public Effect_ExhaustRandomCardFromHand(EffectInfo info) : base(info) { }

    public override void Execute(CardInstance card, object rawTarget)
    {
        BattleManager.Instance.ExhaustRandomCard();
    }
}

public class Effect_CopyCardToHand : CardEffect
{
    public Effect_CopyCardToHand(EffectInfo info) : base(info) { }

    public override void Execute(CardInstance card, object rawTarget)
    {
        string cardId = info.statusOrCardId;
        BattleManager.Instance.CopyCardToHand(cardId);
    }
}
