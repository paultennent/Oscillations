using UnityEngine;
using System.Collections;

[ExecuteInEditMode]
[RequireComponent (typeof (MeshFilter))]
public class TrackGenerator : MonoBehaviour {

    public float startRampRadius = 5f;
    public float lengthTransition = 50f;
    public float radiusLoop = 20f;
    public float gapPercent = 5f;
    public float width=2f;
    public float thickness=1f;
    public float lipHeight = 0.2f;
    public float lipWidth=0.1f;

    public bool generateNew=false;
    
//    #  #
//    ####
    
    
    
    private float []extrusionX={1f,.9f,.9f,-.9f, -.9f, -1, -1 , 1};
    private float []extrusionY={.1f,.1f,0, 0  , .1f , .1f , -1 , -1};
//    private float []extrusionX={1,-1,-1,1};
//    private float []extrusionY={0,0,-1,-1};
   
    private float lastStartRadius=0;
    private float lastTransition=0;
    private float lastRadius=0;
    private float lastGap=0;
    private float lastThick=0;
    private float lastWidth=0;
    private float lastLipWidth=0;
    private float lastLipHeight=0;    

    public void CalculateExtrusion()
    {
        float[] newX={1f,1-lipWidth,1-lipWidth,-1+lipWidth, -1+lipWidth, -1, -1 , 1};
        extrusionX=newX;
        float[] newY={lipHeight,lipHeight,0, 0  , lipHeight , lipHeight , -1 , -1};
        extrusionY=newY;
    }
    
   
    const int NUM_POINTS=61;
    const int START_POINTS=20;
    
	void Start () {
		GetComponent<MeshFilter>().sharedMesh = Create();
	}
	
	void Update () {
        if(lastTransition!=lengthTransition || lastRadius!=radiusLoop || lastGap!=gapPercent || lastThick!=thickness || lastWidth!=width || lastStartRadius!=startRampRadius || lastLipHeight!=lipHeight || lastLipWidth!=lipWidth)
        {
            CalculateExtrusion();
            UpdateMeshVertices(GetComponent<MeshFilter>().sharedMesh,null);
            lastTransition=lengthTransition;
            lastRadius=radiusLoop;
            lastGap=gapPercent;
            lastThick=thickness;
            lastWidth=width;
            lastStartRadius=startRampRadius;
            lastLipWidth=lipWidth;
            lastLipHeight=lipHeight;
        }
         if(generateNew)
         {
             generateNew=false;
//             CreateNewSegment(22,0);
             CreateNewSegment(45,40);
         }
	}
    
    public Vector3 GetTrackPosition(float trackDistance)
    {
        float startDistance = startRampRadius*Mathf.PI*.75f;
        
        if(trackDistance<startDistance)
        {
            float circleAngleRad=((startDistance-trackDistance)/startRampRadius);
            float posY=startRampRadius-Mathf.Cos(circleAngleRad)*startRampRadius;
            float posZ=-Mathf.Sin(circleAngleRad)*startRampRadius;
            return new Vector3(0,posY,posZ);
        }else if(trackDistance-startDistance<lengthTransition)
        {
            return new Vector3(0,0,trackDistance-startDistance);
        }else
        {
            float circleAngleRad=((trackDistance-(lengthTransition+startDistance))/radiusLoop);
            float posY=radiusLoop-Mathf.Cos(circleAngleRad)*radiusLoop;
            float posZ=lengthTransition+Mathf.Sin(circleAngleRad)*radiusLoop;
            return new Vector3(0,posY,posZ);
        }        
    }
    
    public float GetTrackSlopeAngle(float trackDistance)
    {
        float startDistance = startRampRadius*Mathf.PI*.75f;
        if(trackDistance<startDistance)
        {
            float circleAngleRad=((startDistance-trackDistance)/startRampRadius);
            return -circleAngleRad;            
        }else if(trackDistance-startDistance<lengthTransition)
        {
            return 0;
        }else
        {
            float circleAngleRad=((trackDistance-(lengthTransition+startDistance))/radiusLoop);
            return circleAngleRad;
        }
    }
    
    public float GetTrackLength()
    {
        float mult = 1f - gapPercent*.01f;
        return lengthTransition+radiusLoop*2f*Mathf.PI*mult;
    }
    
    float GetDistanceForPoint(int num)
    {
        if(startRampRadius>0)
        {
            float startDistance = startRampRadius*Mathf.PI*.75f;
            if(num==START_POINTS)
            {
                return startDistance;
            }else if(num<START_POINTS)
            {
                float ratio=(float)num/(float)START_POINTS;
                return ratio*startDistance;
            }else
            {
                float ratio=(num-(START_POINTS+1))/(float)(NUM_POINTS-(START_POINTS+1));
                float mult = 1f - gapPercent*.01f;
                return startDistance+lengthTransition + ratio*radiusLoop*2f*Mathf.PI*mult;
            }
        }else
        {
            if(num==0)
            {
                return 0;
            }else
            {
                float ratio=(num-1f)/(float)(NUM_POINTS-1);
                float mult = 1f - gapPercent*.01f;
                return lengthTransition + ratio*radiusLoop*2f*Mathf.PI*mult;
            }
        }
    }
    
    public float GetInitialDistance()
    {
        return startRampRadius*Mathf.PI*.75f;
    }
    
    public GameObject CreateNewSegment(float angleVert,float angleHorz)
    {
        float r= angleVert*Mathf.Deg2Rad;
        Vector3 startPoint=transform.TransformPoint(new Vector3(0,radiusLoop-Mathf.Cos(r)*(radiusLoop-lipHeight*2f),lengthTransition+Mathf.Sin(r)*(radiusLoop-lipHeight*2f)));
        GameObject newObj=Instantiate(gameObject);
        newObj.transform.position=startPoint;
        Quaternion hRot=Quaternion.Euler(0,angleHorz,0);
        Quaternion vRot=Quaternion.Euler(-angleVert,0,0);
        newObj.transform.rotation=newObj.transform.rotation*vRot*hRot;
        return newObj;
    }
    
    Vector3 []vertices;
    int currentSubdivisions;
    Mesh currentMesh;
    
    void UpdateMeshVertices(Mesh mesh,Vector2[] uv)
    {        
        int pointsPerSlice=extrusionX.Length;
            
        for(int i=0,n=0;i<vertices.Length;i+=pointsPerSlice,n+=1)
        {
            float ratio=n/(NUM_POINTS-1);
            float distanceTrack=GetDistanceForPoint(n);
            float angle=GetTrackSlopeAngle(distanceTrack);
            Vector3 pos = GetTrackPosition(distanceTrack);

            //print(n+":"+pos+"["+distanceTrack+"]");
            float cosAngle=Mathf.Cos(angle);
            float sinAngle=Mathf.Sin(angle);
            
            for(int k=0;k<pointsPerSlice;k++)
            {
                float texX=k/(float)pointsPerSlice;
                
                vertices[i+k]=new Vector3(extrusionX[k]*width,pos.y+extrusionY[k]*cosAngle ,pos.z-extrusionY[k]*sinAngle);
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
                triangles[offset + 2] = p1 ; 
                triangles[offset + 1] = p3 ; 
                triangles[offset + 0] = p2; 
                triangles[offset+5 ] = p2 ; 
                triangles[offset + 4] = p3 ; 
                triangles[offset + 3] = p4; 
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