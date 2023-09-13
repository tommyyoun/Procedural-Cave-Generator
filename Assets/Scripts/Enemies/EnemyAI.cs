using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyAI : MonoBehaviour
{
    private List<GameObject> players;
    private GameObject minPlayer;
    private float minDistance = Mathf.Infinity;
    private float tempDistance;

    private IEnemyMovement enemyMovement;

    // Start is called before the first frame update
    void Awake()
    {
        players = new List<GameObject>();

        //foreach (GameObject player in GameObject.Find("Player"))
        
        players.Add(GameObject.Find("Player"));

        enemyMovement = GetComponent<IEnemyMovement>();
    }

    // Update is called once per frame
    void Update()
    {
        MoveTowards(GetNearestPlayer());
    }

    private Transform GetNearestPlayer()
    {
        foreach (GameObject player in players)
        {
            tempDistance = GetDistanceFrom(player);

            if (tempDistance < minDistance)
            {
                minPlayer = player;
                minDistance = tempDistance;
            }
        }

        return minPlayer.transform;
    }

    private float GetDistanceFrom(GameObject player)
    {
        return Vector3.Distance(player.transform.position, transform.position);
    }

    private void MoveTowards(Transform pos)
    {
        enemyMovement.HandleMovement(pos);
    }
}
