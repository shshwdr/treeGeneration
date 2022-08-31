using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TreeGeneration : MonoBehaviour
{
    public GameObject treeRootPrefab;

    Transform root;
    
    public void grow()
    {
        if(root == null)
        {

            var go = Instantiate(treeRootPrefab);
            root = go.transform;
        }
        else
        {

            var trans = root.GetComponent<TreeRoot>().getNode();
            Instantiate(treeRootPrefab, trans.position, trans.rotation, trans);
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.P))
        {
            grow();
        }
    }
}
