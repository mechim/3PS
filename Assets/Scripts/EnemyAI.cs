using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;


public class EnemyAI : MonoBehaviour
{
    [SerializeField]
    private NavMeshAgent agent;
    [SerializeField]
    private Transform player;
    [SerializeField]
    private LayerMask whatIsGround, whatIsPlayer;
    [Header("Patrolling")]
    private Vector3 walkPoint;
    private bool walkPointSet;
    [SerializeField]
    private float walkPointRange;

    [Header("Attacking")]
    [SerializeField]
    private float timeBetweenAttacks;
    private bool alreadyAttacked;
    [SerializeField]
    private Transform gunBarrel;
    [SerializeField]
    private GameObject bulletPrefab;
    [SerializeField]
    private Transform enemyBulletParent;
    [SerializeField]
    private float bulletHitMissDistance;
    [SerializeField]
    private ParticleSystem muzle;

    [Header("States")]
    [SerializeField]
    private float sightRange, attackRange;
    private bool playerInSightRange, playerInAttackRange;
    private Slider healthSlider;

    [Header("Animation")]
    private Animator animator;
    private int pistolWalkAnimation, shootAnimation, isWalking, isShooting;
    private float animationPlayTransition = .15f;
    [SerializeField]
    private Light hilight;
    [SerializeField]
    private Color freezeColor;
    private void Awake(){
        player = GameObject.FindGameObjectWithTag("Player").transform;
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();

        pistolWalkAnimation = Animator.StringToHash("Pistol Walk");
        shootAnimation = Animator.StringToHash("Shooting");
        isWalking = Animator.StringToHash("isWalking");
        isShooting = Animator.StringToHash("isShooting");
    }


    private void Update(){
        //Check for sight and attack range
        playerInSightRange = Physics.CheckSphere(transform.position, sightRange, whatIsPlayer);
        playerInAttackRange = Physics.CheckSphere(transform.position, attackRange, whatIsPlayer);

        if (!playerInSightRange && !playerInAttackRange) Patrolling();
        if (playerInSightRange && !playerInAttackRange) ChasePlayer();
        if (playerInSightRange && playerInAttackRange) AttackPlayer();
    }
    private void Patrolling(){
        animator.SetBool(isShooting, false);
        animator.SetBool(isWalking, true);
        if (!walkPointSet) FindWalkPoint();
        else agent.SetDestination(walkPoint);

        Vector3 distanceToWalkPoint = transform.position - walkPoint;
        //Walkpoint reached
        if (distanceToWalkPoint.magnitude < 1.0f) walkPointSet = false;
    }

    private void FindWalkPoint(){
        //Calculate random point in range
        float randomZ = Random.Range(-walkPointRange, walkPointRange);
        float randomX = Random.Range(-walkPointRange, walkPointRange);

        walkPoint = new Vector3(transform.position.x + randomX, transform.position.y, transform.position.z + randomZ);

        if (Physics.Raycast(walkPoint, -transform.up, 3f, whatIsGround)){
            walkPointSet = true;
        }
    }
    private void ChasePlayer(){
        animator.SetBool(isShooting, false);
        animator.SetBool(isWalking, true);
        agent.SetDestination(player.position);
    }

    private void AttackPlayer(){
        animator.SetBool(isWalking, false);
        animator.SetBool(isShooting, true);
        agent.SetDestination(transform.position);

        transform.LookAt(player);

        if(!alreadyAttacked){
            
            //The attack logic
            Shoot();

            alreadyAttacked = true;
            Invoke(nameof(ResetAttack), timeBetweenAttacks);
        }
    }

    private void Shoot(){
        muzle.Play();
        animator.CrossFade(shootAnimation, animationPlayTransition);
        GameObject bullet = Instantiate(bulletPrefab, gunBarrel.position, Quaternion.identity, enemyBulletParent);
        BulletController bulletController = bullet.GetComponent<BulletController>();
        bulletController.directionSnapshot = transform.forward;
    }

    private void ResetAttack(){
        alreadyAttacked = false;
    }

    private void OnDrawGizmosSelected() {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, sightRange);
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, walkPointRange);    
    }

    public void Freeze(){
        agent.speed /= 2;
        timeBetweenAttacks *= 2;
        var healthManager = gameObject.GetComponent<HealthManager>();
        healthManager.Freeze();
        animator.speed /= 2;
        hilight.color = freezeColor;
    }
}
