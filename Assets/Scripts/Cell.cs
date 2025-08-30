using UnityEngine;

public class Cell : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SetColor(Color color)
    {
        Debug.Log("Cell: SetColor" + color);
        Renderer renderer = GetComponent<Renderer>();
        if (renderer != null)
        {
            // 确保使用材质实例而不是共享材质
            Material materialInstance = renderer.material;
            materialInstance.color = color;
        }
        else
        {
            Debug.LogWarning("Cell: Renderer component not found!");
        }
    }

    /// <summary>
    /// 获取当前颜色
    /// </summary>
    public Color GetColor()
    {
        Renderer renderer = GetComponent<Renderer>();
        if (renderer != null && renderer.material != null)
        {
            return renderer.material.color;
        }
        return Color.white;
    }

    /// <summary>
    /// 设置透明度
    /// </summary>
    public void SetAlpha(float alpha)
    {
        Renderer renderer = GetComponent<Renderer>();
        if (renderer != null)
        {
            Material materialInstance = renderer.material;
            Color currentColor = materialInstance.color;
            currentColor.a = Mathf.Clamp01(alpha);
            materialInstance.color = currentColor;
        }
    }

    /// <summary>
    /// 设置材质
    /// </summary>
    public void SetMaterial(Material material)
    {
        Renderer renderer = GetComponent<Renderer>();
        if (renderer != null)
        {
            renderer.material = material;
        }
        else
        {
            Debug.LogWarning("Cell: Renderer component not found!");
        }
    }

    /// <summary>
    /// 获取材质
    /// </summary>
    public Material GetMaterial()
    {
        Renderer renderer = GetComponent<Renderer>();
        if (renderer != null)
        {
            return renderer.material;
        }
        return null;
    }
}
