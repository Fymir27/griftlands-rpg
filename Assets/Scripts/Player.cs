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

public class Player : Actor
{
    [Header("Player Properties")]
    public bool Moving; // read: in action
    public bool Talking;
    public bool Aiming;
    [Range(1, 10)]
    public int ShootingRange;
    public PlayerCharacter CurCharacter = PlayerCharacter.Sal;
    public PlayerCharacter CharacterUnlockState = PlayerCharacter.Sal;

    [SerializeField]
    Sprite[] characterSprites;
    Guns guns;

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
        if(Talking)
        {
            #region Dialogue Controls
            // Advance dialogue AND check if done
            if (Input.GetButtonDown("Jump") && Textbox.Instance.AdvanceDialogue())
            {
                Talking = false;
                Moving = false;
            }

            if (Input.GetKeyDown(KeyCode.Escape))
            {
                Textbox.Instance.AbortDialogue();
                Talking = false;
                Moving = false;
            }
            #endregion
        }
        else if (!Moving)
        {
            #region Character Selection Controls
            int characterIDSelected = 0;

            if (Input.GetKeyDown(KeyCode.Alpha1)) 
                characterIDSelected = 1;
            else if (Input.GetKeyDown(KeyCode.Alpha2)) 
                characterIDSelected = 2;
            else if (Input.GetKeyDown(KeyCode.Alpha3)) 
                characterIDSelected = 3;

            if(characterIDSelected > 0)
            {
                if(characterIDSelected > (int)CharacterUnlockState)
                {
                    // TODO: UI feedback
                    Debug.LogWarning("Character not yet unlocked!");
                }
                else if(characterIDSelected != (int)CurCharacter)
                {
                    // TODO: switch animation?
                    CurCharacter = (PlayerCharacter)characterIDSelected;
                    Aiming = false;
                }
            }
            #endregion

            #region Aiming & Shooting
            if (CurCharacter == PlayerCharacter.Rook && Input.GetKeyDown(KeyCode.F))
            {
                Aiming = !Aiming;
            }

            if (Input.GetMouseButtonDown(0) && Aiming)
            {
                Moving = true;
                var mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);                
                var lineOfSight = World.Instance.LineOfSight(GridPos, World.Instance.WorldToGridPos(mouseWorldPos), ShootingRange);
                Enemy target = null;
                foreach(var pos in lineOfSight)
                {
                    target = World.Instance.GetActor(pos) as Enemy;
                    if (target != null)
                        break;
                }

                if(target != null)
                {
                    if (guns != null)
                        guns.Shoot(mouseWorldPos, .2f);

                    target.CurHealth -= AttackDamage;

                    World.Instance.SetTimeout(.2f, () =>
                    {
                        Moving = false;
                        MyTurn = false;
                    });

                    return;
                }
            }
            #endregion

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
                // Wait a turn
                MoveTo(GridPos);
            }
            #endregion
        }
    }

    public override void TakeTurn()
    {
        MyTurn = true;
    }  

    void MoveTo(Vector3Int gridPos)
    {
        if (Moving)
            return;

        // probably a wall
        // TODO: breakable walls?
        if (World.Instance.IsSolid(gridPos))
        {
            if(CurCharacter != PlayerCharacter.Smith)
                return;

            if(World.Instance.IsBreakable(gridPos))
            {
                Moving = true;
                World.Instance.BreakTile(gridPos);
                World.Instance.SetTimeout(.2f, () =>
                {
                    Moving = false;
                    MyTurn = false;
                });
                return;
            }
        }
            

        Moving = true;

        switch (World.Instance.GetActor(gridPos))
        {
            case NPC npc:                
                Talking = true;
                Textbox.Instance.Display(npc, "This is placeholder dialogue!");
                // Talking does not end the turn!
                return;

            case Enemy enemy:
                var enemiesHit = new List<Enemy>();
                if (CurCharacter == PlayerCharacter.Smith)
                {
                    foreach (var relPosition in swingPatternSmith[lastStep])
                    {
                        var possibleEnemy = World.Instance.GetActor(GridPos + relPosition) as Enemy;
                        if(possibleEnemy != null)
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
                break;

            case null:
                // Tile seems to be empty so lets move there!                
                World.Instance.MoveActorTo(this, gridPos);
                GridPos = gridPos;

                // TODO: anmiation
                transform.position = World.Instance.GridToWorldPos(gridPos);
                break;
        }

        World.Instance.SetTimeout(.2f, () =>
        {
            Moving = false;
            MyTurn = false;
        });
    }

    private void OnDrawGizmos()
    {
        if (!Aiming)
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
