using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class GameManager : MonoBehaviour
{
    public Block[] blocks;
    public Block[] debugBlocks;
    private int blockIndex = 0;
    public GameObject BgGrids;

    public float blockSpeed = 3f;
    
    [Header("Debug Settings")]
    public bool isDebugMode = false; // 调试模式开关
    public Color occupiedColor = Color.red; // 被占用格子的颜色
    public Color emptyColor = Color.green; // 空闲格子的颜色
    public Color normalColor = Color.gray; // 正常状态下的颜色

    [Header("Clear Animation Settings")]
    public float clearAnimationDuration = 0.5f;  // 消除动画总时长
    public float scaleUpDuration = 0.2f;         // 放大阶段时长
    public float scaleDownDuration = 0.3f;       // 缩小阶段时长
    public float maxScale = 1.3f;                // 最大缩放倍数
    public static GameManager Instance { get; private set; }
    //维护一个字典，记录每个格子的GameObject，null表示未占用
    private Dictionary<string, GameObject> gridCell = new Dictionary<string, GameObject>(); //key: position string, value: cell GameObject (null if empty)
    // 维护背景网格的引用，用于调试可视化
    private Dictionary<string, Grid> bgGridComponents = new Dictionary<string, Grid>();
    public int score = 0;
    
    // 防止重复生成方块的标志
    private bool isSpawningBlock = false;
    private bool isProcessingClearing = false;

    void Awake()
    {
//         Debug.Log("=== GameManager: Awake 被调用 ===");
        // 检查是否已经有实例存在
        if (Instance != null)
        {
//             Debug.LogWarning($"Multiple GameManager instances detected! Destroying duplicate on {gameObject.name}");
            Destroy(gameObject);
            return;
        }
        
        // 设置单例实例
        Instance = this;
//         Debug.Log("GameManager: 单例实例设置完成");
        
        // 可选：在场景切换时不销毁（如果需要的话）
        // DontDestroyOnLoad(gameObject);
    }

    void OnDestroy()
    {
        // 清理单例引用，防止引用已销毁的对象
        if (Instance == this)
        {
            Instance = null;
        }
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
//         Debug.Log("=== GameManager: Start 被调用 ===");
        InitGrid();
        SpawnNextBlock();
    }

    /// <summary>
    /// 将Vector3位置转换为字符串key
    /// </summary>
    /// <param name="position">Vector3位置</param>
    /// <returns>格式化的字符串key</returns>
    private string Vector3ToKey(Vector3 position)
    {
        // 使用固定小数位数避免浮点精度问题
        return $"{position.x:F1}_{position.y:F1}_{position.z:F1}";
    }
    
    /// <summary>
    /// 设置网格单元格的值
    /// </summary>
    /// <param name="position">网格位置</param>
    /// <param name="cellObject">要设置的GameObject，null表示空闲</param>
    /// <param name="context">调用上下文，用于调试</param>
    private void SetGridCell(Vector3 position, GameObject cellObject, string context = "")
    {
        string key = Vector3ToKey(position);
        gridCell[key] = cellObject;
        
        if (!string.IsNullOrEmpty(context) && isDebugMode)
        {
            string status = cellObject == null ? "空闲" : $"被占用({cellObject.name})";
            Debug.Log($"[{context}] 设置网格 {position} (key:{key}): {status}");
        }
    }
    
    /// <summary>
    /// 尝试获取网格单元格的值
    /// </summary>
    /// <param name="position">网格位置</param>
    /// <param name="cellObject">输出的GameObject</param>
    /// <param name="context">调用上下文，用于调试</param>
    /// <returns>是否成功获取到值</returns>
    private bool TryGetGridCell(Vector3 position, out GameObject cellObject, string context = "")
    {
        string key = Vector3ToKey(position);
        bool result = gridCell.TryGetValue(key, out cellObject);
        
        if (!string.IsNullOrEmpty(context) && isDebugMode)
        {
            if (result)
            {
                string status = cellObject == null ? "空闲" : $"被占用({cellObject.name})";
                Debug.Log($"[{context}] 获取网格 {position} (key:{key}): {status}");
            }
            else
            {
                Debug.LogWarning($"[{context}] 网格位置 {position} (key:{key}) 不存在于字典中");
            }
        }
        
        return result;
    }

    void InitGrid()
    {
//         Debug.Log("=== GameManager: 开始初始化网格 ===");
        int gridCount = 0;
        //遍历BgGrids的子object，获取position，然后设置grid的值为null（未占用）
        foreach (Transform child in BgGrids.transform)
        {
            Vector3 pos = child.localPosition;
            SetGridCell(pos, null, "InitGrid");
            
            // 获取Grid组件用于调试可视化
            Grid gridComponent = child.GetComponent<Grid>();
            if (gridComponent != null)
            {
                bgGridComponents[Vector3ToKey(pos)] = gridComponent;
                // 设置初始颜色
                if (isDebugMode)
                {
                    child.name = "Grid_" + pos.x + "_" + pos.y;
                    child.localScale = new Vector3(1f, 1f, 0.1f);
                    gridComponent.SetColor(emptyColor);
                }
                else
                {
                    gridComponent.SetColor(normalColor);
                }
            }
            else
            {
//                 Debug.LogWarning($"GameManager: 背景网格 {child.name} 缺少Grid组件");
            }
            
            gridCount++;
        }
//         Debug.Log($"GameManager: 网格初始化完成，共初始化 {gridCount} 个格子");
        
        if (isDebugMode)
        {
//             Debug.Log("GameManager: 调试模式已启用 - 绿色=空闲，红色=占用");
        }
    }

    public void UpdateGrid(Block block)
    {
//         Debug.Log($"=== GameManager: 开始更新网格，方块类型: {block.name}，isProcessingClearing: {isProcessingClearing} ===");
        
        if (isProcessingClearing)
        {
//             Debug.LogWarning("GameManager: 正在处理消除流程，忽略此次UpdateGrid调用");
            return;
        }
        
        for (int i = 0; i < block.cells.Length; i++)
        {
            var cellPos = block.cells[i].transform.position;
//             Debug.Log($"GameManager: 放置方块cell到位置 {cellPos}");
            
            if (cellPos.y >= Config.TOP_POS_Y)
            {
//                 Debug.LogError("=== GAME OVER: 方块到达顶部 ===");
                return;
            }
            SetGridCell(cellPos, block.cells[i].gameObject, "UpdateGrid");
        }
        
//         Debug.Log("GameManager: 方块放置完成，开始检查消除");
        
        // 更新调试可视化
        if (isDebugMode)
        {
            UpdateDebugVisualization();
        }
        
        //这里需要判断是否可以消除，如果可以消除，则消除，然后加分
        isProcessingClearing = true;
        StartCoroutine(ProcessClearing());
    }

    private IEnumerator ProcessClearing()
    {
//         Debug.Log("=== GameManager: 开始处理消除流程 ===");
        
        bool canClear;
        List<GameObject> fullRowsGameObjects;
        (canClear, fullRowsGameObjects) = CanClear();
        
        if (canClear)
        {
//             Debug.Log($"GameManager: 发现可消除行，共 {fullRowsGameObjects.Count} 个方块需要消除");
            yield return StartCoroutine(Clear(fullRowsGameObjects)); // 等待消除动画完成
            AddScore();                           // 加分
        }
        else
        {
//             Debug.Log("GameManager: 没有可消除的行");
        }
        
//         Debug.Log("GameManager: 消除流程完成，准备生成下一个方块");
        isProcessingClearing = false;
        SpawnNextBlock(); // 协程结束后生成新方块
    }

    private (bool, List<GameObject>) CanClear()
    {
//         Debug.Log("=== GameManager: 检查是否可以消除 ===");
        
        //record all full rows grids position
        List<GameObject> fullRows = new List<GameObject>();
        int fullRowCount = 0;
        
        // 检查每一行是否全部被占据
        for (float y = Config.BOTTOM_POS_Y; y <= Config.TOP_POS_Y; y += Config.BLOCK_SIZE)
        {
            List<GameObject> rows = new List<GameObject>();
            for (float x = Config.LEFT_POS_X; x <= Config.RIGHT_POS_X; x += Config.BLOCK_SIZE)
            {
                Vector3 pos = new Vector3(x, y, 0);
                // 检查该位置是否被占用（有GameObject且不为null）
                if (TryGetGridCell(pos, out GameObject cellObj, "CheckCanClear") && cellObj != null)
                {
                    rows.Add(cellObj);
                }
            }
            if (rows.Count == Config.GRID_WIDTH)
            {
                fullRowCount++;
//                 Debug.Log($"GameManager: 发现满行 y={y}，包含 {rows.Count} 个方块");
                fullRows.AddRange(rows);
            }
        }
        
//         Debug.Log($"GameManager: 检查完成，共发现 {fullRowCount} 个满行，{fullRows.Count} 个方块");
        return (fullRows.Count > 0, fullRows);
    }

    private IEnumerator Clear(List<GameObject> fullRowGameObjects)
    {
//         Debug.Log($"=== GameManager: 开始清除 {fullRowGameObjects.Count} 个方块 ===");
        
        // 记录被消除的行Y坐标
        HashSet<float> clearedRowsY = new HashSet<float>();
        foreach (GameObject cell in fullRowGameObjects)
        {
            float cellY = cell.transform.position.y;
            clearedRowsY.Add(cellY);
        }
        
        // 1. 播放消除动画
        yield return StartCoroutine(PlayClearAnimation(fullRowGameObjects));
        
        // 2. 实际消除行（更新网格字典）
        foreach (GameObject cell in fullRowGameObjects)
        {
//             Debug.Log($"GameManager: 销毁方块 {cell.name} 在位置 {cell.transform.position}");
            //update gridCell
            SetGridCell(cell.transform.position, null, "ClearRow");
            // 注意：GameObject已在动画中被销毁，这里不需要再次Destroy
        }

        //对所有的block.cells，如果是null，则更新UpdateCells
        var allBlocks = FindObjectsByType<Block>(FindObjectsSortMode.None);
        foreach (Block block in allBlocks)
        {
            block.UpdateCells();
        }
        
        // 找到最高的被消除行Y坐标和消除的总行数
        float highestClearedY = float.MinValue;
        foreach (float y in clearedRowsY)
        {
            if (y > highestClearedY)
                highestClearedY = y;
        }
        
        int totalClearedRows = clearedRowsY.Count;
        
//         Debug.Log($"GameManager: 共消除了 {totalClearedRows} 行，最高消除行Y: {highestClearedY}");
//         Debug.Log("GameManager: 方块销毁完成，开始处理掉落");
        
        // 更新调试可视化（消除后）
        if (isDebugMode)
        {
            UpdateDebugVisualization();
        }
        
        // 3. 处理上方行掉落，传递消除的行数和最高消除行Y坐标
        yield return StartCoroutine(DropRows(highestClearedY, totalClearedRows));
    }

    // 播放消除动画：缩放效果
    private IEnumerator PlayClearAnimation(List<GameObject> cellsToDestroy)
    {
//         Debug.Log($"=== GameManager: 开始播放消除动画，共 {cellsToDestroy.Count} 个方块 ===");
        
        // 动画参数
        // 使用可配置的动画参数
        
        // 记录所有方块的原始缩放
        Dictionary<GameObject, Vector3> originalScales = new Dictionary<GameObject, Vector3>();
        foreach (GameObject cell in cellsToDestroy)
        {
            if (cell != null)
            {
                originalScales[cell] = cell.transform.localScale;
            }
        }
        
        // 第一阶段：放大
//         Debug.Log("GameManager: 动画第一阶段 - 放大");
        float elapsedTime = 0f;
        while (elapsedTime < this.scaleUpDuration)
        {
            float progress = elapsedTime / this.scaleUpDuration;
            float currentScale = Mathf.Lerp(1f, this.maxScale, progress);
            
            foreach (GameObject cell in cellsToDestroy)
            {
                if (cell != null && originalScales.ContainsKey(cell))
                {
                    cell.transform.localScale = originalScales[cell] * currentScale;
                }
            }
            
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        
        // 确保达到最大缩放
        foreach (GameObject cell in cellsToDestroy)
        {
            if (cell != null && originalScales.ContainsKey(cell))
            {
                cell.transform.localScale = originalScales[cell] * this.maxScale;
            }
        }
        
        // 第二阶段：缩小到消失
//         Debug.Log("GameManager: 动画第二阶段 - 缩小");
        elapsedTime = 0f;
        while (elapsedTime < this.scaleDownDuration)
        {
            float progress = elapsedTime / this.scaleDownDuration;
            float currentScale = Mathf.Lerp(this.maxScale, 0f, progress);
            
            foreach (GameObject cell in cellsToDestroy)
            {
                if (cell != null && originalScales.ContainsKey(cell))
                {
                    cell.transform.localScale = originalScales[cell] * currentScale;
                }
            }
            
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        
        // 动画完成，销毁所有方块
//         Debug.Log("GameManager: 消除动画完成，销毁方块");
        foreach (GameObject cell in cellsToDestroy)
        {
            if (cell != null)
            {
                Destroy(cell);
            }
        }
        
//         Debug.Log("GameManager: 消除动画播放完成");
    }

    // GetCellCanDropHeight 方法已删除，因为现在所有上方方块统一下降消除的行数

    //处理上方行掉落
    private IEnumerator DropRows(float clearedRowY, int clearedRowCount)
    {
        // 从被消除行的上方一行开始检查
        float startY = clearedRowY + Config.BLOCK_SIZE;
        float dropDistance = clearedRowCount * Config.BLOCK_SIZE;
        
//         Debug.Log("=== GameManager: 开始处理方块掉落 ===");
//         Debug.Log($"GameManager: 最高消除行Y: {clearedRowY}，消除了 {clearedRowCount} 行");
//         Debug.Log($"GameManager: 从 Y: {startY} 开始检查到 {Config.TOP_POS_Y}");
//         Debug.Log($"GameManager: 所有上方方块统一下降 {dropDistance} 单位");
//         Debug.Log($"GameManager: X范围: {Config.LEFT_POS_X} 到 {Config.RIGHT_POS_X}");
        
        int totalDroppedCells = 0;
        int totalCheckedCells = 0;
        
        // 从被消除行的上方开始，逐行向上处理
        // 所有在消除行上方的方块都统一下降 clearedRowCount 行
        for (float y = startY; y <= Config.TOP_POS_Y; y += Config.BLOCK_SIZE) //从消除行上方开始，到顶部结束
        {
//             Debug.Log($"GameManager: 检查第 {y} 行");
            int rowCellCount = 0;
            int rowDroppedCount = 0;
            
            for (float x = Config.LEFT_POS_X; x <= Config.RIGHT_POS_X; x += Config.BLOCK_SIZE)
            {
                Vector3 pos = new Vector3(x, y, 0);
                totalCheckedCells++;
                
//                 Debug.Log($"GameManager: 检查位置 {pos}");
                
                if (TryGetGridCell(pos, out GameObject cellObj, "ProcessClearing"))
                {
                    if (cellObj != null)
                    {
                        rowCellCount++;
//                         Debug.Log($"GameManager: 位置 {pos} 有方块 {cellObj.name}");
                        
                        Vector3 oldPos = cellObj.transform.position;
                        Vector3 newPos = oldPos - new Vector3(0, dropDistance, 0);
                        
//                         Debug.Log($"GameManager: 方块 {cellObj.name} 从 {oldPos} 统一下降到 {newPos}");
                        
                        // 检查新位置是否在字典中
                        if (TryGetGridCell(newPos, out GameObject existingObj, "ProcessClearing_CheckTarget"))
                        {
//                             Debug.Log($"GameManager: 新位置 {newPos} 在字典中，当前值: {existingObj?.name}");
                        }
                        else
                        {
//                             Debug.LogError($"GameManager: 新位置 {newPos} 不在字典中！");
                        }
                        
                        // 更新字典
                        SetGridCell(oldPos, null, "ProcessClearing_ClearSource");
                        SetGridCell(newPos, cellObj, "ProcessClearing_SetTarget");
                        
                        // 移动GameObject
                        cellObj.transform.position = newPos;
                        totalDroppedCells++;
                        rowDroppedCount++;
                        
//                         Debug.Log($"GameManager: 字典更新完成 - 旧位置 {oldPos}: null, 新位置 {newPos}: {cellObj.name}");
                    }
                    else
                    {
//                         Debug.Log($"GameManager: 位置 {pos} 为空 (null)");
                    }
                }
                else
                {
//                     Debug.LogWarning($"GameManager: 位置 {pos} 不在字典中");
                }
            }
            
//             Debug.Log($"GameManager: 第 {y} 行检查完成 - 共 {rowCellCount} 个方块，{rowDroppedCount} 个掉落");
        }
        
//         Debug.Log($"GameManager: 掉落处理完成 - 从Y={startY}开始检查了 {totalCheckedCells} 个位置，共有 {totalDroppedCells} 个方块发生了掉落");
        
        // 验证字典状态
//         Debug.Log("=== GameManager: 验证掉落后字典状态 ===");
        int occupiedCount = 0;
        int emptyCount = 0;
        
        // 只显示有方块的位置，避免日志过多
        for (float y = Config.BOTTOM_POS_Y; y <= Config.TOP_POS_Y; y += Config.BLOCK_SIZE)
        {
            List<string> rowInfo = new List<string>();
            for (float x = Config.LEFT_POS_X; x <= Config.RIGHT_POS_X; x += Config.BLOCK_SIZE)
            {
                Vector3 pos = new Vector3(x, y, 0);
                if (TryGetGridCell(pos, out GameObject cellObj, "ProcessClearing"))
                {
                    if (cellObj != null)
                    {
                        occupiedCount++;
                        rowInfo.Add($"({x},{y}):{cellObj.name}");
                    }
                    else
                    {
                        emptyCount++;
                    }
                }
            }
            if (rowInfo.Count > 0)
            {
//                 Debug.Log($"GameManager: Y={y}行有方块: {string.Join(", ", rowInfo)}");
            }
        }
        
//         Debug.Log($"GameManager: 字典状态总结 - 占用: {occupiedCount}, 空闲: {emptyCount}");
        
        // 更新调试可视化（掉落后）
        if (isDebugMode)
        {
            UpdateDebugVisualization();
        }
        
        yield return null;
    }

    public bool CheckGridOccupied(Block block, Vector3 direction, bool isRotate = false)
    {
        for (int i = 0; i < block.cells.Length; i++)
        {
            var cellPos = block.cells[i].transform.position + direction;

            var isOccupied = TryGetGridCell(cellPos, out GameObject cellObj, "CheckGridOccupied") && cellObj != null;

            if (isOccupied)
            {
                if (isRotate)
                {
                    // Debug.Log($"GameManager: 检测到旋转碰撞，位置 {cellPos} 被 {cellObj.name} 占用");
                }
                else
                {
                    Debug.Log($"GameManager: 检测到移动碰撞，位置 {cellPos} 被 {cellObj.name} 占用");
                }
                return true;
            }
            else
            {
                if (isRotate)
                {
                    // Debug.Log($"GameManager: 检测到旋转碰撞，位置 {cellPos} 未被占用");
                }
                else
                {
                    Debug.Log($"GameManager: 检测到移动碰撞，位置 {cellPos} 未被占用");
                }
            }
            
        }
        return false;
    }

    // 添加缺少的AddScore方法
    private void AddScore()
    {
        int oldScore = score;
        score += 100;
//         Debug.Log($"=== GameManager: 加分！从 {oldScore} 增加到 {score} ===");
    }

    // 更新调试可视化
    private void UpdateDebugVisualization()
    {
        if (!isDebugMode) return;
        
//         Debug.Log("=== GameManager: 更新调试可视化 ===");
        
        // 需要重新遍历所有网格位置，因为bgGridComponents的key现在是string
        for (float y = Config.BOTTOM_POS_Y; y <= Config.TOP_POS_Y; y += Config.BLOCK_SIZE)
        {
            for (float x = Config.LEFT_POS_X; x <= Config.RIGHT_POS_X; x += Config.BLOCK_SIZE)
            {
                Vector3 pos = new Vector3(x, y, 0);
                string key = Vector3ToKey(pos);
                
                if (bgGridComponents.TryGetValue(key, out Grid gridComponent) && gridComponent != null)
                {
                    // 检查该位置是否被占用
                    if (TryGetGridCell(pos, out GameObject occupyingCell, "UpdateBgGrid"))
                    {
                        if (occupyingCell != null)
                        {
                            // 被占用 - 显示红色
                            gridComponent.SetColor(occupiedColor);
//                             Debug.Log($"GameManager: 位置 {pos} 被占用，设为红色");
                        }
                        else
                        {
                            // 空闲 - 显示绿色
                            gridComponent.SetColor(emptyColor);
                        }
                    }
                    else
                    {
//                         Debug.LogWarning($"GameManager: 位置 {pos} 不在gridCell字典中");
                    }
                }
            }
        }
        
//         Debug.Log("GameManager: 调试可视化更新完成");
    }
    
    // 切换调试模式
    public void ToggleDebugMode()
    {
        isDebugMode = !isDebugMode;
//         Debug.Log($"GameManager: 调试模式 {(isDebugMode ? "启用" : "禁用")}");
        
        if (isDebugMode)
        {
            // 启用调试模式，更新所有网格颜色
            UpdateDebugVisualization();
        }
        else
        {
            // 禁用调试模式，恢复正常颜色
            foreach (var kvp in bgGridComponents)
            {
                Grid gridComponent = kvp.Value;
                if (gridComponent != null)
                {
                    gridComponent.SetColor(normalColor);
                }
            }
        }
    }
    
    // 强制刷新调试可视化（用于测试）
    public void RefreshDebugVisualization()
    {
        if (isDebugMode)
        {
//             Debug.Log("GameManager: 手动刷新调试可视化");
            UpdateDebugVisualization();
        }
    }

    // Update is called once per frame
    void Update()
    {
        // 按键切换调试模式（可选功能）
        if (Input.GetKeyDown(KeyCode.F1))
        {
            ToggleDebugMode();
        }
        
        // 手动刷新调试可视化（可选功能）
        if (Input.GetKeyDown(KeyCode.F2))
        {
            RefreshDebugVisualization();
        }
    }

/*    void CreateGrid()
    {
        //create grid parent
        var gridParent = new GameObject("Grid");
        gridParent.transform.parent = transform;
        gridParent.transform.localPosition = Vector3.zero;
        gridParent.transform.localScale = Vector3.one;
        gridParent.transform.localRotation = Quaternion.identity;
        
        //instantiate grid width:10 height:20
        for (int x = -5; x < 5; x++)
        {
            for (int y = -10; y < 10; y++)
            {
                var go = Instantiate(grid, new Vector3(x + 0.5f, y + 0.5f, 0), Quaternion.identity);
                go.transform.parent = gridParent.transform;
                go.name = "Grid_" + x + "_" + y;
                go.GetComponent<Grid>().SetColor(Color.gray);
                go.transform.localScale = new Vector3(0.9f, 0.9f, 0.9f);
            }
        }
    } */

    //每次创建一个block，出生坐标（0.5，5.5，0），随机颜色，然后开始下落，速度是blockSpeed
    public void SpawnNextBlock()
    {
//         Debug.Log($"=== GameManager: SpawnNextBlock 被调用，isSpawningBlock: {isSpawningBlock} ===");
        
        if (isSpawningBlock)
        {
//             Debug.LogWarning("GameManager: 正在生成方块，忽略重复调用");
            return;
        }
        
        isSpawningBlock = true;
        
        var block = isDebugMode ? debugBlocks[blockIndex] : blocks[Random.Range(0, blocks.Length)];
        blockIndex = (blockIndex + 1) % debugBlocks.Length;
        Debug.Log($"GameManager: 选择方块类型 {block.name}");
//         Debug.Log($"GameManager: 在位置 (0.5, 10.5, 0) 生成新方块 {block.name}");
        
        var go = Instantiate(block, new Vector3(0.5f, 10.5f, 0), Quaternion.identity);
        go.transform.parent = transform;
        go.GetComponent<Block>().Init(blockSpeed);
        
//         Debug.Log($"GameManager: 方块 {block.name} 初始化完成，开始下落");
        
        isSpawningBlock = false;
    }
}
