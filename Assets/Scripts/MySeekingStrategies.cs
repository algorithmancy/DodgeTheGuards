using System.Collections;
using System.Collections.Generic;
using UnityEngine;


class ZigZagSeekingStrategy : SeekingStrategy
{

    private int frameCount = 0;

    const int kFramesPerPhase = 60;

    public override Vector2 ChooseWaypoint(ICharacter seeker, ICharacter target, IEnumerable<ICharacter> guards)
    {
        ++frameCount;

        int phase = frameCount / kFramesPerPhase;

        if (frameCount < 180)
            return seeker.position + new Vector2(1, 0);

        if (frameCount < 420)
            return seeker.position + new Vector2(0, -1);


        if (frameCount < 480)
            return seeker.position + new Vector2(-1, 0);


        return target.position;

    }
}


public class FleeAllSeekingStrategy : SeekingStrategy
{
    const float kDangerZone = 2.0f;

    public override Vector2 ChooseWaypoint(ICharacter seeker, ICharacter target, IEnumerable<ICharacter> guards)
    {
        Vector2 sum = Vector2.zero;

        int nThreats = 0;

        foreach (var threat in guards)
        {
            if (threat == null)
                continue;

            // Don't worry about threats that are behind me 
            if (threat.IsBehindSeeker(seeker, target))
                continue;

            var toTarget = target.position - seeker.position;
            var toThreat = threat.position - seeker.position;

            float maxDistance = (threat.radius + seeker.radius) * kDangerZone;

            // This guard is too far away to care about. 
            if (toThreat.sqrMagnitude > maxDistance * maxDistance)
                continue;

            ++nThreats;

            // Run away from the threat, but at an angle 
            var lateral0 = -toThreat.Rotated(20);
            var lateral1 = -toThreat.Rotated(-20);

            // favor the lateral direction that gets us closer to the target 
            if (Vector2.Dot(lateral0, toTarget) > Vector2.Dot(lateral1, toTarget))
                sum += seeker.position + lateral0;
            else
                sum += seeker.position + lateral1;
        }

        // If we have no threats, just head towards the target
        if (nThreats == 0)
            return target.position;

        return sum / nThreats;
    }

}
