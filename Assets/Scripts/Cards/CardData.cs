// 定义卡牌的模板，包含基础信息和效果等

using System.Collections.Generic;
using UnityEngine;

public enum CardType { Attack, Skill, Power }
public enum CardRarity { Common, Uncommon, Rare }
public enum CardTarget { None, Self, SingleEnemy, AllEnemies }

public enum EffectType
{
    // A. 直接数值（基础战斗效果）
    Damage,                 // 对单体造成伤害
    DamageAll,              // 对所有敌人造成伤害（群体攻击）
    MultiHitDamage,         // 多段攻击（例如造成3×5伤害）
    Block,                  // 获得格挡
    BlockNextTurn,          // 下回合获得格挡（如“连续走位”第二段防御）

    // B. 状态相关（Buff / Debuff / 特殊状态）
    ApplyStatus,            // 通用状态系统（精神负担、虚弱、易伤等）
    MultiplyStatus,         // 翻倍或乘以状态（精神放大仪 / Catalyst）
    GainStrength,           // 获得力量（提升攻击伤害）
    GainDexterity,          // 获得敏捷（提升格挡量）

    // C. 卡牌流动类（抽牌、弃牌、生成卡牌等）
    DrawCards,              // 抽 X 张牌
    DiscardCards,           // 弃置手牌（可扩展：随机弃、选择弃）
    DiscardHandThenDraw,    // 弃掉所有手牌并抽等量（豪赌改稿）
    GenerateCard,           // 生成指定卡（幻影飞刃等）
    RetainCardsThisTurn,    // 本回合结束保留 X 张牌（剧本收藏夹）

    // D. 能量 / 费用（经济与资源）
    GainEnergy,             // 获得能量（立即）
    GainEnergyNextTurn,     // 下回合获得额外能量
    SetHandCostThisTurn,    // 本回合手牌费用 = N（子弹时间）
    ReduceCardCostThisCombat,// 某张牌本场战斗永久降费（预埋戏法）
    RefundEnergyIfCondition, // 若满足条件返还能量（偷改台词/Discard 触发）

    // E. 条件与连击（基于历史或条件的效果）
    DamagePerCardPlayedThisTurn,   // 本回合每打出一张攻击牌造成一次伤害（谢幕连斩）
    DamagePerStatusStack,          // 按状态层数造成伤害（凌迟）
    DoubleNextTurnAttackDamage,    // 下回合你的攻击伤害翻倍（幻象双倍）
    PlayNextSkillTwice,            // 下一张技能牌打出两次（咒语回响 / Burst）

    // F. 工具 / 控制（非核心机制，后续慢慢实现）
    Scry,                   // 洞察（查看/处理牌库顶若干张牌）
    MoveCardBetweenPiles,   // 手牌/抽牌堆/弃牌堆之间移动卡牌
    ExhaustRandomCardFromHand, // 随机消耗手牌中一张牌
    CopyCardToHand,         // 复制某张指定卡牌加入手牌（镜像戏法）
}


[System.Serializable]
public class EffectInfo
{
    public EffectType type;

    // 对 ApplyStatus / GenerateCard 等需要字符串 ID 的效果使用（比如 "精神负担"、"幻影飞刃"）
    public string statusOrCardId;

    // 数值：基础值 + 升级值
    public int baseAmount;       // 未升级时的数值
    public int upgradedAmount;   // 升级后的数值

    public CardTarget target;

    /// <summary>
    /// 根据卡牌是否升级，返回本效果当前应该使用的数值
    /// </summary>
    public int GetAmount(bool isUpgraded)
    {
        return isUpgraded ? upgradedAmount : baseAmount;
    }
}



[CreateAssetMenu(menuName = "Game/Card Data")]
public class CardData : ScriptableObject
{
    [Header("Base Info")]
    public string id;          // 唯一 ID（比如 "MindTear"）
    public string cardName;
    public Sprite artwork;
    public CardType cardType;
    public CardRarity rarity;

    [Header("Cost")]
    public int baseCost;
    public int upgradedCost;

    

    [Header("Flags")]
    public bool exhaustAfterUse;
    public bool isInnate;

    [Header("Effects Chain")]
    public List<EffectInfo> effects = new List<EffectInfo>();
}
