using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class GameController : MonoBehaviour 
{
    private rcInputManager inputManager;
    private rcInputManager.Stick stick;
    private rcInputManager.RawTouchStream touches;

    public rcInputManager InputManager { get { return inputManager; } }
    public rcInputManager.Stick Stick { get { return stick; } }

    public rtStdInput stdInput;
    public rcFade fade;
    public Camera uiCamera;

    [HideInInspector]
    public bool isClick;

    [HideInInspector]
    public Vector2 mousePos;

    static public GameController instance;

    bool useAdditiveLoads = true;
    Scene additiveScene;

    string[] gameModeLevelNames = new string[] { 
        "LandingPage",
        "flat_4",
        "flat_10",
        "flat_19"
    };

    public enum GameMode {
        LandingPage,
        Flat4,
        Flat10,
        Flat19,

        None
    };

    GameMode gameMode = GameMode.None;


    void Awake()
    {
        instance = this;
        SetupInputManager();
    }

	void Start () 
    {
        SetupScene();

        //LoadLevel(GameMode.LandingPage);
	}
	
    void SetupScene()
    {
        if (!useAdditiveLoads)
        {
            var scene = SceneManager.GetActiveScene();
            var gameObjs = scene.GetRootGameObjects();
            foreach (var gameObj in gameObjs)
            {
                GameObject.DontDestroyOnLoad(gameObj);
            }
        }
    }

    void SetupInputManager()
    {
        inputManager = gameObject.AddComponent<rcInputManager>();
        stick = inputManager.RegStick();
        stick.EnableTouch();
        
        stdInput.inputManager = inputManager;
    }

    void Update()
    {
        float dt = Time.deltaTime;
        inputManager.UpdateManual(dt, dt);

        if (inputManager.GetVirtualKeyDown("1"))
        {
            Debug.Log("pressed 1");
            LoadLevel(GameMode.Flat4);
        }

        if (inputManager.GetVirtualKeyDown("2"))
        {
            Debug.Log("pressed 2");
            LoadLevel(GameMode.Flat10);
        }

        if (inputManager.GetVirtualKeyDown("3"))
        {
            Debug.Log("pressed 3");
            LoadLevel(GameMode.Flat19);
        }
    }

    public void LoadLevel(GameMode mode)
    {
        var cut = gameMode == GameMode.None || gameMode == GameMode.LandingPage;
        if (cut)
        {
            Debug.Log("cutting down");
        }
        StartCoroutine(LoadLevelAsync(mode, cut));
    }

    public IEnumerator LoadLevelAsync(GameMode mode, bool cut)
    {
        if(gameMode != mode)
        {
            gameMode = mode;

            // lower curtains
            // show load screen behind curtains
            // raise curtains - load screen is visible
            // load level
            // lower curtains over load screen
            // hide load screen
            // raise curtains

            if(!cut)
            {
                yield return StartCoroutine(fade.LowerCurtains());
                fade.LoadingScreen.SetActive(true);
                yield return StartCoroutine(fade.RaiseCurtains());
            }

            if (useAdditiveLoads && additiveScene.IsValid())
            {
                SceneManager.UnloadScene(additiveScene);
            }

            var levelName = gameModeLevelNames[(int)gameMode];
            var loadSceneMode = useAdditiveLoads ? LoadSceneMode.Additive : LoadSceneMode.Single;
            var async = SceneManager.LoadSceneAsync(levelName, loadSceneMode);
            while (!async.isDone)
            {
                yield return null;
            }

            if (useAdditiveLoads)
            {
                additiveScene = SceneManager.GetSceneByName(levelName);
                SceneManager.SetActiveScene(additiveScene);
            }

            yield return StartCoroutine(fade.LowerCurtains());
            fade.LoadingScreen.SetActive(false);
            yield return StartCoroutine(fade.RaiseCurtains());
        }
    }
}
