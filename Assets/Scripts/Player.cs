using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

public enum PlayerCharacter
{
    Sal = 1,
    Rook = 2,
    Smith = 3
}

public enum PlayerState
{
    Idle,
    InAnimation,
    InDialog,
    Aiming,
    PreparingToJump // TODO: same as aiming maybe?
}

public class Player : Actor
{
    [Header("Player Properties")]
    public PlayerState State = PlayerState.Idle;

    [Range(1, 10)]
    public int ShootingRange;
    public PlayerCharacter CurCharacter = PlayerCharacter.Sal;
    public PlayerCharacter CharacterUnlockState = PlayerCharacter.Sal;

    [SerializeField]
    Sprite[] characterSprites;
    [SerializeField]
    AudioClip[] tagInVoicelines;

    Guns guns;

    [SerializeField]
    GameObject reticle;

    public static Player Instance;

    /** to know what's in front */
    Vector3Int lastStep;

    Action onTurnBegin;
    Action onTurnEnd;

    /** this accurately describes Smith's swing pattern in relative positions from him (need to still be added to his current one);
     *  he's always swinging right to left (I have no idea why I made it this accurate, but it's fun!);
     *  usage: var hitFields = swingPatternSmith[swingDirection];
     */
    Dictionary<Vector3Int, Vector3Int[]> swingPatternSmith = new Dictionary<Vector3Int, Vector3Int[]>()
    {
        {
            Vector3Int.up, new Vector3Int[]
            {
                Vector3Int.up + Vector3Int.right,
                Vector3Int.up,
                Vector3Int.up + Vector3Int.left,
            }
        },
        {
            Vector3Int.right, new Vector3Int[]
            {
                Vector3Int.right + Vector3Int.down,
                Vector3Int.right,
                Vector3Int.right + Vector3Int.up,
            }
        },
        {
            Vector3Int.down, new Vector3Int[]
            {
                Vector3Int.down + Vector3Int.left,
                Vector3Int.down,
                Vector3Int.down + Vector3Int.right,
            }
        },
        {
            Vector3Int.left, new Vector3Int[]
            {
                Vector3Int.left + Vector3Int.up,
                Vector3Int.left,
                Vector3Int.left + Vector3Int.down,
            }
        }
    };
    List<Vector3Int> allowedVaultsSal = new List<Vector3Int>()
    {
        Vector3Int.up * 2,
        Vector3Int.right * 2,
        Vector3Int.down * 2,
        Vector3Int.left * 2,
        
        // TODO: optional?
        Vector3Int.up + Vector3Int.right,
        Vector3Int.up + Vector3Int.left,
        Vector3Int.down + Vector3Int.right,
        Vector3Int.down + Vector3Int.left,        
    };

    SpriteRenderer spriteRenderer;

    private void Awake()
    {
        Instance = this;
    }

    // Start is called before the first frame update
    void Start()
    {
        InitActor();
        spriteRenderer = GetComponent<SpriteRenderer>();
        guns = GetComponentInChildren<Guns>();
    }

    // Update is called once per frame
    void Update()
    {
        if (CurHealth <= 0)
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);

        if (hpText != null)
            hpText.text = $"HP: {CurHealth}/{MaxHealth}";

        if (characterNameText != null)
            characterNameText.text = CurCharacter.ToString();

        //TODO: full on animation set switching instead of just sprites
        int characterSpriteIndex = (int)CurCharacter - 1; // 0 indexed
        if(characterSpriteIndex >= 0 && characterSpriteIndex < characterSprites.Length)
        {
            spriteRenderer.sprite = characterSprites[characterSpriteIndex];
        }

        if (MyTurn)
            HandlePlayerInput();        
    }

    void HandlePlayerInput()
    {
        switch (State)
        {
            case PlayerState.Idle:
                #region Character Selection Controls
                int characterIDSelected = 0;

                if (Input.GetKeyDown(KeyCode.Alpha1))
                    characterIDSelected = 1;
                else if (Input.GetKeyDown(KeyCode.Alpha2))
                    characterIDSelected = 2;
                else if (Input.GetKeyDown(KeyCode.Alpha3))
                    characterIDSelected = 3;

                if (characterIDSelected > 0)
                {
                    if (characterIDSelected > (int)CharacterUnlockState)
                    {
                        // TODO: UI feedback
                        Debug.LogWarning("Character not yet unlocked!");
                    }
                    else if (characterIDSelected != (int)CurCharacter)
                    {
                        // TODO: switch animation?
                        CurCharacter = (PlayerCharacter)characterIDSelected;
                        AudioController.Instance.PlayClip(tagInVoicelines[characterIDSelected - 1]); // 0-indexed
                    }
                }
                #endregion

                if (CurCharacter == PlayerCharacter.Rook && Input.GetKeyDown(KeyCode.F))
                {
                    State = PlayerState.Aiming;
                    StartAiming(Color.red);
                }

                if (CurCharacter == PlayerCharacter.Sal && Input.GetKeyDown(KeyCode.LeftAlt))
                {
                    State = PlayerState.PreparingToJump;
                    StartAiming(Color.blue);
                }

                #region Movement/Combat Controls
                // TODO: mouse controls?
                var hor = Input.GetAxisRaw("Horizontal");
                var ver = Input.GetAxisRaw("Vertical");


                if (hor > 0)
                {
                    lastStep = Vector3Int.right;
                    MoveTo(GridPos + lastStep);
                }
                else if (hor < 0)
                {
                    lastStep = Vector3Int.left;
                    MoveTo(GridPos + lastStep);
                }
                else if (ver > 0)
                {
                    lastStep = Vector3Int.up;
                    MoveTo(GridPos + lastStep);
                }
                else if (ver < 0)
                {
                    lastStep = Vector3Int.down;
                    MoveTo(GridPos + lastStep);
                }
                else if (Input.GetButtonDown("Jump"))
                {
                    World.Instance.TriggerStepOn(this, GridPos);
                    EndTurn();
                }
                #endregion

                break;

            case PlayerState.InDialog:
                // Advance dialogue AND check if done
                if (Input.GetButtonDown("Jump") && Textbox.Instance.AdvanceDialogue())
                {
                    State = PlayerState.Idle;
                }

                if (Input.GetKeyDown(KeyCode.Escape))
                {
                    Textbox.Instance.AbortDialogue();
                    EndTurn();
                }
                break;

            case PlayerState.Aiming:
                if (Input.GetMouseButtonDown(0))
                {
                    var mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                    var lineOfSight = World.Instance.LineOfSight(GridPos, World.Instance.WorldToGridPos(mouseWorldPos), ShootingRange);
                    Enemy target = null;
                    foreach (var pos in lineOfSight)
                    {
                        target = World.Instance.GetActor(pos) as Enemy;
                        if (target != null)
                            break;
                    }

                    if (target != null)
                    {
                        StopAiming();
                        State = PlayerState.InAnimation;
                        if (guns != null)
                            guns.Shoot(mouseWorldPos, .2f);

                        target.CurHealth -= AttackDamage;

                        World.Instance.SetTimeout(.2f, EndTurn);
                    }
                } 
                else if(Input.GetKeyDown(KeyCode.Escape))
                {
                    StopAiming();
                    State = PlayerState.Idle;
                }
                break;

            case PlayerState.InAnimation:
                break;

            case PlayerState.PreparingToJump:
                if (Input.GetMouseButtonDown(0)) {
                    var landingSpot = World.Instance.MouseGridPos();
                    var relMovement = landingSpot - GridPos;
                    if(relMovement.magnitude > 1)
                    {
                        var intermediateSpot = GridPos + relMovement / 2;
                        if(World.Instance.IsSolid(intermediateSpot) && !World.Instance.IsJumpable(intermediateSpot))
                        {
                            break;
                        }
                    }
                    if(allowedVaultsSal.Contains(relMovement) && !World.Instance.IsSolid(landingSpot) && World.Instance.GetActor(landingSpot) == null)
                    {
                        MoveTo(landingSpot);
                    }
                }
                else if (Input.GetKeyDown(KeyCode.Escape))
                {
                    StopAiming();
                    State = PlayerState.Idle;
                }
                break;
        }        
    }

    public override void TakeTurn()
    {
        if (onTurnBegin != null)
        {
            onTurnBegin();
            onTurnBegin = null;
        }

        MyTurn = true;
    } 

    void EndTurn()
    {
        if (onTurnEnd != null)
        {
            onTurnEnd();
            onTurnEnd = null;
        }

        StopAiming(); // just in case
        State = PlayerState.Idle;
        MyTurn = false;
    }

    void StartAiming(Color color)
    {
        var reticleRenderer = reticle.GetComponent<SpriteRenderer>();
        reticleRenderer.color = color;
        reticle.SetActive(true);
    }

    void StopAiming()
    {
        reticle.SetActive(false);
    }

    void MoveTo(Vector3Int gridPos)
    {
        var world = World.Instance;

        var otherActor = world.GetActor(gridPos);

        if (otherActor != null)
        {
            int turnsTaken = InteractWithActor(otherActor);
            if (turnsTaken > 0)
            {
                State = PlayerState.InAnimation;          
                world.SetTimeout(.2f, EndTurn);
            }
            return;
        }

        var worldObject = world.GetObject(gridPos);

        if (worldObject != null)
        {
            if (worldObject.Solid)
            {               
                InteractWithObject(worldObject);
                return;
            }

            // delayed dialogue to allow moving first
            onTurnBegin = () => InteractWithObject(worldObject);
        }
           
        if (world.IsSolid(gridPos))
        {
            if(CurCharacter != PlayerCharacter.Smith)
                return;

            if(world.IsBreakable(gridPos))
            {
                State = PlayerState.InAnimation;
                world.BreakTile(gridPos);
                world.SetTimeout(.2f, EndTurn);
                return;
            }
        }
                        
        // At this point nothing is in our way :D       
        State = PlayerState.InAnimation;
        world.MoveActorTo(this, gridPos);
        GridPos = gridPos;

        // TODO: anmiation
        transform.position = world.GridToWorldPos(gridPos);
                                
        world.SetTimeout(.2f, EndTurn);        
    }

    /// <param name="actor"></param>
    /// <returns>number of turns taken</returns>
    private int InteractWithActor(Actor actor)
    {
        switch (actor)
        {
            case NPC npc:
                State = PlayerState.InDialog;
                npc.Interact();
                return 0;

            case Enemy enemy:
                var enemiesHit = new List<Enemy>();
                if (CurCharacter == PlayerCharacter.Smith)
                {
                    foreach (var relPosition in swingPatternSmith[lastStep])
                    {
                        var possibleEnemy = World.Instance.GetActor(GridPos + relPosition) as Enemy;
                        if (possibleEnemy != null)
                            enemiesHit.Add(possibleEnemy);
                    }
                }
                else
                {
                    enemiesHit.Add(enemy);
                }

                // TODO: can Rook melee attack as well?

                // apply damage
                enemiesHit.ForEach(e => e.CurHealth -= AttackDamage);
                return 1;

            case null:                
                return 0;

            default:
                Debug.LogWarning("Unknown actor type: " + actor.GetType());
                return 1;
        }
    }

    private void InteractWithObject(WorldObject thing)
    {
        if (thing == null || !thing.Interactable)
            return;

        State = PlayerState.InDialog;
        thing.Interact();       
    }

    private void OnDrawGizmos()
    {
        if (State != PlayerState.Aiming)
            return;

        var world = World.Instance;
        if (World.Instance == null)
            return;
        var mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mouseWorldPos.z = 0;
        var lineOfSight = world.LineOfSight(GridPos, world.WorldToGridPos(mouseWorldPos), ShootingRange);
        foreach(var pos in lineOfSight)
        {
            Gizmos.DrawSphere(world.GridToWorldPos(pos), .1f);
        }
        var from = world.GridToWorldPos(GridPos);
        Gizmos.DrawLine(from, from + (mouseWorldPos - from).normalized * (ShootingRange - .3f));
    }
}
