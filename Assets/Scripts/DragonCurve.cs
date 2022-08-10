using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DragonCurve
{
    bool isClockWise = false;
    float sqrt2;

    public DragonCurve(bool isClockWise)
    {
        this.isClockWise = isClockWise;
        sqrt2 = Mathf.Sqrt(2f);
    }

    public List<Vector3> SplitByLine(List<Vector3> list)
    {
        List<Vector3> output = new List<Vector3>(list);
        int iterations = output.Count - 1;
        int index = 1;
        for (int i = 0; i < iterations; i++)
        {
            var midPoint = Vector3.Lerp(output[index - 1], output[index], 0.5f);
            output.Insert(index, midPoint);
            index += 2;
        }
        return output;
    }

    public List<Vector3> SplitByCurve(List<Vector3> list)
    {
        List<Vector3> output = new List<Vector3>(list);
        int iterations = output.Count - 1;
        int index = 1;
        bool foward = isClockWise;
        for (int i = 0; i < iterations; i++)
        {
            var offset = output[index] - output[index - 1];
            offset = Quaternion.AngleAxis(45, foward ? Vector3.forward : Vector3.back) * offset;
            offset = offset.normalized * offset.magnitude / sqrt2;
            output.Insert(index, output[index - 1] + offset);
            index += 2;
            foward = !foward;
        }
        return output;
    }
}
