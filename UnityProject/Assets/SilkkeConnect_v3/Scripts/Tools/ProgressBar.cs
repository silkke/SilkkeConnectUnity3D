using UnityEngine;
using UnityEngine.UI;

public class ProgressBar : MonoBehaviour
{
    public Transform filler;

    public void setProgress(float progress)
    {
        filler.localScale = new Vector3(progress, 1f, 1f);
    }
}