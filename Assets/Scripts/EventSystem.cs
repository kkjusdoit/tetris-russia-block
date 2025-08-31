using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 简单的事件系统，支持泛型事件和参数传递
/// </summary>
public static class EventSystem
{
    // 存储所有事件的字典，key是事件类型，value是事件委托列表
    private static Dictionary<Type, List<Delegate>> eventDictionary = new Dictionary<Type, List<Delegate>>();

    /// <summary>
    /// 注册事件监听器
    /// </summary>
    /// <typeparam name="T">事件类型</typeparam>
    /// <param name="action">事件处理函数</param>
    public static void Register<T>(Action<T> action) where T : struct
    {
        Type eventType = typeof(T);
        
        if (!eventDictionary.ContainsKey(eventType))
        {
            eventDictionary[eventType] = new List<Delegate>();
        }
        
        if (!eventDictionary[eventType].Contains(action))
        {
            eventDictionary[eventType].Add(action);
        }
    }

    /// <summary>
    /// 注册无参数事件监听器
    /// </summary>
    /// <typeparam name="T">事件类型</typeparam>
    /// <param name="action">事件处理函数</param>
    public static void Register<T>(Action action) where T : struct
    {
        Type eventType = typeof(T);
        
        if (!eventDictionary.ContainsKey(eventType))
        {
            eventDictionary[eventType] = new List<Delegate>();
        }
        
        if (!eventDictionary[eventType].Contains(action))
        {
            eventDictionary[eventType].Add(action);
        }
    }

    /// <summary>
    /// 取消注册事件监听器
    /// </summary>
    /// <typeparam name="T">事件类型</typeparam>
    /// <param name="action">事件处理函数</param>
    public static void Unregister<T>(Action<T> action) where T : struct
    {
        Type eventType = typeof(T);
        
        if (eventDictionary.ContainsKey(eventType))
        {
            eventDictionary[eventType].Remove(action);
            
            if (eventDictionary[eventType].Count == 0)
            {
                eventDictionary.Remove(eventType);
            }
        }
    }

    /// <summary>
    /// 取消注册无参数事件监听器
    /// </summary>
    /// <typeparam name="T">事件类型</typeparam>
    /// <param name="action">事件处理函数</param>
    public static void Unregister<T>(Action action) where T : struct
    {
        Type eventType = typeof(T);
        
        if (eventDictionary.ContainsKey(eventType))
        {
            eventDictionary[eventType].Remove(action);
            
            if (eventDictionary[eventType].Count == 0)
            {
                eventDictionary.Remove(eventType);
            }
        }
    }

    /// <summary>
    /// 触发事件
    /// </summary>
    /// <typeparam name="T">事件类型</typeparam>
    /// <param name="eventData">事件数据</param>
    public static void Trigger<T>(T eventData) where T : struct
    {
        Type eventType = typeof(T);
        
        if (eventDictionary.ContainsKey(eventType))
        {
            // 创建副本以避免在事件处理过程中修改集合
            List<Delegate> actions = new List<Delegate>(eventDictionary[eventType]);
            
            foreach (Delegate action in actions)
            {
                if (action is Action<T> typedAction)
                {
                    try
                    {
                        typedAction(eventData);
                    }
                    catch (Exception e)
                    {
//                         Debug.LogError($"Error triggering event {eventType.Name}: {e.Message}");
                    }
                }
            }
        }
    }

    /// <summary>
    /// 触发无参数事件
    /// </summary>
    /// <typeparam name="T">事件类型</typeparam>
    public static void Trigger<T>() where T : struct
    {
        Type eventType = typeof(T);
        
        if (eventDictionary.ContainsKey(eventType))
        {
            // 创建副本以避免在事件处理过程中修改集合
            List<Delegate> actions = new List<Delegate>(eventDictionary[eventType]);
            
            foreach (Delegate action in actions)
            {
                if (action is Action typedAction)
                {
                    try
                    {
                        typedAction();
                    }
                    catch (Exception e)
                    {
//                         Debug.LogError($"Error triggering event {eventType.Name}: {e.Message}");
                    }
                }
            }
        }
    }

    /// <summary>
    /// 清除所有事件
    /// </summary>
    public static void ClearAll()
    {
        eventDictionary.Clear();
    }

    /// <summary>
    /// 清除指定类型的所有事件
    /// </summary>
    /// <typeparam name="T">事件类型</typeparam>
    public static void Clear<T>() where T : struct
    {
        Type eventType = typeof(T);
        if (eventDictionary.ContainsKey(eventType))
        {
            eventDictionary.Remove(eventType);
        }
    }

    /// <summary>
    /// 获取指定类型事件的监听器数量
    /// </summary>
    /// <typeparam name="T">事件类型</typeparam>
    /// <returns>监听器数量</returns>
    public static int GetListenerCount<T>() where T : struct
    {
        Type eventType = typeof(T);
        if (eventDictionary.ContainsKey(eventType))
        {
            return eventDictionary[eventType].Count;
        }
        return 0;
    }
} 