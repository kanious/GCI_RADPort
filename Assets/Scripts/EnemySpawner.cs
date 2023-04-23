using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    public GameObject enemy;


    // Start is called before the first frame update
    void Awake()
    {
        StartCoroutine(EnemySpawn());
    }

    IEnumerator EnemySpawn()
    {
        while (true)
        {
            yield return new WaitForSeconds(2f);

            if (transform.childCount >= 10)
                continue;

            Vector3 pos;
            pos.x = Random.Range(-40, 40);
            pos.y = 2f;
            pos.z = Random.Range(-40, 40);

            Vector3 rot;
            rot.x = 0f;
            rot.y = Random.Range(0, 359);
            rot.z = 0f;

            GameObject instantEnemy = Instantiate(enemy, pos, Quaternion.Euler(rot));
            instantEnemy.transform.parent = this.transform;
        }
    }
}
