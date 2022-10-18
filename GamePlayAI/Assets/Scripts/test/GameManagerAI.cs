using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;


public class GameManagerAI : MonoBehaviour
{
    public static GameManagerAI instance;
    public GameObject crownPrefab,enemyPrefab;
    public GameObject winOrLose;
    public Transform boardCellContain;
    public List<GameObject> myCrowns, enemyCrowns;
    public Text oText, xText;
    public bool myTurn = true;
    public bool play = false;
    public bool stuck = false;

    private List<GameObject> boardCell = new List<GameObject>();
    private List<GameObject> boardsEmpty = new List<GameObject>();
    private bool yourTurn = true;
    private bool checkVictory = true;
    float wait = 0f;
    bool checkOneTime = true;
    List<Vector3> direction = new List<Vector3> { 
        new Vector3(0f, 0.35f, 0f), new Vector3(0.35f, 0f, 0f), 
        new Vector3(0f, -0.35f, 0f), new Vector3(-0.35f, 0f, 0f) };

    List<Vector3> distance = new List<Vector3> { 
        new Vector3(0f, 0.2f, 0f), new Vector3(0.2f, 0f, 0f),
        new Vector3(0f, -0.2f, 0f), new Vector3(-0.2f, 0f, 0f) };

    private void Awake()
    {
        instance = this;
    }
    private void Start()
    {
        int rand = Random.Range(0, 10);
        yourTurn = (rand > 5);
        myCrowns = new List<GameObject>();
        enemyCrowns = new List<GameObject>();
        for (int i = 0; i < boardCellContain.childCount; i++)
        {
            boardCell.Add(boardCellContain.GetChild(i).gameObject);
        }
    }
#if !UNITY_EDITOR
	private void OnApplicationPause(bool pause)
    {
		if (!pause)
			SaveStateOnline("1");
		else
			SaveStateOnline("0");
    }
#endif
    void Update()
    {
        play = winOrLose.activeSelf ;

        oText.text = myCrowns.Count.ToString();
        xText.text = enemyCrowns.Count.ToString();

        wait += Time.deltaTime;
        if (!yourTurn && !startMovement())
        {
            yourTurn = true;
            StartCoroutine(PlayAI());
        }
        if (!startMovement())
            CheckIfBaordEmpty();

        //result of match for determine who the winner
        ResultMatch();

        //if the crowns are stuck or the player can't play, this function will be solved
        CrownStuck();
    }


    void CrownStuck()
    {
        if (startMovement())
        {
            if (myTurn && checkOneTime)
            {
                checkOneTime = false;
                for (int i = 0; i < myCrowns.Count; i++)
                {
                    CheckBoardsEmpty(myCrowns[i]);
                }
                if (boardsEmpty.Count == 0)
                {
                    myTurn = false;
                    stuck = true;
                }
                else
                    stuck = false;
                boardsEmpty.Clear();
            }
            else if (!myTurn)
            {
                checkOneTime = true;
                for (int i = 0; i < enemyCrowns.Count; i++)
                {
                    CheckBoardsEmpty(enemyCrowns[i]);
                }
                if (boardsEmpty.Count == 0)
                {
                    myTurn = true;
                }
                boardsEmpty.Clear();
            }
        }
    }
    void CheckBoardsEmpty(GameObject board)
    {
        for (int i = 0; i < 4; i++)
        {
            CheckSurroundCrown(board, direction[i], distance[i]);
        }
    }
    void CheckSurroundCrown(GameObject board, Vector3 direction , Vector3 distance)
    {
        RaycastHit2D hit = Physics2D.Raycast(board.transform.position + direction, Vector3.zero + distance);
        if (hit.collider == null)
            return;
        var hiting = hit.collider.gameObject;
        if(hiting.transform.childCount == 0)
        {
            boardsEmpty.Add(hiting);
            Debug.DrawRay(board.transform.position + direction, Vector3.zero + distance, Color.green);
        }
    }
    void WinOrLose(string result)
    {
        winOrLose.SetActive(true);
        winOrLose.transform.GetChild(0).GetComponent<Text>().text = result;
    }

    bool Victory()
    {
        if (enemyCrowns.Count <= 1)
        {
            return true;
        }else if (myCrowns.Count <= 1)
        {
            return true;
        }
        return false;
    }
    void ResultMatch()
    {
        if (checkVictory && Victory() && startMovement())
        {
            checkVictory = false;
            if (enemyCrowns.Count <= 1)
            {
                WinOrLose("YOU WIN");
            }
            else if (myCrowns.Count <= 1)
            {
                WinOrLose("YOU LOSE");
            }
        }
    }

    public void MasterTurn(string name)
    {
        if(wait < 1f)
        {
            return;
        }
        if (yourTurn && !startMovement())
        {
            yourTurn = false;
            for (int i = 0; i < boardCell.Count; i++)
            {
                if (boardCell[i].name == name)
                {
                    GameObject crown = Instantiate(crownPrefab, boardCell[i].transform.position,Quaternion.identity) as GameObject;
                    crown.transform.SetParent(boardCell[i].transform);
                    //GameObject drop = Instantiate(crownDropCFX, boardCell[i].transform.position, Quaternion.identity);
                    //Destroy(drop, 0.5f);
                    myCrowns.Add(crown);
                    wait = 0f;
                }
            }
        }
    }

    void CheckIfBaordEmpty()
    {
        for (int i = 0; i < boardCell.Count; i++)
        {
            if (boardCell[i].transform.childCount != 0)
            {
                boardCell.Remove(boardCell[i]);
            }
        }
    }

    IEnumerator PlayAI()
    {
        yield return new WaitForSeconds(0.4f);
        if (!startMovement())
        {
            int board = Random.Range(0, boardCell.Count);
            if (boardCell[board].transform.childCount == 0)
            {
                GameObject crown = Instantiate(enemyPrefab, boardCell[board].transform) as GameObject;
                //GameObject drop = Instantiate(crownDropCFX, crown.transform.position, Quaternion.identity);
                //Destroy(drop, 0.5f);
                enemyCrowns.Add(crown);
            }
            else
                yourTurn = false;
        }
    }

    public bool startMovement()
    {
        return boardCell.Count <= 1;
    }


    public void PlayAgain()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
    
    public List<GameObject> DangerousBoards()
    {
        List<GameObject> boards = new List<GameObject>();
        for (int i = 0; i < myCrowns.Count; i++)
        {
            for (int j = 0; j < 4; j++)
            {
                if (DangerousBoardsRaycast(myCrowns[i], direction[j], distance[j]) != null)
                    boards.Add(DangerousBoardsRaycast(myCrowns[i], direction[j], distance[j]));
            }
        }
        //Debug.Log(boards.Count);
        return boards;
    }
    GameObject DangerousBoardsRaycast(GameObject crown,Vector3 dir, Vector3 dis)
    {
        RaycastHit2D hitCrown = Physics2D.Raycast(crown.transform.position + dir, Vector3.zero + dis);
        if (hitCrown.collider != null)
        {
            var boardEmp = hitCrown.collider.gameObject;
            if (boardEmp.transform.childCount == 0)
            {
                RaycastHit2D hitBoard = Physics2D.Raycast(boardEmp.transform.position + dir, Vector3.zero + dis);
                if (hitBoard.collider != null)
                {
                    var boardEmp2 = hitBoard.collider.gameObject;
                    if (boardEmp2.transform.childCount == 0)
                    {
                        if(boardEmp2.tag != "BoardCell")
                        {
                            return null;
                        }

                        if (boardEmp2.GetComponent<BoardCellAI>().CheckCrownGold())
                        {
                            return boardEmp;
                        }
                    }
                    else if (boardEmp2.transform.GetChild(0).tag == "EnemyCrown")
                    {
                        if (boardEmp2.GetComponent<BoardCellAI>().CheckCrownGold())
                        {
                            return boardEmp;
                        }
                    }
                }
            }
        }
        return null;
    }




}
