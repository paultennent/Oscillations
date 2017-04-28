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
    public GameObject beachEndBlock;
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
		BEACH,
        BEACHEND,
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
        PopulateBlockList();
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

	//future ref joe: random.range is exclusive in the upper end for integers (i know!)

    class BlockListItem
    {
        public BlockListItem(LayoutPos thisBlockPos,GameObject []objs,float timeInBlock)
        {
            this.blockPos=thisBlockPos;
            this.objs=objs;
            this.timeInBlock=timeInBlock;
        }
        public LayoutPos blockPos;
        public GameObject []objs;
        public float timeInBlock;
        public float incrementalEndTime;
    };

    List<BlockListItem> blocks=new List<BlockListItem>();
    int blockListIndex=0;
    
    void PopulateBlockList()
    {
        blocks.Add(new BlockListItem(LayoutPos.START,new GameObject[]{startBlock},0));
        blocks.Add(new BlockListItem(LayoutPos.LOW1,lowBlocks,2));
        blocks.Add(new BlockListItem(LayoutPos.MID1,midBlocks,2));
        blocks.Add(new BlockListItem(LayoutPos.HIGH1,highBlocks,1));
        blocks.Add(new BlockListItem(LayoutPos.BEACH,beachBlocks,2));
        blocks.Add(new BlockListItem(LayoutPos.BEACHEND,new GameObject[]{beachEndBlock},0));
        blocks.Add(new BlockListItem(LayoutPos.HIGH2,highBlocks,1));
        blocks.Add(new BlockListItem(LayoutPos.PARKSTART,new GameObject[]{parkStartBlock},0));
        blocks.Add(new BlockListItem(LayoutPos.PARKREST,parkRepeatingBlocks,.5f));
        blocks.Add(new BlockListItem(LayoutPos.PARKEND,new GameObject[]{parkEndBlock},0));
        blocks.Add(new BlockListItem(LayoutPos.HIGH3,highBlocks,2));
        blocks.Add(new BlockListItem(LayoutPos.MID2,midBlocks,2));
        blocks.Add(new BlockListItem(LayoutPos.LOW2,lowBlocks,2));
        blocks.Add(new BlockListItem(LayoutPos.END,new GameObject[]{endBlock},0));
        blocks.Add(new BlockListItem(LayoutPos.FINISHED,null,5));
        float totalTime=0f;
        foreach(BlockListItem b in blocks)
        {
            totalTime+=b.timeInBlock;
        }
        float endTime=0f;
        foreach(BlockListItem b in blocks)
        {
            endTime+=b.timeInBlock;
            b.incrementalEndTime=endTime/totalTime;
        }        
    }
    
    
    void LayoutIncrementally(float untilLength,float fractionThrough)
    {

        BlockListItem block = blocks[blockListIndex];
        if(block.objs!=null)
        {
            blockDropPos=PlaceBlock(currentBlockPos,block.objs[Random.Range(0,block.objs.Length)],blockDropPos);
        }
        if( block.timeInBlock==0 || fractionThrough>=block.incrementalEndTime)
        {
            if(blockListIndex<blocks.Count-1)
            {
                blockListIndex++;
                currentBlockPos=blocks[blockListIndex].blockPos;
            }
        }
    
/*        switch(currentBlockPos)
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
					blockDropPos = PlaceBlock(currentBlockPos, lowBlocks[Random.Range(0, lowBlocks.Length)], blockDropPos);
					if (fractionThrough > 0.2)
					{
						currentBlockPos = LayoutPos.MID1;
					}
				}
				break;
         case LayoutPos.MID1:
            {
				blockDropPos=PlaceBlock(currentBlockPos,midBlocks[Random.Range(0,midBlocks.Length)],blockDropPos);
                if(fractionThrough>0.3)
                {
                    currentBlockPos=LayoutPos.HIGH1;
                }
            }
            break;
         case LayoutPos.HIGH1:
            {
				blockDropPos=PlaceBlock(currentBlockPos,highBlocks[Random.Range(0,highBlocks.Length)],blockDropPos);
                if(fractionThrough>0.4)
                {
                    currentBlockPos=LayoutPos.BEACH;
                }
            }
            break;
         case LayoutPos.BEACH:
				{
					blockDropPos = PlaceBlock(currentBlockPos, beachBlocks[Random.Range(0, beachBlocks.Length)], blockDropPos);
					if (fractionThrough > 0.45)
					{
						currentBlockPos = LayoutPos.BEACHEND;
					}
				}
				break;
            case LayoutPos.BEACHEND:
            {
					blockDropPos = PlaceBlock(currentBlockPos, beachEndBlock, blockDropPos);
                    currentBlockPos=LayoutPos.HIGH2;
            }
            break;
			case LayoutPos.HIGH2:
				{
					blockDropPos = PlaceBlock(currentBlockPos, highBlocks[Random.Range(0, highBlocks.Length )], blockDropPos);
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
				blockDropPos = PlaceBlock (currentBlockPos, parkRepeatingBlocks [Random.Range (0, parkRepeatingBlocks.Length)], blockDropPos);            
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
				blockDropPos=PlaceBlock(currentBlockPos,highBlocks[Random.Range(0,highBlocks.Length)],blockDropPos);
                if(fractionThrough>0.6)
                {
                    currentBlockPos=LayoutPos.MID2;
                }
            }
            break;
         case LayoutPos.MID2:
            {
				blockDropPos=PlaceBlock(currentBlockPos,midBlocks[Random.Range(0,midBlocks.Length)],blockDropPos);
                if(fractionThrough>0.65)
                {
                    currentBlockPos=LayoutPos.LOW2;
                }
            }
            break;
				case LayoutPos.LOW2:
				{
					blockDropPos = PlaceBlock(currentBlockPos, midBlocks[Random.Range(0, midBlocks.Length)], blockDropPos);
					if (fractionThrough > 0.7)
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
        }        */
    }

    public void EnsureEndBlock()
    {
        // make sure that we're in state FINISHED
        if(currentBlockPos!=LayoutPos.FINISHED)
        {
            blockListIndex=blocks.Count-1;
            currentBlockPos=LayoutPos.END;
            float zPos=viewpoint.position.z;
            LayoutIncrementally(zPos,1f);            
        }
    }

    float PlaceBlock(LayoutPos objType,GameObject obj,float curLength)
    {


		GameObject newObj=GameObject.Instantiate(obj);

		//new tiles have arrived with jumbled x/y centre positions - importing them from the originals should help

		//tiles are also not z-centred!!!

        
        newObj.transform.position=new Vector3(newObj.transform.position.x,newObj.transform.position.y,0);
        Bounds objBounds=newObj.GetComponent<Renderer>().bounds;
        Vector3 size=objBounds.size;        
        newObj.transform.position=new Vector3(newObj.transform.position.x,newObj.transform.position.y,curLength+objBounds.size.z*0.5f-objBounds.center.z);

//        newObj.transform.position=new Vector3(localPos.x,localPos.y,curLength) + new Vector3(0,0,objBounds.size.z*0.5f-objBounds.center.z  );
//		newObj.transform.position=new Vector3(obj.transform.localPosition.x,obj.transform.localPosition.y,curLength);

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
