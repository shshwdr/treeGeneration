using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.U2D;

public class FractalPlant : MonoBehaviour
{
    [SerializeField] UnityEngine.UI.Button buttonSplit = default;
    [SerializeField] UnityEngine.UI.Button buttonChangeWood = default;
    [SerializeField] UnityEngine.UI.Button buttonMerge = default;
    [SerializeField] UnityEngine.UI.Button buttonRefresh = default;
    [SerializeField] UnityEngine.UI.Button buttonToggleFlower = default;
    [SerializeField] UnityEngine.UI.Slider forwardCurveSlider = default;
    [SerializeField] UnityEngine.UI.Text labelIteration = default;
    [SerializeField] LineGenerator lineGenerator = default;
    [SerializeField] float initRadius = 300f;
    [SerializeField] float treeThickness = 2f;
    [SerializeField] float branchCurve = 0.3f;
    [SerializeField] float forwardCurve = 10f;
    [SerializeField]
    float rotationDegree = 25;
    [SerializeField]
    float rotationDegreeRandom = 5;
    [SerializeField]
    int iteration = 1;
    [SerializeField]
    float length = 60;

    [SerializeField] Camera camera;
    [SerializeField] GameObject branchPrefab;
    [SerializeField] GameObject leafPrefab;
    [SerializeField] GameObject flowerPrefab;
    [SerializeField] GameObject curveBranchPrefab;

    int branchTextureIndex = 0;
    [SerializeField] List<SpriteShape> curveBranchTextures;

    List<SpriteRenderer> flowersAndLeaves;

    bool showFlower = true;

    //private DragonCurve dragonCurve = null;

    //struct CurveData
    //{
    //    public Vector3[] line;
    //    public Vector3[] curve;
    //    public float width;

    //    public CurveData(Vector3[] line, Vector3[] curve, float width)
    //    {
    //        this.line = line;
    //        this.curve = curve;
    //        this.width = width;
    //    }
    //}

    // private Dictionary<int, CurveData> curveTable = new Dictionary<int, CurveData>();
    private List< List<char>> curveTable = new List< List<char>>();
    private int currentIteration = 1;
    public readonly int kMaxIteration = 8;
    enum DrawType { branch, blossom, leaf};



    float maxX = 0;
    float minX = 0;
    float maxY = 0;
    float minY = 0;

    GameObject connect(Vector3 start, Vector3 end, Vector3 angle, DrawType type, GameObject parent)
    {
        GameObject go = null;
        var randomP = start + (end - start) * Random.Range(0f, 1f);
        switch (type)
        {
            case DrawType.branch:
                go = Instantiate(branchPrefab, start, Quaternion.Euler(angle), parent.transform);
               // Debug.Log($"draw branch {start} to {end}, with angle {angle}");
                break;
            case DrawType.blossom:


                go = Instantiate(flowerPrefab, randomP, Quaternion.Euler(angle), parent.transform);
                //Debug.Log($"draw branch {start} to {end}, with angle {angle}");
                break;
            case DrawType.leaf:

                go = Instantiate(leafPrefab, randomP, Quaternion.Euler(angle), parent.transform);
                //Debug.Log($"draw branch {start} to {end}, with angle {angle}");
                break;
        }
        return go;
    }

    GameObject drawForward(DrawType type,float curve,ref Vector3 position, ref float degree,GameObject go, ref int order, GameObject currentBranch = null, int depth = 1, int maxDepth = 1)
    {
        //if (shouldCurve)
        {

            degree += curve;// + Random.Range(-rotationDegreeRandom, rotationDegreeRandom);
        }
        Vector3 angle = new Vector3(0, 0, degree);
        var q = Quaternion.AngleAxis(degree, Vector3.forward);
        var newPosition = position;
        maxY = Mathf.Max(newPosition.y, maxY);
        minY = Mathf.Min(newPosition.y, minY);
        maxX = Mathf.Max(newPosition.x, maxX);
        minX = Mathf.Min(newPosition.x, minX);
        if (type == DrawType.branch)
        {

            newPosition = position + q * Vector3.right * length;

            if (currentBranch == null)
            {
                Debug.Log("?");
            }
            else
            {
                addBranch(currentBranch,depth, maxDepth , newPosition, position, ref order);
            }
        }
        //var newPosition = position + angle * length;
        var ob = connect(position, newPosition,angle, type,go);
        position = newPosition;
        if (type != DrawType.branch)
        {
            flowersAndLeaves.Add(ob.GetComponent<SpriteRenderer>());

            ob.GetComponent<SpriteRenderer>().enabled = showFlower;
            ob.GetComponent<SpriteRenderer>().sortingOrder = order + 1;
        }
        return ob;
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
    void addBranch(GameObject currentBranch, int depth, int maxDepth, Vector3 endPosition, Vector3 startPosition, ref int order)
    {
        float thickness = (float)depth / (float)maxDepth;
        var spline = currentBranch.GetComponent<SpriteShapeController>().spline;
        currentBranch.GetComponent<SpriteShapeRenderer>().sortingOrder = order;
        order-=2;
        var currentBranchCount = spline.GetPointCount();
        spline.InsertPointAt(currentBranchCount, endPosition);
        spline.SetHeight(currentBranchCount, thickness * treeThickness);
        //spline.SetTangentMode(currentBranchCount, ShapeTangentMode.Continuous);
        Smoothen(currentBranch.GetComponent<SpriteShapeController>(), currentBranchCount-1);
        currentBranch.GetComponent<SpriteShapeController>().spriteShape = curveBranchTextures[branchTextureIndex];
    }


    struct StackData {
        public Vector3 position;
        public float degree;
        public GameObject parent;
        public int branchDepth;
        public StackData(Vector3 p,float d,GameObject pa, int b)
        {
            position = p;
            degree = d;
            parent = pa;
            branchDepth = b;
        }
    }

    void Start()
    {
        buttonSplit.onClick.AddListener(() =>
        {
            if (currentIteration >= kMaxIteration-1 || lineGenerator.IsInFade) return;
            currentIteration++;
            UpdateIterationLabel();
            //var curveData = curveTable[currentIteration];
            drawTree();
            //lineGenerator.UpdateLine(curveData.curve, curveData.width);
        });
        buttonMerge.onClick.AddListener(() =>
        {
            if (currentIteration <= 0 || lineGenerator.IsInFade) return;
            //var curveData = curveTable[currentIteration];
            //lineGenerator.UpdateLine(curveData.line, curveData.width);
            currentIteration--;
            UpdateIterationLabel();

            drawTree();
        });

        buttonChangeWood.onClick.AddListener(() =>
        {
            branchTextureIndex++;
            if (branchTextureIndex >= curveBranchTextures.Count)
            {
                branchTextureIndex = 0;

            }
            drawTree();
        });

        buttonRefresh.onClick.AddListener(() =>
        {
            drawTree();
        });
        length = length / iteration;

        buttonToggleFlower.onClick.AddListener(() =>
        {
            showFlower = !showFlower;
            foreach (var render in flowersAndLeaves)
            {
                render.enabled = showFlower;
            }
        });




        currentIteration =4;
        UpdateIterationLabel();
        drawTree();


    }

    int order = 1000;

    GameObject parent;
    void drawTree()
    {
        flowersAndLeaves = new List<SpriteRenderer>();
        List<char> res = new List<char>() { 'X' };
        var Xr = "F+[[X]-X]-F[-FX]+X";
        var Xr1 = "F-[[X]+X]+F[+FX]-X";
        var Xr5 = "F-[[[X]+X]+F[+FX]-X]++FX";
        var Xr2 = "F[+X][-X]FX";
        var Xr3 = "F[+X]F[-X]+X";
        List<string> Xrs = new List<string>() { Xr5, Xr, Xr1, Xr2,Xr3 };
        var Fr = "FF";

        curveTable.Add(res);

        int test = 0;
        for (int i = 0; i <= currentIteration; i++)
        {
            List<char> newRes = new List<char>();
            for (int j = 0; j < res.Count; j++)
            {
                switch (res[j])
                {
                    case 'F':
                        newRes.AddRange(Fr);
                        break;
                    case 'X':
                        newRes.AddRange(Xrs[Random.Range(0, Xrs.Count)]);
                        break;
                    default:
                        newRes.Add(res[j]);
                        break;
                }
                //test++;
                //if (test > 1000)
                //{
                //    break;
                //}
            }
            res = newRes;
            //if (test > 1000)
            //{
            //    break;
            //}
            curveTable.Add(res);
        }



        maxX = 0;
        minX = 0;
         maxY = 0;
         minY = 0;
        //Debug.Log("current iteration " + currentIteration);
        Vector3 currentPosition = new Vector3(0, 0, 0);
        float currentRotation = 90;
        GameObject currentObject;
        if (parent)
        {
            Destroy(parent);
        }
        parent = new GameObject();
        currentObject = parent;

        List<StackData> stack = new List<StackData>();

        order = 1000;
        bool wasABracket = false;

        int longestFCount = 0;
        //int tempLongest = 0;


        List<int> thicknessStack = new List<int>();

        List<int> thicknessValue = new List<int>();
        for(int i = 0;i< curveTable[currentIteration].Count; i++)
        {
            thicknessValue.Add(0);
        }

        for (int i = 0; i < curveTable[currentIteration].Count; i++)
        {
            switch (curveTable[currentIteration][i])
            {
                case 'F':
                   // tempLongest++;
                    thicknessStack.Add(i);
                    break;
                case '[':
                    wasABracket = true;
                    stack.Add(new StackData(currentPosition, currentRotation, currentObject, thicknessStack[thicknessStack.Count-1]));
                    break;
                case ']':
                    var value = stack[stack.Count - 1];
                    stack.RemoveAt(stack.Count - 1);
                    for(int j = 0;j< thicknessStack.Count; j++)
                    {
                        var id = thicknessStack[thicknessStack.Count - j-1];
                        thicknessValue[id] = Mathf.Max(thicknessValue[id], j);
                        longestFCount = Mathf.Max(j, longestFCount);
                    }
                    //int test = 0;
                    while(thicknessStack[thicknessStack.Count - 1] != value.branchDepth)
                    {
                        thicknessStack.RemoveAt(thicknessStack.Count - 1);

                        //test++;
                        //if (test > 1000)
                        //{
                        //    Debug.LogError("?");
                        //    break;
                        //}
                    }
                    //tempLongest = value.branchDepth;
                    break;
            }
        }
                   // int FCount = 0;

       // Debug.Log(longestFCount);


        GameObject currentBranch = Instantiate(curveBranchPrefab, currentObject.transform);
        currentBranch.GetComponent<SpriteShapeController>().spline.Clear();

        addBranch(currentBranch, thicknessValue[0], longestFCount, currentPosition, currentPosition, ref order);
        //currentBranch.GetComponent<SpriteShapeController>().spline.InsertPointAt(0, currentPosition);
        //currentBranch.GetComponent<SpriteShapeController>().spline.SetHeight(0, 1);
        int lastThickness = 0;

        float currentForwardCurve = (1- Random.Range(0, 2)*2) * Random.Range(2, forwardCurve) ;

        for (int i = 0; i < curveTable[currentIteration].Count; i++)
        {
            switch (curveTable[currentIteration][i])
            {
                case 'F':
                    var parentObject = currentObject;
                    if (Random.Range(0.0f, 1.0f) > 0.7f)
                    {
                        //drawForward(DrawType.leaf, ref currentPosition, currentRotation, currentObject);
                    }
                    currentObject = drawForward(DrawType.branch, i>currentIteration? currentForwardCurve : 0, ref currentPosition, ref currentRotation, currentObject, ref order, currentBranch, thicknessValue[i], longestFCount);
                    lastThickness = thicknessValue[i];
                    //currentObject = drawForward(DrawType.branch, ref currentPosition, currentRotation, currentObject);
                    if (parentObject.transform.rotation.eulerAngles!= currentObject.transform.rotation.eulerAngles && parentObject != parent)
                    {
                            currentObject.GetComponent<RotateByTime>().enabled = true;

                    }
                    else
                    {

                        currentObject.GetComponent<RotateByTime>().enabled = false;
                    }
                    //FCount++;
                    //if (wasABracket)
                    //{
                    //    wasABracket = false;
                    //}
                    //else
                    //{

                    //    currentObject.GetComponent<RotateByTime>().enabled = false;
                    //}
                    break;
                case '+':
                    currentRotation += rotationDegree + Random.Range(-rotationDegreeRandom, rotationDegreeRandom);
                    break;
                case '-':
                    currentRotation -= rotationDegree + Random.Range(-rotationDegreeRandom, rotationDegreeRandom);
                    break;
                case '[':
                    wasABracket = true;
                    stack.Add(new StackData(currentPosition, currentRotation, currentObject, lastThickness));
                    break;
                case ']':

                    drawForward(Random.Range(0.0f, 1.0f) > 0.25 ? DrawType.leaf : DrawType.blossom, 0,ref currentPosition, ref currentRotation, currentObject, ref order);
                    var value = stack[stack.Count - 1];
                    stack.RemoveAt(stack.Count - 1);

                    currentPosition = value.position;
                    currentRotation = value.degree;
                    lastThickness = value.branchDepth;
                    if (currentObject!= value.parent)
                    {

                        if (currentBranch.GetComponent<SpriteShapeController>().spline.GetPointCount() <= 1)
                        {

                        }
                        else
                        {

                            currentBranch = Instantiate(curveBranchPrefab, parent.transform);
                            currentForwardCurve = (1 - Random.Range(0, 2) * 2) * Random.Range(2, forwardCurve);
                        }
                        currentBranch.GetComponent<SpriteShapeController>().spline.Clear();

                        addBranch(currentBranch, lastThickness, longestFCount, currentPosition, currentPosition, ref order);
                    }

                    currentObject = value.parent;

                    //currentBranch.GetComponent<SpriteShapeController>().spline.InsertPointAt(0, currentPosition);
                    //currentBranch.GetComponent<SpriteShapeController>().spline.SetHeight(0, 1);

                    break;

            }

        }


        camera.transform.position = new Vector3((maxX+minX)/2f, (maxY+minY)/2, camera.transform.position.z);
        camera.orthographicSize = (maxY-minY) / 2;
    }


    void UpdateIterationLabel()
    {
        labelIteration.text = "Iteration : " + currentIteration.ToString();
    }
}
