using System.Collections;
using System.Collections.Generic;
using System.Linq; 
using UnityEngine;



public static partial class Utils
{


    private static Vector2 ComputeInverseSquareAverage_Implementation(IEnumerable<Vector2> vectors)
    {
        Vector2 result = Vector2.zero;

        int nVectors = 0;

        foreach (var vector in vectors)
        {
            float magnitude = vector.magnitude;

            if (magnitude <= 0.0001)
                continue; 

            ++nVectors;

            result += vector / (magnitude * magnitude * magnitude);
        }

        if (nVectors > 0)
            result /= nVectors;

        return result;
    }


    public static Vector2 ComputeInverseSquareAverage(IEnumerable<Vector2> vectors) => ComputeInverseSquareAverage_Implementation(vectors); 

    public static Vector2 ComputeInverseSquareAverage(params Vector2[] vectors) => ComputeInverseSquareAverage_Implementation(vectors); 



}

public class RepulsionSeekingStrategy: SeekingStrategy
{
    const float kTargetWeight = .5f; 

    public override Vector2 ChooseWaypoint(ICharacter seeker, ICharacter target, IEnumerable<ICharacter> guards)
    {
        var guardOffsets = guards.Select(guard => seeker.position - guard.position);

        var repulsion = Utils.ComputeInverseSquareAverage(guardOffsets);

        var toTarget = (target.position - seeker.position).normalized * kTargetWeight;


        var relativeResult = (repulsion + toTarget).normalized * seeker.maxSpeed; 

        // add some random noise to break ties 
        relativeResult = relativeResult.Rotated(Random.Range(-1.0f, 1.0f)); 

        return seeker.position + relativeResult; 
    }
}

class GuardBlob
{
    private ICharacter mSeeker;
    private ICharacter mTarget;

    public int numGuards { get; private set; }
      
    private Vector2 mSegment0;
    private Vector2 mSegment1;

    public Vector2 segment0 => mSegment0;
    public Vector2 segment1 => mSegment1; 

    private Vector2 mClosestPoint; 

    public Vector2 toTarget { get; private set; }


    private LineRenderer debugLine; 


    public void Reset(ICharacter seeker, ICharacter target)
    {
        mSeeker = seeker;
        mTarget = target;
        numGuards = 0;
        toTarget = (target.position - seeker.position).normalized; 

        if (debugLine == null)
        {
            var gameBoard = GameManager.instance;

            var lineObject = GameObject.Instantiate(gameBoard.debugLinePrefab.gameObject, gameBoard.transform);

            debugLine = lineObject.GetComponent<LineRenderer>();

            debugLine.startColor = debugLine.endColor = Color.red; 
        }
    }

    public void AddGuard(ICharacter guard)
    {
        // find the closest guard point to the seeker 
        var position = Vector2.MoveTowards(guard.position, mSeeker.position, guard.radius); 

        switch (++numGuards)
        {
            case 1:
                mClosestPoint = mSegment0 = mSegment1 = position;
                break;

            case 2:
                mSegment1 = position;
                mClosestPoint = mSeeker.position.GetClosestPointOnSegment(mSegment0, mSegment1); 
                break;

            default:
                UpdateSegment(position);
                break; 
        }


        if (debugLine != null)
        {
            debugLine.positionCount = 2;
            debugLine.SetPosition(0, mSegment0);
            debugLine.SetPosition(1, mSegment1); 
        }
    }


    private void UpdateSegment(Vector2 newPoint)
    {
        var seekerPos = mSeeker.position; 

        float oldDistanceSq = (mSeeker.position - mClosestPoint).sqrMagnitude;

        var p0 = seekerPos.GetClosestPointOnSegment(mSegment0, newPoint);
        var p1 = seekerPos.GetClosestPointOnSegment(mSegment1, newPoint);

        float p0squared = (p0 - seekerPos).sqrMagnitude;
        float p1squared = (p1 - seekerPos).sqrMagnitude; 

        // Make mSegment0 the closer point 
        if (p1squared < p0squared)
        {
            Utils.Swap(ref p0, ref p1);
            Utils.Swap(ref p0squared, ref p1squared);
            Utils.Swap(ref mSegment0, ref mSegment1); 
        }

        if (p0squared < oldDistanceSq)
        {
            // swap out the further point 
            mSegment1 = newPoint;
            mClosestPoint = p0; 
        }

        // otherwise we stay the same! 
    }


    public Vector2? GetClosestPointToSeeker()
    {
        if (numGuards <= 0)
            return null;

        return mClosestPoint; 
    }

    public void Destroy()
    {
        if (debugLine != null)
        {
            GameObject.Destroy(debugLine.gameObject);

            debugLine = null;
        }
    }


}


public class SingleBlobSeekingStrategy : SeekingStrategy
{
    const float kTargetWeight = 0.5f;

    private GuardBlob mGuardBlob = new GuardBlob();

    public override Vector2 ChooseWaypoint(ICharacter seeker, ICharacter target, IEnumerable<ICharacter> guards)
    {
        mGuardBlob.Reset(seeker, target);

        foreach (var guard in guards)
            mGuardBlob.AddGuard(guard);

        if (mGuardBlob.numGuards <= 0)
            return target.position; 

        var seekerPos = seeker.position;

        var guardOffsets = guards.Select(guard => seekerPos - guard.position);


        var extraOffsets = new Vector2[]
        {
            seekerPos - mGuardBlob.segment0, 
            seekerPos - mGuardBlob.segment1, 
            seekerPos - mGuardBlob.GetClosestPointToSeeker().Value, 
        };

        var repulsion = Utils.ComputeInverseSquareAverage(guardOffsets.Concat(extraOffsets));

        var toTarget = (target.position - seeker.position).normalized * kTargetWeight;

        var relativeResult = (repulsion + toTarget).normalized * seeker.maxSpeed;

        // add some random noise to break ties 
        relativeResult = relativeResult.Rotated(Random.Range(-1.0f, 1.0f));

        return seeker.position + relativeResult;
    }


}


class BlobManager
{
    private Dictionary<ICharacter, ICharacter> mGuardForest = new Dictionary<ICharacter, ICharacter>();
    private Dictionary<ICharacter, int> mBlobMap = new Dictionary<ICharacter, int>(); 
    private List<GuardBlob> mBlobPool = new List<GuardBlob>(); 

    public void Reset()
    {
        mGuardForest.Clear();
        mBlobMap.Clear(); 
    }

    public ICharacter GetBlobLeader(ICharacter guard)
    {
        if (mGuardForest.TryGetValue(guard, out ICharacter commander))
        {
            if (commander != null && commander != guard)
                return mGuardForest[guard] = GetBlobLeader(commander);
        }
        else
        {
            mGuardForest[guard] = null;
        }

        // I'm my own boss
        return guard; 

    }

    public void MergeBlobs(ICharacter a, ICharacter b)
    {
        var leaderA = GetBlobLeader(a);
        var leaderB = GetBlobLeader(b);

        if (leaderA != leaderB)
            mGuardForest[leaderA] = leaderB; 
    }

    public IEnumerable<GuardBlob> BuildBlobs(ICharacter seeker, ICharacter target)
    {
        int nBlobs = 0;

        var guards = mGuardForest.Keys.ToList(); 

        // count the blobs 
        foreach (var guard in guards)
        {
            if (guard == GetBlobLeader(guard))
            {
                // allocate a blob index 
                mBlobMap[guard] = nBlobs++; 
            }
        }

        // create blobs 
        for (int i = mBlobPool.Count; i < nBlobs; ++i)
            mBlobPool.Add(new GuardBlob()); 

        // destroy extra blobs 
        for (int i = nBlobs; i < mBlobPool.Count; ++i)
            mBlobPool[i].Destroy();

        // reset blobs 
        for (int i = 0; i < nBlobs; i++)
            mBlobPool[i].Reset(seeker, target); 

        // populate the blobs 
        foreach (var guard in guards)
        {
            var leader = GetBlobLeader(guard);

            int index = mBlobMap[leader];


            var blob = mBlobPool[index];

            blob.AddGuard(guard); 
        }

        for (int i = 0; i < nBlobs; ++i)
            yield return mBlobPool[i]; 
    }
}


public class MultiBlobSeekingStrategy : SeekingStrategy
{
    const float kTargetWeight = 0.5f;

    const float kSeparationFactor = 4.0f; 


    private BlobManager mBlobManager = new BlobManager();

    private List<ICharacter> mGuardList = new List<ICharacter>(); 

    public override Vector2 ChooseWaypoint(ICharacter seeker, ICharacter target, IEnumerable<ICharacter> guards)
    {
        mBlobManager.Reset();

        mGuardList.Clear();
        mGuardList.AddRange(guards);


        // Loop over all pairs 
        for (int i = 1; i < mGuardList.Count; ++i)
        {
            var guardI = mGuardList[i];

            for (int j = 0; j < i; ++j)
            {
                var guardJ = mGuardList[j];

                float distanceThreshold = (guardI.radius + guardJ.radius) * kSeparationFactor;

                var delta = guardI.position - guardJ.position;

                if (delta.sqrMagnitude < distanceThreshold * distanceThreshold)
                    mBlobManager.MergeBlobs(guardI, guardJ); 
            }
        }


        var blobs = mBlobManager.BuildBlobs(seeker,target); 

        var seekerPos = seeker.position;

        var offsets = (from g in mGuardList select seekerPos - g.position)
            .Concat(from b in blobs select seekerPos - b.segment0)
            .Concat(from b in blobs select seekerPos - b.segment1)
            .Concat(from b in blobs select seekerPos - b.GetClosestPointToSeeker().Value)
            ; 

        var repulsion = Utils.ComputeInverseSquareAverage(offsets);

        var toTarget = (target.position - seeker.position).normalized * kTargetWeight;

        var relativeResult = (repulsion + toTarget).normalized * seeker.maxSpeed;

        // add some random noise to break ties 
        relativeResult = relativeResult.Rotated(Random.Range(-1.0f, 1.0f));

        return seeker.position + relativeResult;
    }


}





