using UnityEngine;
using System.Collections;

[ExecuteInEditMode]
[RequireComponent (typeof (MeshFilter))]
public class Skirt : MonoBehaviour {

	public int subdivisions = 10;
	public float radius1 = 1f;
	public float radius2 = 1f;
	public float height = 2f;

	void Start () {
		GetComponent<MeshFilter>().sharedMesh = Create(subdivisions);
	}
	
	void Update () {
         UpdateMeshVertices(GetComponent<MeshFilter>().sharedMesh,currentSubdivisions);
	}
    
    Vector3 []vertices;
    int currentSubdivisions;
    Mesh currentMesh;
    
    void UpdateMeshVertices(Mesh mesh,int subdivisions)
    {        
        for(int i = 0, n = subdivisions - 1; i < subdivisions; i++) 
        {
            float ratio = (float)i / n;
            float r = ratio * (Mathf.PI * 2f);
            float x1 = Mathf.Cos(r) * radius1;
            float z1 = Mathf.Sin(r) * radius1;
            float x2 = Mathf.Cos(r) * radius2;
            float z2 = Mathf.Sin(r) * radius2;

            // top circle
            vertices[i] = new Vector3(x1,height, z1);
            // bottom circle
            vertices[i+subdivisions] = new Vector3(x2, 0f, z2);
        }
        mesh.vertices=vertices;
		mesh.RecalculateBounds();
		mesh.RecalculateNormals();
    }
    
	Mesh Create (int subdivisions) {
        currentSubdivisions=subdivisions;
        Mesh mesh = new Mesh();
        currentMesh=mesh;

		vertices = new Vector3[subdivisions*2 + 2];
		Vector2[] uv = new Vector2[vertices.Length];
		int[] triangles = new int[(subdivisions * 2) * 3];

		for(int i = 0, n = subdivisions - 1; i < subdivisions; i++) {
			float ratio = (float)i / n;
			uv[i ] = new Vector2(ratio, 0f);
			uv[i+subdivisions ] = new Vector2(ratio, 1f);
		}
		// construct sides
		for(int i = 0, n = subdivisions - 1; i < n; i++) {
			int offset = i * 6;
			triangles[offset + 0] = i ; 
			triangles[offset + 1] = i+subdivisions ; 
			triangles[offset + 2] = i + 1; 
            
			triangles[offset+3 ] = i+1 ; 
			triangles[offset + 4] = i+subdivisions ; 
			triangles[offset + 5] = i + 1+subdivisions; 
		}
        
        UpdateMeshVertices(mesh,subdivisions);
		mesh.uv = uv;
		mesh.triangles = triangles;
		mesh.RecalculateBounds();
		mesh.RecalculateNormals();
        
		return mesh;
	}

    
    
}