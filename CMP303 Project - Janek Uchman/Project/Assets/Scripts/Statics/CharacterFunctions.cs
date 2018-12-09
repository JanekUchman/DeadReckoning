using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class CharacterFunctions{

    public static bool GroundCheck(Vector2 topLeftOfGroundCheck, Vector2 bottomRightOfGroundCheck)
    {
        foreach (var t in Layers.walkableLayersMask)
        {
           
            if (Physics2D.OverlapArea(topLeftOfGroundCheck, bottomRightOfGroundCheck, t))
                return true;
        }
        return false;
    }
}
