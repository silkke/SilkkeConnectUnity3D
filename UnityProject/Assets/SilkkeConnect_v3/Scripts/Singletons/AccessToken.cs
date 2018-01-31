using UnityEngine;
using UnityEngine.SceneManagement;

namespace Silkke
{
    // HOW IT WORK ?
    //  1- On the first connection - a token is assigned to the user.
    //  2- This Token is refreshed every 'expire_time' seconds.
    //  3- If the user go on the login scene - The token, timer are reset - And all the Coroutines are stopped
    //  4- If the API call, have a token fail -> return to the login page (reset)
    //  5- If there is no token in other scene after the login scene -> return to the login page (reset)
    public class AccessToken : MonoBehaviour
    {
        static public int expire_time = 0;
        static public string access_token = null;
        static public string refresh_token = null;

        float startTime     = 0;
        float currentTime   = 0;
        bool isLoginScene = false;

        private static AccessToken m_Instance = null;

        private static AccessToken Instance
        {
            get
            {
                if (m_Instance == null)
                    m_Instance = (new GameObject("[Token Manager]")).AddComponent<AccessToken>();
                return m_Instance;
            }
        }

        private void Awake()
        {
            // Singleton logic
            if (m_Instance == null)
                m_Instance = this;
            else
            { Destroy(gameObject); return; }
            DontDestroyOnLoad(this);
        }

        private void FixedUpdate()
        {
            // If the current scene is not the login scene and the user is connected but the access_token is null (problem to get it from the API) -> load loginScene.
            if (SceneManager.GetActiveScene().buildIndex > 0 && Session.isConnected)
            {
                if (access_token == null)
                    SceneManager.LoadScene(0);
                isLoginScene = false;
            }

            // If we return to the login scene, every variables = reset.
            if (SceneManager.GetActiveScene().buildIndex == 0 && !isLoginScene)
            {
                isLoginScene = true;
                access_token = null;
                refresh_token = null;
                expire_time = 0;
                currentTime = 0;
                StopAllCoroutines();
            }

            // If we have an access_token - refresh it every 'expire_time' seconds with the refresh_token
            if (access_token != null)
            {
                if (currentTime >= expire_time)
                {
                    StartCoroutine(API.getAccessToken(
                        null,
                        success =>
                        {
                            access_token = success["access_token"].ToString();
                            refresh_token = success["refresh_token"].ToString();
                            expire_time = int.Parse(success["expires_in"].ToString());
                        },
                        error =>
                        {
                            access_token = null;
                            refresh_token = null;
                            SceneManager.LoadScene(0);
                        }));
                    currentTime = 0;
                    startTime = Time.time;
                }
                else
                    currentTime = Time.time - startTime;
            }
            else
                startTime = Time.time;
        }   
    }
}
