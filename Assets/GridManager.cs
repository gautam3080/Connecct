using DG.Tweening;
using System.Collections.Generic;
using UnityEngine;

public class GridManager : MonoBehaviour
{
    public Ball ballPrefab;
    public int gridX = 5;
    public int gridY = 5;

    public List<Color> ballColors = new List<Color>();

    public static List<Ball> allBalls = new();
    public static Dictionary<Vector2, Ball> _allBalls = new();
    public static Ball currentBall = null;
    public static List<Ball> selectedBalls = new List<Ball>();
    public Ball[,] balls = null;
    public Vector2[,] grid = null;
    public LineRenderer lineRenderer;
    public List<LineRenderer> LinesInstances = new List<LineRenderer>();
    int score = 0;

    public void Start()
    {
        CreatePosGrid();
        CreateRandomColorBalls();
        SetBallsToGrid();
        MeetNeighbours();
    }
    private void Update()
    {
        if (Input.GetMouseButtonUp(0))
        {
            currentBall = null;
            foreach (Ball ball in selectedBalls)
            {
                ball.color = ball.originalColor;
            }
            if (IsCombo()) ActOnCombo();
            ResetSelectedBalls();
            ClearLines();

        };
        if (Input.GetMouseButtonDown(1))
        {
            ResetGrid();
        }
    }

    bool IsCombo() => selectedBalls.Count > 2;
    void ActOnCombo()
    {

        score += selectedBalls.Count;
        Debug.Log("Selected Balls: " + selectedBalls.Count);

        foreach (Ball sball in selectedBalls)
        {
            int xCord = (int)sball.myGridCords.x;
            int yCord = (int)sball.myGridCords.y;
            RandomizeColor(sball);
            for (int j = gridY - 1; j >= 0; j--)
            {
                if (balls[xCord, j] == null)
                {
                    balls[xCord, j] = sball;
                    sball.transform.localPosition = grid[xCord, j];
                    sball.myGridCords = new Vector2(xCord, j);
                    break;
                }
            }
            balls[xCord, yCord] = null;
        }
        selectedBalls?.Clear();
        MoveBallsDownwards();
        MeetNeighbours();

    }

    public void RandomizeColor(Ball ball)
    {
        Color color = ballColors[Random.Range(0, ballColors.Count)];
        ball.originalColor = color;
        ball.color = color;
    }
    public void CreatePosGrid()
    {
        grid = new Vector2[gridX, gridY * 2];
        Vector2 pos = new Vector2(-5, 5 * 2);
        for (int i = 0; i < gridX; i++)
        {
            for (int j = 0; j < gridY * 2; j++)
            {
                grid[i, j] = pos;
                pos += new Vector2(0, -1.0f);
            }
            pos.y = 5 * 2;
            pos += new Vector2(1.0f, 0);
        }
    }
    public void CreateRandomColorBalls()
    {
        foreach (Ball ball in allBalls)
        {
            Destroy(ball.gameObject);
        }
        allBalls?.Clear();
        for (int i = 0; i < gridX * gridY; i++)
        {
            var ball = Instantiate(ballPrefab, transform);
            RandomizeColor(ball);
            ball.OnDrag = OnBallDragg;
            ball.OnHover = OnBallHover;
            allBalls.Add(ball);

        }
    }
    public void SetBallsToGrid()
    {
        balls = new Ball[gridX, gridY * 2];
        int index = 0;
        for (int i = 0; i < gridX; i++)
        {
            for (int j = gridY; j < 2 * gridY; j++)
            {
                Ball ball = allBalls[index];
                ball.myGridCords = new Vector2(i, j);
                ball.transform.localPosition = grid[i, j];
                balls[i, j] = ball;
                index++;
            }
        }
    }
    public void MeetNeighbours()
    {
        foreach (Ball ball in allBalls)
        {
            ball.ClearNeighbours();
        }
        for (int i = 0; i < gridX; i++)
        {
            for (int j = gridY; j < 2 * gridY; j++)
            {
                var ball = balls[i, j];
                if (ball == null) continue;
                Ball left = !(i - 1 < 0) ? balls[i - 1, j] : null;
                Ball upperLeft = !(i - 1 < 0) && !(j - 1 < gridY) ? balls[i - 1, j - 1] : null;
                Ball upper = !(j - 1 < gridY) ? balls[i, j - 1] : null;
                Ball upperRight = !(i + 1 > gridX - 1) && !(j - 1 < gridY) ? balls[i + 1, j - 1] : null;
                Ball right = !(i + 1 > gridX - 1) ? balls[i + 1, j] : null;
                Ball downRight = !(i + 1 > gridX - 1) && !(j + 1 > (2 * gridY) - 1) ? balls[i + 1, j + 1] : null;
                Ball down = !(j + 1 > (2 * gridY) - 1) ? balls[i, j + 1] : null;
                Ball downLeft = !(i - 1 < 0) && !(j + 1 > (2 * gridY) - 1) ? balls[i - 1, j + 1] : null;

                if (left != null) ball.myNeighbours.Add(left);
                if (upperLeft != null) ball.myNeighbours.Add(upperLeft);
                if (upper != null) ball.myNeighbours.Add(upper);
                if (upperRight != null) ball.myNeighbours.Add(upperRight);
                if (right != null) ball.myNeighbours.Add(right);
                if (downRight != null) ball.myNeighbours.Add(downRight);
                if (down != null) ball.myNeighbours.Add(down);
                if (downLeft != null) ball.myNeighbours.Add(downLeft);

                ball.MakeFriends();
            }
        }

    }

    void OnBallDragg(Ball ball)
    {
        if (currentBall == null)
        {
            currentBall = ball;
            //ball.color = Color.black;
            selectedBalls.Add(ball);
        }

    }
    void OnBallHover(Ball ball)
    {
        if (currentBall?.originalColor == ball.originalColor)
        {
            if (!selectedBalls.Contains(ball))
            {
                foreach (Ball b in ball.friendlyNeigbours)
                {
                    if (selectedBalls.Contains(b))
                    {
                        //ball.color = Color.black;
                        if(!selectedBalls.Contains(ball))
                        selectedBalls.Add(ball);
                        MakeLine(b.transform.position, ball.transform.position, ball.originalColor);
                        //break;
                    }
                    
                }

            }
        }
    }

    void MakeLine(Vector2 pos1, Vector2 pos2, Color color)
    {
        if(lineRenderer != null)
        {
            LineRenderer lr = Instantiate(lineRenderer);
            lr.positionCount = 2;
            lr.SetPosition(0, pos1);
            lr.SetPosition(1, pos2);
            lr.sharedMaterial.color = color;
            LinesInstances.Add(lr);
        }
    }
    void ClearLines()
    {
        foreach(LineRenderer lr in LinesInstances)
        {
            Destroy(lr.gameObject);
        }
        LinesInstances?.Clear();
    }
    void ResetSelectedBalls()
    {
        selectedBalls?.Clear();
    }



    [ContextMenu("ResetGrid")]
    public void ResetGrid()
    {
        CreateRandomColorBalls();
        SetBallsToGrid();
        MeetNeighbours();
    }

    public void MoveBallsDownwards()
    {
        Ball[,] tempBalls = balls;

        for (int i = gridX - 1; i >= 0; i--)
        {
            for (int j = (gridY * 2) - 1; j > -1; j--)
            {
                if (tempBalls[i, j] != null && j + 1 < gridY * 2)
                {

                    if (tempBalls[i, j + 1] == null)
                    {
                        Ball ball = tempBalls[i, j];
                        tempBalls[i, j + 1] = ball;
                        tempBalls[i, j] = null;
                        j = gridY * 2;

                    }

                }
            }
        }

        UpdateBallsGrid(tempBalls);


    }


    public void UpdateBallsGrid(Ball[,] balls)
    {
        for (int i = 0; i < gridX; i++)
        {
            for (int j = gridY; j < gridY * 2; j++)
            {
                Ball ball = balls[i, j];
                if (ball != null)
                {
                    int xCoord = (int)ball.myGridCords.x;
                    int yCoord = (int)ball.myGridCords.y;
                    Vector2 oldPos = grid[xCoord, yCoord];
                    Vector2 newPos = grid[i, j];
                    this.balls = balls;
                    ball.transform.DOLocalMoveY(newPos.y,.2f*(j-ball.myGridCords.y));
                    ball.myGridCords = new Vector2(i, j);
 
                }
            }
        }

    }

    Vector2 GetBottomSpace(Ball b)
    {
        Vector2 result = Vector2.zero;



        return result;
    }
}
