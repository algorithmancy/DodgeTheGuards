using UnityEngine;
using System.Collections.Generic; 

public static class Utils
{
    // Handy extension method that replaces the specifies values of a vector 
    public static Vector3 Replace (this Vector3 vector, float? x = null, float? y = null, float? z = null)
    {
        vector.x = x ?? vector.x;
        vector.y = y ?? vector.y;
        vector.z = z ?? vector.z;

        return vector; 
    }

    public static Vector2 Rotated90(this Vector2 vector)
    {
        return new Vector2(-vector.y, vector.x); 
    }

    public static Vector2 Rotated(this Vector2 vector, int degrees)
    {
        float radians = degrees * Mathf.Deg2Rad; 

        var by90 = vector.Rotated90();

        return vector * Mathf.Cos(radians) + by90 * Mathf.Sin(degrees); 
    }

    public static bool HasSuffix(this string original, string suffix)
    {
        if (suffix.Length > original.Length)
            return false;

        int offset = original.Length - suffix.Length; 

        for (int i = 0; i < suffix.Length; ++i)
        {
            if (suffix[i] != original[i + offset])
                return false; 
        }

        return true; 
    }


    public static string StripSuffix(this string original, string suffix)
    {
        if (original.HasSuffix(suffix))
            return original.Substring(0, original.Length - suffix.Length);

        return original; 
    }


}
