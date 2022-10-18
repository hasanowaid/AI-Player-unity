using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoardCellAI : MonoBehaviour
{
    GameManagerAI gameManagerAI;
    List<Vector3> direction = new List<Vector3> {
        new Vector3(0f, 0.35f, 0f), new Vector3(0.35f, 0f, 0f),
        new Vector3(0f, -0.35f, 0f), new Vector3(-0.35f, 0f, 0f) };

    List<Vector3> distance = new List<Vector3> {
        new Vector3(0f, 0.2f, 0f), new Vector3(0.2f, 0f, 0f),
        new Vector3(0f, -0.2f, 0f), new Vector3(-0.2f, 0f, 0f) }; 

    private void Start()
    {
        gameManagerAI = GameManagerAI.instance;
    }

    private void OnMouseDown()
    {
        if (gameManagerAI.play)
            return;

        if (gameManagerAI.startMovement() && gameObject.transform.childCount != 0)
        {
            if (gameObject.transform.GetChild(0).tag == "Crown")
                transform.GetChild(0).GetComponent<CrownAI>().DodgingErrorDown();
        }
        if(gameObject.transform.childCount == 0)
            gameManagerAI.MasterTurn(gameObject.name);
    }
    private void OnMouseUp()
    {
        if (gameManagerAI.startMovement() && gameObject.transform.childCount != 0)
        {
            if (gameObject.transform.GetChild(0).tag == "Crown")
                transform.GetChild(0).GetComponent<CrownAI>().DodgingErrorUp();
        }
    }

    public bool CheckCrownGold()
    {
        for (int i = 0; i < 4; i++)
        {
            if (CheckCrownGoldRaycast(gameObject, direction[i], distance[i]))
                return true;
        }
        return false;
    }

    bool CheckCrownGoldRaycast(GameObject crown, Vector3 dir, Vector3 dis)
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

    public bool CheckCrownSilver()
    {
        for (int i = 0; i < 4; i++)
        {
            if (CheckCrownSilverRaycast(gameObject, direction[i], distance[i]))
                return true;
        }
        return false;
    }
    bool CheckCrownSilverRaycast(GameObject crown, Vector3 dir, Vector3 dis)
    {
        RaycastHit2D hit = Physics2D.Raycast(crown.transform.position + dir, Vector3.zero + dis);
        if (hit.collider == null)
            return false;
        var gameOb = hit.collider.gameObject;
        if (gameOb.transform.childCount == 0)
            return false;
        if (gameOb.transform.GetChild(0).tag == "EnemyCrown")
            return true;
        return false;
    }

    public bool CheckStuckCrownGold()
    {
        for (int i = 0; i < 4; i++)
        {
            if (CheckStuckCrownGoldRaycast(gameObject, direction[i], distance[i]))
                return true;
        }
        return false;
    }

    bool CheckStuckCrownGoldRaycast(GameObject crown, Vector3 dir, Vector3 dis)
    {
        RaycastHit2D hit = Physics2D.Raycast(crown.transform.position + dir, Vector3.zero + dis);
        if (hit.collider == null)
            return false;
        var gameOb = hit.collider.gameObject;
        if (gameOb.transform.childCount == 0)
            return false;
        if (gameOb.transform.GetChild(0).tag == "EnemyCrown")
        {
            RaycastHit2D hitBoard = Physics2D.Raycast(gameOb.transform.position + dir, Vector3.zero + dis);
            if (hitBoard.collider == null)
                return false;
            var boardEmp2 = hitBoard.collider.gameObject;
            if (boardEmp2.transform.childCount == 0)
                return false;
            if (boardEmp2.transform.GetChild(0).tag == "Crown")
                return true;

        }
        return false;
    }
}
