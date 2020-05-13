using System;
using System.Linq; 
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager instance { get; private set;  }


    void Start()
    {
        RestartGame();         
    }

    private void Awake()
    {
        instance = this;
    }


    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (winner != null)
                RestartGame();
            else
                isPaused = !isPaused;
        }

        if (Input.GetMouseButtonDown(1))
        {
            var mouseInWorld = Camera.main.ScreenToWorldPoint(Input.mousePosition);

            GameObject.Instantiate(guardPrefab.gameObject, mouseInWorld.Replace(z: 0), Quaternion.identity, this.transform);
        }
    }

    private void FixedUpdate()
    {

        if (winner != null)
            return;


        // Get all the characters on the board. Note that they are subclasses of Character. 
        var characters = GetComponentsInChildren<Character>();

        CheckForVictory(characters);

        if (winner == null && !isPaused)
        {
            UpdateAllCharacters(characters, Time.fixedDeltaTime);
            mResumeText = "Resume"; 
        }
    }


    public Team? winner = null;

    public bool isPaused = true;
    private string mResumeText = "Start";


    private void UpdateAllCharacters(Character[] characters, float deltaTime)
    {

        // It would be better to hold onto this array and re-use it every frame. 
        var waypoints = new Vector2[characters.Length]; 


        // Phase 1: Choose all the waypoints.  
        // We want an index so we don't use foreach here. 
        for (int i = 0; i < characters.Length; ++i)
        {
            var mover = characters[i];

            var target = mover.GetTarget();

            // Don't use linq in an inner loop in a real game.           
            var obstacles = from c in characters where c.team != mover.team && c is Guard select c;

            waypoints[i] = mover.ChooseWaypoint(target, obstacles); 
        }

        // Phase 2: Move every character to its waypoint 
        for (int i = 0; i < characters.Length; ++i)
        {
            characters[i].MoveToWaypoint(waypoints[i], deltaTime); 
        }

        // separate overlapping characters 
        HandleOverlap(characters); 
    }

    private bool CheckForVictory(Character[] characters)
    {
        var hunters = characters.OfType<Hunter>();

        foreach (var hunter in hunters)
        {
            foreach (var other in characters)
            {
                if (other == hunter)
                    continue;

                // Check for overlap 
                float distSq = (hunter.position - other.position).sqrMagnitude;

                float radiusSum = hunter.radius + other.radius;

                if (distSq > radiusSum * radiusSum)
                    continue; 


                // we are overlapping 

                if (other is Guard)
                {
                    winner = Team.Quarry;
                    return true; 
                }
                else if (other == hunter.quarry)
                {
                    winner = Team.Hunter;
                    // DON'T return true 
                    // Quarry wins ties 
                }
            }

        }

        return winner != null; 
    }

    private const float kDisplacementCoeff = 0.5f; 

    public void HandleOverlap(Character[] characters)
    {
        for (int i=1; i < characters.Length; ++i)
        {
            for (int j = 0; j < i; ++j)
            {
                var first = characters[i];
                var second = characters[j];

                // enemies don't stay out of each other's way.  
                if (first.team != second.team)
                    continue;

                var delta = first.position - second.position;

                float radiusSum = first.radius + second.radius;

                // Check for overlap 

                if (delta.sqrMagnitude < radiusSum * radiusSum )
                {
                    float overlap = radiusSum - delta.magnitude;

                    var displacement = delta.normalized * (overlap * kDisplacementCoeff * 0.5f);

                    first.position += displacement;
                    second.position -= displacement; 
                }
            }
        }
    }

    public Hunter hunterPrefab;
    public Quarry quarryPrefab;
    public Guard guardPrefab;

    public Vector2 hunterStart = new Vector2(0, 3);
    public Vector2 quarryStart = new Vector2(0, -3);

    public Rect guardStartZone = new Rect(-Vector2.one, new Vector2(2, 2));
    private int numGuards = 1;

    public SeekingStrategy[] mSeekingStrategies;


    public void RestartGame()
    {
        // kill everything on the game board  
        while (transform.childCount > 0)
        {
            var child = transform.GetChild(0);

            child.SetParent(null);
            GameObject.Destroy(child.gameObject); 
        }


        // Now make some new ones 

        GameObject.Instantiate(hunterPrefab.gameObject, hunterStart, Quaternion.identity, this.transform);
        GameObject.Instantiate(quarryPrefab.gameObject, quarryStart, Quaternion.identity, this.transform);

        for (int i = 0; i < numGuards; ++i)
        {
            Vector2 guardStart;

            guardStart.x = UnityEngine.Random.Range(guardStartZone.xMin, guardStartZone.xMax);
            guardStart.y = UnityEngine.Random.Range(guardStartZone.yMin, guardStartZone.yMax);

            GameObject.Instantiate(guardPrefab.gameObject, guardStart, Quaternion.identity, this.transform);
        }

        // Re-instantiate the seeking strategies just in case they have state
        mSeekingStrategies = smSeekingStrategyTypes.Select(t => (SeekingStrategy)Activator.CreateInstance(t)).ToArray();

        mResumeText = "Start";
        winner = null; 

    }

    public static Type[] smSeekingStrategyTypes; 
    public static GUIContent[] smStrategyNames;
    static GameManager()
    {
        var strategies = from t in typeof(SeekingStrategy).Assembly.GetTypes()
                         where t.IsSubclassOf(typeof(SeekingStrategy))
                             && !t.IsAbstract
                         select t;

        smSeekingStrategyTypes = strategies.ToArray(); 

        smStrategyNames = strategies.Select(t => new GUIContent(t.Name.StripSuffix("SeekingStrategy"))).ToArray(); 
    }


    private static GUIStyle smCenteredTextStyle;

    public static SeekingStrategy selectedSeekingStrategy => instance.GetCurrentSeekingStrategy(); 

    private SeekingStrategy GetCurrentSeekingStrategy() => mSeekingStrategies[mStrategyIndex]; 

    private int mStrategyIndex = 0; 

    private void OnGUI()
    {
        var mouseInWorld = Camera.main.ScreenToWorldPoint(Input.mousePosition);


        GUILayout.BeginArea(Screen.safeArea);

        GUILayout.Label($"Mouse: {(Vector2)mouseInWorld}");


        GUILayout.FlexibleSpace();


        GUILayout.BeginHorizontal();

        GUILayout.Label("Guards: ");
        string guardsText = GUILayout.TextField(numGuards.ToString(), GUILayout.MinWidth(30));

        if (int.TryParse(guardsText, out int result))
            numGuards = result; 

        if (GUILayout.Button("Restart"))
            RestartGame();
        if (GUILayout.Button(isPaused ? "Resume" : "Pause"))
            isPaused = !isPaused;


        GUILayout.Label("Strategy: ");
        mStrategyIndex = GUILayout.SelectionGrid(mStrategyIndex, smStrategyNames, 8 ); 

        GUILayout.FlexibleSpace(); 


        GUILayout.EndHorizontal(); 
        GUILayout.EndArea();


        string centeredText = null;

        if (winner != null)
        {
            string victory = (winner == Team.Hunter) ? "Victory" : "Defeat";
            centeredText = victory; 
        }
        else if (isPaused)
        {
            centeredText = $"Paused (Space Bar to {mResumeText})";
        }


        if (centeredText != null)
        {
            if (smCenteredTextStyle == null)
            {
                smCenteredTextStyle = new GUIStyle(GUI.skin.GetStyle("Label"));
                smCenteredTextStyle.alignment = TextAnchor.MiddleCenter;
                smCenteredTextStyle.fontSize = 30; 
            }

            GUI.Label(Screen.safeArea, centeredText, smCenteredTextStyle); 
        }
    }
}
