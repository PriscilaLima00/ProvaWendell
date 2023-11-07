using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Inimigo : MonoBehaviour
{
    [Header("atributos")] 
    public float totalHealth = 100;
    public float attackDamage;
    public float movementSpeed;
    public float lookRadius;
    public float colliderRadius = 2f;
    public float rotationSpeed;

    [Header("Components")] 
    private Animator anim;
    private CapsuleCollider capsule;
    private NavMeshAgent agent;

    [Header("others")] 
    private Transform player;
    
    private bool walking;
    private bool attacking;
    private bool hiting;
    private bool waitFor;
    public bool playerIsDead;

    [Header("wayPoints")] 
    public List<Transform> wayPoints = new List<Transform>();
    public int currentPathIndex;
    public float pathDistance;

    // Start is called before the first frame update
    void Start()
    {
        anim = GetComponent<Animator>();
        capsule = GetComponent<CapsuleCollider>();
        agent = GetComponent<NavMeshAgent>();

        player = GameObject.FindGameObjectWithTag("Player").transform;
    }

    // Update is called once per frame
    void Update()
    {
        if (totalHealth > 0)
        {
            float distance = Vector3.Distance(player.position, transform.position);

            if (distance <= lookRadius)
            {
                // O PERSONAGEM ESTA NO RAIO DE AÇÃO
                agent.isStopped = false;

                if (!attacking)
                {
                    agent.SetDestination(player.position);
                    anim.SetBool("Walk Forward", true);
                    walking = true;
                }

                if (distance <= agent.stoppingDistance)
                {
                    //O PERSONAGEM ESTA NO RAIO DE ATAQUE    
                    // AQUI VEM O MÉTODO DE ATAQUE

                    StartCoroutine("Attack");
                    LookTarget();
                }
                else
                {
                    attacking = false;
                }
            }

            else
            {
                //FORA DO RAIO DE AÇÃO
                //agent.isStopped = true;
                anim.SetBool("Walk Forward", false);
                walking = false;
                attacking = false;
                MoveTowayPoint();
            }
        }
    }

    void MoveTowayPoint()
    {
        if (wayPoints.Count > 0)
        {
            float distance = Vector3.Distance(wayPoints[currentPathIndex].position, transform.position);
            agent.destination = wayPoints[currentPathIndex].position;
            if (distance <= pathDistance)
            {
                //parte para o próximo ponto
                currentPathIndex = Random.Range(0, wayPoints.Count);
            }
        }
    }
    IEnumerator Attack()
    {
        if (!waitFor && !hiting && !playerIsDead)
        {
            
            waitFor = true;
            attacking = true;
            walking = false;
            anim.SetBool("Walk Forward",false);
            anim.SetBool("Bite Attack",true);
            yield return new WaitForSeconds(1.2f);
            GetPlayer();
            //yield return new WaitForSeconds(1f);
            waitFor = false;
        }

        if (playerIsDead)
        {
            anim.SetBool("Walk Forward",false);
            anim.SetBool("Bite Attack",false);
            walking = false;
            attacking = false;
            agent.isStopped = true;
        }

    }


    void GetPlayer()
    {
        foreach (Collider c in Physics.OverlapSphere((transform.position + transform.forward * colliderRadius), colliderRadius))
        {
            if (c.gameObject.CompareTag("Player"))
            {
                // APLICA DANO NO PLAYER
                c.gameObject.GetComponent<Player>().GetHit(attackDamage);
                playerIsDead = c.gameObject.GetComponent<Player>().isDead;
            }
        }
    }

    public void GetHit(float damage)
    {
        totalHealth -= damage;
        
        if (totalHealth > 0)
        {
            StopCoroutine("Attack");
            anim.SetTrigger("Take Damage");
            hiting = true;
            StopCoroutine("RecoveryFromHit");
            //inimigo vivo
        }
        else
        {
            //inimigo morre
            anim.SetTrigger("Die");
        }
    }

    IEnumerator RecoveryFromHit()
    {
        yield return new WaitForSeconds(1f);
        anim.SetBool("Walk Forward",false);
        anim.SetBool("Bite Attack",false);
        hiting = false;
        waitFor = false;
    }

    void LookTarget()
    {
        Vector3 direction = (player.position - transform.position).normalized;
        Quaternion lookRotation = Quaternion.LookRotation(new Vector3(direction.x, 0 , direction.z));
        transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * rotationSpeed);
    }
    private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(transform.position, lookRadius);
        }
    }