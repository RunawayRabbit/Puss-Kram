using UnityEngine;

public class Utils
{
    public static int GetLayerNumberFromMask(LayerMask gridLayer)
    {
        uint mask = (uint)gridLayer.value;
        for (int i = 0; i < sizeof(int)*8; i++)
        {
            /* keep shifting until we find a bit. 
                * @TODO: This is janky and can probably be improved. */
            uint test = mask & (uint)(1 << i);
            if (test != 0)
            {
                return i;
            }
        }
        return -1;
    }

    public static void DrawDebugBox(Vector3 Center, Vector3 halfExtents, Color color, float duration)
    {
        Vector3 frontTopLeft     = new Vector3(-halfExtents.x,  halfExtents.y, -halfExtents.z) + Center;
        Vector3 frontTopRight    = new Vector3( halfExtents.x,  halfExtents.y, -halfExtents.z) + Center;
        Vector3 frontBottomLeft  = new Vector3(-halfExtents.x, -halfExtents.y, -halfExtents.z) + Center;
        Vector3 frontBottomRight = new Vector3( halfExtents.x, -halfExtents.y, -halfExtents.z) + Center;

        Vector3 backTopLeft      = new Vector3(-halfExtents.x,  halfExtents.y,  halfExtents.z) + Center;
        Vector3 backTopRight     = new Vector3( halfExtents.x,  halfExtents.y,  halfExtents.z) + Center;
        Vector3 backBottomLeft   = new Vector3(-halfExtents.x, -halfExtents.y,  halfExtents.z) + Center;
        Vector3 backBottomRight  = new Vector3( halfExtents.x, -halfExtents.y,  halfExtents.z) + Center;


        Debug.DrawLine(frontTopLeft, frontTopRight, color, duration);
        Debug.DrawLine(frontTopRight, frontBottomRight, color, duration);
        Debug.DrawLine(frontBottomRight, frontBottomLeft, color, duration);
        Debug.DrawLine(frontBottomLeft, frontTopLeft, color, duration);

        Debug.DrawLine(backTopLeft, backTopRight, color, duration);
        Debug.DrawLine(backTopRight, backBottomRight, color, duration);
        Debug.DrawLine(backBottomRight, backBottomLeft, color, duration);
        Debug.DrawLine(backBottomLeft, backTopLeft, color, duration);

        Debug.DrawLine(frontTopLeft, backTopLeft, color, duration);
        Debug.DrawLine(frontTopRight, backTopRight, color, duration);
        Debug.DrawLine(frontBottomRight, backBottomRight, color, duration);
        Debug.DrawLine(frontBottomLeft, backBottomLeft, color, duration);
    }
}
