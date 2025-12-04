using UnityEngine;

/// <summary>
/// 玩家战斗单位。
/// 负责：能量、格挡、下回合效果、双倍伤害标记等。
/// </summary>
public class PlayerCombatant : Combatant
{
    [Header("Energy")]
    public int maxEnergy = 3;
    public int energy = 3;

    // 下回合生效的预存效果
    int pendingNextTurnBlock = 0;
    int pendingNextTurnEnergy = 0;

    // 下回合攻击伤害翻倍（由某些卡牌效果设置）
    public bool DoubleNextTurnAttackDamage { get; set; }

    #region 回合相关接口（供 TurnManager / BattleManager 调用）

    /// <summary>
    /// 回合开始：恢复能量、应用“下回合能量”与“下回合格挡”
    /// </summary>
    public void OnTurnStart()
    {
        // 标准做法：先还原基础能量，再加上额外能量
        energy = maxEnergy + pendingNextTurnEnergy;
        pendingNextTurnEnergy = 0;

        // 下回合格挡：等回合开始时加入
        block += pendingNextTurnBlock;
        pendingNextTurnBlock = 0;

        // DoubleNextTurnAttackDamage 一般只持续一回合，到这里可以清空，
        // 也可以由触发它的效果自行管理生命周期
    }

    /// <summary>
    /// 回合结束：一般会清空临时格挡（和 STS 一样）
    /// </summary>
    public void OnTurnEnd()
    {
        block = 0;
    }

    #endregion

    #region 能量与格挡

    public virtual void GainBlock(int amount)
    {
        if (amount <= 0) return;

        block += amount;
        Debug.Log($"{displayName} 获得 {amount} 点格挡（当前：{block}）");
    }

    public virtual void GainEnergy(int amount)
    {
        if (amount == 0) return;

        energy += amount;
        Debug.Log($"{displayName} 获得 {amount} 点能量（当前：{energy}）");
    }

    public virtual void AddNextTurnBlock(int amount)
    {
        if (amount <= 0) return;

        pendingNextTurnBlock += amount;
        Debug.Log($"{displayName} 下回合预存格挡 +{amount}（累计：{pendingNextTurnBlock}）");
    }

    public virtual void AddNextTurnEnergy(int amount)
    {
        if (amount <= 0) return;

        pendingNextTurnEnergy += amount;
        Debug.Log($"{displayName} 下回合预存能量 +{amount}（累计：{pendingNextTurnEnergy}）");
    }

    #endregion

    public override void TakeDamage(int amount)
    {
        base.TakeDamage(amount);

        // TODO：这里可以加入：如果玩家 HP <= 0 通知 BattleManager → 结算失败
        // if (currentHP <= 0) BattleManager.Instance.OnPlayerDied();
    }
}
