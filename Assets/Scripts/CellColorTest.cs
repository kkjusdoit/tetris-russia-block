using UnityEngine;

/// <summary>
/// 测试Cell颜色设置功能的脚本
/// </summary>
public class CellColorTest : MonoBehaviour
{
    [Header("测试设置")]
    public Color testColor = Color.red;
    public KeyCode testKey = KeyCode.Space;
    
    private Cell cellComponent;
    
    void Start()
    {
        // 获取Cell组件
        cellComponent = GetComponent<Cell>();
        
        if (cellComponent == null)
        {
//             Debug.LogError("CellColorTest: Cell component not found on this GameObject!");
            return;
        }
        
        // 设置初始颜色为白色
        cellComponent.SetColor(Color.white);
//         Debug.Log("CellColorTest: Initial color set to white");
    }
    
    void Update()
    {
        // 按空格键测试颜色设置
        if (Input.GetKeyDown(testKey))
        {
            TestColorChange();
        }
    }
    
    void TestColorChange()
    {
        if (cellComponent != null)
        {
            cellComponent.SetColor(testColor);
//             Debug.Log($"CellColorTest: Color changed to {testColor}");
            
            // 随机改变测试颜色
            testColor = new Color(Random.value, Random.value, Random.value, 1f);
        }
    }
    
    // 在Inspector中测试颜色设置
    [ContextMenu("Test Color Change")]
    void TestColorChangeInInspector()
    {
        if (cellComponent != null)
        {
            cellComponent.SetColor(testColor);
//             Debug.Log($"CellColorTest: Color changed to {testColor} via Inspector");
        }
    }
} 