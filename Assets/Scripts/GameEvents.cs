using UnityEngine;

/// <summary>
/// 俄罗斯方块游戏事件类型定义
/// </summary>

// 方块落地事件
public struct BlockLandedEvent
{
    public Vector3 position;
    public BlockType blockType;
    
    public BlockLandedEvent(Vector3 pos, BlockType type)
    {
        position = pos;
        blockType = type;
    }
}

// 行消除事件
public struct LineClearedEvent
{
    public int rowIndex;
    public int linesCleared;
    
    public LineClearedEvent(int row, int count)
    {
        rowIndex = row;
        linesCleared = count;
    }
}

// 游戏开始事件
public struct GameStartedEvent
{
    public int level;
    public float speed;
    
    public GameStartedEvent(int lvl, float spd)
    {
        level = lvl;
        speed = spd;
    }
}

// 游戏结束事件
public struct GameOverEvent
{
    public int finalScore;
    public int finalLevel;
    public int linesCleared;
    
    public GameOverEvent(int score, int level, int lines)
    {
        finalScore = score;
        finalLevel = level;
        linesCleared = lines;
    }
}

// 分数变化事件
public struct ScoreChangedEvent
{
    public int newScore;
    public int scoreIncrease;
    
    public ScoreChangedEvent(int score, int increase)
    {
        newScore = score;
        scoreIncrease = increase;
    }
}

// 等级变化事件
public struct LevelChangedEvent
{
    public int newLevel;
    public float newSpeed;
    
    public LevelChangedEvent(int level, float speed)
    {
        newLevel = level;
        newSpeed = speed;
    }
}

// 方块旋转事件
public struct BlockRotatedEvent
{
    public Vector3 position;
    public Quaternion rotation;
    public BlockType blockType;
    
    public BlockRotatedEvent(Vector3 pos, Quaternion rot, BlockType type)
    {
        position = pos;
        rotation = rot;
        blockType = type;
    }
}

// 方块移动事件
public struct BlockMovedEvent
{
    public Vector3 fromPosition;
    public Vector3 toPosition;
    public BlockType blockType;
    
    public BlockMovedEvent(Vector3 from, Vector3 to, BlockType type)
    {
        fromPosition = from;
        toPosition = to;
        blockType = type;
    }
}

// 暂停/恢复事件
public struct GamePausedEvent
{
    public bool isPaused;
    
    public GamePausedEvent(bool paused)
    {
        isPaused = paused;
    }
}

// 方块类型枚举
public enum BlockType
{
    I,  // 长条
    O,  // 正方形
    T,  // T型
    S,  // S型
    Z,  // Z型
    J,  // J型
    L   // L型
} 