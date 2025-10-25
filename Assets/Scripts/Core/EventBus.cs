using System;
using System.Collections.Generic;

/// <summary>
/// ȫ���¼����ߣ���̬�ࣩ
/// ģ���ͨ���¼�����ͨ�ţ�����ֱ��������
/// </summary>
public static class EventBus
{
    private static readonly Dictionary<string, Action<object>> eventTable = new();

    /// <summary>
    /// �����¼�
    /// </summary>
    public static void Subscribe(string eventName, Action<object> callback)
    {
        if (!eventTable.ContainsKey(eventName))
            eventTable[eventName] = delegate { };
        eventTable[eventName] += callback;
    }

    /// <summary>
    /// ȡ�������¼�
    /// </summary>
    public static void Unsubscribe(string eventName, Action<object> callback)
    {
        if (eventTable.ContainsKey(eventName))
            eventTable[eventName] -= callback;
    }

    /// <summary>
    /// �����¼�
    /// </summary>
    public static void Publish(string eventName, object data = null)
    {
        if (eventTable.ContainsKey(eventName))
            eventTable[eventName]?.Invoke(data);
    }

    /// <summary>
    /// ��������¼������ڳ�������ʱʹ�ã�
    /// </summary>
    public static void Clear() => eventTable.Clear();
}
