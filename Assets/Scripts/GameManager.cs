using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public GameObject[] cubePrefab;    
    public int[,,] GameTileID;
    public GameObject[,,] GameTileInstance;
    public GameObject Board;

    void Awake()
    {
        GameTileID = new int[9,9,9];
        GameTileInstance = new GameObject[9, 9, 9];
    }
    // Start is called before the first frame update
    void Start()
    {
        BuildBoard();
    }

    // Update is called once per frame
    void Update()
    {
    }

    void InitGame()
    {
        for(int z = 0; z < 9; z++)
        {
            for(int y = 0; y < 9; y++)
            {
                for(int x = 0; x < 9; x++)
                {
                    GameTileID[x, y, z] = 0;
                    GameTileInstance[x, y, z] = null;
                }
            }
        }
        Board.transform.rotation = Quaternion.identity;
    }

    void BuildBoard()
    {
        int x, y, z, r;
        for (int z1 = -4; z1 < 5; z1++)
        {
            for (int y1 = -4; y1 < 5; y1++)
            {
                for (int x1 = -4; x1 < 5; x1++)
                {
                    x = x1 + 4; y = y1 + 4; z = z1 + 4; r = Random.Range(0, cubePrefab.Length);
                    GameTileID[x, y, z] = r;
                    Instantiate(cubePrefab[r], new Vector3(x1, y1, z1), Quaternion.identity, Board.transform);
                }
            }
        }        
    }

    public void RotateCube(int a)
    {
        if(a == 1) //Rotate up
        {
            StartCoroutine(RotateBoard(Vector3.right, .8f));
        }
        if (a == 2)
        {
            StartCoroutine(RotateBoard(-Vector3.up, .8f));
        }
        if (a == 3)
        {
            StartCoroutine(RotateBoard(-Vector3.right, .8f));
        }
        if (a == 4)
        {
            StartCoroutine(RotateBoard(Vector3.up, .8f));
        }
    }

    private Vector3 NearestWorldAxis(Vector3 v)
    {
        if (Mathf.Abs(v.x) < Mathf.Abs(v.y))
        {
            v.x = 0;
            if (Mathf.Abs(v.y) < Mathf.Abs(v.z))
                v.y = 0;
            else
                v.z = 0;
        }
        else
        {
            v.y = 0;
            if (Mathf.Abs(v.x) < Mathf.Abs(v.z))
                v.x = 0;
            else
                v.z = 0;
        }
        return v;
    }

    IEnumerator RotateBoard(Vector3 angle, float inTime)
    {

        Quaternion fromAngle = Board.transform.rotation;
        Quaternion toAngle = Quaternion.AngleAxis(90, angle) * fromAngle;
        for (var t = 0f; t < 1; t += Time.deltaTime / inTime)
        {
            Board.transform.rotation = Quaternion.Slerp(fromAngle, toAngle, t);
            yield return null;
        }
        Vector3 alignedForward = NearestWorldAxis(Board.transform.forward);
        Vector3 alignedUp = NearestWorldAxis(Board.transform.up);
        Board.transform.rotation = Quaternion.LookRotation(alignedForward, alignedUp);
    }
}
