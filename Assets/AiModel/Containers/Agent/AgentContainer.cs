using System.Collections.Generic;
using UnityEngine;
using MathNet.Numerics;

public class AgentContainer : Container {

    public Agent[] agents;
    public WheelDrive[] trackedAgents;
    public RoadContainer rc;

    public void Start()
    {
        agents = new Agent[trackedAgents.Length];

        for(int i = 0; i < trackedAgents.Length; i++)
        {
            agents[i] = new Agent((long)trackedAgents[i].GetInstanceID(), this, rc);
            agents[i].selfIndex = i;
            FindNewPath(agents[i], i, trackedAgents[i].gameObject.activeSelf);
            agents[i].PopFirst(trackedAgents[i].gameObject.activeSelf);
            trackedAgents[i].myAgent = agents[i];
        }
        
    }
}
