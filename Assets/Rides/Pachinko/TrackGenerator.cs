using UnityEngine;
using System.Collections;

[ExecuteInEditMode]
[RequireComponent (typeof (MeshFilter))]
public class TrackGenerator : MonoBehaviour {

    public float lengthInlet = 10f;
    public float radiusLoop = 20f;
    public float gapPercent = 5f;
    public float width=2f;

	void Start () {
		GetComponent<MeshFilter>().sharedMesh = Create();
	}
	
	void Update () {
         UpdateMeshVertices(GetComponent<MeshFilter>().sharedMesh);
	}
    
    Vector3 []vertices;
    int currentSubdivisions;
    Mesh currentMesh;
    
    void UpdateMeshVertices(Mesh mesh)
    {        
        vertices[0]=new Vector3(-width,0,0);
        vertices[1]=new Vector3(width,0,0);
        float centreY=radiusLoop;
        float centreZ=radiusLoop+lengthInlet;
        float n = vertices.Length-2;
        float mult=1f-0.01f*gapPercent;
        for(int i=2;i<vertices.Length;i+=2)
        {
            float ratio = (float)(i-2) / n;
            float r = mult*ratio * (Mathf.PI * 2f);
            float y=centreY-Mathf.Cos(r)*radiusLoop;
            float z=Mathf.Sin(r)*radiusLoop + centreZ;
            vertices[i]=new Vector3(-width,y,z);
            vertices[i+1]=new Vector3(width,y,z);
        }
        mesh.vertices=vertices;
		mesh.RecalculateBounds();
		mesh.RecalculateNormals();
    }
    
	Mesh Create () {
        
        Mesh mesh = new Mesh();
        currentMesh=mesh;

        // vertices:
        // 40 on either side of the loop        
        // 2 into it
        // gap in the loop just before the entrance
                
        
		vertices = new Vector3[41*2];
		Vector2[] uv = new Vector2[vertices.Length];
		int[] triangles = new int[(vertices.Length-1)*3];
        float n=vertices.Length;
        for(int i=0;i<vertices.Length-2;i+=2)
        {
            float ratio = (float)i / n;

            int offset=i*3;
			triangles[offset + 0] = i ; 
			triangles[offset + 1] = i+1 ; 
			triangles[offset + 2] = i + 2; 

			triangles[offset+3 ] = i+2 ; 
			triangles[offset + 4] = i+1 ; 
			triangles[offset + 5] = i + 3; 
            
            uv[i]=new Vector2(0,ratio);
            uv[i+1]=new Vector2(1,ratio);
        }
        uv[vertices.Length-2]=new Vector2(0,1);
        uv[vertices.Length-1]=new Vector2(1,1);
        
        UpdateMeshVertices(mesh);
		mesh.uv = uv;
		mesh.triangles = triangles;
		mesh.RecalculateBounds();
		mesh.RecalculateNormals();
        
		return mesh;
	}

    
    
}