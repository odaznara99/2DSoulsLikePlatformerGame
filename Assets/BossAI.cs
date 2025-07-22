using UnityEngine;

public class BossAI : MonoBehaviour
{
    [Header("References")]
    public Transform player;
    public Animator animator;

    [Header("Detection")]
    public float followRange = 8f;
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

    private Rigidbody2D rb;
    private bool playerInArea = false;
    private bool isAttacking = false;
    private bool facingRight = true;
    private float lastAttackTime = -999f;

    

    [Header("Patrol Timing")]
    public float patrolPauseDuration = 2f; // 200 milliseconds
    [SerializeField] private float patrolPauseTimer = 0f;
    [SerializeField] private Transform currentPatrolTarget;
    [SerializeField] private bool hasArrivedAtPatrolPoint = false;


    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        if (animator == null)
            animator = GetComponent<Animator>();

        currentPatrolTarget = rightPatrolPoint;
    }

    void Update()
    {

        animator.SetFloat("XVelocity", Mathf.Abs(rb.velocity.x));
        animator.SetFloat("YVelocity", rb.velocity.y);

        if (isAttacking)
            return;


        if (playerInArea && player != null)
        {
            float dist = Vector2.Distance(transform.position, player.position);

            if (dist <= attackRange && Time.time >= lastAttackTime + attackCooldown)
            {
                Attack();
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
            //animator.SetBool("isMoving", false);

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
            //animator.SetBool("isMoving", false);
            return;
        }

        float dir = currentPatrolTarget.position.x > transform.position.x ? 1 : -1;
        Move(dir);
    }




    private void Move(float dir)
    {
        rb.velocity = new Vector2(dir * moveSpeed, rb.velocity.y);
        //animator.SetBool("isMoving", true);

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
            PlayerHealth.Instance.TakeDamage(attackDamage,this.gameObject,3f,3f);
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackHitRange);
    }



    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (((1 << collision.gameObject.layer) & playerLayer) != 0)
        {
            playerInArea = true;
            player = collision.transform;
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.transform == player)
        {
            playerInArea = false;
            player = null;
        }
    }

    // Optional: Call this to apply damage even during attack
    public void TakeDamage(int amount)
    {
        // Add your damage logic here (e.g., reduce HP, play flinch animation, etc.)
        Debug.Log("Boss took damage: " + amount);
    }
}
