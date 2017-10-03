using System.Collections.Generic;
using UnityEngine;

public class Container : MonoBehaviour {

    protected List<AI_Behavior> observingBehaviors = new List<AI_Behavior>();

    public void Attach(AI_Behavior b)
    {
        observingBehaviors.Add(b);
    }

    public void Detach(AI_Behavior b)
    {
        observingBehaviors.Remove(b);
    }

    protected void Notify()
    {
        foreach (AI_Behavior b in observingBehaviors) b.Update();
    }
}
