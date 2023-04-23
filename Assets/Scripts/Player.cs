using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Player : MonoBehaviour
{
    public float run_speed;
    public float walk_speed;
    public float jump_power;
    public GameObject[] weapons;
    public bool[] hasWeapon;

    public int ammo;
    public int coin;
    public int health;
    public int hasGrenades;
    public int maxAmmo;
    public int maxCoin;
    public int maxHealth;
    public int maxHasGrenades;
    public GameObject[] grenades;

    public AudioSource reloadSound;
    public AudioSource itemSound;

    float hAxis;
    float vAxis;
    bool isBorder;

    bool walk;
    bool jump;
    bool isJump;
    bool isDodge;
    bool isSwap;
    
    Vector3 vMove;
    Vector3 vDodge;
    Rigidbody body;
    Animator anim;

    // Item
    GameObject nearObject;
    Weapon equipWeapon;
    int curWeaponIdx = -1;
    bool interaction;
    bool weapon1;
    bool weapon2;
    bool weapon3;

    // attack
    bool fire;
    bool isFireReady = true;
    float fireDelay;

    // reload
    bool reload;
    bool isReload;


    // Start is called before the first frame update
    void Start()
    {
        body = GetComponent<Rigidbody>();
        anim = GetComponentInChildren<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        GetInput();
        Move();
        Turn();
        Jump();
        Attack();
        Reload();
        Dodge();
        Swap();
        Interaction();
    }

    void GetInput()
    {
        hAxis = Input.GetAxisRaw("Horizontal");
        vAxis = Input.GetAxisRaw("Vertical");
        walk = Input.GetButton("Walk");
        jump = Input.GetButtonDown("Jump");
        fire = Input.GetButton("Fire1");
        reload = Input.GetButtonDown("Reload");
        interaction = Input.GetButtonDown("Interation");
        weapon1 = Input.GetButtonDown("Swap1");
        weapon2 = Input.GetButtonDown("Swap2");
        weapon3 = Input.GetButtonDown("Swap3");
    }
    void Move()
    {
        vMove = new Vector3(hAxis, 0f, vAxis).normalized;

        if (isDodge)
            vMove = vDodge;

        if (isSwap || !isFireReady || isReload)
            vMove = Vector3.zero;

        if (!isBorder)
        {
            // move
            if (walk)
                transform.position += vMove * walk_speed * Time.deltaTime;
            else
                transform.position += vMove * run_speed * Time.deltaTime;
        }

        anim.SetBool("isRun", vMove != Vector3.zero);
        anim.SetBool("isWalk", walk);
    }

    void Turn()
    {
        transform.LookAt(transform.position + vMove);
    }

    void Jump()
    {
        if (jump && vMove == Vector3.zero && !isJump && !isDodge && !isSwap && !isReload)
        {
            body.AddForce(Vector3.up * jump_power, ForceMode.Impulse);
            isJump = true;
            anim.SetBool("isJump", true);
            anim.SetTrigger("doJump");
        }
    }

    void Attack()
    {
        if (equipWeapon == null)
            return;

        if (equipWeapon.type == Weapon.AttackType.Range && equipWeapon.curAmmo == 0)
            return;

        fireDelay += Time.deltaTime;
        isFireReady = equipWeapon.rate < fireDelay;
        
        if (fire && isFireReady && !isDodge && !isSwap && !isReload)
        {
            equipWeapon.Use();
            anim.SetTrigger(equipWeapon.type == Weapon.AttackType.Melee ? "doSwing" : "doShot");
            fireDelay = 0f;
        }
    }

    void Reload()
    {
        if (equipWeapon == null)
            return;

        if (equipWeapon.type == Weapon.AttackType.Melee)
            return;

        if (ammo == 0)
            return;

        if (equipWeapon.curAmmo == equipWeapon.maxAmmo)
            return;

        if (reload && !isJump && !isDodge && !isSwap && !isReload && isFireReady)
        {
            anim.SetTrigger("doReload");
            isReload = true;
            reloadSound.Play();

            Invoke("ReloadOut", 2.5f);
        }
    }

    void ReloadOut()
    {
        int gap = equipWeapon.maxAmmo - equipWeapon.curAmmo;

        if (gap > ammo)
        {
            equipWeapon.curAmmo += ammo;
            ammo = 0;
        }
        else
        {
            equipWeapon.curAmmo += gap;
            ammo -= gap;
        }

        isReload = false;
    }

    void Dodge()
    {
        if (jump && vMove != Vector3.zero && !isJump && !isDodge && !isSwap && !isReload)
        {
            vDodge = vMove;
            run_speed *= 2;
            anim.SetTrigger("doDodge");
            isDodge = true;

            Invoke("DodgeOut", 0.4f);
        }
    }

    void DodgeOut()
    {
        run_speed *= 0.5f;
        isDodge = false;
    }

    void Swap()
    {
        if (weapon1 && (!hasWeapon[0] || curWeaponIdx == 0))
            return;

        if (weapon2 && (!hasWeapon[1] || curWeaponIdx == 1))
            return;

        if (weapon3 && (!hasWeapon[2] || curWeaponIdx == 2))
            return;

        int weaponIndex = -1;
        if (weapon1) weaponIndex = 0;
        if (weapon2) weaponIndex = 1;
        if (weapon3) weaponIndex = 2;

        if ((weapon1 || weapon2 || weapon3) && !isJump && !isDodge && !isSwap && !isReload)
        {
            if (equipWeapon != null)
                equipWeapon.gameObject.SetActive(false);
            equipWeapon = weapons[weaponIndex].GetComponent<Weapon>();
            equipWeapon.gameObject.SetActive(true);

            curWeaponIdx = weaponIndex;
            anim.SetTrigger("doSwap");
            isSwap = true;

            Invoke("SwapOut", 0.4f);
        }
    }

    void SwapOut()
    {
        isSwap = false;
    }

    void Interaction()
    {
        if (interaction && nearObject != null && !isJump && !isDodge && !isSwap && !isReload)
        {
            if (nearObject.tag == "Weapon")
            {
                Item item = nearObject.GetComponent<Item>();
                int weaponIndex = item.value;
                hasWeapon[weaponIndex] = true;

                Destroy(nearObject);
                nearObject = null;
                itemSound.Play();
            }
        }
    }

    void FreezeRotation()
    {
        body.angularVelocity = Vector3.zero;
    }

    void StopToWall()
    {
        isBorder = Physics.Raycast(transform.position, transform.forward, 2, LayerMask.GetMask("Wall"));
    }

    void FixedUpdate()
    {
        FreezeRotation();
        StopToWall();
    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.tag == "Floor")
        {
            isJump = false;
            anim.SetBool("isJump", false);
        }
    }

    void OnTriggerStay(Collider other)
    {
        if (other.tag == "Weapon")
        {
            nearObject = other.gameObject;
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.tag == "Weapon")
        {
            nearObject = null;
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Item")
        {
            Item item = other.GetComponent<Item>();
            switch(item.type)
            {
                case Item.ItemType.Ammo:
                    ammo += item.value;
                    if (ammo > maxAmmo)
                        ammo = maxAmmo;
                    break;

                case Item.ItemType.Coin:
                    coin += item.value;
                    if (coin > maxCoin)
                        coin = maxCoin;
                    break;

                case Item.ItemType.Heart:
                    health += item.value;
                    if (health > maxHealth)
                        health = maxHealth;
                    break;

                case Item.ItemType.Grenade:
                    grenades[hasGrenades].SetActive(true);
                    hasGrenades += item.value;
                    if (hasGrenades > maxHasGrenades)
                        hasGrenades = maxHasGrenades;
                    break;
            }
            itemSound.Play();
            Destroy(other.gameObject);
        }
    }
}
