using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Inimigo : MonoBehaviour
{
    [Header("atributos")]
    public float totalHealth;

    public float attackDamage;

    public float movementSpeed;
    public float lookRadius;

    [Header("Components")] 
    private Animator anim;

    private CapsuleCollider capsule;

    private NavMeshAgent agent;

    [Header("others")]
    private Transform player;

    private bool walking;

    private bool attacking;
    
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

                agent.isStopped = true;
            }
            else
            {
                attacking = false;
            }
        }
        
        else
        {
            //FORA DO RAIO DE AÇÃO
            anim.SetBool("Walk Forward", false);
            walking = false;
            attacking = false;
            agent.isStopped = false;
        }
    }
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, lookRadius);
    }
}
