using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public enum PlayerCharacter
{
    Sal = 1,
    Rook = 2,
    Smith = 3
}

public enum PlayerState
{
    Idle,
    Walking,
    Vaulting,
    Attacking,
    InDialog,
    Aiming,
    PreparingToJump, // TODO: same as aiming maybe?
    Dying
}

public class Player : Actor
{
    [Header("Player Properties")]
    public PlayerState State = PlayerState.Idle;

    [Range(1, 10)]
    public int ShootingRange;
    [Range(1, 5)]
    public float walkingSpeed;
    [Range(5, 20)]
    public float vaultingSpeed;
    [Range(1, 5)]
    public int vaultCooldown;
    [SerializeField]
    int curVaultCooldown;

    public Vector3 walkingTo;

    public PlayerCharacter CurCharacter = PlayerCharacter.Sal;
    public PlayerCharacter CharacterUnlockState = PlayerCharacter.Sal;

    [SerializeField]
    ActorAnimationSet[] animations;
    [SerializeField]
    AudioClip[] tagInVoicelines;

    Guns guns;

    [SerializeField]
    GameObject reticle;

    [SerializeField]
    GameObject smithExplosionPrefab;
    [SerializeField]
    SpriteRenderer salVaultIndicator;
    [SerializeField]
    SpriteRenderer rookRangeIndicator;
    [SerializeField]
    GameObject ammoBar;

    public static Player Instance;

    /** to know what's in front */
    Vector3Int lastStep;

    /** this accurately describes Smith's swing pattern in relative positions from him (need to still be added to his current one);
     *  he's always swinging right to left (I have no idea why I made it this accurate, but it's fun!);
     *  usage: var hitFields = swingPatternSmith[swingDirection];
     */
    Dictionary<Vector3Int, Vector3Int[]> swingPatternSmith = new Dictionary<Vector3Int, Vector3Int[]>()
    {
        {
            Vector3Int.up, new Vector3Int[]
            {
                Vector3Int.right,
                Vector3Int.up + Vector3Int.right,
                Vector3Int.up,
                Vector3Int.up + Vector3Int.left,
                Vector3Int.left,
            }
        },
        {
            Vector3Int.right, new Vector3Int[]
            {
                Vector3Int.down,
                Vector3Int.right + Vector3Int.down,
                Vector3Int.right,
                Vector3Int.right + Vector3Int.up,
                Vector3Int.up,
            }
        },
        {
            Vector3Int.down, new Vector3Int[]
            {
                Vector3Int.left,
                Vector3Int.down + Vector3Int.left,
                Vector3Int.down,
                Vector3Int.down + Vector3Int.right,
                Vector3Int.right,
            }
        },
        {
            Vector3Int.left, new Vector3Int[]
            {
                Vector3Int.up,
                Vector3Int.left + Vector3Int.up,
                Vector3Int.left,
                Vector3Int.left + Vector3Int.down,
                Vector3Int.down
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
    Image ammoBarImage;

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
        ammoBarImage = ammoBar.GetComponent<Image>();
        curVaultCooldown = vaultCooldown;
    }

    // Update is called once per frame
    void Update()
    {
        if (State == PlayerState.Dying)
            return;

        if (CurHealth <= 0)
        {
            State = PlayerState.Dying;
            Die();
            return;
        }

        if (hpText != null)
            hpText.text = $"HP: {CurHealth}/{MaxHealth}";

        if (characterNameText != null)
            characterNameText.text = CurCharacter.ToString();

        //TODO: full on animation set switching instead of just sprites
        int characterSpriteIndex = (int)CurCharacter - 1; // 0 indexed
        if(characterSpriteIndex >= 0 && characterSpriteIndex < animations.Length)
        {
            animator.AnimationSet = animations[characterSpriteIndex];
        }

        if (MyTurn)
        {
            HandlePlayerInput();
        }
    }

    void HandlePlayerInput()
    {
        if(Input.GetKeyDown(KeyCode.F8))
        {
            Credits.Instance.Toggle();
            return;
        }

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
                        rookRangeIndicator.enabled = false;
                        salVaultIndicator.enabled = false;
                        State = PlayerState.Idle;
                        return;
                    }
                }
                #endregion

                if (CurCharacter == PlayerCharacter.Rook && Input.GetKeyDown(KeyCode.F))
                {
                    ammoBar.SetActive(true);
                    //rookRangeIndicator.enabled = true;
                    State = PlayerState.Aiming;                    
                }

                if (curVaultCooldown == 0 && CurCharacter == PlayerCharacter.Sal && Input.GetKeyDown(KeyCode.LeftAlt))
                {
                    salVaultIndicator.enabled = true;
                    State = PlayerState.PreparingToJump;                   
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
                var mouseWorldPos = World.Instance.MouseGridPos();
                var aimedGridPos = World.Instance.WorldToGridPos(mouseWorldPos);
                var lineOfSight = World.Instance.LineOfSight(GridPos, aimedGridPos, ShootingRange, true, false);
                int ammoLeft = Mathf.Max(0, ShootingRange + 1 - lineOfSight.Count);
                ammoBarImage.fillAmount = (1f / ShootingRange) * ammoLeft;
                if(aimedGridPos != lineOfSight.Last())
                {
                    ammoBarImage.fillAmount = 0f;
                    EnableReticle(Color.red);
                    break;
                } 
                
                EnableReticle(Color.cyan);
                
                if (Input.GetMouseButtonDown(0))
                {                                       
                    Enemy target = null;
                    foreach (var pos in lineOfSight)
                    {
                        target = World.Instance.GetActor(pos) as Enemy;
                        if (target != null)
                            break;
                    }

                    if (target != null)
                    {
                        rookRangeIndicator.enabled = false;                        
                        DisableReticle();
                        State = PlayerState.Idle;
                        if (guns != null)
                            guns.Shoot(mouseWorldPos, .2f);

                        target.ApplyDamage(AttackDamage);

                        World.Instance.SetTimeout(.2f, EndTurn);
                    }
                } 
                else if(Input.GetKeyDown(KeyCode.Escape))
                {
                    rookRangeIndicator.enabled = false;
                    DisableReticle();
                    State = PlayerState.Idle;
                }
                break;

            case PlayerState.Walking:
            case PlayerState.Vaulting:
                float speed = State == PlayerState.Walking ? walkingSpeed : vaultingSpeed;
                if (speed > 0)
                    transform.position = Vector3.MoveTowards(transform.position, walkingTo, speed * Time.deltaTime);
                else
                    transform.position = walkingTo;

                if (transform.position == walkingTo)
                {
                    EndTurn();
                }
                break;

            case PlayerState.PreparingToJump:
                var landingSpot = World.Instance.MouseGridPos();
                var relMovement = landingSpot - GridPos;
                if(!allowedVaultsSal.Contains(relMovement))
                {
                    EnableReticle(Color.red);
                    break;
                } 
                else
                {
                    EnableReticle(Color.blue);
                }
                if (Input.GetMouseButtonDown(0)) {                    
                    if(relMovement.magnitude > 1)
                    {
                        var intermediateSpot = GridPos + relMovement / 2;
                        if(World.Instance.IsSolid(intermediateSpot) && !World.Instance.IsJumpable(intermediateSpot))
                        {
                            break;
                        }
                    }
                    if(!World.Instance.IsSolid(landingSpot) && World.Instance.GetActor(landingSpot) == null)
                    {
                        // TODO: vault animation
                        salVaultIndicator.enabled = false;
                        VaultTo(landingSpot);
                    }
                }
                else if (Input.GetKeyDown(KeyCode.Escape))
                {
                    DisableReticle();
                    salVaultIndicator.enabled = false;
                    State = PlayerState.Idle;
                }
                break;
        }        
    }

    public override void TakeTurn()
    {
        MyTurn = true;

        curVaultCooldown = Mathf.Max(0, curVaultCooldown - 1);

        var thing = World.Instance.GetObject(GridPos);
        if (thing != null)
            InteractWithObject(thing);

        var convo = World.Instance.GetConversation(GridPos);
        if (convo != null)
        {
            Textbox.Instance.StartConversation(convo);
            State = PlayerState.InDialog;
            
            // only play convo ONCE
            World.Instance.SetConversation(null, GridPos);
        }
    } 

    void EndTurn()
    {
        DisableReticle(); // just in case
        State = PlayerState.Idle;
        MyTurn = false;
    }

    void EnableReticle(Color color)
    {
        var reticleRenderer = reticle.GetComponent<SpriteRenderer>();
        reticleRenderer.color = color;
        reticle.SetActive(true);
    }

    void DisableReticle()
    {
        ammoBar.SetActive(false);
        reticle.SetActive(false);
    }

    /// <summary>
    /// Does no checks, so check first if vault is possible
    /// </summary>
    /// <param name="gridPos"></param>
    void VaultTo(Vector3Int gridPos)
    {
        curVaultCooldown = vaultCooldown;
        var world = World.Instance;
        State = PlayerState.Vaulting;
        world.MoveActorTo(this, gridPos);
        walkingTo = world.GridToWorldPos(gridPos);
        animator.TriggerAnimation(ActorAnimationState.Walk, gridPos - GridPos);
        GridPos = gridPos;
    }
    void MoveTo(Vector3Int gridPos)
    {
        var world = World.Instance;

        var otherActor = world.GetActor(gridPos);

        if (otherActor != null)
        {
            animator.TriggerAnimation(ActorAnimationState.Idle, gridPos - GridPos);
            int turnsTaken = InteractWithActor(otherActor);
            if (turnsTaken > 0)
            {
                State = PlayerState.Attacking;
                animator.TriggerAnimation(ActorAnimationState.Attack, gridPos - GridPos);
                world.SetTimeout(.2f, EndTurn);
            }
            return;
        }

        var worldObject = world.GetObject(gridPos);

        if (worldObject != null)
        {
            animator.TriggerAnimation(ActorAnimationState.Idle, gridPos - GridPos);
            if (worldObject.Solid)
            {               
                InteractWithObject(worldObject);
                return;
            }
            // otherwise interaction happens at the start of next turn
        }
           
        if (world.IsSolid(gridPos))
        {
            if(CurCharacter != PlayerCharacter.Smith)
                return;

            if(world.IsBreakable(gridPos))
            {                
                State = PlayerState.Attacking;
                animator.TriggerAnimation(ActorAnimationState.Attack, gridPos - GridPos);

                // Explosion effects (should destroy themselves)
                Instantiate(smithExplosionPrefab, World.Instance.GridToWorldPos(gridPos), Quaternion.identity, transform);

                world.BreakTile(gridPos);
                world.SetTimeout(.2f, EndTurn);               
            }
            return;
        }
                        
        // At this point nothing is in our way :D       
        State = PlayerState.Walking;
        world.MoveActorTo(this, gridPos);
        walkingTo = world.GridToWorldPos(gridPos);
        animator.TriggerAnimation(ActorAnimationState.Walk, gridPos - GridPos);
        GridPos = gridPos;               
                        
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
                        // Explosion effects (should destroy themselves)
                        Instantiate(smithExplosionPrefab, World.Instance.GridToWorldPos(GridPos + relPosition), Quaternion.identity, transform);

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
                enemiesHit.ForEach(e => e.ApplyDamage(AttackDamage));
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

    private void OnDestroy()
    {
        //SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void UnlockCharacter(PlayerCharacter character)
    {
        if((int)CharacterUnlockState < (int)character)
        {
            CharacterUnlockState = character;
        } 
        else
        {
            Debug.LogError("Unlock character called with already unlocked character!");
        }
    }

    public override void Die()
    {
        animator.TriggerAnimation(ActorAnimationState.Death);
        animator.OnAnimationComplete += () => World.Instance.SetTimeout(1f, () => SceneManager.LoadScene(SceneManager.GetActiveScene().name));
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
        var lineOfSight = world.LineOfSight(GridPos, world.WorldToGridPos(mouseWorldPos), ShootingRange, true, false);        
        foreach (var pos in lineOfSight)
        {
            Gizmos.DrawSphere(world.GridToWorldPos(pos), .1f);
        }
        var from = world.GridToWorldPos(GridPos);
        Gizmos.DrawLine(from, from + (mouseWorldPos - from).normalized * (ShootingRange - .3f));
    }
}
