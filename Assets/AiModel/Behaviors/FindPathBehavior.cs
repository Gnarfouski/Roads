using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MathNet.Numerics;

public class FindPathBehavior : AI_Behavior
{
    AgentContainer AC;
    RoadContainer RC;
    int selfIndex;

    public override void Update()
    {
        if(AC.agents[selfIndex].indexPath.Count == 0)
        {
            int firstIndex = RC.FindStartingIndex(AC.agents[selfIndex].position);
            int lastIndex = RC.GetRandomSegmentIndex();
            Tuple<Stack<int>, Stack<float>> path = GetPath(firstIndex, lastIndex);



            //if (print) Debug.Log("Path towards " + rc.segments[lastIndex].id + " " + path.Item1.Count);
            AC.agents[selfIndex].indexPath = path.Item1;
            AC.agents[selfIndex].currentIndex = firstIndex;
            AC.agents[selfIndex].relativeTimePath = path.Item2;
        }
    }

    internal Tuple<Stack<int>, Stack<float>> GetPath(int firstIndex, int lastIndex)
    {
        //Debug.Log("looking for path " + segments[firstIndex].id + " " + segments[lastIndex].id);
        if (firstIndex == lastIndex) return new Tuple<Stack<int>, Stack<float>>(new Stack<int>(), new Stack<float>());
        resetSegmentSearchParameters();

        RC.segments[firstIndex].value = 0;

        List<Segment> toSearch = new List<Segment>();
        List<Segment> tempSearch = new List<Segment>();
        toSearch.Add(RC.segments[firstIndex]);

        Segment check = null;
        while (toSearch.Count != 0)
        {
            //Debug.Log(toSearch.Count);
            foreach (Contact c in toSearch[0].contacts)
            {
                check = c.target;
                if (check.selfIndex == lastIndex)
                {
                    //Debug.LogWarning(check.id);
                    check.predecessor = toSearch[0];
                    return BuildPath(check, firstIndex);
                }

                if (check.CheckPath(toSearch[0], toSearch[0].value + 1))
                {
                    tempSearch.Add(check);
                }
            }
            toSearch.RemoveAt(0);
            if (toSearch.Count == 0 && tempSearch.Count != 0)
            {
                toSearch.AddRange(tempSearch);
                tempSearch.Clear();
            }
        }

        Debug.LogError("Failed to Find Path from " + RC.segments[firstIndex].id + " to " + RC.segments[lastIndex].id);
        return null;
    }

    private Tuple<Stack<int>, Stack<float>> BuildPath(Segment check, int endIndex)
    {
        Stack<int> indexPath = new Stack<int>();
        Stack<float> tPath = new Stack<float>();

        //Debug.Log("building path");

        Segment last = check;
        indexPath.Push(last.selfIndex);
        tPath.Push(-1f);

        while (last.predecessor != null)
        {
            foreach (Contact c in last.predecessor.contacts)
            {
                if (c.target == last)
                {
                    tPath.Push(c.switchT);
                    break;
                }
            }

            last = last.predecessor;
            indexPath.Push(last.selfIndex);
        }

        return new Tuple<Stack<int>, Stack<float>>(indexPath, tPath);
    }

    void resetSegmentSearchParameters()
    {
        foreach (Segment s in RC.segments)
        {
            s.value = int.MaxValue;
            s.predecessor = null;
        }
    }
}
