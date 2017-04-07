using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlockLayout : MonoBehaviour {
    
    static BlockLayout sBlockLayout=null;
    public static BlockLayout GetBlockLayout()
    {
        return sBlockLayout;
    }

	public GameObject startBlock;
	public GameObject[] lowBlocks;
    public GameObject[] midBlocks;
    public GameObject[] highBlocks;
	public GameObject[] beachBlocks;
    public GameObject parkStartBlock;
    public GameObject[] parkRepeatingBlocks;
    public GameObject parkEndBlock;
	public GameObject endBlock;

    public float drawForward=1000f;
    public Transform viewpoint;
    
    
    private float startTime=0f;
    private float blockDropPos=0f;
    public enum LayoutPos
    {
        START,
		LOW1,
        MID1,
        HIGH1,
		BEACH1,
		HIGH2,
        PARKSTART,
        PARKREST,
		PARKEND,
        HIGH3,
        MID2,
		LOW2,
        END,
        FINISHED
    };
    
    // if the block has a 'target' point (e.g. start and end)
    public Transform currentTarget;

	public Transform blockParent;

    private LayoutPos currentBlockPos=LayoutPos.START;

	public class BlockDescription
	{
	public LayoutPos type;
	public float zMin;
	public float zMax;
	};

	private List<BlockDescription> allBlocks=new List<BlockDescription>();

    public float GetMaxZ()
    {
        if(currentBlockPos==LayoutPos.FINISHED)
        {
            return currentTarget.position.z;
        }
        return 0;
    }
    
	public LayoutPos GetBlockAt(float z)
	{
		foreach (BlockDescription b in allBlocks) {
			if (b.zMin <= z && b.zMax > z) {
				return b.type;
			}
		}
		return LayoutPos.FINISHED;
	}
    
	// Use this for initialization
	void Start () {
        sBlockLayout=this;
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
			AbstractGameEffects ag = AbstractGameEffects.GetSingleton ();
			float timeFraction = ag.climaxRatio*0.5f;
			if (!ag.countUp) {
				timeFraction = 1f - timeFraction;
			}			
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
                float newPos=PlaceBlock(currentBlockPos,startBlock,blockDropPos);        ;
                // we start in the middle of block zero
                blockDropPos+=newPos;
                currentBlockPos=LayoutPos.LOW1;
            }
            break;
		case LayoutPos.LOW1:
				{
					blockDropPos = PlaceBlock(currentBlockPos, lowBlocks[Random.Range(0, lowBlocks.Length - 1)], blockDropPos);
					if (fractionThrough > 0.2)
					{
						currentBlockPos = LayoutPos.MID1;
					}
				}
				break;
         case LayoutPos.MID1:
            {
				blockDropPos=PlaceBlock(currentBlockPos,midBlocks[Random.Range(0,midBlocks.Length-1)],blockDropPos);
                if(fractionThrough>0.3)
                {
                    currentBlockPos=LayoutPos.HIGH1;
                }
            }
            break;
         case LayoutPos.HIGH1:
            {
				blockDropPos=PlaceBlock(currentBlockPos,highBlocks[Random.Range(0,highBlocks.Length-1)],blockDropPos);
                if(fractionThrough>0.4)
                {
                    currentBlockPos=LayoutPos.BEACH1;
                }
            }
            break;
         case LayoutPos.BEACH1:
				{
					blockDropPos = PlaceBlock(currentBlockPos, beachBlocks[Random.Range(0, beachBlocks.Length - 1)], blockDropPos);
					if (fractionThrough > 0.45)
					{
						currentBlockPos = LayoutPos.HIGH2;
					}
				}
				break;
			case LayoutPos.HIGH2:
				{
					blockDropPos = PlaceBlock(currentBlockPos, highBlocks[Random.Range(0, highBlocks.Length - 1)], blockDropPos);
					if (fractionThrough > 0.5)
					{
						currentBlockPos = LayoutPos.PARKSTART;
					}
				}
				break;
			case LayoutPos.PARKSTART:
            {
				blockDropPos=PlaceBlock(currentBlockPos,parkStartBlock,blockDropPos);
                currentBlockPos=LayoutPos.PARKREST;
            }
            break;
		case LayoutPos.PARKREST:
			{
				blockDropPos = PlaceBlock (currentBlockPos, parkRepeatingBlocks [Random.Range (0, parkRepeatingBlocks.Length - 1)], blockDropPos);            
				if (fractionThrough > 0.6) {
					currentBlockPos = LayoutPos.PARKEND;
				}
			}
			break;
		case LayoutPos.PARKEND:
			{
                blockDropPos=PlaceBlock(currentBlockPos,parkEndBlock,blockDropPos);
                currentBlockPos=LayoutPos.HIGH3;
            }
            break;
         case LayoutPos.HIGH3:
            {
				blockDropPos=PlaceBlock(currentBlockPos,highBlocks[Random.Range(0,highBlocks.Length-1)],blockDropPos);
                if(fractionThrough>0.7)
                {
                    currentBlockPos=LayoutPos.MID2;
                }
            }
            break;
         case LayoutPos.MID2:
            {
				blockDropPos=PlaceBlock(currentBlockPos,midBlocks[Random.Range(0,midBlocks.Length-1)],blockDropPos);
                if(fractionThrough>0.8)
                {
                    currentBlockPos=LayoutPos.LOW2;
                }
            }
            break;
				case LayoutPos.LOW2:
				{
					blockDropPos = PlaceBlock(currentBlockPos, midBlocks[Random.Range(0, midBlocks.Length - 1)], blockDropPos);
					if (fractionThrough > 0.8)
					{
						currentBlockPos = LayoutPos.END;
					}
				}
				break;
         case LayoutPos.END:
            {
                // layout end block
				float newPos=PlaceBlock(currentBlockPos,endBlock,blockDropPos);        ;
                // we start in the middle of block zero
                blockDropPos+=newPos;
                currentBlockPos=LayoutPos.FINISHED;
            }
            break;
         default:
            break;
        }        
    }

    public void EnsureEndBlock()
    {
        // make sure that we're in state FINISHED
        if(currentBlockPos!=LayoutPos.FINISHED)
        {
            currentBlockPos=LayoutPos.END;
            float zPos=viewpoint.position.z;
            LayoutIncrementally(zPos,1f);            
        }
    }

    float PlaceBlock(LayoutPos objType,GameObject obj,float curLength)
    {


		GameObject newObj=GameObject.Instantiate(obj);
        newObj.transform.position=new Vector3(0,0,curLength);

        Vector3 size=newObj.GetComponent<Renderer>().bounds.size;

		BlockDescription newBlock=new BlockDescription();
		newBlock.zMin = curLength;
		newBlock.zMax = curLength + size.z;
		newBlock.type = objType;
		allBlocks.Add (newBlock);

		curLength+=size.z;
        newObj.SetActive(true);
        if(newObj.transform.childCount>0)
        {
            currentTarget=newObj.transform.GetChild(0);
        }else
        {
            currentTarget=null;
        }
		newObj.transform.parent = blockParent;
        return curLength;
    }
}
