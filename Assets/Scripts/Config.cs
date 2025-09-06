using UnityEngine;

public static class Config
{
    public static int GRID_WIDTH = 10;
    public static int GRID_HEIGHT = 20;
    public static int TOP_POS_Y = 9;
    public static int BOTTOM_POS_Y = -10;
    public static int LEFT_POS_X = -5;
    public static int RIGHT_POS_X = 4;

    public static float BLOCK_SIZE = 1f;

    //这些也可以根据gameobject读取子object的position来获取
    public static Vector3[] GetBlockOffsets(BlockTypeEnum blockType)
    {
        switch (blockType)
        {
            case BlockTypeEnum.OneByOne:
                return new Vector3[] { new Vector3(0, 0, 0) };
            case BlockTypeEnum.OneByThree:
                return new Vector3[] { new Vector3(0, 0, 0), new Vector3(0, 1, 0), new Vector3(0, -1, 0) };
            case BlockTypeEnum.TwoByTwo:
                return new Vector3[] { new Vector3(0, 0, 0), new Vector3(1, 0, 0), new Vector3(0, -1, 0), new Vector3(1, -1, 0) };
            case BlockTypeEnum.L:
                return new Vector3[] { new Vector3(0, 0, 0), new Vector3(0, -1, 0), new Vector3(0, -2, 0), new Vector3(1, -2, 0) };
            case BlockTypeEnum.T:
                return new Vector3[] { new Vector3(0, 0, 0), new Vector3(-1, 0, 0), new Vector3(1, 0, 0), new Vector3(0, -1, 0), new Vector3(0, -2, 0) };
            default:
                return new Vector3[] { new Vector3(0, 0, 0) };
        }
    }


}