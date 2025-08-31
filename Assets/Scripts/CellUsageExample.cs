using UnityEngine;

/// <summary>
/// Cell颜色功能使用示例
/// </summary>
public class CellUsageExample : MonoBehaviour
{
    [Header("颜色设置")]
    public Color[] blockColors = new Color[]
    {
        Color.cyan,    // I型方块
        Color.yellow,  // O型方块
        Color.magenta, // T型方块
        Color.green,   // S型方块
        Color.red,     // Z型方块
        Color.blue,    // J型方块
        Color.white    // L型方块
    };

    private Cell cell;

    void Start()
    {
        cell = GetComponent<Cell>();
        
        if (cell == null)
        {
//             Debug.LogError("CellUsageExample: Cell component not found!");
            return;
        }

        // 设置初始颜色
        cell.SetColor(Color.gray);
//         Debug.Log("Cell initialized with gray color");
    }

    void Update()
    {
        // 数字键1-7切换不同颜色
        for (int i = 0; i < blockColors.Length && i < 7; i++)
        {
            if (Input.GetKeyDown(KeyCode.Alpha1 + i))
            {
                SetBlockColor((BlockType)i);
            }
        }

        // 空格键随机颜色
        if (Input.GetKeyDown(KeyCode.Space))
        {
            SetRandomColor();
        }

        // A键设置透明度
        if (Input.GetKeyDown(KeyCode.A))
        {
            SetRandomAlpha();
        }
    }

    /// <summary>
    /// 根据方块类型设置颜色
    /// </summary>
    public void SetBlockColor(BlockType blockType)
    {
        if (cell != null)
        {
            int colorIndex = (int)blockType;
            if (colorIndex < blockColors.Length)
            {
                cell.SetColor(blockColors[colorIndex]);
//                 Debug.Log($"Set color for {blockType} block: {blockColors[colorIndex]}");
            }
        }
    }

    /// <summary>
    /// 设置随机颜色
    /// </summary>
    public void SetRandomColor()
    {
        if (cell != null)
        {
            Color randomColor = new Color(Random.value, Random.value, Random.value, 1f);
            cell.SetColor(randomColor);
//             Debug.Log($"Set random color: {randomColor}");
        }
    }

    /// <summary>
    /// 设置随机透明度
    /// </summary>
    public void SetRandomAlpha()
    {
        if (cell != null)
        {
            float randomAlpha = Random.value;
            cell.SetAlpha(randomAlpha);
//             Debug.Log($"Set random alpha: {randomAlpha}");
        }
    }

    /// <summary>
    /// 获取当前颜色信息
    /// </summary>
    [ContextMenu("Get Current Color Info")]
    public void GetCurrentColorInfo()
    {
        if (cell != null)
        {
            Color currentColor = cell.GetColor();
//             Debug.Log($"Current color: {currentColor}, Alpha: {currentColor.a}");
        }
    }
} 