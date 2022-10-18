using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ControlEnemyAI1 : MonoBehaviour
{
    public static ControlEnemyAI1 instance;
    public Transform boardCells, graveyard;
    public float speed = 1f;
    public List<Vector3> direction = new List<Vector3>(4);
    public List<Vector3> distance = new List<Vector3>(4);
    GameObject boardTarget;
    List<GameObject> rangeBoards = new List<GameObject>();
    List<GameObject> boardsEmpty = new List<GameObject>();
    List<GameObject> dangerousBoard = new List<GameObject>();
    List<GameObject> victim = new List<GameObject>();
    List<GameObject> boardSafety = new List<GameObject>();

    GameManagerAI gameManagerAI;
    public bool checkOneMore = true;
    float errors = 0f;
    private void Awake()
    {
        instance = this;
    }
    private void Start()
    {
        checkOneMore = true;
        for (int i = 0; i < boardCells.childCount; i++)
        {
            boardsEmpty.Add(boardCells.GetChild(i).gameObject);
        }
        gameManagerAI = GameManagerAI.instance;
    }

    void Update()
    {
        if (!gameManagerAI.myTurn && !gameManagerAI.play && gameManagerAI.startMovement() && checkOneMore)
        {
            errors = 0f;
            checkOneMore = false;
            ClearAllSteps();
            CheckBoardsEmpty();
        }

        if (!checkOneMore)
        {
            errors += Time.deltaTime;
            if(errors >= 10f)
            {
                errors = 0f;
                checkOneMore = true;
            }
        }
    }

    void CheckBoardsEmpty()
    {
        for (int i = 0; i < boardsEmpty.Count; i++)
        {
            if (boardsEmpty[i].transform.childCount == 0)
            {
                CheckConditionBoard(boardsEmpty[i]);
            }
        }
        ChoosingCrownMove();
    }
    void CheckConditionBoard(GameObject board)
    {
        for (int i = 0; i < 4; i++)
        {
            CheckBoardNearCrown(board, direction[i], distance[i]);
        }
    }


    #region Checking Boards Empty


    private void CheckBoardNearCrown(GameObject board, Vector3 direction, Vector3 distance)
    {
        RaycastHit2D hit = Physics2D.Raycast(board.transform.position + direction, Vector3.zero + distance);
        if (hit.collider == null)
            return;

        if (hit.collider.gameObject.transform.childCount == 0)
            return;

        GameObject crown = hit.collider.gameObject;
        

        if (crown.transform.GetChild(0).tag == "Crown")
        {
            RaycastHit2D nextHit = Physics2D.Raycast(crown.transform.position + direction, Vector3.zero + distance);
            if (nextHit.collider != null && nextHit.collider.transform.childCount != 0)
            {
                var otherSideCrown = nextHit.collider.gameObject;
                if (otherSideCrown.transform.GetChild(0).tag != "Crown")
                {
                    if (board.GetComponent<BoardCellAI>().CheckCrownSilver())
                    {
                        boardTarget = board;
                        return;
                    }
                }
            }
        }
        else
        {
            if (!boardSafety.Contains(board))
                boardSafety.Add(board);

            RaycastHit2D nextHit = Physics2D.Raycast(crown.transform.position + direction, Vector3.zero + distance);
            if (nextHit.collider != null && nextHit.collider.transform.childCount != 0)
            {
                var otherSideCrown = nextHit.collider.gameObject;
                if (otherSideCrown.transform.GetChild(0).tag == "Crown")
                {
                    if (board.GetComponent<BoardCellAI>().CheckCrownGold())
                    {
                        if (!dangerousBoard.Contains(board))
                            dangerousBoard.Add(board);
                    }
                }
            }
            if (!rangeBoards.Contains(board))
                rangeBoards.Add(board);
        }
    }
    #endregion
    #region Movement Crown
    void ChoosingCrownMove()
    {
        List<GameObject> dangerous = new List<GameObject>();
        dangerous = gameManagerAI.DangerousBoards();
        if(dangerous.Count > 0)
        {
            for (int i = 0; i < dangerous.Count; i++)
            {
                boardSafety.Remove(dangerous[i]);
            }
        }

        if (boardTarget != null)
        {
            for (int i = 0; i < 4; i++)
            {
                CheckingVictims(boardTarget, direction[i], distance[i]);
            }
            StartCoroutine(MoveCrownToDestroy(boardTarget, GetCrownToMove(boardTarget)));
            return;
        }
        if (gameManagerAI.stuck)
        {
            for (int i = 0; i < rangeBoards.Count; i++)
            {
                if (GetCrownToMove(rangeBoards[i]).GetComponent<EnemyCrownAI>().CheckOpponentCrown())
                {
                    StartCoroutine(MoveCrownToPlay(rangeBoards[i], GetCrownToMove(rangeBoards[i])));
                    return;
                }
            }
        }

        if (dangerousBoard.Count > 0) 
        {
            int rangeBoard = Random.Range(0, dangerousBoard.Count);
            StartCoroutine(MoveCrownToPlay(dangerousBoard[rangeBoard], GetCrownToMove(dangerousBoard[rangeBoard])));
            return;
        }
        if(boardSafety.Count > 0)
        {
            int range = Random.Range(0, rangeBoards.Count);

            StartCoroutine(MoveCrownToPlay(rangeBoards[range], GetCrownToMove(rangeBoards[range])));
            return;
        }

        if (rangeBoards.Count > 0)
        {
            int range = Random.Range(0, rangeBoards.Count);

            StartCoroutine(MoveCrownToPlay(rangeBoards[range], GetCrownToMove(rangeBoards[range])));
            return;
        }
        gameManagerAI.myTurn = true;
        checkOneMore = true;
    }

    void CheckingVictims(GameObject board, Vector3 direction, Vector3 distance)
    {
        RaycastHit2D hit = Physics2D.Raycast(board.transform.position + direction, Vector3.zero + distance);
        if (hit.collider == null)
            return;
        var exa = hit.collider.gameObject;
        if (exa.transform.childCount == 0)
            return;
        if (exa.transform.GetChild(0).tag == "Crown")
        {
            RaycastHit2D subhit = Physics2D.Raycast(exa.transform.GetChild(0).position + direction, Vector3.zero + distance);
            if (subhit.collider == null)
                return;
            var subexa = subhit.collider.gameObject;
            if (subexa.transform.childCount == 0)
                return;
            if(subexa.transform.GetChild(0).tag != "Crown")
            {
                victim.Add(exa.transform.GetChild(0).gameObject);
            }
        }
    }

    GameObject GetCrownToMove(GameObject board)
    {
        for (int i = 0; i < 4; i++)
        {
            if (CheckCrownsToMove(board, direction[i], distance[i]))
                return CheckCrownsToMove(board, direction[i], distance[i]);
        }
        return null;
    }
    private GameObject CheckCrownsToMove(GameObject board, Vector3 direction, Vector3 distance)
    {
        RaycastHit2D hit = Physics2D.Raycast(board.transform.position + direction, Vector3.zero + distance);
        if (hit.collider == null)
            return null;
        if (hit.collider.transform.childCount == 0)
            return null;
        var crown = hit.collider.gameObject;
        if (crown.transform.GetChild(0).tag != "Crown")
        {
            return crown.transform.GetChild(0).gameObject;
        }
        return null;
    }
    IEnumerator MoveCrownToDestroy(GameObject board, GameObject crown)
    {
        crown.transform.SetParent(board.transform);
        while (true)
        {
            crown.transform.position = Vector3.MoveTowards(crown.transform.position, board.transform.position, speed * Time.deltaTime);
            if (crown.transform.position == board.transform.position)
            {
                for (int i = 0; i < victim.Count; i++)
                {
                    victim[i].GetComponent<CrownAI>().OnDestroystart();
                }
                ClearAllSteps();
                break;
            }
            yield return null;
        }
    }
    IEnumerator MoveCrownToPlay(GameObject board, GameObject crown)
    {
        if (board != null && crown != null)
        {
            crown.transform.SetParent(board.transform);
            while (true)
            {
                crown.transform.position = Vector3.MoveTowards(crown.transform.position, board.transform.position, speed * Time.deltaTime);
                if (crown.transform.position == board.transform.position)
                {
                    gameManagerAI.myTurn = true;
                    if(victim.Count > 0)
                    {
                        for (int i = 0; i < victim.Count; i++)
                        {
                            victim[i].GetComponent<CrownAI>().OnDestroystart();
                            gameManagerAI.myTurn = false;
                        }
                    }
                    checkOneMore = true;
                    ClearAllSteps();
                    break;
                }
                yield return null;
            }
        }
        else
        {
            ClearAllSteps();
            checkOneMore = true;
        }
    }

    void ClearAllSteps()
    {
        boardTarget = null;
        dangerousBoard.Clear();
        victim.Clear();
        rangeBoards.Clear();
        boardSafety.Clear();
    }
    #endregion

}
