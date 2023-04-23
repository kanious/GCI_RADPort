using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    public int maxHealth;
    public int curHealth;
    public GameObject[] items;

    Rigidbody body;
    BoxCollider boxCollider;
    Material mat;

    public AudioSource hitSound;
    public AudioSource deathSound;

    void Awake()
    {
        body = GetComponent<Rigidbody>();
        boxCollider = GetComponent<BoxCollider>();
        mat = GetComponent<MeshRenderer>().material;
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Melee")
        {
            Weapon weapon = other.GetComponent<Weapon>();
            curHealth -= weapon.damage;
            if (curHealth < 0)
                curHealth = 0;

            Vector3 reactVec = transform.position - other.transform.position;

            StartCoroutine(OnDamage(reactVec));
        }
        else if (other.tag == "Bullet")
        {
            Bullet bullet = other.GetComponent<Bullet>();
            curHealth -= bullet.damage;
            if (curHealth < 0)
                curHealth = 0;
            
            Vector3 reactVec = transform.position - other.transform.position;

            StartCoroutine(OnDamage(reactVec));
        }
    }

    IEnumerator OnDamage(Vector3 reactVec)
    {
        mat.color = Color.red;
        yield return new WaitForSeconds(0.1f);

        reactVec = reactVec.normalized;
        reactVec += Vector3.up;

        if (curHealth > 0)
        {
            hitSound.Play();
            mat.color = Color.white;
            body.AddForce(reactVec * 5, ForceMode.Impulse);
        }
        else
        {
            deathSound.Play();
            mat.color = Color.gray;
            gameObject.layer = 14;

            body.AddForce(reactVec * 12, ForceMode.Impulse);

            int itemIdx = UnityEngine.Random.Range(0, 5);
            GameObject instantItem = Instantiate(items[itemIdx], transform.position, transform.rotation);

            Destroy(gameObject, 4);
        }
    }
}
