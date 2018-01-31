using Silkke;
using UnityEngine;
using UnityEngine.UI;

public class UIResizer : MonoBehaviour
{
    [Header("Bottom panel")]
    public Text eventTxt;

    private Animator UIAnimator;
    private float _currentRatio;
    private ScreenOrientation _currentOrientation;

    private void Start()
    {
        UIAnimator = GetComponent<Animator>();

        _currentOrientation = Screen.orientation;
        _currentRatio = Screen.width / Screen.height;

        updateRatio();
    }

    private void Update()
    {
        switch (Platform.currentPlatform)
        {
            case RuntimePlatform.Android | RuntimePlatform.IPhonePlayer:
                if (_currentOrientation != Screen.orientation)
                    updateRatio();
                break;
            default:
                if (_currentRatio != Screen.width / Screen.height)
                    updateRatio();
                break;
        }
    }

    private void updateOrientation()
    {
        _currentOrientation = Screen.orientation;
        UIAnimator.SetBool("isPortrait", _currentOrientation == ScreenOrientation.Portrait ? true : false);

        // update font size
        eventTxt.resizeTextMaxSize = UIAnimator.GetBool("isPortrait") ? 52 : 58;
    }

    private void updateRatio()
    {
        _currentRatio = Screen.width / Screen.height;
        UIAnimator.SetBool("isPortrait", _currentRatio == 0 ? true : false);
        
        // update font size
        eventTxt.resizeTextMaxSize = UIAnimator.GetBool("isPortrait") ? 52 : 58;
    }
}