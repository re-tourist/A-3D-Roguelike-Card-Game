// 定义卡牌的实例，把“静态模板”和“会变的数值”分开。
// 会变的数值包括当前的成本、是否升级、是否被临时降费等。

public class CardInstance
{
    public CardData Data;      // 指向静态模板

    public int CurrentCost;    // 当前费用（可能被临时修改）
    public bool IsUpgraded;    // 是否使用升级版数值

    public bool IsFreeThisTurn; // 本回合是否视为 0 费（比如某些效果）

    public CardInstance(CardData data, bool upgraded = false)
    {
        Data = data;
        IsUpgraded = upgraded;

        CurrentCost = upgraded ? data.upgradedCost : data.baseCost;
        IsFreeThisTurn = false;
    }

    // 获取某个 Effect 在当前升级状态下应该使用的数值。
    public int GetEffectAmount(EffectInfo effectInfo)
    {
        return effectInfo.GetAmount(IsUpgraded);
    }
}
