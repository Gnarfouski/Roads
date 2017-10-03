using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Agent
{
    public long id;
    public int selfIndex;
    public Vector3 position;

    public AgentContainer a;
    public RoadContainer rc;

    public Stack<int> indexPath;
    public int currentIndex;
    public Stack<float> relativeTimePath;
    public float nextT;
    public bool reverseDirection;

    public Agent(long id, AgentContainer parent, RoadContainer r)
    {
        this.id = id;
        indexPath = new Stack<int>();
        relativeTimePath = new Stack<float>();

        a = parent;
        rc = r;
    }

    public void Update(WheelDrive wd, bool print)
    {
        position = wd.transform.position;
        float currentT = rc.CheckClosestRoot(currentIndex, position);

        //Debug.Log(currentT);
        //if (print) Debug.Log("Updating " + wd.gameObject.name + " in " + rc.segments[currentIndex].id + " (" + currentT + ") going to " + rc.segments[indexPath.Peek()].id);


        if (reverseDirection && currentT <= nextT || !reverseDirection && currentT >= nextT)
        {
            //Debug.Log("EOS : " + rc.segments[currentIndex].id + ", next : " + rc.segments[indexPath.Peek()] + " (" + relativeTimePath.Peek() + ")");

            PopFirst(true);
            if (nextT == -1)
            {
                //Debug.Log("Finding Path");
                a.FindNewPath(this, selfIndex, true);
                PopFirst(true);
            }
        }
        wd.SetVectors(rc.segments[currentIndex].GetDirectionVectors(position, currentT), rc.segments[currentIndex].parentLane.direction);
    }

    public void PopFirst(bool enabled)
    {
        while (indexPath.Count == 0) a.FindNewPath(this, selfIndex, enabled);
        currentIndex = indexPath.Pop();
        nextT = relativeTimePath.Pop();
        reverseDirection = rc.segments[currentIndex].parentLane.direction;

        //if(enabled && indexPath.Count != 0) Debug.Log("Current : " + rc.segments[currentIndex].id + " (" + nextT + ") going to " + rc.segments[indexPath.Peek()].id);
    }
}
