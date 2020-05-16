using System.Collections.Generic;
using UnityEngine; 


public class DirectSeekingStrategy: SeekingStrategy
{
    public override Vector2 ChooseWaypoint(ICharacter seeker, ICharacter target, IEnumerable<ICharacter> obstacles)
    {
       return target.position; 
    }
}


public class SpiralInwardSeekingStrategy : SeekingStrategy
{

    const float kWindingCoeff = 5.0f; 
    public override Vector2 ChooseWaypoint(ICharacter seeker, ICharacter target, IEnumerable<ICharacter> obstacles)
    {
        var toTarget = target.position - seeker.position;

        return seeker.position + toTarget + kWindingCoeff * toTarget.Rotated90(); 
    }
}


public class FleeClosestSeekingStrategy : SeekingStrategy
{
    const float kDangerZone = 1.1f;

    public override Vector2 ChooseWaypoint(ICharacter seeker, ICharacter target, IEnumerable<ICharacter> obstacles)
    {
        var threat = FindBiggestThreat(seeker, target, obstacles);

        if (threat == null)
            return target.position;

        var toTarget = target.position - seeker.position;


        var toThreat = threat.position - seeker.position;

        float maxDistance = (threat.radius + seeker.radius) * kDangerZone;

        if (toThreat.sqrMagnitude > maxDistance * maxDistance)
            return target.position;

        // Run away from the threat, but at an angle 
        var lateral0 = -toThreat.Rotated(20);
        var lateral1 = -toThreat.Rotated(-20);

        // favor the lateral direction that gets us closer to the target 
        if (Vector2.Dot(lateral0, toTarget) > Vector2.Dot(lateral1, toTarget))
            return seeker.position + lateral0;
        else
            return seeker.position + lateral1; 
    }

    private ICharacter FindBiggestThreat(ICharacter seeker, ICharacter target, IEnumerable<ICharacter> obstacles)
    {
        var toTarget = target.position - seeker.position;

        float maxDistanceForward = float.MaxValue;

        ICharacter biggestForwardThreat = null;

        foreach (var threat in obstacles)
        {
            var toThreat = threat.position - seeker.position;

            float score = toThreat.sqrMagnitude;


            if (Vector2.Dot(toThreat, toTarget) > 0)
            {
                if (score < maxDistanceForward)
                {
                    biggestForwardThreat = threat;
                    maxDistanceForward = score;
                }
            }

        }

        return biggestForwardThreat; 
    }
}


