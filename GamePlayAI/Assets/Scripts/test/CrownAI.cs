using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CrownAI : MonoBehaviour
{
    //[SerializeField] float speed = 5f;
    //[SerializeField] float distance = 5f;
    [SerializeField] GameObject newBoardCell, oldBoardCell;
    List<Vector3> direction = new List<Vector3> {
        new Vector3(0f, 0.35f, 0f), new Vector3(0.35f, 0f, 0f),
        new Vector3(0f, -0.35f, 0f), new Vector3(-0.35f, 0f, 0f) };

    List<Vector3> distance = new List<Vector3> {
        new Vector3(0f, 0.2f, 0f), new Vector3(0.2f, 0f, 0f),
        new Vector3(0f, -0.2f, 0f), new Vector3(-0.2f, 0f, 0f) };
    GameObject crownSelectionCFX, cellSelectioniCFX;
    List<GameObject> cellEmpty = new List<GameObject>();
    List<GameObject> stuckCrowns = new List<GameObject>();
    bool moveCrown = false;
    Camera cam;
    Animator animator;
    GameManagerAI gameManagerAI;
    ControlEnemyAI1 controlEnemyAI1;
    Vector3 currentPosCrown;


    private void Start()
    {
        controlEnemyAI1 = ControlEnemyAI1.instance;
        oldBoardCell = gameObject.transform.parent.gameObject;
        gameManagerAI = GameManagerAI.instance;
        animator = GetComponent<Animator>();
        cam = Camera.main;
    }

    private void Update()
    {
        if (gameManagerAI.myTurn && gameManagerAI.startMovement())
        {
            //test();
            if (moveCrown)
                MoveTheCrown();
        }

    }

    private void OnMouseDown()
    {
        DodgingErrorDown();

    }
    private void OnMouseUp()
    {
        DodgingErrorUp();
    }

    private void OnTriggerEnter2D(Collider2D col)
    {
        if (col.gameObject.tag == "BoardCell" && col.gameObject.transform.childCount == 0)
        {
            newBoardCell = col.gameObject;
        }
        else
        {
            newBoardCell = oldBoardCell;
        }
    }

    void MoveTheCrown()
    {
        Vector2 cameraPos = cam.ScreenToWorldPoint(Input.mousePosition);
        transform.position = cameraPos;
    }


    void PhysicsHit(Transform cellpos,Vector3 direction, Vector3 distance)
    {
        RaycastHit2D hit = Physics2D.Raycast(cellpos.position + direction, Vector3.zero + distance);
        if (hit.collider != null)
        {
            var boards = hit.collider.gameObject;
            if (boards.transform.childCount != 0)
                return;
            cellEmpty.Add(boards);
        }
   
    }
    void Checkingsurroundboard(Transform cellpos, Vector3 direction, Vector3 distance)
    {
        RaycastHit2D hit = Physics2D.Raycast(cellpos.position + direction, Vector3.zero + distance);
        if (hit.collider != null)
        {
            var boards = hit.collider.gameObject;
            if (boards.transform.childCount == 0)
                return;
            if (boards.transform.GetChild(0).tag != "EnemyCrown")
                return;

            DestroyCrown(boards.transform, direction, distance);
        }

    }
    void DestroyCrown(Transform cellPos, Vector3 direction, Vector3 distance)
    {
        RaycastHit2D hit = Physics2D.Raycast(cellPos.position + direction, Vector3.zero + distance);
        if (hit.collider != null)
        {
            GameObject cell = hit.collider.gameObject;
            if (cell.transform.childCount == 0)
                return;

            if (cell.transform.GetChild(0).tag == "Crown")
            {
                var victim = cellPos.GetChild(0).gameObject;
                gameManagerAI.myTurn = true;
                victim.transform.SetParent(controlEnemyAI1.graveyard);
                victim.GetComponent<EnemyCrownAI>().OnDestroystart();

            }
        }
    }


    void CheckCanDestroy(GameObject newCell, bool compare)
    {

        for (int i = 0; i < 4; i++)
        {
            if (compare)
                PhysicsHit(newCell.transform, direction[i], distance[i]);
            else
                Checkingsurroundboard(newCell.transform, direction[i], distance[i]);
        }
    }

    public void DodgingErrorDown()
    {
        if (!gameManagerAI.startMovement() || gameObject.tag == "EnemyCrown" || !gameManagerAI.myTurn || gameManagerAI.play)
        {
            return;
        }
        CheckCanDestroy(this.gameObject, true);




        moveCrown = true;
        currentPosCrown = transform.position;
/*        crownSelectionCFX = Instantiate(gameManagerAI.crownSelectionCFX, transform.position, Quaternion.identity);
        cellSelectioniCFX = Instantiate(gameManagerAI.cellSelectionCFX, oldBoardCell.transform.position, Quaternion.identity);
        crownSelectionCFX.transform.SetParent(gameObject.transform);*/
    }
    public void DodgingErrorUp()
    {
        if (!gameManagerAI.startMovement() || gameObject.tag == "EnemyCrown")
        {
            return;
        }

        stuckCrowns.Clear();
        //stuck = true;
        moveCrown = false;
        if (cellEmpty.Contains(newBoardCell) && oldBoardCell != newBoardCell)
        {
            cellEmpty.Clear();
            transform.SetParent(newBoardCell.transform);
            transform.position = newBoardCell.transform.position;
            oldBoardCell = newBoardCell;
            gameManagerAI.myTurn = false;
            CheckCanDestroy(oldBoardCell, false);
        }
        transform.position = oldBoardCell.transform.position;
/*        Destroy(crownSelectionCFX);
        Destroy(cellSelectioniCFX);*/
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
        controlEnemyAI1.checkOneMore = true;
        gameManagerAI.myCrowns.Remove(gameObject);
        Destroy(this.gameObject);
    }

}
