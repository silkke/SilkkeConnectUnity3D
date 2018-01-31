using LitJson;
using UnityEngine;
using Boomlagoon.JSON;

public class JsonReader : MonoBehaviour
{
    private JsonData _jsonData = null;
    private string _jsonText;

    void Awake()
    {
        Language.jsonReader = this;
    }

    public string ReadValue(string value)
    {
        if (_jsonData == null) GetJsonFile();
        if (_jsonData == null || _jsonData[value] == null) return "UNKNOWN";
        return _jsonData[value].ToString();
    }

    public string ReadValue(string subject, string elem)
    {
        if (_jsonData == null) GetJsonFile();
        if (_jsonData == null || _jsonData[subject][elem] == null) return "UNKNOWN";
        return _jsonData[subject][elem].ToString();
    }

    private void GetJsonFile()
    {
        string path = string.Format("Languages/silkkeConnector_{0}", Language.currentLanguage);
        _jsonText = Resources.Load<TextAsset>(path).text;
        _jsonData = JsonMapper.ToObject(_jsonText);
    }

    private JSONObject _jsonGameData = null;
    private string _jsonTextGame;

    private string gameName = null;

    public void SetGameName(string name)
    {
        gameName = name;
    }

    public string ReadGameValue(string value)
    {
        if (_jsonGameData == null) GetJsonGameFile();
        if (_jsonGameData == null || _jsonGameData[value] == null) return "UNKNOWN";
        return _jsonGameData[value].ToString().Replace("\"", "");
    }

    public void GetJsonGameFile()
    {
        if (string.IsNullOrEmpty(gameName))
        {
            Debug.LogError("Please specity your game name SetGameName('name') to get languages files");
            return;
        }

        string path = string.Format(gameName + "_{0}", Language.currentLanguage);
        _jsonTextGame = Resources.Load<TextAsset>(path).text;
        _jsonGameData = JSONObject.Parse(_jsonTextGame);
    }
}
