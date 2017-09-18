using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoadContainer : Container{

    private List<Lane> Lanes = new List<Lane>();
    private List<List<int>> TrackedAgentIDs = new List<List<int>>();
    private List<List<int>> OwnedAgentIDs = new List<List<int>>();

}
