using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LayerLayout : MonoBehaviour
{

    static LayerLayout sLayerLayout = null;
    public static LayerLayout GetLayerLayout()
    {
        return sLayerLayout;
    }

    public GameObject startBlock;
    public GameObject[] caveRepeats;
    public GameObject caveTop;
    public GameObject midBase;
    public GameObject[] midRepeats;
    public GameObject midTop;
    public GameObject[] corals;
    public GameObject endBlock;

    public float drawForward = 1000f;
    public Transform viewpoint;


    private float startTime = 0f;
    private float blockDropPos = 0f;
    public enum LayoutPos
    {
        START,
        CAVE_RPT,
        CAVE_TOP,
        MID_BASE,
        MID_RPT,
        MID_TOP,
        CORAL,
        END,
        FINISHED,
    };

    // if the block has a 'target' point (e.g. start and end)
    public Transform currentTarget;

    public Transform blockParent;

    private LayoutPos currentBlockPos = LayoutPos.START;

    public class BlockDescription
    {
        public LayoutPos type;
        public float yMin;
        public float yMax;
    };

    private List<BlockDescription> allBlocks = new List<BlockDescription>();

    public float GetMaxY()
    {
        if (currentBlockPos == LayoutPos.FINISHED)
        {
            return currentTarget.position.y;
        }
        return 0;
    }

    public LayoutPos GetBlockAt(float y)
    {
        foreach (BlockDescription b in allBlocks)
        {
            if (b.yMin <= y && b.yMax > y)
            {
                return b.type;
            }
        }
        return LayoutPos.FINISHED;
    }

    // Use this for initialization
    void Start()
    {
        sLayerLayout = this;
        startTime = Time.time;
        LayoutIncrementally(0, 0);
        if (currentTarget != null)
        {
            viewpoint.position = currentTarget.position;
        }
        //		LayoutCity(1000f);
    }

    // Update is called once per frame
    void Update()
    {
        float yPos = viewpoint.position.y;
        if (currentBlockPos == LayoutPos.FINISHED)
        {
            if (currentTarget != null)
            {
                if (yPos > currentTarget.position.y)
                {
                    viewpoint.position = new Vector3(viewpoint.position.x, currentTarget.position.y, viewpoint.position.z);
                }
            }
        }
        else if (yPos + drawForward > blockDropPos || Input.GetKeyDown("n"))
        {
            AbstractGameEffects ag = AbstractGameEffects.GetSingleton();
            float timeFraction = ag.climaxRatio * 0.5f;
            if (!ag.countUp)
            {
                timeFraction = 1f - timeFraction;
            }
            if (timeFraction > 1f) timeFraction = 1f;
            LayoutIncrementally(yPos, timeFraction);
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

    void LayoutIncrementally(float untilLength, float fractionThrough)
    {
        float yOffsetScale = 1f;
        LayoutPos nextBlockPos = currentBlockPos;
        GameObject theBlock = null;
        switch (currentBlockPos)
        {
            case LayoutPos.START:
                theBlock = startBlock;
                nextBlockPos = LayoutPos.CAVE_RPT;
                break;
            case LayoutPos.CAVE_RPT:
                theBlock = caveRepeats[Random.Range(0, caveRepeats.Length - 1)];
                yOffsetScale = .9f;
                if (fractionThrough > 0.2 || Input.GetKeyDown("n"))
                {
                    nextBlockPos = LayoutPos.CAVE_TOP;
                }
                break;
            case LayoutPos.CAVE_TOP:
                theBlock = caveTop;
                nextBlockPos = LayoutPos.MID_BASE;
                break;
            case LayoutPos.MID_BASE:
                theBlock = midBase;
                nextBlockPos = LayoutPos.MID_RPT;
                yOffsetScale = .8f;
                break;
            case LayoutPos.MID_RPT:
                theBlock = midRepeats[Random.Range(0, midRepeats.Length - 1)];
                if (fractionThrough > 0.4f || Input.GetKeyDown("n"))
                {
                    nextBlockPos = LayoutPos.MID_TOP;
                }
                break;
            case LayoutPos.MID_TOP:
                theBlock = midTop;
                nextBlockPos = LayoutPos.CORAL;
                break;
            case LayoutPos.CORAL:
                theBlock = corals[Random.Range(0, corals.Length - 1)];
                //yOffsetScale=0.3f;
                //maybe lower these so they're more densely packed?
                yOffsetScale = 0.1f;

                if (fractionThrough > 0.9f || Input.GetKeyDown("n"))
                {
                    nextBlockPos = LayoutPos.FINISHED;
                }
                break;
            case LayoutPos.END:
                // place end cube
                break;
            case LayoutPos.FINISHED:
                break;
        }

        if (theBlock != null)
        {
            float newPos = PlaceBlock(currentBlockPos, theBlock, blockDropPos);
            blockDropPos += (newPos - blockDropPos) * yOffsetScale;
        }
        currentBlockPos = nextBlockPos;

    }

    public void EnsureEndBlock()
    {
        // make sure that we're in state FINISHED
        if (currentBlockPos != LayoutPos.FINISHED)
        {
            currentBlockPos = LayoutPos.END;
            float zPos = viewpoint.position.z;
            LayoutIncrementally(zPos, 1f);
        }
    }

    float PlaceBlock(LayoutPos objType, GameObject obj, float curLength)
    {
        GameObject newObj = GameObject.Instantiate(obj);
        newObj.transform.position = Vector3.zero;
        Bounds objBounds = newObj.GetComponent<Renderer>().bounds;
        Vector3 size = objBounds.size;
        //print(newObj.name + ":" + size + ":" + curLength + objBounds.center);
        newObj.transform.position = new Vector3(0, curLength, 0) - objBounds.center + new Vector3(0, objBounds.size.y * 0.5f, 0);

        ///do a z rotation - not on start
        if (objType != LayoutPos.START)
        {
            int[] rots = new int[] { 0, 90, 180, 270 };
            newObj.transform.localEulerAngles = new Vector3(newObj.transform.localEulerAngles.x, newObj.transform.localEulerAngles.y + rots[Random.Range(0, rots.Length)], newObj.transform.localEulerAngles.z);
        }

        BlockDescription newBlock = new BlockDescription();
        newBlock.yMin = curLength;
        newBlock.yMax = curLength + size.y;
        newBlock.type = objType;
        allBlocks.Add(newBlock);

        curLength += size.y;
        newObj.SetActive(true);
        if (newObj.transform.childCount > 0)
        {
            currentTarget = newObj.transform.GetChild(0);
        }
        else
        {
            currentTarget = null;
        }
        newObj.transform.parent = blockParent;
        return curLength;
    }
}
