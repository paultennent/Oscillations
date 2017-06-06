using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;
using rcCore;

public class rcFade : MonoBehaviour
{

    // Duration of fade in seconds
    public float FadeDurationInSeconds = 0.2f;

    // Game object to use as loading screen (this is not a very nice solution, but going with it anyway)
    public GameObject LoadingScreen;

    // The above duration is converted into a "speed"
    private float fadeSpeed;

    private bool isFadingDown = false;
    public bool IsFadingDown { get { return isFadingDown; } }

    private bool isFadingUp = false;
    public bool IsFadingUp { get { return isFadingUp; } }

    private Color fadeColor;
    public Color FadeColor { get { return fadeColor; } set { fadeColor = value; } }

    private bool gameFrozen = false;

    private static rcFade instance;

    public static rcFade getInstance()
    {
        if (instance == null)
        {
            rcFade fade = (rcFade)FindObjectOfType(typeof(rcFade));
            if (fade != null)
            {
                instance = fade;
                Debug.LogError("IMPORTANT!! There was a rcFade already in scene!");
            }
        }
        return instance;
    }
    
    public void OnDestroy()
    {
        if(instance==this)
        {
            instance=null;
        }
    }

    public void Reinit()
    {
        fadeSpeed = 1.0f / FadeDurationInSeconds;
    }


    public void FadeDown(Color color)
    {
        float previousAlpha = fadeColor.a;
        fadeColor = color;
        fadeColor.a = previousAlpha;
        isFadingDown = true;
    }

    public void FadeUp(Color color)
    {
        float previousAlpha = fadeColor.a;
        fadeColor = color;
        fadeColor.a = previousAlpha;
        isFadingUp = true;
    }

    public void InitialiseFading()
    {//changed to public, should fadeDurationInSeconds be changed, this needs to be run to update fadeSpeed.
     // Fade has to transition from fully opaque to full transparent in the specified time
        fadeSpeed = 1.0f / FadeDurationInSeconds;

        // Set the state flags
        isFadingDown = false;
        isFadingUp = false;

        // Default fade colour is white
        //fadeColor = new Color(1.0f, 1.0f, 1.0f, 0.0f);
    }

    private void UpdateFading()
    {
        if (isFadingDown)
        {
            fadeColor.a += Time.fixedDeltaTime * fadeSpeed;
            if (fadeColor.a >= 1.0f)
            {
                fadeColor.a = 1.0f;
                isFadingDown = false;
            }

            FreezeGame();
        }
        else if (isFadingUp)
        {
            fadeColor.a -= Time.fixedDeltaTime * fadeSpeed;
            if (fadeColor.a <= 0.0f)
            {
                fadeColor.a = 0.0f;
                isFadingUp = false;
            }

            FreezeGame();
        }
        else if (gameFrozen)
        {
            UnFreezeGame();
        }
    }

    private void FreezeGame()
    {
        gameFrozen = true;
    }

    private void UnFreezeGame()
    {
        gameFrozen = false;
    }

    void Awake()
    {
        instance = this;
    }

    void Start()
    {
        InitialiseFading();
        StartCoroutine(DrawFadingPlane());

        /*
        SceneManager.sceneLoaded += (Scene scene, LoadSceneMode mode) =>
        {
            if (scene.name == "Level1" || scene.name == "Level2" || scene.name == "Level3")
            {
                StartCoroutine(TurnOffLoadingScreen());
            }
        };*/
    }

    void Update()
    {
        UpdateFading();
    }

    private IEnumerator TurnOffLoadingScreen()
    {
        yield return StartCoroutine(LowerCurtains());
        LoadingScreen.SetActive(false);
        yield return StartCoroutine(RaiseCurtains());
    }

    private IEnumerator DrawFadingPlane()
    {
        while (true)
        {
            yield return new WaitForEndOfFrame();

            if (fadeColor.a > 0.0f)
            {
                //GL.InvalidateState();
                rcGL.SetMaterialTransparentColor();
                rcGL.Begin(true);
                rcGL.Color(fadeColor);
                rcGL.Rect(new Vector2(0.5f, 0.5f), new Vector2(1.0f, 1.0f), true);
                rcGL.End();
            }
        }
    }

    public IEnumerator LowerCurtains()
    {
        // Lower-curtains; Fade from clear to black
        Debug.Log("Fading level down...");
        getInstance().FadeDown(new Color(0.0f, 0.0f, 0.0f, 0.0f));
        while (getInstance().IsFadingDown)
        {
            yield return 0;
        }
    }

    public IEnumerator CutCurtains()
    {
        // Lower-curtains; Fade from clear to black
        Debug.Log("Cut level down...");
        getInstance().FadeDown(new Color(0.0f, 0.0f, 0.0f, 1.0f));
        while (getInstance().IsFadingDown)
        {
            yield return 0;
        }
    }

    public IEnumerator RaiseCurtains()
    {
        // Raise curtains; Fade from black to clear
        Debug.Log("Fading loading screen up...");
        getInstance().FadeUp(new Color(0.0f, 0.0f, 0.0f, 1.0f));
        while (getInstance().IsFadingUp)
        {
            yield return 0;
        }
    }
}
