using Silkke;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class logDisplayer : MonoBehaviour
{
    private Animator logAnim;

    [Header("Logs color")]
    public Color errorColor;
    public Color successColor;
    public Color warningColor;

    [Header("Logs settings")]
    public Text logTxt;
    public float displayTime = 3.0f;

    public enum logStatus { success, warning, error };

    static public logDisplayer m_Instance = null;

    private bool canDisplay = false;
    private float currentTimer = 0;

    private Dictionary<logStatus, Color> logs = new Dictionary<logStatus, Color>()
    {
        { logStatus.success, Color.white },
        { logStatus.warning, Color.white },
        { logStatus.error, Color.white },
    };

    private void Start()
    {
        if (m_Instance == null)
            m_Instance = this;
        logAnim = GetComponent<Animator>();

        logs[logStatus.success] = successColor;
        logs[logStatus.warning] = warningColor;
        logs[logStatus.error] = errorColor;
    }

    private void Update()
    {
        if (canDisplay)
        {
            if (currentTimer < displayTime)
            {
                if (!logAnim.GetBool("isDisplayed"))
                    logAnim.SetBool("isDisplayed", true);
                currentTimer += Time.deltaTime;
            }
            else
            {
                canDisplay = false;
                currentTimer = 0;
                logAnim.SetBool("isDisplayed", false);
            }
        }
    }

    static public void displayLog(logStatus status, string message)
    {
        m_Instance.displayLogs(status, message);
    }

    public void displayLogs(logStatus status, string message)
    {
        currentTimer = 0;
        canDisplay = true;
        logTxt.text = message;
    }
}
