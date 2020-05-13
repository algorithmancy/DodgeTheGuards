

using System.Collections.Generic;
using UnityEngine;

public enum Team
{
    Hunter,
    Quarry, 
}

public interface ICharacter
{
    float maxSpeed { get; }
    float radius { get; }
    Team team { get; }

    Vector2 position { get; }
    float facing { get;  }

    Vector2 velocity { get;  }
}

public abstract class Character: MonoBehaviour, ICharacter 
{
    public float maxSpeed = 1.0f; // coord units/sec
    public float turnRate = 360.0f;  // degrees.sec 

    public float radius = 0.5f;  

    public abstract Team team { get;  }

    public Vector2 position
    {
        get => transform.position;
        set => transform.position = transform.position.Replace(x: value.x, y: value.y); 
    }

    public float facing
    {
        get => transform.eulerAngles.z;
        set => transform.eulerAngles = transform.eulerAngles.Replace(z: value); 
    }

    public Vector2 velocity { get; private set;  }


    public virtual Vector2 ChooseWaypoint(ICharacter target, IEnumerable<ICharacter> obstacles) => target.position;

    public abstract ICharacter GetTarget(); 

    public void MoveToWaypoint(Vector2 waypoint, float deltaTime)
    {
        var lastPosition = position;

        position = Vector2.MoveTowards(lastPosition, waypoint, maxSpeed * deltaTime);

        velocity = (position - lastPosition) / deltaTime; 

        // face our direction of motion 
        if (velocity.sqrMagnitude > 0.001)
        {
            float currentFacing = facing;
            float desiredFacing = Mathf.Atan2(velocity.y, velocity.x) * Mathf.Rad2Deg;

            facing = Mathf.MoveTowardsAngle(currentFacing, desiredFacing, turnRate * deltaTime); 
        }

        // TODO: debug draw waypoint line 
    }


    // Create properties for fields 
    // This allows them to work in the editor and in unity 
    float ICharacter.maxSpeed => maxSpeed;
    float ICharacter.radius => radius;
    Team ICharacter.team => team; 




}
