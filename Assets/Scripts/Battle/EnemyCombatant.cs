using UnityEngine;

/// <summary>
/// 敌人战斗单位。
/// 当前只是在 Combatant 的基础上，扩展死亡时的行为：
/// - 从 BattleContext.Enemies 中移除自身
/// - 未来可以在这里调用 EnemyAI / 掉落逻辑
/// </summary>
public class EnemyCombatant : Combatant
{
    protected override void Die()
    {
        base.Die();

        // 从全局战斗上下文里移除，避免后续卡牌还打死者
        if (BattleContext.Enemies.Contains(this))
            BattleContext.Enemies.Remove(this);

        // TODO：通知 BattleManager 检查战斗是否结束（全体敌人死亡）
        // BattleManager.Instance.OnEnemyDied(this);
    }

    // 如果你以后需要给敌人专属的加护盾之类，也可以在这里写额外方法
}
