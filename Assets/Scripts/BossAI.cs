using UnityEngine;

public class BossAI : MonoBehaviour
{
    [Header("References")]
    public Transform player;
    public Animator animator;

    
    [Header("Boss Health")]
    [SerializeField] private float maxHealth = 300f;
    public float currentHealth;
    [SerializeField] private GameObject floatingTextPrefab;
    [SerializeField] private Transform textSpawnPoint;
    [SerializeField] private Transform worldCanvas;

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



    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        if (animator == null)
            animator = GetComponent<Animator>();

        currentPatrolTarget = rightPatrolPoint;
        currentHealth = maxHealth;
        worldCanvas = GameObject.Find("WorldSpaceCanvas").GetComponent<Transform>();
    }

    void Update()
    {
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
                Debug.Log("Do nothing.");

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
        if (animator.GetBool("IsHurting"))
        {
            rb.velocity = new Vector2(0, rb.velocity.y);
            animator.SetBool("isMoving", false);
            return;
        }

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

    public void DealDamage()
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

        currentHealth -= damageAmount;

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

        };

        if (IsDead())
        {
            Die();
        }
    }

    public void EndHurtAnimation()
    {
        if (animator)
        {
            animator.SetBool("IsHurting", false);
        }
    }

    public bool IsDead()
    {
        return currentHealth <= 0;
    }

    private void Die()
    {
        Debug.Log("Boss has died!");
        if (animator) animator.SetTrigger("Death");

        // Disable AI or controls
        this.enabled = false;

        // Disable collider or damage dealing
        //Collider2D col = GetComponent<Collider2D>();
        //if (col) col.enabled = false;

        // Optional: Destroy after delay
        Destroy(gameObject, 20f);
    }


    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackHitRange);
    }

    public void SpawnExplosionEffects()
    {
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


