using Silkke;
using UnityEngine;
using UnityEngine.UI;

public class avatarBtn : MonoBehaviour
{
    private Image childImage;
    private Animator btnAnimator;

    private float _currentRatio = 0;
    private ScreenOrientation _currentOrientation;

    private void Awake()
    {
        btnAnimator = GetComponent<Animator>();
        childImage = transform.GetChild(0).GetComponent<Image>();

        updateRatio();
    }

    private void Update()
    {
        // Update UI Ratio on orientation change
        if (_currentRatio != (Screen.width / Screen.height) || _currentOrientation != Screen.orientation)
            updateRatio();
    }

    private void updateRatio()
    {
        // For Mobile
        if (Platform.currentPlatform == RuntimePlatform.Android || Platform.currentPlatform == RuntimePlatform.IPhonePlayer)
        {
            _currentOrientation = Screen.orientation;
            btnAnimator.SetBool("isPortrait", _currentOrientation == ScreenOrientation.Portrait ? true : false);
        }
        // For Editor
        else
        {
            _currentRatio = Screen.width / Screen.height;
            btnAnimator.SetBool("isPortrait", _currentRatio == 0 ? true : false);
        }
    }

    // Update btn state
    public void updateBtnState(bool isActif)
    {
        btnAnimator.SetBool("isSelected", isActif);
        transform.GetComponent<Button>().interactable = isActif;

        Color newColor = childImage.color;
        newColor.a = isActif ? 1.0f : 0.5f;
        childImage.color = newColor;
    }
}
