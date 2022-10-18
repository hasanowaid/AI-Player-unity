using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyCrownAI : MonoBehaviour
{
    [SerializeField] GameObject destroyEffect1,destroyEffect2;
    GameManagerAI gameManagerAI;
    ControlEnemyAI1 controlEnemyAI1;
    Animator animator;
    List<Vector3> direction = new List<Vector3> {
        new Vector3(0f, 0.35f, 0f), new Vector3(0.35f, 0f, 0f),
        new Vector3(0f, -0.35f, 0f), new Vector3(-0.35f, 0f, 0f) };

    List<Vector3> distance = new List<Vector3> {
        new Vector3(0f, 0.2f, 0f), new Vector3(0.2f, 0f, 0f),
        new Vector3(0f, -0.2f, 0f), new Vector3(-0.2f, 0f, 0f) };
    private void Start()
    {
        animator = GetComponent<Animator>();
        gameManagerAI = GameManagerAI.instance;
        controlEnemyAI1 = ControlEnemyAI1.instance;
    }

    public void OnDestroystart()
    {
        animator.SetBool("startDestroy", true);
/*        GameObject effect = Instantiate(destroyEffect1, transform.position, Quaternion.identity);
        Destroy(effect, 1.4f);*/
    }

    public void OnDestroyEnd()
    {
        animator.SetBool("startDestroy", false);
/*        GameObject effect = Instantiate(destroyEffect2, transform.position, Quaternion.identity);
        Destroy(effect, 1.5f);*/
        gameManagerAI.enemyCrowns.Remove(gameObject);
        Destroy(this.gameObject);
    }

    public bool CheckVictim()
    {
        for (int i = 0; i < 4; i++)
        {
            if (CheckVictimRaycast(gameObject, direction[i], distance[i]))
                return true;
        }
        return false;
    }

    private bool CheckVictimRaycast(GameObject crown, Vector3 dir, Vector3 dis)
    {
        RaycastHit2D hit = Physics2D.Raycast(crown.transform.position + dir, Vector3.zero + dis);
        if (hit.collider == null)
            return false;
        var board = hit.collider.gameObject;
        if (board.transform.childCount == 0)
            return false;
        if(board.transform.GetChild(0).tag == gameObject.tag)
        {
            RaycastHit2D hit2 = Physics2D.Raycast(board.transform.position + dir, Vector3.zero + dis);
            if (hit2.collider == null)
                return false;
            if (hit2.collider.transform.childCount == 0)
                return false;
            if (hit2.collider.transform.GetChild(0).tag == "Crown")
            {
                return true;
            }
        }
        return false;
    }

    public bool CheckOpponentCrown()
    {
        for (int i = 0; i < 4; i++)
        {
            if (CheckOpponentCrownRaycast(gameObject, direction[i], distance[i]))
                return true;
        }
        return false;
    }

    bool CheckOpponentCrownRaycast(GameObject crown,Vector3 dir, Vector3 dis)
    {
        RaycastHit2D hit = Physics2D.Raycast(crown.transform.position + dir, Vector3.zero + dis);
        if (hit.collider == null)
            return false;
        var gameOb = hit.collider.gameObject;
        if (gameOb.transform.childCount == 0)
            return false;
        if (gameOb.transform.GetChild(0).tag == "Crown")
            return true;
        return false;
    }
}
