using System;
using System.Linq;
using UnityEngine;

public static class UtilityGameFuncs
{
    public static Vector2 ClampVector(Vector2 vector, int min_value, int max_value) {
        vector.x = Math.Clamp(vector.x, min_value, max_value);
        vector.y = Math.Clamp(vector.y, min_value, max_value);

        return vector;
    }

}