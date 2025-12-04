using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 战斗单位基类：玩家和敌人都继承它。
/// 负责：血量、格挡、状态容器等共性逻辑。
/// </summary>
public class Combatant : MonoBehaviour
{
    [Header("Identity")]
    public string displayName = "Unit";

    [Header("HP & Block")]
    public int maxHP = 50;
    public int currentHP = 50;
    public int block = 0;

    // 简单版状态容器：用字典替代文档里的 StatusContainer
    Dictionary<string, int> statuses = new Dictionary<string, int>();

    #region 状态相关

    /// <summary>
    /// 施加状态（精神负担、心理动摇等）
    /// </summary>
    public virtual void AddStatus(string statusId, int amount)
    {
        if (amount == 0 || string.IsNullOrEmpty(statusId)) return;

        if (!statuses.TryGetValue(statusId, out int current))
            current = 0;

        int newValue = Mathf.Max(current + amount, 0);
        statuses[statusId] = newValue;

        Debug.Log($"{displayName} 获得状态 {statusId} x{amount}（当前：{newValue}）");
    }

    /// <summary>
    /// 获取某个状态的层数
    /// </summary>
    public virtual int GetStatusStacks(string statusId)
    {
        return statuses.TryGetValue(statusId, out int value) ? value : 0;
    }

    /// <summary>
    /// 将某个状态层数按倍数放大（例如精神负担翻倍/乘3）
    /// </summary>
    public virtual void MultiplyStatus(string statusId, int factor)
    {
        if (factor <= 1) return;

        int current = GetStatusStacks(statusId);
        int newValue = current * factor;
        statuses[statusId] = newValue;

        Debug.Log($"{displayName} 的状态 {statusId} 乘以 {factor} → {newValue}");
    }

    #endregion

    #region 伤害与死亡

    /// <summary>
    /// 计算伤害：先吃格挡，再扣血。
    /// Enemy / Player 都共用这套公式。
    /// </summary>
    public virtual void TakeDamage(int amount)
    {
        if (amount <= 0) return;

        int damageAfterBlock = Mathf.Max(amount - block, 0);
        block = Mathf.Max(block - amount, 0);

        currentHP -= damageAfterBlock;
        Debug.Log($"{displayName} 受到 {amount} 点伤害（实际伤害 {damageAfterBlock}，剩余 HP = {currentHP}，Block = {block}）");

        if (currentHP <= 0)
            Die();
    }

    protected virtual void Die()
    {
        Debug.Log($"{displayName} 死亡");
        // 具体的“移出战斗 / 播放动画 / 结算”等留给子类或 BattleManager 处理
    }

    #endregion
}
