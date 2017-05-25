using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class MenuGenerator : MonoBehaviour {

    public GameObject menuParent;
    public Transform buttonPrefab;
    
	// Use this for initialization
	void Start () {
           print("All " + Resources.FindObjectsOfTypeAll<UnityEngine.Object>().Length);
        print("Textures " + Resources.FindObjectsOfTypeAll<Texture>().Length);
        print("AudioClips " + Resources.FindObjectsOfTypeAll<AudioClip>().Length);
        print("Meshes " + Resources.FindObjectsOfTypeAll<Mesh>().Length);
        print("Materials " + Resources.FindObjectsOfTypeAll<Material>().Length);
        print("GameObjects " + Resources.FindObjectsOfTypeAll<GameObject>().Length);
        print("Components " + Resources.FindObjectsOfTypeAll<Component>().Length);
        // empty any memory allocated by previous scenes
        Resources.UnloadUnusedAssets();
        GC.Collect();
    
        
		// find all scenes in build except scene 0 (menu)
        Regex nameExtractor=new Regex(".*/(([^/-]*).*).unity");
        List<string> sceneNames=new List<string>();
        List<string> sceneDisplayNames=new List<string>();
        for(int c=1;c<20;c++)
        {
            string name = SceneUtility.GetScenePathByBuildIndex(c);                           
            if(name==null || name.Length==0)break;
            Match match=nameExtractor.Match(name);
            if(match.Success)
            {
                sceneNames.Add(match.Groups[1].Value);
                sceneDisplayNames.Add(match.Groups[2].Value);
            }
        }
        float topPos=205f;
        float endPos=-681f;
        float numButtons=sceneNames.Count;
        float stepButton=(endPos-topPos)/6;
        float y=topPos;
        for(int c=0;c<sceneNames.Count;c++)
        {
            string scene=sceneNames[c];
            string sceneDisplay=sceneDisplayNames[c];
            Transform newButton=Instantiate(buttonPrefab);
            newButton.transform.SetParent(menuParent.transform,false);
            newButton.transform.SetAsFirstSibling();
            newButton.GetComponent<RectTransform>().localPosition=new Vector3(0,y,0);
            newButton.transform.GetChild(0).gameObject.GetComponent<Text>().text=sceneDisplay;
            newButton.name=scene;
            y+=stepButton;
        }            
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
