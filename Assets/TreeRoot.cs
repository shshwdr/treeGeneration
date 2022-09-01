using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.U2D;

public class TreeRoot : MonoBehaviour
{

    [SerializeField] GameObject treeRootPrefab;


    [SerializeField] float initRadius = 300f;
    [SerializeField] float treeThickness = 2f;
    [SerializeField] float forwardCurve = 10f;
    [SerializeField]
    float rotationDegree = 25;
    [SerializeField]
    float rotationDegreeRandom = 5;
    [SerializeField]
    int iteration = 1;
    float length = 60;
    [SerializeField]
    float originalLength = 2;


    [SerializeField] float lengthDecrease = 0.9f;

    [SerializeField] float widthDecrease = 0.9f;
    [SerializeField] float treeThicknessScale = 2;

    [SerializeField] GameObject branchPrefab;
    [SerializeField] GameObject leafPrefab;
    [SerializeField] GameObject flowerPrefab;
    [SerializeField] GameObject curveBranchPrefab;

    int branchTextureIndex = 0;
    [SerializeField] List<SpriteShape> curveBranchTextures;

    List<SpriteRenderer> flowersAndLeaves;

    bool showFlower = true;




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
    enum DrawType { branch, blossom, leaf };

    // Start is called before the first frame update
    void Start()
    {
        drawTree();
    }

    string Xr = "F+[[X]-X]-F[-FX][+X]";
    string XrNew = "F+[X][-X]-F[-FX][+X]";
    string Xr1 = "F-[[X]+X]+F[+FX]-X";
    string Xr5 = "F-[[[X]+X]+F[+FX]-X][++FX]";
    string Xr2 = "F[+X][-X]FX";
    string Xr3 = "F[+X]F[-X]+X";
    string XrTest = "F[+X][-X]";
    List<string> Xrs;
    string Fr = "F";
    bool hasStarted = false;

    public bool isTrueRoot;

    int branchIndex;

    public float startWidth = 1;
    void drawTree()
    {
        length = originalLength;
        //goIndex = 0;
        //hasStarted = false;
        Xrs = new List<string>() { XrTest };
        flowersAndLeaves = new List<SpriteRenderer>();
        //List<char> res = XrNew;
        //curveTable.Add(res);
        //goToWidth = new Dictionary<GameObject, float>();
        //branchData = new List<BranchData>();

        var start = XrNew;// Xrs[Random.Range(0, Xrs.Count)];

        splitAndGenerateBranchData(start, 0, transform, startWidth);

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

        currentBranch.GetComponent<BranchGrowth>().addPoint(startPosition, endPosition, branchIndex, thickness * treeThickness);
        currentBranch.GetComponent<SpriteShapeController>().spriteShape = curveBranchTextures[branchTextureIndex];
    }


    struct BracketStackData
    {
        public Vector3 position;
        public float rotation;
        public int branchIndex;
        public float width;

        public BracketStackData(Vector3 p, float r, int b,float w)
        {
            position = p;
            rotation = r;
            branchIndex = b;
            width = w;
        }
    }
    Stack<BracketStackData> bracketStack = new Stack<BracketStackData>();

    int order = 1000;

    Dictionary<GameObject, int> nodeToBranchIndex = new Dictionary<GameObject, int>();
    Dictionary<GameObject, float> nodeToWidth = new Dictionary<GameObject, float>();
    List<GameObject> unvisitedNode = new List<GameObject>();
    GameObject currentBranch = null;
    void splitAndGenerateBranchData(string str, int offset, Transform parent, float width, List<int> isNewList = null)
    {

        Vector3 currentPosition = Vector3.zero;
        float currentRotation = 0;

        BranchData data = new BranchData();
        data.init();
        data.parent = parent;
        data.startWidth = width;
        data.fowardCurve = (1 - Random.Range(0, 2) * 2) * Random.Range(2, forwardCurve);
        bool isNew = true;
        int bracketIndex = 0;


        currentBranch = Instantiate(curveBranchPrefab, /*parent.transform.position,Quaternion.identity/* Quaternion.EulerAngles(0,0,currentRotation), */parent);
        currentBranch.GetComponent<SpriteShapeController>().spline.Clear();
        addBranch2(currentBranch, width, currentPosition, currentPosition, ref order);


        branchIndex = 0;
        for (int i = offset; i < str.Length; i++)
        {
            switch (str[i])
            {
                case 'F':
                    //if this has an original branch, update current branch(sprite shape) to its branch
                    // else if it is new, create a sprite shape
                    data.str += 'F';


                    width *= widthDecrease;

                    branchIndex++;
                    drawForward2(DrawType.branch, /*i > 1 ? data.fowardCurve : 0*/0, ref currentPosition, ref currentRotation, parent.gameObject, ref order, currentBranch, width);
                    break;
                case '[':
                    bracketStack.Push(new BracketStackData(currentPosition, currentRotation, branchIndex, width));
                    break;
                case ']':
                    var popup = bracketStack.Pop();
                    currentPosition = popup.position;
                    currentRotation = popup.rotation;
                    branchIndex = popup.branchIndex;
                    width = popup.width;
                    break;
                case '+':
                    currentRotation += rotationDegree;// + Random.Range(-rotationDegreeRandom, rotationDegreeRandom);
                    data.gos.Add(null);
                    data.str += str[i];
                    break;
                case '-':
                    currentRotation -= rotationDegree;// + Random.Range(-rotationDegreeRandom, rotationDegreeRandom);
                    data.gos.Add(null);
                    data.str += str[i];
                    break;
                case 'X':
                    var go = new GameObject("child");
                    //go.transform.parent = transform;

                    go.transform.position = new Vector3(1, 1, 1); //transform.TransformPoint(currentPosition);
                    Debug.Log("transform position " + go.transform.position);
                    go.transform.localEulerAngles = (new Vector3(0, 0, currentRotation +transform.rotation.eulerAngles.z));
                    StartCoroutine(test(go));
                    // go.transform.Rotate(Vector3.forward,/* transform.rotation.eulerAngles.z+*/ currentRotation);
                    nodeToBranchIndex[go] = branchIndex;
                    nodeToWidth[go] = width;
                    unvisitedNode.Add(go);
                    TreeGeneration.Instance.updatePosition(go.transform.position);
                    break;
                default:
                    data.gos.Add(null);
                    data.str += str[i];
                    break;
            }
        }
        currentBranch.GetComponent<BranchGrowth>().nodeToBranchIndex = nodeToBranchIndex;
    }

    IEnumerator test(GameObject go)
    {
        yield return new WaitForSeconds(0.1f);
        go.transform.parent = transform;
    }

    public void trunkGrow()
    {
        TreeGeneration.Instance.growTrunk();
        currentBranch.GetComponent<BranchGrowth>().grow();
        growTime++;
    }
    int growTime = 0;
    public Transform getNode(ref float width)
    {
        if (growTime <= 0)
        {
            trunkGrow();
            return null;
        }

        var rand = Random.Range(0, unvisitedNode.Count);

        var node = unvisitedNode[rand];
        if (node.GetComponentInChildren<TreeRoot>())
        {
            return node.GetComponentInChildren<TreeRoot>().getNode(ref width);
        }
        //unvisitedNode.RemoveAt(0);
        width = getWidth(node);
        return node.transform;
    }



    public float getWidth(GameObject go)
    {
        return nodeToWidth[go];
    }
}
