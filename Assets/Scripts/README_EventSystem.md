# 事件系统使用说明

## 概述
这是一个简单而强大的事件系统，专门为俄罗斯方块游戏设计。它支持泛型事件和参数传递，提供了类型安全的事件处理机制。

## 核心文件
- `EventSystem.cs` - 事件系统的核心实现
- `GameEvents.cs` - 游戏事件类型定义
- `EventExample.cs` - 使用示例

## 基本用法

### 1. 注册事件监听器
```csharp
// 注册带参数的事件
EventSystem.Register<BlockLandedEvent>(OnBlockLanded);
EventSystem.Register<LineClearedEvent>(OnLineCleared);

// 注册无参数的事件
EventSystem.Register<GamePausedEvent>(OnGamePaused);
```

### 2. 触发事件
```csharp
// 触发带参数的事件
BlockLandedEvent eventData = new BlockLandedEvent(position, blockType);
EventSystem.Trigger(eventData);

// 触发无参数的事件
EventSystem.Trigger<GamePausedEvent>();
```

### 3. 取消注册事件
```csharp
// 取消注册事件监听器
EventSystem.Unregister<BlockLandedEvent>(OnBlockLanded);
EventSystem.Unregister<LineClearedEvent>(OnLineCleared);
```

## 可用的事件类型

### BlockLandedEvent
方块落地事件
```csharp
public struct BlockLandedEvent
{
    public Vector3 position;    // 方块位置
    public BlockType blockType; // 方块类型
}
```

### LineClearedEvent
行消除事件
```csharp
public struct LineClearedEvent
{
    public int rowIndex;      // 消除的行索引
    public int linesCleared;  // 消除的行数
}
```

### ScoreChangedEvent
分数变化事件
```csharp
public struct ScoreChangedEvent
{
    public int newScore;      // 新分数
    public int scoreIncrease; // 分数增加量
}
```

### GameStartedEvent
游戏开始事件
```csharp
public struct GameStartedEvent
{
    public int level;   // 游戏等级
    public float speed; // 游戏速度
}
```

### GameOverEvent
游戏结束事件
```csharp
public struct GameOverEvent
{
    public int finalScore;     // 最终分数
    public int finalLevel;     // 最终等级
    public int linesCleared;   // 消除的总行数
}
```

## 最佳实践

### 1. 在Start中注册，在OnDestroy中取消注册
```csharp
void Start()
{
    EventSystem.Register<BlockLandedEvent>(OnBlockLanded);
}

void OnDestroy()
{
    EventSystem.Unregister<BlockLandedEvent>(OnBlockLanded);
}
```

### 2. 使用有意义的事件处理函数名称
```csharp
private void OnBlockLanded(BlockLandedEvent eventData)
{
    // 处理方块落地逻辑
}

private void OnLineCleared(LineClearedEvent eventData)
{
    // 处理行消除逻辑
}
```

### 3. 在适当的地方触发事件
```csharp
// 在方块落地时触发
public void LandBlock()
{
    // 方块落地逻辑...
    
    // 触发事件
    BlockLandedEvent eventData = new BlockLandedEvent(transform.position, blockType);
    EventSystem.Trigger(eventData);
}
```

## 高级功能

### 清除所有事件
```csharp
EventSystem.ClearAll();
```

### 清除特定类型的事件
```csharp
EventSystem.Clear<BlockLandedEvent>();
```

### 获取监听器数量
```csharp
int count = EventSystem.GetListenerCount<BlockLandedEvent>();
```

## 注意事项

1. **内存管理**: 确保在对象销毁时取消注册事件，避免内存泄漏
2. **异常处理**: 事件系统内置了异常处理，但建议在事件处理函数中也添加适当的错误处理
3. **性能**: 事件系统使用字典存储，性能良好，但避免在Update等高频函数中频繁注册/取消注册事件
4. **线程安全**: 当前实现不是线程安全的，如需多线程支持请添加适当的同步机制

## 扩展

如需添加新的事件类型，只需在 `GameEvents.cs` 中定义新的结构体：

```csharp
public struct NewGameEvent
{
    public string message;
    public int value;
    
    public NewGameEvent(string msg, int val)
    {
        message = msg;
        value = val;
    }
}
``` 