using UnityEngine;

public class Block : MonoBehaviour
{
    public Cell[] cells;
    float speed;
    bool isMoving = false;
    public Color[] colors;
    private Rigidbody rb;
    private float fallTimer = 0f;
    private float stepInterval = 1f; // Time between each downward step

    // public BlockTypeEnum blockType;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    public void Init(float speed)
    {
        isMoving = true;
        PrepareCells();
        //set tag to "Block"
        // gameObject.tag = "Block";
        //set collider to trigger
        GetComponent<Collider>().isTrigger = true;
        
        // Add Rigidbody for trigger detection
        // rb = GetComponent<Rigidbody>();
        // if (rb == null)
        // {
        //     rb = gameObject.AddComponent<Rigidbody>();
        // }
        // rb.isKinematic = true; // Prevent physics from affecting movement
        // rb.useGravity = false; // We handle movement manually
        SetSpeed(speed);
        // Initialize fall timer
        fallTimer = 0f;

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
            colors[1] = Color.red;
            colors[2] = Color.yellow;
        }

        SetColor(colors[Random.Range(0, colors.Length)]);
    }

    // Update is called once per frame
    void Update()
    {
        if (!isMoving)
        {
            return;
        }
        Move();

        //control the block by left and right arrow keys
        if (Input.GetKeyDown(KeyCode.LeftArrow))
        {   
            MoveImpl(new Vector3(-1, 0, 0));
        }
        if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            MoveImpl(new Vector3(1, 0, 0));
        }
        // Down arrow for instant drop
        if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            MoveImpl(new Vector3(0, -1, 0));    
        }
    }   

    bool CheckCanMove(Vector3 direction)
    {
        var oldPosition = transform.position;
        var newPosition = oldPosition + direction;
        foreach (Cell cell in cells)
        {
            var newPos = cell.transform.position + direction;
            if (newPos.x < -4.5f || newPos.x > 4.5f || newPos.y < -9.5f)
            {
                return false;
            }
        }
        return true;
    }

    void MoveImpl(Vector3 direction)
    {
        var oldPosition = transform.position;
        //check if the block is in the grid and within the border
        if (!CheckCanMove(direction))
        {
            return;
        }
        var newPosition = oldPosition + direction;

        //if new pos is occupied
        if (GameManager.Instance.CheckGridOccupied(this, direction))
        {
            // 区分垂直和水平移动
            if (direction.y < 0) // 向下移动碰到障碍
            {
                StopMoving(); // 只有向下移动碰到障碍才停止
                return;
            }
            else // 水平移动碰到障碍
            {
                // 简单忽略这次移动，不停止方块
                return;
            }
        }

        //if reach the bottom, stop moving
        foreach (Cell cell in cells)
        {
            if (cell.transform.position.y + direction.y == -9.5f)
            {
                transform.position = newPosition;
                StopMoving();
                return;
            }
        }
        
        transform.position = newPosition;
    }

    public void SetSpeed(float speed)
    {
        this.speed = speed;
        // Update step interval based on speed (lower speed = faster falling)
        stepInterval = Mathf.Max(0.1f, speed); // Minimum 0.1 seconds between steps
    }

    void Move()
    {
        // Step-based falling instead of smooth movement
        fallTimer += Time.deltaTime;
        if (fallTimer >= stepInterval)
        {
            MoveImpl(new Vector3(0, -1, 0)); // Move exactly 1 unit down
            fallTimer = 0f; // Reset timer
        }
    }



    void OnTriggerEnter(Collider other)
    {
        // Debug.Log("Block: OnTriggerEnter" + other.gameObject.tag + " " + other.gameObject.name + " " + isMoving);
        // if (!isMoving)
        // {
        //     return;
        // }
        // if (other.gameObject.tag == "Border")
        // {
        //     StopMoving();
        // }

        // if (other.gameObject.tag == "Block")
        // {
        //     // Handle block-to-block collision
        //     StopMoving();
        //     Debug.Log("Block collided with another block!");
        // }
    }

    public void SetColor(Color color)
    {
        // Debug.Log("Block: SetColor " + color + " " + cells.Length);
        foreach (Cell cell in cells)
        {
            cell.SetColor(color);
        }
    }

    public void StopMoving()
    {

        isMoving = false;
        Debug.Log("Block: StopMoving");
        GameManager.Instance.UpdateGrid(this);
    }
}
