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
            Smoothen(splineController, branchIndex - 1);
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
            }
            else
            {
                spline.SetPosition(i, targetPosition);
            }

            foreach(var pair in attachedGameObjectToIndex)
            {
                if(pair.Value-1 == i)
                {
                    pair.Key.transform.localPosition = spline.GetPosition(i);
                }
            }
        }
    }


    private void Smoothen(SpriteShapeController sc, int pointIndex)

    {
        if (pointIndex < 1)
        {
            return;
        }
        Vector3 position = sc.spline.GetPosition(pointIndex);

        Vector3 positionNext = sc.spline.GetPosition(SplineUtility.NextIndex(pointIndex, sc.spline.GetPointCount()));

        Vector3 positionPrev = sc.spline.GetPosition(SplineUtility.PreviousIndex(pointIndex, sc.spline.GetPointCount()));

        Vector3 forward = gameObject.transform.forward;



        float scale = Mathf.Min((positionNext - position).magnitude, (positionPrev - position).magnitude) * branchCurve;



        Vector3 leftTangent = (positionPrev - position).normalized * scale;

        Vector3 rightTangent = (positionNext - position).normalized * scale;



        sc.spline.SetTangentMode(pointIndex, ShapeTangentMode.Continuous);

        SplineUtility.CalculateTangents(position, positionPrev, positionNext, forward, scale, out rightTangent, out leftTangent);



        sc.spline.SetLeftTangent(pointIndex, leftTangent);

        sc.spline.SetRightTangent(pointIndex, rightTangent);

    }


}