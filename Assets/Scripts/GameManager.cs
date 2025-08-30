using UnityEngine;

public class GameManager : MonoBehaviour
{
    public Block[] blocks;
    public Grid grid;

    public float blockSpeed = 1f;

    public static GameManager Instance;

    void Awake()
    {
        Instance = this;
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        CreateGrid();
        CreateBlocks();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void CreateGrid()
    {
        //instantiate grid width:10 height:20
        for (int x = -5; x < 5; x++)
        {
            for (int y = -10; y < 10; y++)
            {
                var go = Instantiate(grid, new Vector3(x + 0.5f, y + 0.5f, 0), Quaternion.identity);
                go.transform.parent = transform;
                go.name = "Grid_" + x + "_" + y;
                go.GetComponent<Grid>().SetColor(Color.gray);
                go.transform.localScale = new Vector3(0.9f, 0.9f, 0.9f);
            }
        }
    }


    //每次创建一个block，出生坐标（0.5，5.5，0），随机颜色，然后开始下落，速度是blockSpeed
    void CreateBlocks()
    {
        var go = Instantiate(blocks[Random.Range(0, blocks.Length)], new Vector3(0.5f, 5.5f, 0), Quaternion.identity);
        go.transform.parent = transform;
        go.GetComponent<Block>().SetSpeed(blockSpeed);
        //set random color
        go.GetComponent<Block>().Init();
        //set tag to "Block"
        
    }
}
