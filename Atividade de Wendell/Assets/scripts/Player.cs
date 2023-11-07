using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    private CharacterController controller;
    public float totalHealth;
    public float speed;
    public float gravity;
    public float damage = 20;

    private Animator anim;

    private Transform cam;

    Vector3 moveDirection;
    
    private bool isWalking;
    private bool waitFor;
    private bool isHitting;

    public bool isDead;
    
    public float smoothRotTime;
    private float turnSmoothVelocity;

    public float colliderRadius;
    public List<Transform> enemyList = new List<Transform>();

    // Start is called before the first frame update
    void Start()
    {
        anim = GetComponent<Animator>();
        controller = GetComponent<CharacterController>();
        cam = Camera.main.transform;
    }

    // Update is called once per frame
    void Update()
    {
        if (!isDead)
        {
            Move();
            GetMouseInput();
        }
        
    }

    void Move()
    {
        
            if (controller.isGrounded )
        {
            float horizontal = Input.GetAxisRaw("Horizontal");
            float vertical = Input.GetAxisRaw("Vertical");

            Vector3 direction = new Vector3(horizontal, 0f, vertical);

            if (direction.magnitude > 0)
            {
                if (!anim.GetBool("atack"))
                {
                    float angle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg + cam.eulerAngles.y;
                    float smoothAngel = Mathf.SmoothDampAngle(transform.eulerAngles.y, angle, ref turnSmoothVelocity, smoothRotTime);
            
                    transform.rotation = Quaternion.Euler(0f,angle,0f);
                
                    moveDirection = Quaternion.Euler(0f, angle, 0f) * Vector3.forward * speed ;
            
                    anim.SetInteger("transition",1);

                    isWalking = true;
                }
                else
                {
                    anim.SetBool("andando",false);
                    moveDirection = Vector3.zero;
                }
            }
            else if(isWalking)
            {
                anim.SetBool("andando",false);
                anim.SetInteger("transition",0);
                moveDirection = Vector3.zero;

                isWalking = false;
            }
        }

        moveDirection.y -= gravity * Time.deltaTime;
        
        controller.Move(moveDirection * Time.deltaTime);
    }


    void GetMouseInput()
    {
        if (controller.isGrounded)
        {
            if (Input.GetMouseButtonDown(0))
            {
                if (anim.GetBool("andando"))
                {
                    anim.SetBool("andando",false);
                    anim.SetInteger("transition", 0);
                }

                if (!anim.GetBool("andando"))
                {
                    StartCoroutine("atack"); 
                }
                     
                
            }
        }
    }

    IEnumerator atack()
    {
        if (!waitFor && !isHitting)
        {
            waitFor = true;

            anim.SetBool("atack", true);
            anim.SetInteger("transition", 2);

            yield return new WaitForSeconds(0.4f);

            GetEnemiesList();

            foreach (Transform e in enemyList)
            {
                //aplica dano ao inimigo
                Inimigo enemy = e.GetComponent<Inimigo>();

                if (enemy != null)
                {
                    enemy.GetHit(damage);
                }
            }

            yield return new WaitForSeconds(1f);

            anim.SetInteger("transition", 0);
            anim.SetBool("atack", false);
            waitFor = false;
        }
    }

    void GetEnemiesList()
    {
        enemyList.Clear();
        foreach ( Collider  c  in  Physics.OverlapSphere((transform.position + transform.forward * colliderRadius),colliderRadius))
        {
            if (c.gameObject.CompareTag("Enemy"))
            {
                enemyList.Add(c.transform);
            }
        }
    }
    
    public void GetHit(float damage)
    {
        totalHealth -= damage;
        
        if (totalHealth > 0)
        {
            //player vivo
            StopCoroutine("Attack");
            anim.SetInteger("transition", 3);
            isHitting = true;
            StopCoroutine("RecoveryFromHit");
            
        }
        else
        {
            //player morre
            isDead = true;
            anim.SetTrigger("Die");
        }
    }

    IEnumerator RecoveryFromHit()
    {
        yield return new WaitForSeconds(1f);
        anim.SetInteger("transition", 0);
        isHitting = false;
        anim.SetBool("andando", false);

    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position + transform.forward , colliderRadius);
    }
}
