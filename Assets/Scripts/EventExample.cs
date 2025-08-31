using UnityEngine;

/// <summary>
/// 事件系统使用示例
/// </summary>
public class EventExample : MonoBehaviour
{
    void Start()
    {
        // 注册事件监听器
        RegisterEvents();
        
        // 触发一些示例事件
        TriggerExampleEvents();
    }

    void OnDestroy()
    {
        // 取消注册事件监听器
        UnregisterEvents();
    }

    private void RegisterEvents()
    {
        // 注册带参数的事件
        EventSystem.Register<BlockLandedEvent>(OnBlockLanded);
        EventSystem.Register<LineClearedEvent>(OnLineCleared);
        EventSystem.Register<ScoreChangedEvent>(OnScoreChanged);
        
        // 注册无参数的事件（如果需要的话）
        // EventSystem.Register<GamePausedEvent>(OnGamePaused);
    }

    private void UnregisterEvents()
    {
        // 取消注册事件监听器
        EventSystem.Unregister<BlockLandedEvent>(OnBlockLanded);
        EventSystem.Unregister<LineClearedEvent>(OnLineCleared);
        EventSystem.Unregister<ScoreChangedEvent>(OnScoreChanged);
    }

    private void TriggerExampleEvents()
    {
        // 触发方块落地事件
        BlockLandedEvent landedEvent = new BlockLandedEvent(
            new Vector3(5, 0, 0), 
            BlockType.T
        );
        EventSystem.Trigger(landedEvent);

        // 触发行消除事件
        LineClearedEvent clearedEvent = new LineClearedEvent(10, 2);
        EventSystem.Trigger(clearedEvent);

        // 触发分数变化事件
        ScoreChangedEvent scoreEvent = new ScoreChangedEvent(1500, 100);
        EventSystem.Trigger(scoreEvent);
    }

    // 事件处理函数
    private void OnBlockLanded(BlockLandedEvent eventData)
    {
//         Debug.Log($"方块落地: 位置 {eventData.position}, 类型 {eventData.blockType}");
        
        // 这里可以添加方块落地后的逻辑
        // 比如播放音效、粒子效果等
    }

    private void OnLineCleared(LineClearedEvent eventData)
    {
//         Debug.Log($"行消除: 第 {eventData.rowIndex} 行, 消除了 {eventData.linesCleared} 行");
        
        // 这里可以添加行消除后的逻辑
        // 比如更新分数、播放特效等
    }

    private void OnScoreChanged(ScoreChangedEvent eventData)
    {
//         Debug.Log($"分数变化: 新分数 {eventData.newScore}, 增加 {eventData.scoreIncrease}");
        
        // 这里可以添加分数变化后的逻辑
        // 比如更新UI、检查等级提升等
    }
} 