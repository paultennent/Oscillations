using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlockLayout : MonoBehaviour {

    public GameObject[] midBlocks;
    public GameObject[] highBlocks;
    public GameObject startBlock;
    public GameObject endBlock;
    public GameObject parkStartBlock;
    public GameObject[] parkRepeatingBlocks;
    public GameObject parkEndBlock;

    public float drawForward=1000f;
    public Transform viewpoint;
    
    private float startTime=0f;
    private float blockDropPos=0f;
    enum LayoutPos
    {
        START,
        MID1,
        HIGH1,
        PARKSTART,
        PARKREST,
        HIGH2,
        MID2,
        END,
        FINISHED
    };
    
    // if the block has a 'target' point (e.g. start and end)
    private Transform currentTarget;
    private LayoutPos currentBlockPos=LayoutPos.START;
    
	// Use this for initialization
	void Start () {
        startTime=Time.time;
        LayoutIncrementally(0,0);
        if(currentTarget!=null)
        {
            viewpoint.position=currentTarget.position;
        }
//		LayoutCity(1000f);
	}
	
	// Update is called once per frame
	void Update () {
		float zPos=viewpoint.position.z;
        if(currentBlockPos==LayoutPos.FINISHED)
        {
            if(currentTarget!=null)
            {
                if(zPos>currentTarget.position.z)
                {
                    viewpoint.position=new Vector3(viewpoint.position.x,viewpoint.position.y,currentTarget.position.z);
                }
            }
        }
        else if(zPos+drawForward>blockDropPos)
        {
            float timeFraction=(Time.time-startTime)/60f;
            if(timeFraction>1f)timeFraction=1f;
            LayoutIncrementally(zPos,timeFraction);
        }
	}
    
    // need to do this incrementally
    // NB: this means:
    // 1) At each point, we need to think about what is visible and layout all that
    // 2) So, e.g. park must be laid out when we are close to it 
    // 3) You can't see so far from high rise blocks 
    // 4) Do we want to put a fake road in at that point?
    // 
    // At points:
    // start: need intro tile and 1 mid
    // 
    // at end of intro tile: need 3 mid by this point
    // in normal city, need 3 blocks ahead
    // park start needs 2 blocks ahead of it
    // rest of park can build half way through last high rise block
    // build high rise after park when we are on park start tile
    // then stick 1 block ahead
    // except end block needs to be 2 blocks ahead
    // 
    // or possibly just layout at 1500 (draw distance) for safety
    
    // need to know:
    // a) when we are supposed to be where we are currently (i.e. how much time is left)
    // b) how long that means we want it to be
    // 
    // possibly easiest way is to:
    // introduce transitions at particular times
    // (taking into account the draw forward)
    // skipping sections if we run low on time
    // 
    // stick in the final section with a lot of time to go (e.g. 30s)
    // stop when we hit end whatever
    // if they stop for some reason and run out of time, zoom to end anyway

    void LayoutIncrementally(float untilLength,float fractionThrough)
    {
        switch(currentBlockPos)
        {
         case LayoutPos.START:
            {
                // layout start block
                float newPos=PlaceBlock(startBlock,blockDropPos);        ;
                // we start in the middle of block zero
                blockDropPos+=newPos;
                currentBlockPos=LayoutPos.MID1;
            }
            break;
         case LayoutPos.MID1:
            {
                blockDropPos=PlaceBlock(midBlocks[Random.Range(0,midBlocks.Length-1)],blockDropPos);
                if(fractionThrough>0.25)
                {
                    currentBlockPos=LayoutPos.HIGH1;
                }
            }
            break;
         case LayoutPos.HIGH1:
            {
                blockDropPos=PlaceBlock(highBlocks[Random.Range(0,highBlocks.Length-1)],blockDropPos);
                if(fractionThrough>0.4)
                {
                    currentBlockPos=LayoutPos.PARKSTART;
                }
            }
            break;
         case LayoutPos.PARKSTART:
            {
                blockDropPos=PlaceBlock(parkStartBlock,blockDropPos);
                currentBlockPos=LayoutPos.PARKREST;
            }
            break;
         case LayoutPos.PARKREST:
            {
                for(int c=0;c<2;c++)
                {
                    blockDropPos=PlaceBlock(parkRepeatingBlocks[Random.Range(0,parkRepeatingBlocks.Length-1)],blockDropPos);            
                }
                blockDropPos=PlaceBlock(parkEndBlock,blockDropPos);
                currentBlockPos=LayoutPos.HIGH2;
            }
            break;
         case LayoutPos.HIGH2:
            {
                blockDropPos=PlaceBlock(highBlocks[Random.Range(0,highBlocks.Length-1)],blockDropPos);
                if(fractionThrough>0.7)
                {
                    currentBlockPos=LayoutPos.MID2;
                }
            }
            break;
         case LayoutPos.MID2:
            {
                blockDropPos=PlaceBlock(midBlocks[Random.Range(0,midBlocks.Length-1)],blockDropPos);
                if(fractionThrough>0.9)
                {
                    currentBlockPos=LayoutPos.END;
                }
            }
            break;
         case LayoutPos.END:
            {
                // layout end block
                float newPos=PlaceBlock(endBlock,blockDropPos);        ;
                // we start in the middle of block zero
                blockDropPos+=newPos;
                currentBlockPos=LayoutPos.FINISHED;
            }
            break;
         default:
            break;
        }        
    }
    
    void LayoutCity()
    {
        float curLength=0;
        curLength=PlaceBlock(startBlock,curLength);        
        for(int c=0;c<4;c++)
        {
            // mid rise
            curLength=PlaceBlock(midBlocks[Random.Range(0,midBlocks.Length-1)],curLength);
        }
        for(int c=0;c<2;c++)
        {
            // high rise
            curLength=PlaceBlock(highBlocks[Random.Range(0,highBlocks.Length-1)],curLength);
        }
        curLength=PlaceBlock(parkStartBlock,curLength);                
        for(int c=0;c<2;c++)
        {
            curLength=PlaceBlock(parkRepeatingBlocks[Random.Range(0,highBlocks.Length-1)],curLength);            
        }
        curLength=PlaceBlock(parkEndBlock,curLength);        
        for(int c=0;c<2;c++)
        {
            // high rise
            curLength=PlaceBlock(highBlocks[Random.Range(0,highBlocks.Length-1)],curLength);
        }
        for(int c=0;c<4;c++)
        {
            // mid rise
            curLength=PlaceBlock(midBlocks[Random.Range(0,midBlocks.Length-1)],curLength);
        }
        curLength=PlaceBlock(endBlock,curLength);        
        print (curLength);
    }
    
    
    float PlaceBlock(GameObject obj,float curLength)
    {
        GameObject newObj=GameObject.Instantiate(obj);
        newObj.transform.position=new Vector3(0,0,curLength);
        Vector3 size=newObj.GetComponent<Renderer>().bounds.size;
        curLength+=size.z;
        newObj.SetActive(true);
        if(newObj.transform.childCount>0)
        {
            currentTarget=newObj.transform.GetChild(0);
        }else
        {
            currentTarget=null;
        }
        return curLength;
    }
}
