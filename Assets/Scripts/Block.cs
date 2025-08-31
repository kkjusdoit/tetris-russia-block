using UnityEngine;
using System.Collections.Generic;
using System.Collections;

// 定义操作类型和优先级
public enum BlockOperationType
{
    Stop = 0,           // 最高优先级：停止移动
    Rotate = 1,         // 高优先级：旋转
    ManualMove = 2,     // 中优先级：手动移动
    AutoFall = 3        // 最低优先级：自动下降
}

// 操作数据结构
public struct BlockOperation
{
    public BlockOperationType type;
    public Vector3 direction;
    public int priority => (int)type;
    
    public BlockOperation(BlockOperationType operationType, Vector3 moveDirection = default)
    {
        type = operationType;
        direction = moveDirection;
    }
}

public class Block : MonoBehaviour
{
    public Cell[] cells;
    float speed;
    bool isMoving = false;
    public Color[] colors;
    private float fallTimer = 0f;
    private float stepInterval = 1f; // Time between each downward step
    private bool hasStopped = false; // 防止重复调用StopMoving
    
    // 操作队列系统
    private Queue<BlockOperation> operationQueue = new Queue<BlockOperation>();
    private bool isProcessingOperation = false; // 状态锁定
    private BlockOperation? currentOperation = null;

    // public BlockTypeEnum blockType;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    public void Init(float speed)
    {
//         Debug.Log($"=== Block: {gameObject.name} Init 被调用，速度: {speed} ===");
        isMoving = true;
        hasStopped = false;
        
        // 初始化操作队列
        operationQueue.Clear();
        isProcessingOperation = false;
        currentOperation = null;
        
        PrepareCells();
        
        SetSpeed(speed);
        // Initialize fall timer
        fallTimer = 0f;
//         Debug.Log($"Block: {gameObject.name} 初始化完成");
    }

    void PrepareCells()
    {
        // Debug.Log("Block: PrepareCells " + transform.childCount);
        //check child node if not with Cell component then add it
        foreach (Transform child in transform)
        {
            if (child.GetComponent<Cell>() == null)
            {
                // Debug.Log("Block: PrepareCells  add cell component:" + child.name);
                child.gameObject.AddComponent<Cell>();
            }
        }
        if (cells == null || cells.Length == 0)
        {
            cells = GetComponentsInChildren<Cell>();
            // Debug.Log("Block: PrepareCells " + cells.Length);
        }
        if (colors == null || colors.Length == 0)
        {
            colors = new Color[3];
            colors[0] = Color.blue;
            colors[1] = Color.cyan;
            colors[2] = Color.yellow;
        }

        SetColor(colors[Random.Range(0, colors.Length)]);
    }

    public void UpdateCells()
    {
        var validCells = new List<Cell>(cells.Length);
        
        foreach (Cell cell in cells)
        {
            if (cell != null)
            {
                validCells.Add(cell);
            }
        }
        
        cells = validCells.ToArray();
        // 不保留对旧数组的引用，允许GC回收
    }

    // 操作队列管理方法
    private void EnqueueOperation(BlockOperation operation)
    {
//         Debug.Log($"Block: {gameObject.name} 入队操作 {operation.type} {operation.direction}");
        
        // 如果是停止操作，清空队列并立即执行
        if (operation.type == BlockOperationType.Stop)
        {
//             Debug.Log($"Block: {gameObject.name} 停止操作优先级最高，清空队列");
            operationQueue.Clear();
            operationQueue.Enqueue(operation);
            return;
        }
        
        // 检查队列中是否已有相同或更高优先级的操作
        var tempQueue = new Queue<BlockOperation>();
        bool shouldAdd = true;
        
        while (operationQueue.Count > 0)
        {
            var existingOp = operationQueue.Dequeue();
            
            // 如果新操作优先级更高，丢弃现有的低优先级操作
            if (operation.priority < existingOp.priority)
            {
//                 Debug.Log($"Block: {gameObject.name} 新操作 {operation.type} 优先级更高，丢弃 {existingOp.type}");
                continue;
            }
            // 如果现有操作优先级更高或相等，保留现有操作
            else if (existingOp.priority <= operation.priority)
            {
                tempQueue.Enqueue(existingOp);
                // 如果是相同类型的操作，不添加新操作
                if (existingOp.type == operation.type)
                {
                    shouldAdd = false;
//                     Debug.Log($"Block: {gameObject.name} 队列中已有相同操作 {operation.type}，忽略新操作");
                }
            }
        }
        
        // 恢复队列
        while (tempQueue.Count > 0)
        {
            operationQueue.Enqueue(tempQueue.Dequeue());
        }
        
        // 添加新操作
        if (shouldAdd)
        {
            operationQueue.Enqueue(operation);
//             Debug.Log($"Block: {gameObject.name} 操作 {operation.type} 已入队，队列长度: {operationQueue.Count}");
        }
    }
    
    private void ProcessOperationQueue()
    {
        // 如果正在处理操作或队列为空，返回
        if (isProcessingOperation || operationQueue.Count == 0)
        {
            return;
        }
        
        // 获取下一个操作
        var operation = operationQueue.Dequeue();
        currentOperation = operation;
        isProcessingOperation = true;
        
//         Debug.Log($"Block: {gameObject.name} 开始处理操作 {operation.type} {operation.direction}");
        
        // 执行操作
        StartCoroutine(ExecuteOperation(operation));
    }
    
    private IEnumerator ExecuteOperation(BlockOperation operation)
    {
        switch (operation.type)
        {
            case BlockOperationType.Stop:
//                 Debug.Log($"Block: {gameObject.name} 执行停止操作");
                ExecuteStopMoving();
                break;
                
            case BlockOperationType.Rotate:
//                 Debug.Log($"Block: {gameObject.name} 执行旋转操作");
                ExecuteRotate();
                break;
                
            case BlockOperationType.ManualMove:
            case BlockOperationType.AutoFall:
//                 Debug.Log($"Block: {gameObject.name} 执行移动操作 {operation.direction}");
                ExecuteMoveImpl(operation.direction, operation.type);
                break;
        }
        
        // 操作完成，释放锁定
        isProcessingOperation = false;
        currentOperation = null;
        
//         Debug.Log($"Block: {gameObject.name} 操作 {operation.type} 执行完成");
        
        yield return null;
    }

    // Update is called once per frame
    void Update()
    {
        if (!isMoving)
        {
            return;
        }
        
        // 处理输入，将操作加入队列
        HandleInput();
        
        // 处理自动下降
        HandleAutoFall();
        
        // 处理操作队列
        ProcessOperationQueue();
    }
    
    private void HandleInput()
    {
        // 左右移动 - 手动移动优先级
        if (Input.GetKeyDown(KeyCode.LeftArrow))
        {   
            EnqueueOperation(new BlockOperation(BlockOperationType.ManualMove, new Vector3(-1, 0, 0)));
        }
        if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            EnqueueOperation(new BlockOperation(BlockOperationType.ManualMove, new Vector3(1, 0, 0)));
        }
        // 手动下降 - 手动移动优先级
        if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            EnqueueOperation(new BlockOperation(BlockOperationType.ManualMove, new Vector3(0, -1, 0)));
        }
        // 旋转 - 高优先级
        if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            EnqueueOperation(new BlockOperation(BlockOperationType.Rotate));
        }
    }
    
    private void HandleAutoFall()
    {
        // 自动下降计时器
        fallTimer += Time.deltaTime;
        if (fallTimer >= stepInterval)
        {
            EnqueueOperation(new BlockOperation(BlockOperationType.AutoFall, new Vector3(0, -1, 0)));
            fallTimer = 0f; // Reset timer
        }
    }   

    // 执行旋转操作的实际方法
    void ExecuteRotate()
    {
//         Debug.Log($"Block: {gameObject.name} 执行旋转操作");
        var beforeRotate = transform.localRotation;
        transform.Rotate(0, 0, 90);
        var direction = new Vector3(0, 0, 0);
        if (CheckCanMove(direction) && !GameManager.Instance.CheckGridOccupied(this, direction, true))
        {
//             Debug.Log($"Block: {gameObject.name} 旋转成功");
        }
        else
        {
//             Debug.Log($"Block: {gameObject.name} 旋转失败，恢复原状态");
            transform.localRotation = beforeRotate;
        }
    }
    
    // 保留原有的Rotate方法以便兼容（如果有其他地方调用）
    void Rotate()
    {
        EnqueueOperation(new BlockOperation(BlockOperationType.Rotate));
    }

    bool CheckCanMove(Vector3 direction)
    {
        var oldPosition = transform.position;
        var newPosition = oldPosition + direction;
        foreach (Cell cell in cells)
        {
            var newPos = cell.transform.position + direction;
            if (newPos.x < Config.LEFT_POS_X || newPos.x > Config.RIGHT_POS_X || newPos.y < Config.BOTTOM_POS_Y)
            {
                return false;
            }
        }
        return true;
    }

    // 执行移动操作的实际方法
    void ExecuteMoveImpl(Vector3 direction, BlockOperationType operationType)
    {
//         Debug.Log($"Block: {gameObject.name} 执行移动操作 {direction} (类型: {operationType})");
        
        var oldPosition = transform.position;
        //check if the block is in the grid and within the border
        if (!CheckCanMove(direction))
        {
//             Debug.Log($"Block: {gameObject.name} 无法移动到 {direction}，超出边界");
            return;
        }
        var newPosition = oldPosition + direction;

        //if new pos is occupied
        if (GameManager.Instance.CheckGridOccupied(this, direction))
        {
//             Debug.Log($"Block: {gameObject.name} 在方向 {direction} 检测到碰撞");
            // 区分垂直和水平移动
            if (direction.y < 0) // 向下移动碰到障碍
            {
//                 Debug.Log($"Block: {gameObject.name} 向下移动碰撞，触发停止操作");
                EnqueueOperation(new BlockOperation(BlockOperationType.Stop)); // 触发停止操作
                return;
            }
            else // 水平移动碰到障碍
            {
//                 Debug.Log($"Block: {gameObject.name} 水平移动碰撞，忽略移动");
                // 简单忽略这次移动，不停止方块
                return;
            }
        }

        //if reach the bottom, stop moving
        foreach (Cell cell in cells)
        {
            if (cell.transform.position.y + direction.y <= Config.BOTTOM_POS_Y)
            {
//                 Debug.Log($"Block: {gameObject.name} 到达底部，触发停止操作");
                transform.position = newPosition;
                EnqueueOperation(new BlockOperation(BlockOperationType.Stop)); // 触发停止操作
                return;
            }
        }
        
        transform.position = newPosition;
//         Debug.Log($"Block: {gameObject.name} 移动到位置 {newPosition}");
    }
    
    // 保留原有的MoveImpl方法以便兼容（如果有其他地方调用）
    void MoveImpl(Vector3 direction)
    {
        EnqueueOperation(new BlockOperation(BlockOperationType.ManualMove, direction));
    }

    public void SetSpeed(float speed)
    {
        this.speed = speed;
        // Update step interval based on speed (lower speed = faster falling)
        stepInterval = Mathf.Max(0.1f, speed); // Minimum 0.1 seconds between steps
    }

    // Move方法已删除，自动下降逻辑移到HandleAutoFall中


    public void SetColor(Color color)
    {
        // Debug.Log("Block: SetColor " + color + " " + cells.Length);
        foreach (Cell cell in cells)
        {
            cell.SetColor(color);
        }
    }

    // 执行停止操作的实际方法
    void ExecuteStopMoving()
    {
//         Debug.Log($"Block: {gameObject.name} 执行停止操作");
        
        if (hasStopped)
        {
//             Debug.LogWarning($"Block: {gameObject.name} 已经停止过了，忽略重复调用");
            return;
        }
        
        hasStopped = true;
        isMoving = false;
        
        // 清空操作队列，因为已经停止
        operationQueue.Clear();
        isProcessingOperation = false;
        
//         Debug.Log($"Block: {gameObject.name} 正式停止移动，通知GameManager更新网格");
        GameManager.Instance.UpdateGrid(this);
//         Debug.Log($"Block: {gameObject.name} UpdateGrid 调用完成");
    }
    
    // 保留原有的StopMoving方法以便兼容（如果有其他地方调用）
    public void StopMoving()
    {
        EnqueueOperation(new BlockOperation(BlockOperationType.Stop));
    }
}
