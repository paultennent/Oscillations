using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
public class TopColliderAdder : MonoBehaviour {

    [MenuItem("Assets/Add Colliders to top floors")]
    public static void AddColliders()
    {
        print( "woo");
		MeshFilter[] objs=GameObject.FindObjectsOfType(typeof(MeshFilter)) as MeshFilter[]; //returns Object[]
        foreach(MeshFilter obj in objs)
        {
           GameObject go=  obj.gameObject;
           if(go.transform.parent!=null)
           {
               GameObject gParent=go.transform.parent.gameObject;
               if(gParent.name.StartsWith("Floor1"))
               {
                       print("boo");
                   if(go.GetComponent<MeshCollider>()==null)
                   {
                       print("yay");
                       MeshCollider me =go.AddComponent<MeshCollider>() as MeshCollider; 
                       me.sharedMesh=obj.sharedMesh;
                   }
               }
           }
        }
    }

	// Use this for initialization
	void Start () {
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
