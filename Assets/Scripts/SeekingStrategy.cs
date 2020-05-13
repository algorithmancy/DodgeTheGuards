using System.Collections.Generic;
using UnityEngine; 


public abstract class SeekingStrategy
{
    public abstract Vector2 ChooseWaypoint(ICharacter seeker, ICharacter target, IEnumerable<ICharacter> obstacles); 
}
