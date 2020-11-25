using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public static int Points;

    public GameObject[] cubePrefab;
    public GameObject Board, highlightedCubePrefab;
    public Text PointText, ScoreText;
    public int[,,] GameTileID;
    public int[,,,] Match;
    public GameObject[,,] GameTileInstance;
    public float slide_speed, zoom_speed;

    private GameObject selected_tile, highlightedCube, swapCube;
    private List<float> scoreQueue = new List<float>();
    private bool isRotating = false, isPosting = false, is2ndClick = false, isSwapping = false;
    private float mx = 0, my = 0, mz = 0;

    void Awake()
    {
        GameTileID = new int[9, 9, 9];
        Match = new int[9, 9, 9, cubePrefab.Length];
        GameTileInstance = new GameObject[9, 9, 9];        
    }
    // Start is called before the first frame update
    void Start()
    {
        InitGame();
        BuildBoard();
        MakeMatchs();
    }

    // Update is called once per frame
    void Update()
    {
        //GameCube(board) Movement
        if (!isSwapping && !isRotating && Input.GetKeyUp(KeyCode.UpArrow)) RotateCube(1); //Rotate up
        if (!isSwapping && !isRotating && Input.GetKeyUp(KeyCode.RightArrow)) RotateCube(2); //Rotate right
        if (!isSwapping && !isRotating && Input.GetKeyUp(KeyCode.DownArrow)) RotateCube(3); //rotate down
        if (!isSwapping && !isRotating && Input.GetKeyUp(KeyCode.LeftArrow)) RotateCube(4); //Rotate left
        if (!isSwapping && !isRotating && Input.GetKeyDown(KeyCode.W)) { my = -slide_speed; } //Start slide down
        if (!isSwapping && !isRotating && Input.GetKeyUp(KeyCode.W)) { my = 0; } //Stop slide down
        if (!isSwapping && !isRotating && Input.GetKeyDown(KeyCode.A)) { mx = slide_speed; } //Start Slide right
        if (!isSwapping && !isRotating && Input.GetKeyUp(KeyCode.A)) { mx = 0; } //Stop Slide right
        if (!isSwapping && !isRotating && Input.GetKeyDown(KeyCode.S)) { my = slide_speed; } // Start Slide up
        if (!isSwapping && !isRotating && Input.GetKeyUp(KeyCode.S)) { my = 0; } // Stop Slide up
        if (!isSwapping && !isRotating && Input.GetKeyDown(KeyCode.D)) { mx = -slide_speed; } //Start Slide left
        if (!isSwapping && !isRotating && Input.GetKeyUp(KeyCode.D)) { mx = 0; } //Stop Slide left
        if (!isSwapping && !isRotating && Input.GetKeyDown(KeyCode.PageUp)) { mz = -zoom_speed; } //Start Zoom In
        if (!isSwapping && !isRotating && Input.GetKeyUp(KeyCode.PageUp)) { mz = 0; } //Stop Zoom In
        if (!isSwapping && !isRotating && Input.GetKeyDown(KeyCode.PageDown)) { mz = zoom_speed; } //Start Zoom Out
        if (!isSwapping && !isRotating && Input.GetKeyUp(KeyCode.PageDown)) { mz = 0; } //Stop Zoom Out
        Board.transform.position = new Vector3(Board.transform.position.x + mx, Board.transform.position.y + my, Board.transform.position.z + mz);

        //UI Points & Score text
        if (!isPosting && scoreQueue.Count > 0) { isPosting = true; Points += (int)scoreQueue[0]; StartCoroutine(PostScore(scoreQueue[0])); scoreQueue.RemoveAt(0); }
        PointText.text = "Points: " + Points;

        //Mouse Control
        if (!isSwapping && !isRotating && Input.GetMouseButtonUp(0)) //Left Mouse Button (LMB)
        {
            if (is2ndClick) SwapTiles(Input.mousePosition);
            if (!is2ndClick) SelectTile(Input.mousePosition);
            
        }

        //Keep Hilighted Cube in position        
        if(selected_tile == null) highlightedCube.transform.position = new Vector3(0, 0, -100);
        else highlightedCube.transform.position = selected_tile.transform.position;

        //Debug
        //if (Input.GetKeyUp(KeyCode.P)) { scoreQueue.Add(100); scoreQueue.Add(42); scoreQueue.Add(-200); }
    }

    void InitGame()
    {
        for (int z = 0; z < 9; z++)
        {
            for (int y = 0; y < 9; y++)
            {
                for (int x = 0; x < 9; x++)
                {
                    GameTileID[x, y, z] = 0;
                    GameTileInstance[x, y, z] = null;
                    for (int w = 0; w < cubePrefab.Length; w++) Match[x, y, z, w] = 0;
                }
            }
        }
        selected_tile = null;
        Board.transform.rotation = Quaternion.identity;
        if (highlightedCube != null) Destroy(highlightedCube);
        highlightedCube = Instantiate(highlightedCubePrefab, new Vector3(0, 0, -100), Quaternion.identity);
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
        isRotating = true;
        if (a == 1) //Rotate up
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

    private void SelectTile(Vector3 mpos)
    {
        Ray ray = Camera.main.ScreenPointToRay(mpos);
        RaycastHit hit;
        if(Physics.Raycast(ray, out hit, 100))
        {
            selected_tile = hit.transform.gameObject;
            Debug.Log("Coords = " + hit.transform.position);
        }
        is2ndClick = true;        
    }

    private void SwapTiles(Vector3 mpos)
    {
        Ray ray = Camera.main.ScreenPointToRay(mpos);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, 100))
        {
            bool isValidSwap = false;
            if (selected_tile.transform.position.x == (hit.transform.position.x - 1) && selected_tile.transform.position.y == hit.transform.position.y && selected_tile.transform.position.z == hit.transform.position.z) isValidSwap = true; //x-1
            if (selected_tile.transform.position.x == (hit.transform.position.x + 1) && selected_tile.transform.position.y == hit.transform.position.y && selected_tile.transform.position.z == hit.transform.position.z) isValidSwap = true; //x+1
            if (selected_tile.transform.position.y == (hit.transform.position.y - 1) && selected_tile.transform.position.x == hit.transform.position.x && selected_tile.transform.position.z == hit.transform.position.z) isValidSwap = true; //y-1
            if (selected_tile.transform.position.y == (hit.transform.position.y + 1) && selected_tile.transform.position.x == hit.transform.position.x && selected_tile.transform.position.z == hit.transform.position.z) isValidSwap = true; //y+1
            if (selected_tile.transform.position.z == (hit.transform.position.z - 1) && selected_tile.transform.position.x == hit.transform.position.x && selected_tile.transform.position.y == hit.transform.position.y) isValidSwap = true; //z-1
            if (selected_tile.transform.position.z == (hit.transform.position.z + 1) && selected_tile.transform.position.x == hit.transform.position.x && selected_tile.transform.position.y == hit.transform.position.y) isValidSwap = true; //z+1
            if (isValidSwap)
            {
                isSwapping = true;
                swapCube = hit.transform.gameObject;
                StartCoroutine(SwapTilesLocs(selected_tile.transform.position, swapCube.transform.position));
            }
            if (!isValidSwap)
            {
                is2ndClick = false;
                isSwapping = false;
                selected_tile = null;
                highlightedCube.transform.position = new Vector3(0, 0, -100);
            }
        }        
    }

    private void MakeMatchs()
    {
        for(int cubeType = 0; cubeType < cubePrefab.Length; cubeType++)
        {
            //Detect matches
            for(int z = 0; z < 9; z++)
            {
                for(int y = 0; y < 9; y++)
                {
                    for(int x = 0; x < 9; x++)
                    {
                        
                    }
                }
            }
            //Process matches
            for (int z = 0; z < 9; z++)
            {
                for (int y = 0; y < 9; y++)
                {
                    for (int x = 0; x < 9; x++)
                    {
                        if(Match[x,y,z,cubeType] > 3)
                        {
                            Destroy(GameTileInstance[x, y, z]);
                            GameTileID[x, y, z] = 0;
                            Match[x, y, z, cubeType] = 0;
                        }
                    }
                }
            }
        }
    }

        IEnumerator RotateBoard(Vector3 angle, float inTime)
    {

        Quaternion fromAngle = Board.transform.rotation;
        Quaternion toAngle = Quaternion.AngleAxis(90, angle) * fromAngle;
        for (float t = 0f; t < 1; t += Time.deltaTime / inTime)
        {
            Board.transform.rotation = Quaternion.Slerp(fromAngle, toAngle, t);
            yield return null;
        }
        Vector3 alignedForward = NearestWorldAxis(Board.transform.forward);
        Vector3 alignedUp = NearestWorldAxis(Board.transform.up);
        Board.transform.rotation = Quaternion.LookRotation(alignedForward, alignedUp);
        isRotating = false;
    }

    IEnumerator PostScore(float value)
    {
        string mod = "+ ";
        if (value >= 0) { ScoreText.color = new Color(0, 255, 0, 0); mod = "+ "; }
        if (value < 0) { ScoreText.color = new Color(255, 0, 0, 0); mod = " "; }
        for(float a = 0f; a < 1f; a += .01f)
        {
            ScoreText.color = new Color(ScoreText.color.r, ScoreText.color.g, ScoreText.color.b, a);
            ScoreText.text = mod + value.ToString();
            yield return null;
        }
        for (float a = 1f; a > 0f; a -= .01f)
        {
            ScoreText.color = new Color(ScoreText.color.r, ScoreText.color.g, ScoreText.color.b, a);
            ScoreText.text = mod + value.ToString();
            yield return null;
        }
        ScoreText.text = value.ToString();
        isPosting = false;
    }

    IEnumerator SwapTilesLocs(Vector3 tile1, Vector3 tile2)
    {
        //for(int t = 0; t < 10; t++)
        for (float t = 0f; t < 1; t += Time.deltaTime * 2)
        {
            selected_tile.transform.position = Vector3.Slerp(tile1, tile2, t);
            swapCube.transform.position = Vector3.Slerp(tile2, tile1, t);
            //Debug.Log("run = " + t + " = " + selected_tile.transform.position + " / " + swapCube.transform.position);
            yield return null;
        }
        selected_tile.transform.position = tile2;
        swapCube.transform.position = tile1;
        is2ndClick = false;
        isSwapping = false;
        selected_tile = null;
        highlightedCube.transform.position = new Vector3(0, 0, -100);
    }
}
