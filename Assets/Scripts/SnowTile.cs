
using UnityEngine;


[CreateAssetMenu]
public class SnowTile : ScriptableObject
{
    public const int Width = 7;
    [SerializeField] float[] heights = new float[Width * Width];
    public float this[int i]
    {
        get => heights[i];
    }
    
    public float GetCoord(int x, int y)
    {
        return heights[Width * x + y];
    }

    public SnowTile Rotated()
    {
        SnowTile retval = new SnowTile();
        if(Width % 2 == 1)
        {
            //@NOTE: Corner-case for odd sized matrices
            int centerIndex = (Width / 2 * Width) + Width / 2;
            retval.heights[centerIndex] = heights[centerIndex];
        }
        for (int start = 0, end = Width-1;
            start <= end;
            start++, end--)
        {
            for (int i = start; i < end; i++)
            {
                int offset = i - start;

                int topIndex    = Width * start + i;
                int rightIndex  = Width * i + end;
                int bottomIndex = (Width * end) + end - offset;
                int leftIndex   = Width * (end - offset) + start;

                float top    = heights[topIndex];
                float right  = heights[rightIndex];
                float bottom = heights[bottomIndex];
                float left   = heights[leftIndex];

                retval.heights[topIndex] = left;
                retval.heights[rightIndex] = top;
                retval.heights[bottomIndex] = right;
                retval.heights[leftIndex] = bottom;
            }
        }
        return retval;
    }
}
