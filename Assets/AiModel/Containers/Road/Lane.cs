using System.Collections.Generic;
using UnityEngine;

public class Lane {

    public long id;
    public float offset;
    public float width;
    public bool direction;

    private bool[] authorizedVehicleTypes = new bool[8];
    private float[] restrictedVehicleAttributes = new float[3];
    private float[] speedLimit = new float[2];


    public Lane(long id, float offset, float width, bool direction)
    {
        this.id = id;
        this.offset = offset;
        this.width = width;
        this.direction = direction;
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

}
