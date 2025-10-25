using System;
using System.Collections.Generic;

/// <summary>
/// 全局事件总线（静态类）
/// 模块间通过事件驱动通信，避免直接依赖。
/// </summary>
public static class EventBus
{
    private static readonly Dictionary<string, Action<object>> eventTable = new();

    /// <summary>
    /// 订阅事件
    /// </summary>
    public static void Subscribe(string eventName, Action<object> callback)
    {
        if (!eventTable.ContainsKey(eventName))
            eventTable[eventName] = delegate { };
        eventTable[eventName] += callback;
    }

    /// <summary>
    /// 取消订阅事件
    /// </summary>
    public static void Unsubscribe(string eventName, Action<object> callback)
    {
        if (eventTable.ContainsKey(eventName))
            eventTable[eventName] -= callback;
    }

    /// <summary>
    /// 触发事件
    /// </summary>
    public static void Publish(string eventName, object data = null)
    {
        if (eventTable.ContainsKey(eventName))
            eventTable[eventName]?.Invoke(data);
    }

    /// <summary>
    /// 清空所有事件（可在场景重置时使用）
    /// </summary>
    public static void Clear() => eventTable.Clear();
}
