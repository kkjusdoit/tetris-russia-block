using UnityEngine;

public class Block : MonoBehaviour
{
    Cell[] cells;
    float speed;
    bool isMoving = false;
    public Color[] colors;
    private Rigidbody rb;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
    }

    public void Init()
    {
        isMoving = true;
        AssignCells();
        SetColor(colors[Random.Range(0, colors.Length)]);
        //set tag to "Block"
        gameObject.tag = "Block";
        //set collider to trigger
        GetComponent<Collider>().isTrigger = true;
        
        // Add Rigidbody for trigger detection
        rb = GetComponent<Rigidbody>();
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody>();
        }
        rb.isKinematic = true; // Prevent physics from affecting movement
        rb.useGravity = false; // We handle movement manually
    }

    void AssignCells()
    {
        if (cells == null)
        {
            cells = GetComponentsInChildren<Cell>();
        }
        if (colors == null || colors.Length == 0)
        {
            colors = new Color[6];
            colors[0] = Color.cyan;
            colors[1] = Color.red;
            colors[2] = Color.green;
            colors[3] = Color.blue;
            colors[4] = Color.yellow;
            colors[5] = Color.magenta;
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (isMoving)
        {
            Move();
            CheckCollision();
        }

        //control the block by left and right arrow keys
        if (Input.GetKeyDown(KeyCode.LeftArrow) && isMoving && CheckCanMove())
        {   
            transform.position += new Vector3(-1, 0, 0);
        }
        if (Input.GetKeyDown(KeyCode.RightArrow) && isMoving && CheckCanMove())
        {
            transform.position += new Vector3(1, 0, 0);
        }
    }   

    // public void SetColor(Color color)
    // {
    //     GetComponent<Renderer>().material.color = color;
    // }

    public void SetSpeed(float speed)
    {
        this.speed = speed;
    }

    void Move()
    {
        bool canMove = CheckCanMove();
        if (canMove)
        {
            transform.position += new Vector3(0, -speed * Time.deltaTime, 0);
        }
    }

    bool CheckCanMove()
    {
        foreach (var cell in cells)
        {
            if (cell.transform.position.y <= -5.5f)
            {
                return false;
            }
            if (cell.transform.position.x <= -4.5f || cell.transform.position.x >= 5.5f)
            {
                return false;
            }
        }
        return true;
    }

    void CheckCollision()
    {
        foreach (var cell in cells)
        {

        }
    }

    void OnTriggerEnter(Collider other)
    {
        Debug.Log("Block: OnTriggerEnter" + other.gameObject.tag + " " + other.gameObject.name);
        if (other.gameObject.tag == "BottomBorder")
        {
            StopMoving();
        }

        if (other.gameObject.tag == "Block")
        {
            // Handle block-to-block collision
            StopMoving();
            Debug.Log("Block collided with another block!");
        }
    }

    public void SetColor(Color color)
    {
        foreach (var cell in cells)
        {
            cell.SetColor(color);
        }
    }

    public void StopMoving()
    {
        isMoving = false;
    }
}
