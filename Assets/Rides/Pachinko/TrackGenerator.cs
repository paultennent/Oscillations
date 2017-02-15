using UnityEngine;
using System.Collections;

[ExecuteInEditMode]
[RequireComponent (typeof (MeshFilter))]
public class TrackGenerator : MonoBehaviour {

    public float lengthInlet = 10f;
    public float radiusLoop = 20f;
    public float gapPercent = 5f;
    public float width=2f;
    public float thickness=1f;

    public bool generateNew=false;
    
    private float []extrusionX={1,-1,-1,1};
    private float []extrusionY={1,1,-1,-1};
   
   
    const int NUM_POINTS=41;
    
	void Start () {
		GetComponent<MeshFilter>().sharedMesh = Create();
	}
	
	void Update () {
         UpdateMeshVertices(GetComponent<MeshFilter>().sharedMesh,null);
         if(generateNew)
         {
             generateNew=false;
//             CreateNewSegment(22,0);
             CreateNewSegment(45,40);
         }
	}
    
    public GameObject CreateNewSegment(float angleVert,float angleHorz)
    {
        float r= angleVert*Mathf.Deg2Rad;
        Vector3 startPoint=transform.TransformPoint(new Vector3(0,radiusLoop-Mathf.Cos(r)*radiusLoop,lengthInlet+Mathf.Sin(r)*radiusLoop));
        GameObject newObj=Instantiate(gameObject);
        newObj.transform.position=startPoint;
        Quaternion hRot=Quaternion.Euler(0,angleHorz,0);
        Quaternion vRot=Quaternion.Euler(-angleVert,0,0);
        newObj.transform.rotation=newObj.transform.rotation*vRot*hRot;
//        newObj.transform.Rotate(new Vector3(-angleVert,angleHorz,0));
        return newObj;
    }
    
    Vector3 []vertices;
    int currentSubdivisions;
    Mesh currentMesh;
    
    void UpdateMeshVertices(Mesh mesh,Vector2[] uv)
    {        
        int pointsPerSlice=extrusionX.Length;
        //start ramp
        for(int k=0;k<pointsPerSlice;k++)
        {
            float texX=k/(float)pointsPerSlice;
            vertices[k]=new Vector3(extrusionX[k]*width,-extrusionY[k]*thickness,0);
            if(uv!=null)
            {
                uv[k]=new Vector2(texX,0);
            }
        }      
        float centreY=radiusLoop;
        float centreZ=lengthInlet;
        //loop
        float mult = 1.0f-gapPercent*0.01f;
        float n = vertices.Length-pointsPerSlice;        
        for(int i=pointsPerSlice;i<vertices.Length;i+=pointsPerSlice)
        {
            float ratio = (float)(i-4) / n;
            float r = mult*ratio * (Mathf.PI * 2f);
            float cosR=Mathf.Cos(r);
            float sinR=Mathf.Sin(r);
            
            for(int k=0;k<pointsPerSlice;k++)
            {
                float texX=k/(float)pointsPerSlice;
                float y=centreY-cosR*(radiusLoop+extrusionY[k]*thickness);
                float z=sinR*(radiusLoop+extrusionY[k]*thickness) + centreZ;
                vertices[i+k]=new Vector3(extrusionX[k]*width,y,z);
                if(uv!=null)
                {
                    uv[i+k]=new Vector2(texX,ratio);
                }
            }
        }

        mesh.vertices=vertices;
		mesh.RecalculateBounds();
		mesh.RecalculateNormals();
    }
    
	Mesh Create () {
        
        Mesh mesh = new Mesh();
        currentMesh=mesh;
        int pointsPerSlice=extrusionX.Length;

		vertices = new Vector3[NUM_POINTS*pointsPerSlice];
		Vector2[] uv = new Vector2[vertices.Length];
		int[] triangles = new int[(NUM_POINTS-1)*6*pointsPerSlice];
        // build triangle extrusion
        for(int i=0;i<NUM_POINTS-1;i++)
        {
            for(int k=0;k<pointsPerSlice;k++)
            {
                int offset= (i*pointsPerSlice + k)*6;
                int k2 = (k+1)%pointsPerSlice;
                int p1=i*pointsPerSlice+k;
                int p2=i*pointsPerSlice+k2;
                int p3=(i+1)*pointsPerSlice+k;
                int p4=(i+1)*pointsPerSlice+k2;
                triangles[offset + 0] = p1 ; 
                triangles[offset + 1] = p3 ; 
                triangles[offset + 2] = p2; 
                triangles[offset+3 ] = p2 ; 
                triangles[offset + 4] = p3 ; 
                triangles[offset + 5] = p4; 
            }
        }
        UpdateMeshVertices(mesh,uv);
		mesh.uv = uv;
		mesh.triangles = triangles;
		mesh.RecalculateBounds();
		mesh.RecalculateNormals();
        
		return mesh;
	}

    
    
}