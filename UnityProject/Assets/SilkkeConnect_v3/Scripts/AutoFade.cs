using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.SceneManagement;
using Silkke;

public class AutoFade : MonoBehaviour
{
    private static AutoFade m_Instance = null;
    private string m_LevelName = "";
    private int m_LevelIndex = 0;
    private bool m_Fading = false;

    private Image fadeImg;
    public GameObject loader;

    private static AutoFade Instance
    {
        get
        {
            if (m_Instance == null)
                m_Instance = (new GameObject("AutoFade")).AddComponent<AutoFade>();
            return m_Instance;
        }
    }

    public static bool Fading
    {
        get { return Instance.m_Fading; }
    }

    private Animator FaderAnimator;

    private void Awake()
    {
        // Singleton logic
        if (m_Instance == null)
            m_Instance = this;
        else
        { Destroy(gameObject); return; }
        DontDestroyOnLoad(this);

        fadeImg = GetComponent<Image>();
        FaderAnimator = GetComponent<Animator>();
    }

    private int _currentRatio = 0;
    private ScreenOrientation _currentOrientation;

    // Update UI ratio when changing orientation
    void updateRatio()
    {
        Debug.Log("Update orientation to: " + Screen.orientation);

        _currentOrientation = Screen.orientation;
        _currentRatio = Screen.width / Screen.height;

        // Mobile
        if (Platform.currentPlatform == RuntimePlatform.Android || Platform.currentPlatform == RuntimePlatform.IPhonePlayer)
            FaderAnimator.SetBool("isPortrait", _currentOrientation == ScreenOrientation.Portrait ? true : false);
        // Editor
        else
            FaderAnimator.SetBool("isPortrait", _currentRatio == 0 ? true : false);
    }

    private void Update()
    {
        // Update UI Ratio on orientation change
        if (_currentOrientation != Screen.orientation || _currentRatio != Screen.width / Screen.height)
            updateRatio();
    }

    private void DrawQuad(Color aColor, float aAlpha)
    {
        aColor.a = aAlpha;

        // Set new color to img
        fadeImg.color = aColor;
    }

    static AsyncOperation op;

    private IEnumerator Fade(float aFadeOutTime, float aFadeInTime, Color aColor)
    {
        float t = 0.0f;

        Scene currentScene = SceneManager.GetActiveScene();
        
        // Fade In
        while (t < 1.0f)
        {
            yield return new WaitForEndOfFrame();
            t = Mathf.Clamp01(t + Time.deltaTime / aFadeOutTime);
            DrawQuad(aColor, t);
        }

        loader.SetActive(true);
        StartCoroutine(LoadAsync());

        while (SceneManager.GetActiveScene() == currentScene)
            yield return null;
        loader.SetActive(false);

        // Fade Out
        while (t > 0.0f)
        {
            yield return new WaitUntil(() => SceneManager.GetActiveScene() != currentScene);
            t = Mathf.Clamp01(t - Time.deltaTime / aFadeInTime);
            DrawQuad(aColor, t);
        }
        m_Fading = false;
    }

    private IEnumerator LoadAsync()
    {
        if (m_LevelName != "")
            op = SceneManager.LoadSceneAsync(m_LevelName);
        else
            op = SceneManager.LoadSceneAsync(m_LevelIndex);

        op.allowSceneActivation = false;
        while (op.progress < 0.9f)
            yield return null;
        op.allowSceneActivation = true;

        float perc = 0.5f;
        while (!op.isDone)
        {
            perc = Mathf.Lerp(perc, 1f, 0.05f);
            Debug.Log("Loading: " + perc);
            yield return null;
        }
    }

    private void StartFade(float aFadeOutTime, float aFadeInTime, Color aColor)
    {
        m_Fading = true;
        StartCoroutine(Fade(aFadeOutTime, aFadeInTime, aColor));
    }

    public static void LoadLevel(string aLevelName, float aFadeOutTime, float aFadeInTime, Color aColor)
    {
        if (Fading) return;
        Instance.m_LevelName = aLevelName;
        Instance.StartFade(aFadeOutTime, aFadeInTime, aColor);
    }

    public static void LoadLevel(int aLevelIndex, float aFadeOutTime, float aFadeInTime, Color aColor)
    {
        if (Fading) return;
        Instance.m_LevelName = "";
        Instance.m_LevelIndex = aLevelIndex;
        Instance.StartFade(aFadeOutTime, aFadeInTime, aColor);
    }
}