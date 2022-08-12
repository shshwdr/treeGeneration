using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.U2D;
using UnityEngine.UI;

public class FractalPlant : MonoBehaviour
{
    UnityEngine.UI.Button buttonSplit = default;
    UnityEngine.UI.Button buttonChangeWood = default;
    UnityEngine.UI.Button buttonMerge = default;
    UnityEngine.UI.Button buttonRefresh = default;
    UnityEngine.UI.Button buttonToggleFlower = default;
    UnityEngine.UI.Slider forwardCurveSlider = default;
    UnityEngine.UI.Text labelIteration = default;
    LineGenerator lineGenerator = default;
    [SerializeField] float initRadius = 300f;
    [SerializeField] float treeThickness = 2f;
    [SerializeField] float forwardCurve = 10f;
    [SerializeField]
    float rotationDegree = 25;
    [SerializeField]
    float rotationDegreeRandom = 5;
    [SerializeField]
    int iteration = 1;
    [SerializeField]
    float length = 60;

    [SerializeField] float widthDecrease = 0.9f;
    [SerializeField] float treeThicknessScale = 2;

    Camera camera;
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
    private List<List<char>> curveTable = new List<List<char>>();
    private int currentIteration = 1;
    public readonly int kMaxIteration = 8;
    enum DrawType { branch, blossom, leaf };



    float maxX = 0;
    float minX = 0;
    float maxY = 0.28f;
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

    void calculateNextPosition(float curve, ref Vector3 position, ref float degree, GameObject currentBranch = null, float thickness = 1)
    {
        {

            degree += curve;// + Random.Range(-rotationDegreeRandom, rotationDegreeRandom);
        }
        Vector3 angle = new Vector3(0, 0, degree);
        var q = Quaternion.AngleAxis(degree, Vector3.forward);
        var newPosition = position;

            newPosition = position + q * Vector3.right * length;



        currentBranch.GetComponent<BranchGrowth>().updatePoint(position, newPosition, branchIndex, thickness * treeThickness);
        position = newPosition;

    }

    GameObject drawForward2(DrawType type, float curve, ref Vector3 position, ref float degree, GameObject go, ref int order, GameObject currentBranch = null, float width = 1)
    {
        //if (shouldCurve)
        {

            degree += curve;// + Random.Range(-rotationDegreeRandom, rotationDegreeRandom);
        }
        Vector3 angle = new Vector3(0, 0, degree);
        var q = Quaternion.AngleAxis(degree, Vector3.forward);
        var newPosition = position;
        if (type == DrawType.branch)
        {

            newPosition = position + q * Vector3.right * length;

            //Debug.Log("positions: " + position + " " + newPosition);

            if (currentBranch == null)
            {
                Debug.Log("?");
            }
            else
            {
                addBranch2(currentBranch, width, newPosition, position, ref order);
            }
        }
        //var newPosition = position + angle * length;
        //var ob = connect(position, newPosition, angle, type, go);
        position = newPosition;
        //if (type != DrawType.branch)
        //{
        //    flowersAndLeaves.Add(ob.GetComponent<SpriteRenderer>());

        //    ob.GetComponent<SpriteRenderer>().enabled = showFlower;
        //    ob.GetComponent<SpriteRenderer>().sortingOrder = order + 1;
        //}
        return null;
    }


    
    void addBranch2(GameObject currentBranch, float thickness, Vector3 endPosition, Vector3 startPosition, ref int order)
    {
        
        currentBranch.GetComponent<SpriteShapeRenderer>().sortingOrder = order;
        order -= 2;

        currentBranch.GetComponent<BranchGrowth>().addPoint(startPosition,endPosition, branchIndex, thickness * treeThickness);
        currentBranch.GetComponent<SpriteShapeController>().spriteShape = curveBranchTextures[branchTextureIndex];
    }


    struct StackData
    {
        public Vector3 position;
        public float degree;
        public GameObject parent;
        public int branchDepth;
        public StackData(Vector3 p, float d, GameObject pa, int b)
        {
            position = p;
            degree = d;
            parent = pa;
            branchDepth = b;
        }
    }

    void Start()
    {
        buttonSplit = GameObject.Find("ButtonSplit").GetComponent<Button>();
        buttonMerge = GameObject.Find("ButtonMerge").GetComponent<Button>();
        buttonChangeWood = GameObject.Find("ButtonChangeWood").GetComponent<Button>();
        buttonRefresh = GameObject.Find("ButtonRefresh").GetComponent<Button>();
        buttonToggleFlower = GameObject.Find("ButtonToggleFlower").GetComponent<Button>();
        labelIteration = GameObject.Find("LabelIteration").GetComponent<Text>();
        lineGenerator = GameObject.Find("LineGenerator").GetComponent<LineGenerator>();
        camera = Camera.main;
        buttonSplit.onClick.AddListener(() =>
        {
            if (currentIteration >= kMaxIteration - 1 || lineGenerator.IsInFade) return;
            currentIteration++;
            UpdateIterationLabel();
            nextIteration();
            //var curveData = curveTable[currentIteration];
            //drawTree();
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




        currentIteration = 4;
        UpdateIterationLabel();
        drawTree();


    }

    int order = 1000;

    GameObject parent;



    struct BranchData
    {
        public string str;
        public List<GameObject> gos;
        public Transform parent;
        public float fowardCurve;
        public GameObject branch;
        public float startWidth;

        public List<int> bracketToChildIndex;
        public void init()
        {
            gos = new List<GameObject>();
            str = "";
            bracketToChildIndex = new List<int>();
        }
    }

    List<BranchData> branchData;

    struct BracketStackData
    {
        public GameObject go;
        public float width;

        public BracketStackData(GameObject g,float w)
        {
            go = g;
            width = w;
        }
    }
    Stack<BracketStackData> bracketStack = new Stack<BracketStackData>();
    int goIndex = 0;
    Dictionary<GameObject, float> goToWidth;
    int branchIndex;
    void splitAndGenerateBranchData(string str, int offset, Transform parent, int originalBranchDataIndex,float width, List<int> isNewList = null)
    {

        Vector3 currentPosition = Vector3.zero;
        float currentRotation =0;

        if(goToWidth.ContainsKey(parent.gameObject))
        {
            width = goToWidth[parent.gameObject];
        }

        BranchData data = new BranchData();
        data.init();
        data.parent = parent;
        data.startWidth = width;
        GameObject currentBranch = null;
        data.fowardCurve = (1 - Random.Range(0, 2) * 2) * Random.Range(2, forwardCurve);
        bool isNew = true;
        int bracketIndex = 0;
        branchIndex = 0;
        for (int i = offset; i < str.Length; i++)
        {
            switch (str[i])
            {
                case 'F':
                    //if this has an original branch, update current branch(sprite shape) to its branch
                    // else if it is new, create a sprite shape
                    data.str += 'F';
                    data.gos.Add(null);
                    //do we need isNew? or just check currentBranch is null
                    if (isNew)
                    {

                        if (originalBranchDataIndex >= 0 && branchData[originalBranchDataIndex].branch)
                        {
                            currentBranch = branchData[originalBranchDataIndex].branch;
                        }
                        else
                        {
                            currentBranch = Instantiate(curveBranchPrefab, parent);
                            currentBranch.GetComponent<SpriteShapeController>().spline.Clear();
                            addBranch2(currentBranch, width, currentPosition, currentPosition, ref order);
                        }
                        data.branch = currentBranch;
                        isNew = false;
                    }

                    width *= widthDecrease;

                    branchIndex++;
                    //update branch(sprite shape), this would draw a line
                    if (originalBranchDataIndex>=0 && isNewList[i] >= 0)
                    {
                        calculateNextPosition(i > currentIteration ? data.fowardCurve : 0, ref currentPosition, ref currentRotation,currentBranch,width);

                    }
                    else
                    {

                        drawForward2(DrawType.branch, i > currentIteration ? data.fowardCurve : 0, ref currentPosition, ref currentRotation, parent.gameObject, ref order, currentBranch, width);
                    }

                    break;
                case '[':
                    //there potentially be another branch, add a gameobject here.
                    // if this is not a new one, dont add, instead update gameobject rotation and position
                    data.str += '[';

                    if (isNewList!=null && isNewList[i]>=0 && branchData[originalBranchDataIndex].gos[isNewList[i]])
                    {
                        
                        GameObject go = branchData[originalBranchDataIndex].gos[isNewList[i]];
                        
                        go.transform.parent = parent;

                        //go.transform.localPosition = currentPosition;
                        go.transform.rotation = parent.rotation;
                        go.transform.Rotate(Vector3.forward, currentRotation);
                        //data.gos.Add(go);
                        bracketStack.Push(new BracketStackData(go, width));


                        //maxY = Mathf.Max(go.transform.position.y, maxY);
                        //minY = Mathf.Min(go.transform.position.y, minY);
                        //maxX = Mathf.Max(go.transform.position.x, maxX);
                        //minX = Mathf.Min(go.transform.position.x, minX);

                        //if 
                        if(originalBranchDataIndex>=0 && bracketIndex < branchData[originalBranchDataIndex].bracketToChildIndex.Count)
                        {

                            var childBranch = branchData[originalBranchDataIndex].bracketToChildIndex[bracketIndex];
                            var bb = branchData[childBranch];
                            bb.startWidth = width;// .Insert(0, branchData.Count);
                            branchData[childBranch] = bb;
                        }

                        goToWidth[go] = width;
                        if(currentBranch)
                        {

                            currentBranch.GetComponent<BranchGrowth>().addObject(go, branchIndex);
                        }
                        else
                        {
                            //Debug.LogError("no branch?");
                        }
                    }
                    else
                    {


                        GameObject go = new GameObject("go" + goIndex);
                        goIndex++;
                        go.transform.parent = parent;
                        //go.transform.localPosition = currentPosition;
                        go.transform.rotation = parent.rotation;
                        go.transform.Rotate(Vector3.forward, currentRotation);


                        //maxY = Mathf.Max(go.transform.position.y, maxY);
                        //minY = Mathf.Min(go.transform.position.y, minY);
                        //maxX = Mathf.Max(go.transform.position.x, maxX);
                        //minX = Mathf.Min(go.transform.position.x, minX);

                        data.gos.Add(go);
                        bracketStack.Push(new BracketStackData(go, width));
                        goToWidth[go] = width;
                        if (currentBranch)
                        {

                            currentBranch.GetComponent<BranchGrowth>().addObject(go, branchIndex);
                        }
                        else
                        {
                            //Debug.LogError("no branch?");
                        }
                    }

                    //wasABracket = true;
                    //stack.Add(new StackData(currentPosition, currentRotation, currentObject, thicknessStack[thicknessStack.Count - 1]));
                    break;
                case ']':
                    //end of a branch
                    data.gos.Add(null);
                    if(data.str.Length==0)
                    {
                        return;
                    }
                    data.str += str[i];
                    // if this is the original branch, dont do anything, otherwise add this branch data
                    if (originalBranchDataIndex>=0)
                    {
                        branchData[originalBranchDataIndex] = data;
                    }
                    else
                    {
                        branchData.Add(data);
                    }

                    var popGo = bracketStack.Pop();

                    data.startWidth = popGo.width;


                    data.bracketToChildIndex.Insert(0, branchData.Count);
                    splitAndGenerateBranchData(str, i + 1, popGo.go.transform, -1, popGo.width);
                    return;
                case '+':
                    currentRotation += rotationDegree + Random.Range(-rotationDegreeRandom, rotationDegreeRandom);
                    data.gos.Add(null);
                    data.str += str[i];
                    break;
                case '-':
                    currentRotation -= rotationDegree + Random.Range(-rotationDegreeRandom, rotationDegreeRandom);
                    data.gos.Add(null);
                    data.str += str[i];
                    break;
                default:
                    data.gos.Add(null);
                    data.str += str[i];
                    break;
            }
        }
    }


    string Xr = "F+[[X]-X]-F[-FX][+X]";
    string Xr1 = "F-[[X]+X]+F[+FX]-X";
    string Xr5 = "F-[[[X]+X]+F[+FX]-X]++FX";
    string Xr2 = "F[+X][-X]FX";
    string Xr3 = "F[+X]F[-X]+X";
    string XrTest = "F[+X][-X]";
    List<string> Xrs;
    string Fr = "FF";
    bool hasStarted = false;
    void drawTree()
    {
        goIndex = 0;
        hasStarted = false;
        Xrs = new List<string>() { XrTest };
        flowersAndLeaves = new List<SpriteRenderer>();
        List<char> res = new List<char>() { 'X' };
        //var Fr = "FF";
        curveTable.Add(res);
        goToWidth = new Dictionary<GameObject, float>();
        branchData = new List<BranchData>();

        var start = Xrs[Random.Range(0, Xrs.Count)];
        if (parent)
        {
            Destroy(parent);
        }
        parent = new GameObject();
        splitAndGenerateBranchData(start,0,parent.transform,-1,1);
        parent.transform.Rotate(Vector3.forward, 90);
        //addBranches();
        currentIteration = 1;
        UpdateIterationLabel();


        camera.transform.position = new Vector3((maxX + minX) / 2f, (maxY + minY) / 2, camera.transform.position.z);
        camera.orthographicSize = (maxY - minY) / 2;
    }

    void growOneBranch()
    {

    }

    void nextIteration()
    {
        //treeThicknessScale *= 2;
        var originCount = branchData.Count;
        for (int i = 0;i< originCount; i++)
        {
            var branch = branchData[i];
            var str = branch.str;
            var newStr = "";
            List<int> isNew = new List<int>();
            for (int j = 0; j < str.Length; j++)
            {
                switch (str[j])
                {
                    case 'F':
                        newStr+=Fr;
                        isNew.Add(j);
                        for (int k = 1; k < Fr.Length; k++)
                        {

                            isNew.Add(-1);
                        }
                        break;
                    case 'X':
                        var xr = Xrs[Random.Range(0, Xrs.Count)];
                        newStr += xr;
                        for (int k = 0; k < xr.Length; k++)
                        {

                            isNew.Add(-1);
                        }
                        break;
                    default:
                        newStr += str[j];
                        isNew.Add(j);
                        break;
                }
            }
            splitAndGenerateBranchData(newStr, 0, branch.parent.transform, i,branch.startWidth,isNew);
            //addBranches();
            //break;
        }

        camera.transform.position = new Vector3((maxX + minX) / 2f, (maxY + minY) / 2, camera.transform.position.z);
        camera.orthographicSize = (maxY - minY) / 2;
    }

    void UpdateIterationLabel()
    {
        labelIteration.text = "Iteration : " + currentIteration.ToString();
    }
}
