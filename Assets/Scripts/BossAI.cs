using UnityEngine;
using UnityEngine.SceneManagement;

public class BossAI : MonoBehaviour
{
    [Header("Persistence")]
    [Tooltip("Unique ID for this boss within the scene. Set this to a non-empty string to make the boss permanently dead after being defeated (survives scene reloads and game restarts).")]
    public string persistentId;

    [Header("References")]
    public Transform player;
    public Animator animator;

    
    [Header("Boss Health")]
    [SerializeField] private float maxHealth = 300f;
    public float currentHealth;
    [SerializeField] private GameObject floatingTextPrefab;
    [SerializeField] private Transform textSpawnPoint;
    [SerializeField] private Transform worldCanvas;
    [SerializeField] private BossHealthUI bossHealthUI;


    [Header("Detection")]
    public BossFollowRangeTrigger followRangeScript;
    public float attackRange = 2f;
    public LayerMask playerLayer;

    [Header("Movement")]
    public float moveSpeed = 2f;
    public Transform leftPatrolPoint;
    public Transform rightPatrolPoint;

    [Header("Attack Settings")]
    public float attackDamage = 30f;
    public string[] attackTriggers = { "Attack1", "Attack2","Attack3" };
    public float attackCooldown = 2f;
    [SerializeField] private float attackHitRange = 1.5f;

    [Header("Explosion Settings")]
    public GameObject explosionEffects;
    public BossFollowRangeTrigger explosionRange;

    private Rigidbody2D rb;
    private bool isAttacking = false;
    private bool facingRight = true;
    private float lastAttackTime = -999f;

    

    [Header("Patrol Timing")]
    public float patrolPauseDuration = 2f; // 200 milliseconds
    [SerializeField] private float patrolPauseTimer = 0f;
    [SerializeField] private Transform currentPatrolTarget;
    [SerializeField] private bool hasArrivedAtPatrolPoint = false;


    [Header("Chase Break Settings")]
    [SerializeField] private float chaseDuration = 3f;       // how long to chase before stopping
    [SerializeField] private float restDuration = 1.5f;       // how long to rest before chasing again

    private float chaseTimer = 0f;
    private float restTimer = 0f;
    private bool isResting = false;
    private bool isDead = false;



    void Start()
    {
        // If this boss has a persistent ID and was already defeated, remove it
        // immediately so it does not appear in the scene again.
        if (!string.IsNullOrEmpty(persistentId) &&
            SaveManager.Instance != null &&
            SaveManager.Instance.GetObjectState(SceneManager.GetActiveScene().name, persistentId))
        {
            Destroy(gameObject);
            return;
        }

        rb = GetComponent<Rigidbody2D>();
        if (animator == null)
            animator = GetComponent<Animator>();

        currentPatrolTarget = rightPatrolPoint;
        currentHealth = maxHealth;
        worldCanvas = GameObject.Find("WorldSpaceCanvas").GetComponent<Transform>();
        player = GameObject.FindGameObjectWithTag("Player").transform;

        if (bossHealthUI != null)
        {
            bossHealthUI.SetMaxHealth(maxHealth);
        }

        //bossHealthUI.SetHealthUIActive(false);
    }

    void Update()
    {
        if (isDead) return;

        if (!player)
        {
            player = GameObject.FindGameObjectWithTag("Player").transform;
        }

        animator.SetFloat("XVelocity", Mathf.Abs(rb.velocity.x));
        animator.SetFloat("YVelocity", rb.velocity.y);

        if (isAttacking)
            return;

        // Handle Resting State
        if (isResting)
        {
            restTimer -= Time.deltaTime;
            if (restTimer <= 0f)
            {
                isResting = false;
                chaseTimer = 0f;
            }

            rb.velocity = Vector2.zero;
            animator.SetBool("isMoving", false);
            return; // Skip follow/attack/patrol while resting
        }



        if (followRangeScript.IsPlayerInArea && player != null 
            && player.GetComponent<PlayerHealth>().IsDead() == false)
        {
            float dist = Vector2.Distance(transform.position, player.position);

            if (dist <= attackRange && Time.time >= lastAttackTime + attackCooldown)
            {
                Attack();
            }
            else if (dist <= attackRange)
            {
                //Debug.Log("Do nothing.");

            }
            else
            {
                FollowPlayer();
            }
        }
        else
        {
            Patrol();
        }
    }

    private void FollowPlayer()
    {
        if (player == null) return;

        if (GetHurtBool()) return;

        chaseTimer += Time.deltaTime;

        float distance = Vector2.Distance(transform.position, player.position);

        // If chase too long and still not in attack range → rest
        if (chaseTimer >= chaseDuration && distance > attackRange)
        {
            isResting = true;
            restTimer = restDuration;
            rb.velocity = Vector2.zero;
            animator.SetBool("isMoving", false);
            animator.SetTrigger("Taunt");
            return;
        }

        // Otherwise, keep chasing
        Vector2 direction = (player.position - transform.position).normalized;
        Move(direction.x);
    }

    private void Patrol()
    {
        if (GetHurtBool()) return;

        if (hasArrivedAtPatrolPoint)
        {
            patrolPauseTimer -= Time.deltaTime;
            patrolPauseTimer = Mathf.Max(patrolPauseTimer, 0f);

            rb.velocity = new Vector2(0f, rb.velocity.y);
            animator.SetBool("isMoving", false);

            if (patrolPauseTimer <= 0f)
            {
                hasArrivedAtPatrolPoint = false;
                currentPatrolTarget = currentPatrolTarget == leftPatrolPoint ? rightPatrolPoint : leftPatrolPoint;
            }

            return;
        }

        float xDistance = Mathf.Abs(transform.position.x - currentPatrolTarget.position.x);

        if (xDistance < 0.1f)
        {
            hasArrivedAtPatrolPoint = true;
            patrolPauseTimer = patrolPauseDuration;
            rb.velocity = new Vector2(0f, rb.velocity.y);
            animator.SetBool("isMoving", false);
            return;
        }

        float dir = currentPatrolTarget.position.x > transform.position.x ? 1 : -1;
        Move(dir);
    }




    private void Move(float dir)
    {
        if (GetHurtBool()) return;

        rb.velocity = new Vector2(dir * moveSpeed, rb.velocity.y);
        animator.SetBool("isMoving", true);

        // Flip
        if ((dir > 0 && !facingRight) || (dir < 0 && facingRight))
            Flip();
    }

    private void Flip()
    {
        facingRight = !facingRight;
        Vector3 scale = transform.localScale;
        scale.x *= -1;
        transform.localScale = scale;
    }

    private void Attack()
    {
        if (GetHurtBool()) return;

        // Ensure the boss is facing the player
        if ((player.position.x > transform.position.x && !facingRight) ||
            (player.position.x < transform.position.x && facingRight))
        {
            Flip();
        }

        isAttacking = true;
        rb.velocity = Vector2.zero;
        //animator.SetBool("isMoving", false);

        string randomTrigger = attackTriggers[Random.Range(0, attackTriggers.Length)];
        animator.SetTrigger(randomTrigger);

        lastAttackTime = Time.time;
    }



    // Called via Animation Event at end of attack
    public void EndAttack()
    {
        isAttacking = false;
    }

    public void DestroySelf() // Called via Animation Event at end of attack
    {
        Destroy(gameObject);
    }

    bool GetHurtBool()
    {
      return animator.GetBool("IsHurting");
    }

    public void DealDamage() // this is called via Animation Event at end of attack
    {
        Collider2D hit = Physics2D.OverlapCircle(transform.position, attackHitRange, playerLayer);

        if (hit != null)
        {
            player.GetComponent<PlayerHealth>().TakeDamage(attackDamage,this.gameObject,3f,3f);
        }
    }

    public void TakeDamage(float damageAmount)
    {
        if (IsDead()) return;

        if (isAttacking) return;

        if (GetHurtBool()) return;

        currentHealth -= damageAmount;
        currentHealth = Mathf.Clamp(currentHealth, 0f, maxHealth);

        // Optional: Show floating damage text
        if (floatingTextPrefab && worldCanvas)
        {
            GameObject ft = Instantiate(floatingTextPrefab, textSpawnPoint.position, Quaternion.identity, worldCanvas.transform);
            ft.GetComponent<FloatingText>().SetText(damageAmount.ToString());
        }

        // Optional: Trigger Hurt animation
        if (animator) 
        {
            animator.SetTrigger("Hurt");
            animator.SetBool("IsHurting", true);
            // Play Hurting Sounds of the Dragon Lord
            AudioManager.Instance.PlaySFX("DragonHurt");

        };

        if (bossHealthUI != null)
        {
            bossHealthUI.SetHealth(currentHealth);
        }

        if (IsDead())
        {
            Die();
        }

        Invoke(nameof(EndHurtAnimation), 0.5f); // Adjust time as needed
    }



    public void EndHurtAnimation() // this is called via Animation Event at end of Hurt
    {
        
        
            animator.ResetTrigger("Hurt");
            animator.SetBool("IsHurting", false);
        
    }

    public bool IsDead()
    {
        return currentHealth <= 0;
    }

    private void Die()
    {
        //Debug.Log("Boss has died!");
        // Stop Velocity
        rb.velocity = Vector2.zero;
        // Stop Fight Music
        AudioManager.Instance.StopFightMusic();
        // Show Victory Message
        MessageManager.Instance.victoryAchievedText.FadeInThenOut();
        // Play death animation
        if (animator) animator.SetTrigger("Death");
        // Play Dying Sounds
        AudioManager.Instance.PlaySFX("DragonDie");
        isDead = true;

        // Persist the defeat so this boss is not respawned in future sessions.
        if (!string.IsNullOrEmpty(persistentId) && SaveManager.Instance != null)
            SaveManager.Instance.SetObjectState(SceneManager.GetActiveScene().name, persistentId, true);

        // Disable AI or controls
        this.enabled = false;

        // Disable collider or damage dealing
        //Collider2D col = GetComponent<Collider2D>();
        //if (col) col.enabled = false;

        // Optional: Destroy after delay
        //Destroy(gameObject, 20f);
    }


    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackHitRange);
    }

    public void SpawnExplosionEffects()
    {
        CameraShake.Instance.Shake(0.5f, 0.5f); // Shake the camera for 0.5 seconds with a magnitude of 0.5
        AudioManager.Instance.PlaySFX("Explosion");
        if (explosionEffects)
        {
            Instantiate(explosionEffects, transform.position, Quaternion.identity);
        }

        if (explosionRange.IsPlayerInArea)
        {
                player.GetComponent<PlayerHealth>().TakeDamage(attackDamage, this.gameObject, 3f, 3f);
        }
    }
}


