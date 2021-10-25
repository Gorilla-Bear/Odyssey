using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class NewPlayer : MonoBehaviour  {
        [SerializeField] private float m_MaxSpeed = 15f;                            // The fastest the player can travel in the x axis.
        [SerializeField] private float m_JumpForce = 400f;                      // Amount of force added when the player jumps.
        [SerializeField] private int m_NumberOfDashes = 1;                      // Determines the number of jumps in mid air allowed    
        [SerializeField] private LayerMask m_WhatIsGround;                      // A mask determining what is ground to the character
        [Range(0,1)][SerializeField] private float m_RememberGroundedFor = 1f;  // Determines how long the player will stay grounded for
        [SerializeField] private float m_FallMultiplier = 2.5f;                 // Used to determine fall speed
        [SerializeField] private float m_LowJumpMultiplier = 2f;                // Used to determine low jump  
        [SerializeField] private float m_DashForce;                             // Amount of force added when the player dashes
        [SerializeField] private float m_StopMultiplier;                        // Used to determine stopping rate
        [SerializeField] private float m_DashDuration;                          // Determines how long the dash lasts
        [SerializeField] private float m_VerticalDashMultiplier;                // Determines the multiplier for the vertical portion of dash
        [SerializeField] private float m_MaxVerticalSpeed;                      // Determines max vertical speed
        [SerializeField] private float m_AdditionalHorizontalDash;              // Extra time for Horizontal dash
        [SerializeField] private float m_AdditionalVerticalDash;

        private Transform Feet;             // A position marking where to check if the player is grounded.
        private bool m_Grounded;            // Used to determine if player is on the ground
        const float k_GroundedRadius = .2f; // Radius of the overlap circle to determine if grounded
        private Rigidbody2D m_Rigidbody2D;
        private int m_DashesLeft;        // Used to keep track of remaining mid air jumps
        private float lastTimeGrounded;     // Used to keep track of time since leaving the ground
        private float hx;                   // Horizontal input
        private float vx;                   // Vertical input
        private bool toDash;                // Dash input
        private bool MidDash;               // If MidDash
        private float dashStartTime;        // Used to keep track of time since the start of the dash
        private float dashInputX;           // Used to store and use x component of dash input
        private float dashInputY;           // Used to store and use y component of dash input
        private bool dashStateAssist;   
        private Animator animator;
        private string currentState;
        private bool skip;

        // Animation States
        const string PlayerRun = "Player_run";
        const string PlayerIdle = "Player_idle";
        const string PlayerJump = "Player_jump1";
        const string PlayerDash = "Player_dash";
        const string PlayerDashFall = "Player_dashfall";

    // Start is called before the first frame update
    void Start() {
        m_Rigidbody2D = GetComponent<Rigidbody2D>();
        Feet = transform.Find("Feet");
        animator = GetComponent<Animator>();
    }

    private void ChangeAnimationState(string newState) {
        if (currentState == newState) {
        }
        // plays the animation
        animator.Play(newState);

        currentState = newState;
    }

    // Update is called once per frame
    // Used mainly for input
    private void Update() {
        hx = Input.GetAxisRaw("Horizontal");
        vx = Input.GetAxisRaw("Vertical");
        if (Input.GetKeyDown(KeyCode.Escape)) {
            Time.timeScale = 0f;
        }
        if (Input.GetKeyDown(KeyCode.Space) && !MidDash) {
            dashInputX = hx;
            dashInputY = vx;
            MidDash = true;
            dashStateAssist = true;
        }
        if (Input.GetKey(KeyCode.Tab) && Input.GetKey(KeyCode.Q)) {
                int nextSceneToLoad = SceneManager.GetActiveScene().buildIndex + 1; 
                SceneManager.LoadScene(nextSceneToLoad);
        }
    }

    // FixedUpdate may be called more than once per frame, depends on number of physics frames
    void FixedUpdate() {
        Move();
        Jump();
        Dash();
        TweakJump();
        TweakMovement();
        CheckIfGrounded();
        Flip();
        RunAnimations();
        DashAnimations();
    }

    // Move the character
    public void Move()  {
        if (Math.Abs(m_Rigidbody2D.velocity.x) <= m_MaxSpeed && !MidDash) {
            m_Rigidbody2D.AddForce(new Vector2(hx * m_MaxSpeed * 10, 0f));
        }
    }

    private void Jump() {
        if (vx > 0 && (m_Grounded || Time.time - lastTimeGrounded <= m_RememberGroundedFor) && !MidDash) {
            m_Rigidbody2D.velocity = new Vector2(m_Rigidbody2D.velocity.x, 0f);
            m_Rigidbody2D.AddForce(new Vector2(0f, m_JumpForce));
            ChangeAnimationState(PlayerJump);
        }
    }

    // To make the jump feel better
    private void TweakJump() {
        if (!MidDash) {
            if (m_Rigidbody2D.velocity.y < 0) {
                m_Rigidbody2D.velocity += Vector2.up * Physics2D.gravity * (m_FallMultiplier - 1) * Time.deltaTime;
            } else if (m_Rigidbody2D.velocity.y > 0 && vx <= 0) {
                m_Rigidbody2D.velocity += Vector2.up * Physics2D.gravity * (m_LowJumpMultiplier - 1) * Time.deltaTime;
            } else if (vx < 0) {
                m_Rigidbody2D.velocity = new Vector2(m_Rigidbody2D.velocity.x, 0f);
                m_Rigidbody2D.velocity += Vector2.up * Physics2D.gravity * (m_FallMultiplier - 1) * Time.deltaTime;
            }
        }
    }

    // To make lateral movement feel better
    private void TweakMovement() {
        if (!MidDash) {
            if (Math.Abs(m_Rigidbody2D.velocity.x) >= m_MaxSpeed) {
                if (m_Rigidbody2D.velocity.x < 0) {
                    m_Rigidbody2D.velocity = new Vector2(-1 * m_MaxSpeed, m_Rigidbody2D.velocity.y);
                } else {
                    m_Rigidbody2D.velocity = new Vector2(m_MaxSpeed, m_Rigidbody2D.velocity.y);    
                }
            }  else if (Math.Abs(m_Rigidbody2D.velocity.x) < 1) {
                m_Rigidbody2D.velocity = new Vector2(0f, m_Rigidbody2D.velocity.y);
            } else if (m_Rigidbody2D.velocity.x > 0) {
                if (hx < 0) {
                    m_Rigidbody2D.velocity += Vector2.left * m_StopMultiplier * Time.deltaTime;
                } else if (hx == 0) {
                    m_Rigidbody2D.velocity += Vector2.left * m_StopMultiplier * 2/3 * Time.deltaTime;
                }
            } else if (m_Rigidbody2D.velocity.x < 0) {
                if (hx > 0) {
                    m_Rigidbody2D.velocity += Vector2.right * m_StopMultiplier * Time.deltaTime;
                } else if (hx == 0) {
                    m_Rigidbody2D.velocity += Vector2.right * m_StopMultiplier * 2/3 * Time.deltaTime;
                }
            }
        }
    }

    private void CheckIfGrounded() {
        Collider2D collider = Physics2D.OverlapCircle(Feet.position, k_GroundedRadius, m_WhatIsGround);
        if (collider != null) { 
            m_Grounded = true;
            DashReset();
        } else { 
            if (m_Grounded) {
                lastTimeGrounded = Time.time;
            }
            m_Grounded = false;
        } 
    }

    public void DashReset() {
        m_DashesLeft = m_NumberOfDashes;
        dashStateAssist = false;
    }

    private void Dash() {
        if (MidDash)  {
            if (m_DashesLeft > 0 && !m_Grounded && (dashInputY != 0 || dashInputX != 0) && dashStateAssist) {
                dashStartTime = Time.time;
                m_Rigidbody2D.velocity = new Vector2(dashInputX * m_DashForce, dashInputY * m_DashForce * m_VerticalDashMultiplier);
                m_DashesLeft--;
            } else if (Time.time - dashStartTime <= m_DashDuration || (Time.time - dashStartTime <= m_DashDuration + m_AdditionalHorizontalDash
                         && dashInputX != 0 && dashInputY == 0) || (Time.time - dashStartTime <= m_DashDuration + m_AdditionalVerticalDash
                         && dashInputX == 0 && dashInputY != 0)) {
                m_Rigidbody2D.velocity = new Vector2(dashInputX * m_DashForce, dashInputY * m_DashForce * m_VerticalDashMultiplier);
            } else {
                MidDash = false;
                ChangeAnimationState(PlayerDashFall);
            }
        }
    }

     private IEnumerator ItemTimeRespawn(Collider2D collision) {
        yield return new WaitForSeconds(2.5f);
        collision.gameObject.SetActive(true);
    }

    private void OnTriggerEnter2D(Collider2D info) {
        if (info.tag == "DashReset") {
            // Object Respawn
            StartCoroutine(ItemTimeRespawn(info));
            // Remove Object
            info.gameObject.SetActive(false);
            // Dash Reset
            this.DashReset();
        }
    } 

    // Animation Control
    
    // Flip control
    private void Flip() {
        if (hx < 0) {
            transform.localScale = new Vector2(-3, 3);
        } else if (hx > 0) {
            transform.localScale = new Vector2(3, 3);
        }
    }
    
    // Run control
    private void RunAnimations() {
        if (m_Grounded && !MidDash) {
            if (m_Rigidbody2D.velocity.x != 0) {
                ChangeAnimationState(PlayerRun);
            } else {
                ChangeAnimationState(PlayerIdle);
            }
        }
    }

    // Dash Animation Control
    private void DashAnimations() {
        if (MidDash && (dashInputY != 0 || dashInputX != 0)) {
            ChangeAnimationState(PlayerDash);
        } else if (m_DashesLeft > 0 && !m_Grounded) {
            ChangeAnimationState(PlayerJump);
        }
    }
}
