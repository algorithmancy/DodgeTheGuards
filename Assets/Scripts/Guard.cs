using System.Collections.Generic;
using UnityEngine;
using System.Linq; 

public class Guard: Character
{
    public Hunter hunter;

    private void Start()
    {
        facing = 90;

    }

    public override Team team => Team.Quarry;

    public float leadTime = 0.25f;

    public override Vector2 ChooseWaypoint(ICharacter target, IEnumerable<ICharacter> obstacles)
    {
        var result = target.position + target.velocity * leadTime;
        return result;
    }

    public override ICharacter GetTarget()
    {
        if (hunter == null)
            hunter = transform.parent.GetComponentInChildren<Hunter>();

        return (ICharacter)hunter ?? this; 
    }






}
