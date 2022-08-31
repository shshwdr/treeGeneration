using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.U2D;

public class BranchGrowth : MonoBehaviour
{

    public float growSpeed = 1f;
    float currentGrowTime;
    [SerializeField] float branchCurve = 0.3f;

    SpriteShapeController splineController;

    [SerializeField] float equalAllowance = 0.1f;

    struct BranchData
    {
        public Vector3 position;
        public float width;
        public BranchData(Vector3 p, float w)
        {
            position = p;
            width = w;
        }
        public void setPosition(Vector3 p)
        {
            position = p;
        }
    }

    List<BranchData> targetData= new List<BranchData>();

    public void init(Vector3 startPosition, Vector3 endPosition, float width)
    {
    }

    public void updatePoint(Vector3 startPosition, Vector3 endPosition, int branchIndex, float width)
    {
        splineController = GetComponent<SpriteShapeController>();



        var spline = splineController.spline;
        var currentBranchCount = spline.GetPointCount();


        //if (currentBranchCount == 0)
        //{
        //    // first one, add directly
        //    spline.InsertPointAt(currentBranchCount, endPosition);
        //    spline.SetHeight(currentBranchCount, width);
        //    //spline.SetTangentMode(currentBranchCount, ShapeTangentMode.Continuous);
        //    Smoothen(splineController, currentBranchCount - 1);
        //}
        //else
        {
            // add a point to the end
            if (branchIndex == 0)
            {
                branchIndex = 1;
            }
            var lastPointPosition = spline.GetPosition(branchIndex - 1);


            var dir = endPosition - lastPointPosition;
            dir.Normalize();
            var NewlastPointPosition = lastPointPosition + dir * growSpeed * 0.1f;
            Debug.Log("lastPoint " + (branchIndex - 1) + NewlastPointPosition + " " + endPosition);
            spline.SetPosition(branchIndex, NewlastPointPosition);
            //spline.SetHeight(currentBranchCount, width);
            //spline.SetTangentMode(currentBranchCount, ShapeTangentMode.Continuous);
            Smoothen(splineController, branchIndex - 1);
            // Smoothen(splineController, branchIndex);
        }
        targetData[branchIndex] = new BranchData(endPosition, width);
    }

    public void addPoint(Vector3 startPosition, Vector3 endPosition, int branchIndex, float width)
    {

        splineController = GetComponent<SpriteShapeController>();



        var spline = splineController.spline;
        var currentBranchCount = spline.GetPointCount();


        if(currentBranchCount == 0)
        {
            // first one, add directly
            spline.InsertPointAt(currentBranchCount, endPosition);
            spline.SetHeight(currentBranchCount, width);
            //spline.SetTangentMode(currentBranchCount, ShapeTangentMode.Continuous);
            Smoothen(splineController, currentBranchCount - 1);
        }
        else
        {
            // add a point to the end
            if(branchIndex == 0)
            {
                branchIndex = 1;
            }
            var lastPointPosition = spline.GetPosition(branchIndex - 1);


            var dir = endPosition - lastPointPosition;
            dir.Normalize();
            var NewlastPointPosition = lastPointPosition + dir * growSpeed * 0.1f;
            Debug.Log("lastPoint " + (branchIndex - 1) + NewlastPointPosition + " " + endPosition);
            spline.InsertPointAt(branchIndex, NewlastPointPosition);
            spline.SetHeight(currentBranchCount, width);
            //spline.SetTangentMode(currentBranchCount, ShapeTangentMode.Continuous);
            //Smoothen(splineController, branchIndex - 1);
            //Smoothen(splineController, branchIndex);
            // Smoothen(splineController, branchIndex);
        }
        targetData.Insert(branchIndex, new BranchData(endPosition, width));
    }

    Dictionary<GameObject, int> attachedGameObjectToIndex = new Dictionary<GameObject, int>();

    public void addObject(GameObject go, int branchIndex)
    {
        attachedGameObjectToIndex[go] = branchIndex;
    }

    private void Update()
    {
        var spline = splineController.spline;
        var currentBranchCount = spline.GetPointCount();
        if(currentBranchCount <= 1)
        {
            return;
        }
        for (int i = 0;i< currentBranchCount; i++)
        {
            var pointPosition = spline.GetPosition(i);
            var targetPosition = targetData[i].position;
            if ((pointPosition - targetPosition).sqrMagnitude>equalAllowance)
            {
                var dir = targetPosition - pointPosition;
                dir.Normalize();
                spline.SetPosition(i, pointPosition + dir * growSpeed * Time.deltaTime);
                Smoothen(splineController, i);
            }
            else
            {
                spline.SetPosition(i, targetPosition);
               // Smoothen(splineController, i);
            }

            foreach(var pair in attachedGameObjectToIndex)
            {
                if(pair.Value == i)
                {
                    pair.Key.transform.localPosition = spline.GetPosition(i);
                }
            }
        }

        foreach(var pair in nodeToBranchIndex)
        {
            pair.Key.transform.position = spline.GetPosition(pair.Value);
        }
    }


    private void Smoothen(SpriteShapeController sc, int pointIndex)

    {
        if (pointIndex < 0)
        {
            return;
        }
        Vector3 position = sc.spline.GetPosition(pointIndex);


        Vector3 positionNext = pointIndex < targetData.Count - 1 ? targetData[pointIndex + 1].position : position;
        Vector3 positionPrev = pointIndex > 0 ? targetData[pointIndex - 1].position : position;

        //Vector3 positionNext = sc.spline.GetPosition(SplineUtility.NextIndex(pointIndex, sc.spline.GetPointCount()));

        //Vector3 positionPrev = sc.spline.GetPosition(SplineUtility.PreviousIndex(pointIndex, sc.spline.GetPointCount()));

        Vector3 forward = gameObject.transform.forward;



        float scale = Mathf.Max((positionNext - position).magnitude, (positionPrev - position).magnitude) * branchCurve;



        Vector3 leftTangent = (positionPrev - position).normalized * scale;

        Vector3 rightTangent = (positionNext - position).normalized * scale;



        sc.spline.SetTangentMode(pointIndex, ShapeTangentMode.Continuous);
        

        SplineUtility.CalculateTangents(position, positionPrev, positionNext, forward, scale, out rightTangent, out leftTangent);



        sc.spline.SetLeftTangent(pointIndex, leftTangent);

        sc.spline.SetRightTangent(pointIndex, rightTangent);

    }


    public Dictionary<GameObject, int> nodeToBranchIndex = new Dictionary<GameObject, int>();

    public void grow()
    {
        var spline = splineController.spline;
        var currentBranchCount = spline.GetPointCount();
        var originTargetData = new List<BranchData>(targetData);
        for (int i = 1; i < currentBranchCount; i++)
        {
            var pointPosition = originTargetData[i].position;
            var lastPosition = originTargetData[i-1].position;
            var diff = pointPosition - lastPosition;
            targetData[i] = new BranchData (targetData[i-1].position + diff*2, targetData[i].width);
        }
    }
}
