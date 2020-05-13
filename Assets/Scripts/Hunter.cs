using System.Collections.Generic;
using UnityEngine;
using System.Linq; 

public class Hunter: Character
{
    public Quarry quarry;

    public SeekingStrategy strategy => GameManager.selectedSeekingStrategy; 

    public override  Team team => Team.Hunter;


    private void Start()
    {
        facing = -90; 
    }


    public override Vector2 ChooseWaypoint(ICharacter target, IEnumerable<ICharacter> obstacles)
    {
        var result =  strategy.ChooseWaypoint(this, target, obstacles);
        SetDebugWaypoint(result);
        return result; 
    }

    public override ICharacter GetTarget()
    {
        // if our quarry isn't set, just use any quarry on the board 
        if (quarry == null)
            quarry = transform.parent.GetComponentInChildren<Quarry>();

        return (ICharacter)quarry ?? this; 
    }

    public LineRenderer debugLine;



    private void SetDebugWaypoint(Vector2 waypoint)
    {
        if (debugLine == null)
            return;

        debugLine.positionCount = 2;

        debugLine.SetPosition(0, position);
        debugLine.SetPosition(1, waypoint);
    }


}
