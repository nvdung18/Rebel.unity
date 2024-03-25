using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    public float maxSpeed = 1.2f;
    public float maxJump = 4f;
    private bool isGrounded = false;

    [Header("Sprite orientation")]
    private bool facingRight = true;
    private bool wasCrounching = false;
    private bool wasFiring = false;
    private bool wasFiring2 = false;

    [Header("Marco Controller")]
    public Animator topAnimator;
    private Animator bottomAnimator;
    public GameObject bottom;
    public GameObject up;

    private Rigidbody2D rb;

    [Header("Time shoot")]
    private float shotTime = 0.0f;
    public float fireDelta = 0.3f;
    private float nextFire = 0.2f;

    [Header("Time Crouch")]
    private float crouchTime = 0.0f;
    public float crouchDelta = 0.5f;
    private float nextCrouch = 0.5f;

    [Header("Time jump")]
    private float jumpTime = 0.0f;
    public float jumpDelta = 0.8f;
    private float nextJump = 0.5f;

    [Header("Bullet")]
    public GameObject projSpawner;

    [Header("Granate")]
    public GameObject granadeSpawner;
    public GameObject granate;

    public GameObject foreground;
    Cinemachine.CinemachineBrain cinemachineBrain;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        bottomAnimator = bottom.GetComponent<Animator>();
        cinemachineBrain = Camera.main.GetComponent<Cinemachine.CinemachineBrain>();
    }

    void Update()
    {
        Fire();
        ThrowGranate();
        MoveHorizontally();
        MoveVertically();
        Jump();
        Crouch();

        FlipShoot();
    }

    void Fire()
    {
        shotTime = shotTime + Time.deltaTime;

        if (Input.GetButton("Fire1"))
        {
            if (!wasFiring)
            {
                topAnimator.SetBool("isFiring", true);
                bottomAnimator.SetBool("isFiring", true);

                if (shotTime > nextFire)
                {
                    nextFire = shotTime + fireDelta;

                    StartCoroutine(WaitFire());

                    nextFire = nextFire - shotTime;
                    shotTime = 0.0f;
                }

                wasFiring = true;
            }
            else
            {
                topAnimator.SetBool("isFiring", false);
                bottomAnimator.SetBool("isFiring", false);
            }
        }
        else
        {
            topAnimator.SetBool("isFiring", false);
            bottomAnimator.SetBool("isFiring", false);
            wasFiring = false;
        }
    }

    void ThrowGranate()
    {
        if (GameManager.GetBombs() > 0)
        {
            shotTime = shotTime + Time.deltaTime;
            if (Input.GetKeyDown(KeyCode.G))
            {
                GameManager.RemoveBomb();
                if (!wasFiring2)
                {
                    if (bottomAnimator.GetBool("isCrouched"))
                    {
                        bottomAnimator.SetBool("isThrowingGranate", true);
                    }
                    else
                    {
                        topAnimator.SetBool("isThrowingGranate", true);
                    }

                    if (shotTime > nextFire)
                    {
                        nextFire = shotTime + fireDelta;

                        StartCoroutine(WaitGranate());

                        nextFire = nextFire - shotTime;
                        shotTime = 0.0f;
                    }

                    wasFiring2 = true;
                }
                else
                {
                    if (bottomAnimator.GetBool("isCrouched"))
                    {
                        bottomAnimator.SetBool("isThrowingGranate", false);
                    }
                    else
                    {
                        topAnimator.SetBool("isThrowingGranate", false);
                    }
                }
            }
            else
            {
                if (bottomAnimator.GetBool("isCrouched"))
                {
                    bottomAnimator.SetBool("isThrowingGranate", false);
                }
                else
                {
                    topAnimator.SetBool("isThrowingGranate", false);
                }
                wasFiring2 = false;
            }
        }
        else
        {
            if (bottomAnimator.GetBool("isCrouched"))
            {
                bottomAnimator.SetBool("isThrowingGranate", false);
            }
            else
            {
                topAnimator.SetBool("isThrowingGranate", false);
            }
            return;
        }
    }

    void MoveHorizontally()
    {
        float moveH = Input.GetAxis("Horizontal");

        if (moveH != 0 && !(bottomAnimator.GetBool("isCrouched") && topAnimator.GetBool("isFiring")))
        {
            rb.velocity = new Vector2(moveH * maxSpeed, rb.velocity.y);
            topAnimator.SetBool("isWalking", true);
            bottomAnimator.SetBool("isWalking", true);

            //Flip sprite orientantion if the user is walking right or left
            if (moveH > 0 && !facingRight)
            {
                //Moving right
                Flip();
            }
            else if (moveH < 0 && facingRight)
            {
                //Moving left
                Flip();
            }
        }
        else
        {
            topAnimator.SetBool("isWalking", false);
            bottomAnimator.SetBool("isWalking", false);
        }
    }

    void MoveVertically()
    {
        float moveV = Input.GetAxis("Vertical");
        if (moveV != 0)
        {

            //bottomAnimator.SetBool("isWalking", true);

            //Flip sprite orientantion if the user is walking right or left
            if (moveV > 0)
            {
                //Moving UP
                topAnimator.SetBool("isLookingUp", true);
            }
        }
        else
        {
            //No
            if (topAnimator.GetBool("isLookingUp"))
            {
                topAnimator.SetBool("isLookingUp", false);
            }
        }
    }
    void Jump()
    {
        jumpTime = jumpTime + Time.deltaTime;
        
        if (Input.GetButton("Jump") && isGrounded && !bottomAnimator.GetBool("isCrouched"))
        {
            if (jumpTime > nextJump)
            {
                Debug.Log(jumpTime);
                rb.AddForce(new Vector2(0, maxJump), ForceMode2D.Impulse);
                topAnimator.SetBool("isJumping", true);
                bottomAnimator.SetBool("isJumping", true);
                isGrounded = false;

                nextJump = jumpTime + jumpDelta;
                nextJump = nextJump - jumpTime;
                jumpTime = 0.0f;
            }
        }
    }

    void Crouch()
    {
        crouchTime = crouchTime + Time.deltaTime;
        if (Input.GetButton("Crouch") && !Input.GetButton("Jump") && ((bottomAnimator.GetBool("isWalking") && wasCrounching) || !bottomAnimator.GetBool("isWalking")) && isGrounded)
        {
            if (crouchTime > nextCrouch)
            {
                bottomAnimator.SetBool("isCrouched", true);

                gameObject.GetComponent<BoxCollider2D>().enabled = false;
                bottom.GetComponent<BoxCollider2D>().enabled = true;

                if (isGrounded)
                {
                    StartCoroutine(WaitCrouch());
                }

                if (!wasCrounching)
                {
                    maxSpeed -= 0.4f;
                    projSpawner.transform.position = new Vector3(projSpawner.transform.position.x, projSpawner.transform.position.y - 0.14f, 0);
                }
                nextCrouch = crouchTime + crouchDelta;
                nextCrouch = nextCrouch - crouchTime;
                crouchTime = 0.0f;
                wasCrounching = true;
            }

        } else if(Input.GetButton("Crouch") && Input.GetButton("Jump"))
        {

        }
        else
        {
            if (isGrounded)
            {
                bottomAnimator.SetBool("isCrouched", false);

                gameObject.GetComponent<BoxCollider2D>().enabled = true;
                bottom.GetComponent<BoxCollider2D>().enabled = false;

                if (isGrounded)
                {
                    up.GetComponent<SpriteRenderer>().enabled = true;
                }

                if (wasCrounching)
                {
                    maxSpeed += 0.4f;
                    projSpawner.transform.position = new Vector3(projSpawner.transform.position.x, projSpawner.transform.position.y + 0.14f, 0);
                }

                wasCrounching = false;
            }
        }
    }

    //Flip sprite
    void Flip()
    {
        Vector3 scale = transform.localScale;
        scale.x *= -1;
        transform.localScale = scale;

        facingRight = !facingRight;
    }

    void FlipShoot()
    {
        if (topAnimator.GetBool("isLookingUp"))
        {
            //Fire up
            projSpawner.transform.rotation = Quaternion.Euler(0, 0, 90);
        }
        else if (facingRight)
        {
            //Fire right
            projSpawner.transform.rotation = Quaternion.Euler(0, 0, 0);
        }
        else
        {
            //Fire left
            projSpawner.transform.rotation = Quaternion.Euler(0, 0, -180);
        }

        //Granade
        if (facingRight)
        {
            //Fire right
            granadeSpawner.transform.rotation = Quaternion.Euler(0, 0, 0);
        }
        else
        {
            //Fire left
            granadeSpawner.transform.rotation = Quaternion.Euler(0, 0, -180);
        }
    }

    void OnCollisionStay2D(Collision2D col)
    {
        if (col.gameObject.CompareTag("Walkable"))
        {
            isGrounded = true;
            topAnimator.SetBool("isJumping", false);
            bottomAnimator.SetBool("isJumping", false);
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        
    }


    private void OnTriggerEnter2D(Collider2D collision)
    {
        
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        
    }

    private IEnumerator WaitFire()
    {
        yield return new WaitForSeconds(0.1f); //Gives the animation time to make the first frame
        BulletManager.GetNormalBulletPool().Spawn(projSpawner.transform.position, projSpawner.transform.rotation);
        yield return new WaitForSeconds(0.2f); //Prevents the button from being spammed
    }

    private IEnumerator WaitGranate()
    {
        yield return new WaitForSeconds(0.1f);
        BulletManager.GetGrenadePool().Spawn(granadeSpawner.transform.position, granadeSpawner.transform.rotation);
        yield return new WaitForSeconds(0.15f);
    }

   

    private IEnumerator WaitCrouch()
    {
        yield return new WaitForSeconds(0.25f);
        up.GetComponent<SpriteRenderer>().enabled = false;
        yield return new WaitForSeconds(0.25f);
    }

    
}
