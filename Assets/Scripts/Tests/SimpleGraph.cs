using UnityEngine;
using UnityEngine.UI;

using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

//public class SimpleGraph : Graphic
public class SimpleGraph : MaskableGraphic
{
    public string title="";

    private float[] dataPoints;

    private float minY=-1f;
    private float maxY=1f;
    
    private bool fixedRange=false;

    private GameObject mLabel=null;
    
    public void FixRange(float[] minMax)
    {
        if(minMax!=null && minMax.Length==2)
        {
            minY=minMax[0];
            maxY=minMax[1];
            fixedRange=true;
        }else
        {
            fixedRange=false;
        }
    }
  
    public void AddLabel()
    {
        if(mLabel==null && Application.isPlaying)
        {
            int index=-1;
            string indexStr=Regex.Match(name, @"\d+").Value;
            if(indexStr!=null)
            {
                index=Int32.Parse(indexStr);
            }
            string labelName="Label_for_"+name;
            Transform obj=transform.parent.Find(labelName);
            if(obj!=null)                
            {
                mLabel=obj.gameObject;
            }else
            {
                mLabel=new GameObject(labelName);
                mLabel.transform.parent=transform.parent;
                Text text=mLabel.AddComponent<Text>();
                text.text = title;
                text.fontSize=12;
                text.alignment=TextAnchor.LowerLeft;

                Font ArialFont = (Font)Resources.GetBuiltinResource(typeof(Font), "Arial.ttf");
                text.font = ArialFont;
                text.material = ArialFont.material;
                text.alignment=TextAnchor.MiddleLeft;
                text.color=color;

                RectTransform tThem=mLabel.GetComponent<RectTransform>();
				tThem.localScale = new Vector3 (1f, 1f, 1f);
				tThem.localPosition = new Vector3 (0f, 0f, 0f);
//                if(index!=-1)
//                {
//                    tThem.anchorMin=new Vector2(0.1f*(index),0);
//                    tThem.anchorMax=new Vector2(1,.1f);
//                }else
//                {
                    tThem.anchorMin=new Vector2(0,0);
                    tThem.anchorMax=new Vector2(1,1);
                //}
                tThem.offsetMin=new Vector2(0,0);
                tThem.offsetMax=new Vector2(0,0);
            }
        } 
    }

	public void setLabel(string text){
		if (mLabel != null) {
			title = text;
			Text t = mLabel.GetComponent<Text>();
			t.text = title;
		}
	}
  
    public void SetPoints(float[]points)
    {
        dataPoints=points;
        if(points!=null && points.Length>0 && !fixedRange)
        {
            minY=points[0];
            maxY=points[0];
            for(int c=0;c<points.Length;c++)
            {
                if(minY>points[c])minY= points[c];
                if(maxY<points[c])maxY=points[c];
            }
        }
    }

    Mesh myMesh;
    VertexHelper myVH;
	// Update is called once per frame
	void Update () 
    {
        DoDraw();
	}

    void DoDraw()
    {
        if(mLabel==null)
        {
            AddLabel();
        }
        if(myMesh==null)
        {
            myMesh=new Mesh();
        }
        DrawToMesh(myMesh);
        canvasRenderer.SetMesh(myMesh);
    }
    
    bool firstCall=true;

    Vector3 []vertices;
        
    // this is by far the fastest way to do this (as of unity 5.5b11), and doesn't allocate any objects unless the data length changes
    // I tried doing this in various ways but they all involved garbage collection or were slow
    protected void DrawToMesh(Mesh m)
    {
        if(firstCall)
        {
            rectTransform.localScale=new Vector2(rectTransform.rect.width/2,rectTransform.rect.height/2);
            firstCall=false;            
        }
        if(dataPoints==null)
        {
            float[] newFloat={-1f,1f,-1f,1,-1,1};
            dataPoints=newFloat;
            minY=-1;
            maxY=1;
        }
        if(m.vertexCount!=dataPoints.Length*4 || vertices==null)
        {
            float x1=-1;
            float scaleX=2.0f /(float)dataPoints.Length;
            int[] triangles=new int[dataPoints.Length*6];
            vertices=new Vector3[dataPoints.Length*4];
            Vector2 []uv=new Vector2[dataPoints.Length*4];
            Color32[] colors=new Color32[vertices.Length];
            for(int c=0;c<dataPoints.Length;c++)
            {
                float x2 = scaleX*(float)c-1;
                
                vertices[c*4]=new Vector3(x1,-0.1f,0);
                vertices[c*4+1]=new Vector3(x1,0.1f,0);
                vertices[c*4+2]=new Vector3(x2,0.1f,0);
                vertices[c*4+3]=new Vector3(x2,-0.1f,0);
                
                uv[c*4]=new Vector2(0,0);
                uv[c*4+1]=new Vector2(0,1);
                uv[c*4+2]=new Vector2(1,1);
                uv[c*4+3]=new Vector3(1,0);
                
                colors[c*4]=color;
                colors[c*4+1]=color;
                colors[c*4+2]=color;
                colors[c*4+3]=color;
                                
                triangles[c*6]=c*4;
                triangles[c*6+1]=c*4+1;
                triangles[c*6+2]=c*4+2;
                
                triangles[c*6+3]=c*4+2;
                triangles[c*6+4]=c*4+3;
                triangles[c*6+5]=c*4;                
                x1=x2;
            }
            m.Clear();
            m.vertices=vertices;
            m.colors32=colors;
            m.uv=uv;
            m.triangles=triangles;            
        }
        int vertexIndex=0;
        float lastY=0;
        float scaleY=2.0f / (maxY-minY);
        float offsetY=-minY;
        for(int c=0;c<dataPoints.Length;c++)
        {
            float y=-1+(dataPoints[c]+offsetY)*scaleY;
            vertices[vertexIndex].y=lastY-.01f;
            vertices[vertexIndex+1].y=lastY+.01f;
            vertices[vertexIndex+2].y=y+.01f;
            vertices[vertexIndex+3].y=y-.01f;
            vertexIndex+=4;
            lastY=y;
        }
        m.vertices=vertices;
        
    }

    protected override void OnPopulateMesh(VertexHelper vh)
    {
 //       Update();
    }

    
}