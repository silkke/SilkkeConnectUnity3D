using UnityEngine;

public class Language : MonoBehaviour
{
    public static string currentLanguage = "fr";

    public static JsonReader jsonReader;

    static private Language instance;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
            CheckLanguage();
        }
    }

    void CheckLanguage()
    {
        switch (Application.systemLanguage)
        {
            case SystemLanguage.French: currentLanguage = "fr"; break;
            default: currentLanguage = "en"; break;
        }
    }
}
