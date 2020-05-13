using System.Collections.Generic;
using UnityEngine;

public class Quarry: Character
{
    public override Team team => Team.Quarry;



    private class MouseTarget : ICharacter
    {
        public Vector2 position => Camera.main.ScreenToWorldPoint(Input.mousePosition);


        public float maxSpeed => 0;
        public float radius => 0;
        public Team team => Team.Quarry;
        public float facing => 0;
        public Vector2 velocity => Vector2.zero; 
    }

    private static MouseTarget smMouseTarget = new MouseTarget(); 


    // If the mouse button is held down, we seek the mouse.  
    // Otherwise we stand still 
    public override ICharacter GetTarget()
    {
        if (Input.GetMouseButton(0))
            return smMouseTarget;
        else 
            return this; 
    }

}
