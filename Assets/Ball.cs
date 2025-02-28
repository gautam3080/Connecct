using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(Collider2D))]
public class Ball : MonoBehaviour
{
    public Color originalColor;
    public Color color
     {
        get => myRenderer.material.color;
        set
        {
            myRenderer.material.color = value; 
            if(originalColor==null) originalColor = value;
        }

    }

    public List<Ball> myNeighbours = new List<Ball>();
    public List<Ball> friendlyNeigbours = new List<Ball>(); 
    public Action<Ball> OnDrag;
    public Action<Ball> OnHover;

    public Bounds myBounds;
    private SpriteRenderer myRenderer;

    public Vector2 myGridCords;

    private void Awake()
    {
        myBounds = GetComponent<Collider2D>().bounds;
        myRenderer = GetComponent<SpriteRenderer>();
    }

    private void OnMouseDrag()
    {
        if (Input.GetMouseButton(0))
            OnDrag?.Invoke(this);
    }

    private void OnMouseOver()
    {
        if(Input.GetMouseButton(0))
        OnHover?.Invoke(this);  
        /*if (!(Input.GetMouseButton(0) && GridManager.currentBall != null)) return;
        Debug.Log("BT_A1::" + name);
        if (GridManager.currentBall == null) GridManager.currentBall = this;
        else if (GridManager.currentBall.color == color)
        {
            color = Color.black;
            GridManager.selectedBalls.Add(this);
        }
        else
        {
            GridManager.currentBall = null;
            GridManager.selectedBalls = new List<Ball>();
        }*/
    }

    public void MakeFriends()
    {
        foreach(Ball ball in myNeighbours)
        {
            if(ball.originalColor == originalColor)
            {
                friendlyNeigbours.Add(ball);
            }
        }
    }
    public void ClearNeighbours()
    {
        myNeighbours = new();
        friendlyNeigbours = new();
    }

}
