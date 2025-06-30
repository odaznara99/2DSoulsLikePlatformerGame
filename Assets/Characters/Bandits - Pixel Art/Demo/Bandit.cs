using UnityEngine;
using System.Collections;

public class Bandit : MonoBehaviour {

    //[SerializeField] float      m_speed = 4.0f;
    [SerializeField] float      m_jumpForce = 7.5f;

    private Animator            m_animator;
    private Rigidbody2D         m_body2d;
    private Sensor_Bandit       m_groundSensor;
    public ParticleSystem       m_bloodSplash; // Reference to the blood splash particle system
    private bool                m_grounded = false;
    private bool                m_combatIdle = false;
    private bool                m_isDead = false;

    //Added: Odaz 09/29/2024    
    public Transform attackPoint; // Attach this to a point in the scene or a child of the enemy
    public float     moveSpeed = 2f; // Speed of the enemy
    public float     followRange = 10f; // Range in which the enemy follows the player
    public float     attackRange = 1.5f; // Range in which the enemy attacks the player    
    public float     attackCooldown = 1f; // Time between attacks
    public float     attackTiming = 0.5f; // Timing the end of Attack Animation
    public int       health = 100; // Health of the enemy
    public int       damage = 10; // Damage dealt to the player

    private Transform   player; // Reference to the player position
    private PlayerController  playerScript; // Reference to the player main script
    private float       lastAttackTime = 0f; // Track when the enemy last attacked
    private bool        isAttacking = false; // Track if the enemy is currently attacking
    private bool        isFacingRight = false; // Track which direction the enemy is facing
    private bool        isHurting = false; // Track when the bandit is being Hurt


    


    // Use this for initialization
    void Start () {
        m_animator      = GetComponent<Animator>();
        m_body2d        = GetComponent<Rigidbody2D>();
        m_groundSensor  = transform.Find("GroundSensor").GetComponent<Sensor_Bandit>();

        playerScript    = GameObject.Find("HeroKnight").GetComponent<PlayerController>();
        player          = GameObject.Find("HeroKnight").GetComponent<Transform>();
        attackPoint     = transform.Find("AttackPoint").GetComponent<Transform>();
        lastAttackTime  = Time.time - attackCooldown;

        // Ignore Collision Between Enemy
        Physics2D.IgnoreLayerCollision(6, 6);
    }
	
	// Update is called once per frame
	void Update () {

        //Reference to the Player if not assigned
        if (player == null)
        {
            player = GameObject.Find("HeroKnight").GetComponent<Transform>();
        }

        //Reference to the AttackPoint if not assigned
        if (attackPoint == null)
        {
            attackPoint = transform.Find("AttackPoint").GetComponent<Transform>();
        }

        // Calculate the distance between the enemy and the player
        float distanceToPlayer = Vector2.Distance(transform.position, player.position);

        // Calculate the distance between the enemy and the attack point
        float distanceToAttackPoint = Vector2.Distance(attackPoint.position, player.position);

        if (!m_isDead && !isHurting && !playerScript.playerIsDead)
        {
            //Follow Player
            if (distanceToPlayer <= followRange && distanceToAttackPoint > attackRange)
            {
                FollowPlayer();
                //m_combatIdle = true;
            }
            //Player Out of Range
            else
            {
                // Stop moving if outside the follow range or too close (attack range)
                StopMovingHorizontally();
                m_combatIdle = false;
            }

            //Attack the Player on Range
            if (distanceToPlayer <= attackRange)
            {
                m_combatIdle = true;
                StopMovingHorizontally();

                if (!isAttacking)
                {
                    StartCoroutine(AttackPlayer());
                }
            }
        }
        //Player is Dead
        else if (playerScript.playerIsDead) 
        {
            m_combatIdle = false;
        }
        //Enemy is Dead
        else
        {
            StopMovingHorizontally();
        }
        // Update sprite direction based on movement
        FlipSpriteBasedOnVelocity(); 

        //Check if character just landed on the ground
        if (!m_grounded && m_groundSensor.State()) {
            m_grounded = true;
            m_animator.SetBool("Grounded", m_grounded);
        }

        //Check if character just started falling
        if(m_grounded && !m_groundSensor.State()) {
            m_grounded = false;
            m_animator.SetBool("Grounded", m_grounded);
        }

        //Set AirSpeed in animator
        m_animator.SetFloat("AirSpeed", m_body2d.velocity.y);

       /* // -- Handle Animations --
        //Death
        if (Input.GetKeyDown("e")) {
            if(!m_isDead)
                m_animator.SetTrigger("Death");
            else
                m_animator.SetTrigger("Recover");

            m_isDead = !m_isDead;
        }
            
        //Hurt
        else if (Input.GetKeyDown("q"))
            m_animator.SetTrigger("Hurt");

        //Attack
        else if(Input.GetMouseButtonDown(0)) {
            m_animator.SetTrigger("Attack");
        }

        //Change between idle and combat idle
        else if (Input.GetKeyDown("f"))
            m_combatIdle = !m_combatIdle;

        //Jump
        else if (Input.GetKeyDown("space") && m_grounded) {
            m_animator.SetTrigger("Jump");
            m_grounded = false;
            m_animator.SetBool("Grounded", m_grounded);
            m_body2d.velocity = new Vector2(m_body2d.velocity.x, m_jumpForce);
            m_groundSensor.Disable(0.2f);
        }*/

        //Run
        if (Mathf.Abs(m_body2d.velocity.x) > Mathf.Epsilon)
            m_animator.SetInteger("AnimState", 2);

        //Combat Idle
        else if (m_combatIdle)
            m_animator.SetInteger("AnimState", 1);

        //Idle
        else
            m_animator.SetInteger("AnimState", 0);
    }

    // Method to follow the player
    void FollowPlayer()
    {
        // Calculate the direction to the player
        Vector2 direction = (player.position - transform.position).normalized;

        // Set Y to zero, so the enemy can only moves Horizontally
        direction.y = 0;

        // Maintain the current vertical velocity (y) to preserve gravity
        m_body2d.velocity = new Vector2(direction.x * moveSpeed, m_body2d.velocity.y);
    }

    // Method to attack the player
    IEnumerator AttackPlayer()
    {
        if (Time.time >= lastAttackTime + attackCooldown)
        {
            isAttacking = true;
            m_animator.SetTrigger("Attack");

            // Assuming you want to wait for the animation to reach a certain point before applying damage
            yield return new WaitForSeconds(attackTiming); // Adjust this timing as per your animation

            // Reference to Player Health
            PlayerHealth    playerHealth        = player.GetComponent<PlayerHealth>();

            if (playerHealth != null && !isHurting && !m_isDead)
            {
                Debug.Log("Enemy: Attacks the player!");
                playerHealth.TakeDamage(damage);               
            }
            else
            {
                Debug.Log("Enemy: Attack was Interrupted!");
            }

            lastAttackTime = Time.time; // Update the time of the last attack           
            isAttacking = false;
        }
    }

    // Method to receive damage when attacked by the player
    public IEnumerator TakeDamage(int damageAmount)
    {
        m_animator.SetTrigger("Hurt");
        health -= damageAmount;

        if (health <= 0)
        {
            Die();
        }
        else
        {
            isHurting = true;
            Instantiate(m_bloodSplash, transform.position, Quaternion.identity); // Instantiate blood splash effect
            StopMovingHorizontally();
            StopCoroutine(AttackPlayer());
            Debug.Log("Enemy took " + damageAmount + " damage! Remaining health: " + health);
            //Duration when the Enemy will be on Hurt State
            yield return new WaitForSeconds(1f);
            isHurting = false;
        }
    }

    // Method to destroy the enemy when its health reaches zero
    void Die()
    {
        Debug.Log("Enemy died!");
        m_animator.SetTrigger("Death");
        m_isDead = true;       
        Destroy(gameObject,1f); // Destroy the enemy object
        //GetComponent<Bandit>().enabled = false;
    }
    //Method to make the Bandit Jump
    void Jump() {

        m_animator.SetTrigger("Jump");
        m_grounded = false;
        m_animator.SetBool("Grounded", m_grounded);
        m_body2d.velocity = new Vector2(m_body2d.velocity.x, m_jumpForce);
        m_groundSensor.Disable(0.2f);
    }

    // Optional: For visual representation, you can use Gizmos to show the follow and attack ranges in the editor
    private void OnDrawGizmosSelected()
    {
        // Draw follow range
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, followRange);

        // Draw attack range around the attackPoint
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(attackPoint.position, attackRange);
    }

    void FlipSpriteBasedOnVelocity()
    {
        // Check the enemy's velocity on the X-axis to determine direction
        if (m_body2d.velocity.x > 0 && !isFacingRight)
        {
            // Moving right but currently facing left, so flip to face right
            FlipSprite();
        }
        else if (m_body2d.velocity.x < 0 && isFacingRight)
        {
            // Moving left but currently facing right, so flip to face left
            FlipSprite();
        }
    }

    void FlipSprite()
    {
        // Flip the sprite by changing the local scale on the X-axis
        isFacingRight = !isFacingRight;
        Vector3 localScale = transform.localScale;
        localScale.x *= -1; // Reverse the scale on X-axis
        transform.localScale = localScale;
    }

    void StopMovingHorizontally() {
        m_body2d.velocity = new Vector2(0, m_body2d.velocity.y);
    }
}
