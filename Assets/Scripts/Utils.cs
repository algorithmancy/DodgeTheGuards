using UnityEngine;
using System.Collections.Generic; 

public static partial class Utils
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

    public static Vector2 Rotated(this Vector2 vector, float degrees)
    {
        float radians = degrees * Mathf.Deg2Rad; 

        var by90 = vector.Rotated90();

        return vector * Mathf.Cos(radians) + by90 * Mathf.Sin(radians); 
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


    public static bool IsBehindSeeker(this ICharacter guard, ICharacter seeker, ICharacter target)
    {
        var toTarget = (target.position - seeker.position).normalized;

        var toGuard = guard.position - seeker.position;

        return Vector2.Dot(toGuard, toTarget) < -guard.radius; 
    }


    public static float Cross(this Vector2 a, Vector2 b) => a.x * b.y - a.y * b.x;

    // for syntactic consistency 
    public static float Dot(this Vector2 a, Vector2 b) => Vector2.Dot(a, b); 



    public static Vector2 GetClosestPointOnSegment(this Vector2 point, Vector2 segment0, Vector2 segment1)
    {
        var alongSegment = segment1 - segment0;

        var toPoint = point - segment0;

        float dotProduct = alongSegment.Dot(toPoint);

        float weight = dotProduct / alongSegment.sqrMagnitude;

        // We are relying on Lerp to clamp to (0,1) here 
        return Vector2.Lerp(segment0, segment1, weight); 
    }


    public static void Swap<T>(ref T a, ref T b)
    {
        var temp = a;
        a = b;
        b = temp; 
    }



}
