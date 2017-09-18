using System.Collections.Generic;
using UnityEngine;

public class Lane {

    private Segment[] segments;
    private List<int>[] trackedAgents;
    private List<int>[] incomingAgents;
    private float width;
    private float approximatedLength;

    private bool[] authorizedVehicleTypes = new bool[8];
    private float[] restrictedVehicleAttributes = new float[3];
    private float[] speedLimit = new float[2];

    private List<PointContact> exitPoints = new List<PointContact>();
    private List<PointContact> entryPoints = new List<PointContact>();
    private List<LaneContact> neighboringLanes = new List<LaneContact>();

    public Lane(int id, QuadraticPolynomial[] pieces, float offset)
    {

    }

    public void EnterLane()
    {

    }

    public void EnterSegment(int id)
    {
        //segments[id].
    }

    public bool CanAccess(float[] vehicleAttrs)
    {
        if(vehicleAttrs.Length != 3)
        {
            Debug.LogWarning("Bad vehicle attributes param");
            return false;
        }

        for(int i = 0;i<3;i++)
        {
            if (vehicleAttrs[i] > restrictedVehicleAttributes[i]) return false;
        }

        return true;
    }
    
    public Vector3 GetClosestTangent(Vector3 worldPosition)
    {
        return new Vector3();
    }
    
    public Vector3 GetClosestPosition(Vector3 worldPosition)
    {
        return new Vector3();
    }
}
