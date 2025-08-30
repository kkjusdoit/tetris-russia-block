using UnityEngine;
using System.Collections.Generic;

public class GameManager : MonoBehaviour
{
    public Block[] blocks;
    public GameObject BgGrids;

    public float blockSpeed = 3f;

    public static GameManager Instance { get; private set; }
    //维护一个二维数组，记录每个格子的状态
    private Dictionary<Vector3, bool> gridDict = new Dictionary<Vector3, bool>(); //key: position, value: occupied
    void Awake()
    {
        // 检查是否已经有实例存在
        if (Instance != null)
        {
            Debug.LogWarning($"Multiple GameManager instances detected! Destroying duplicate on {gameObject.name}");
            Destroy(gameObject);
            return;
        }
        
        // 设置单例实例
        Instance = this;
        
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
        InitGrid();
        SpawnNextBlock();
    }

    void InitGrid()
    {
        //遍历BgGrids的子object，获取position，然后设置grid的值为false
        foreach (Transform child in BgGrids.transform)
        {
            Debug.Log("GameManager: InitGrid " + child.position);
            gridDict[child.position] = false;
        }
    }



    public void UpdateGrid(Block block)
    {
        for (int i = 0; i < block.cells.Length; i++)
        {
            var cellPos = block.cells[i].transform.position;
            if (cellPos.y >= 9.5f)
            {
                Debug.LogError("Game Over");
                return;
            }
            gridDict[cellPos] = true;
        }
        SpawnNextBlock();
    }



    public bool CheckGridOccupied(Block block, Vector3 direction)
    {
        for (int i = 0; i < block.cells.Length; i++)
        {
            var cellPos = block.cells[i].transform.position + direction;
            
            // 如果字典里没有这个key，视为没有被占据（false）
            if (gridDict.TryGetValue(cellPos, out bool isOccupied) && isOccupied)
            {
                return true;
            }
        }
        return false;
    }


    // Update is called once per frame
    void Update()
    {
        
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
        var block = blocks[Random.Range(0, blocks.Length)];
        Debug.Log("GameManager: SpawnNextBlock " + block.name);
        var go = Instantiate(block, new Vector3(0.5f, 10.5f, 0), Quaternion.identity);
        go.transform.parent = transform;
        go.GetComponent<Block>().Init(blockSpeed);
    }

}
