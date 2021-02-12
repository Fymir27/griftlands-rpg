using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class TurnManager : MonoBehaviour
{
    public enum Phase
    {
        PlayerTurn,
        NPCTurn,
        EnemyTurn,
    }

    public static TurnManager Instance;

    [SerializeField]
    Phase curPhase;
    List<Enemy> enemies = new List<Enemy>();
    List<NPC> npcs = new List<NPC>();    

    void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        Player.Instance.TakeTurn();
    }

    // Update is called once per frame
    void Update()
    {
        switch (curPhase)
        {
            case Phase.PlayerTurn:
                if (!Player.Instance.MyTurn)
                {
                    //Debug.Log("NPC Turn");
                    foreach (var npc in npcs)
                        npc.TakeTurn();
                    curPhase = Phase.NPCTurn;
                }
                break;

            case Phase.NPCTurn:
                if (npcs.All(npc => !npc.MyTurn))
                {
                    //Debug.Log("Enemy Turn");
                    foreach (var enemy in enemies)
                        enemy.TakeTurn();
                    curPhase = Phase.EnemyTurn;
                }
                break;

            case Phase.EnemyTurn:
                if (enemies.All(enemies => !enemies.MyTurn))
                {
                    //Debug.Log("Player Turn");
                    Player.Instance.TakeTurn();
                    curPhase = Phase.PlayerTurn;                    
                }
                break;
        }
    }   

    public void EnqueueActor(Actor actor)
    {
        switch(actor)
        {
            case NPC npc:
                npcs.Add(npc);
                break;

            case Enemy enemy:
                enemies.Add(enemy);
                break;

            default:
                Debug.LogError("Trying to enqueue invalid actor type: " + typeof(Actor));
                break;
        }
    }

    public void RemoveActor(Actor actor)
    {
        switch (actor)
        {
            case NPC npc:
                npcs.Remove(npc);
                break;

            case Enemy enemy:
                enemies.Remove(enemy);
                break;

            case Player player:
                // This should only happen when the game closes/scene unloads
                break;

            default:
                Debug.LogError("Trying to remove invalid actor type: " + typeof(Actor));
                break;
        }
    }
}
