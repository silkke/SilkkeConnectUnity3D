using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

namespace Silkke
{
    public class Login : MonoBehaviour
    {
        [Header("Login Form Elements")]
        public InputField id;
        public InputField pw;
        public Toggle checkBox;
        public Text checkBoxTxt;
        public Button signInButton;
        public Button signUpButton;
        public Sprite[] checkBoxSprite;

        [Header("Silkke Logo")]
        public Transform logo;
        public Transform fullLogo;

        [Header("All panels")]
        public GameObject cartouche;
        public GameObject loginPanel;
        public GameObject regisPanel;

        [Header("Registration Form Elements")]
        public InputField email;
        public InputField zipcode;
        public Text country;
        public Text logoText;

        [Header("API")]
        public bool isInProd = false;

        [Header("Offline settings")]
        public GameObject defaultAvatar = null;

        [Header("Application configuration")]
        public AppConfiguration config;
        public bool randomLevel = false;
        public int levelToLoad = 2;
        public applicationOrientation appOrientation;

        private bool isWaitingRequest = false;
        private List<Dropdown.OptionData> countryISO = new List<Dropdown.OptionData>();

        private void Awake()
        {
            Session.resetAppOrientation();

            if (!checkBox.isOn)
                Caching.CleanCache();
            Session.CleanAll();    
        }

        private void Start()
        {
            // Initialize placeholder text for login AND password inputfields
            id.placeholder.GetComponent<Text>().text = Language.jsonReader.ReadValue("LoginPage", "id_input").ToUpper();
            pw.placeholder.GetComponent<Text>().text = Language.jsonReader.ReadValue("LoginPage", "pw_input").ToUpper();

            checkBoxTxt.text = Language.jsonReader.ReadValue("LoginPage", "remember");
            signUpButton.transform.GetChild(0).GetComponent<Text>().text = Language.jsonReader.ReadValue("LoginPage", "signup");

            // Initialize signup page
            logoText.text = Language.jsonReader.ReadValue("SignUpPage", "logoText").ToUpper();

            // Is application in production OR development boolean
            API.isInProd = isInProd;

            // Initialize login config.
            initConfig();

            // Initialize countries ISO
            initCountry();

            // Add listener to different buttons
            signInButton.onClick.AddListener(Connect);
        }

        private void Update()
        {
            // Fade animation when an answer is waited from the server
            logo.GetComponent<Animator>().SetBool("isFading", isWaitingRequest);

            if (Input.GetKeyDown(KeyCode.Escape))
                Application.Quit();

            if (Input.GetKeyDown(KeyCode.Return))
                Connect();

            FocusHandler();

            if (Input.GetKeyDown(KeyCode.Tab))
            {
                if (EventSystem.current.currentSelectedGameObject != null)
                {
                    Selectable next = EventSystem.current.currentSelectedGameObject.GetComponent<Selectable>().FindSelectableOnDown();
                    if (next != null)
                    {
                        InputField inputfield = next.GetComponent<InputField>();
                        if (inputfield != null) inputfield.OnPointerClick(new PointerEventData(EventSystem.current));
                        EventSystem.current.SetSelectedGameObject(next.gameObject, new BaseEventData(EventSystem.current));
                    }
                }
            }
        }

        void initConfig()
        {
            // Set currentPlatform
            Platform.currentPlatform = Application.platform;

            if (randomLevel)
            {
                Random.InitState((int)System.DateTime.Now.Ticks);
                Session.IdNextScene = Random.Range(2, SceneManager.sceneCountInBuildSettings);
            }
            else
                Session.IdNextScene = levelToLoad;

            // Reset avatar to null | connection to null and appOrientation
            Session.avatar = null;
            Session.isConnected = false;
            Session.appOrientation = appOrientation;
            
            // Initialize appConfig from appConfig file
            if (config)
            {
                appConfig.applicationName = config.applicationName;
                appConfig.clientID = config.clientID;
                appConfig.secretID = config.secretID;
            }
            else
                logDisplayer.displayLog(logDisplayer.logStatus.error, "You forget to pass your config file to [Login] object...");

            // Initialize Useraccount and password if checkbox ON
            if (UserAccount.Load() && UserAccount.checkBoxStatus)
            {
                id.text = UserAccount.accountName;
                pw.text = UserAccount.accountPassword;
                checkBox.isOn = UserAccount.checkBoxStatus;
            }
        }

        // When connexion button click
        private void Connect()
        {
            ClickOut();

            if (id.text == "" || pw.text == "")
                logDisplayer.displayLog(logDisplayer.logStatus.error, "Please enter a valid login or password...");
            else
            {
                isWaitingRequest = true;
                if (AccessToken.access_token == null)
                    StartCoroutine(API.getAccessToken(
                        GetLoginArgs(),
                        success =>
                        {
                            isWaitingRequest = false;
                            Session.isConnected = true;
                            AccessToken.access_token = success["access_token"].ToString();
                            AccessToken.refresh_token = success["refresh_token"].ToString();
                            AccessToken.expire_time = int.Parse(success["expires_in"].ToString());

                            UserAccount.accountName = id.text;
                            UserAccount.accountPassword = pw.text;
                            UserAccount.checkBoxStatus = checkBox.isOn;

                            if (checkBox.isOn)
                                UserAccount.Save();
                            SceneManager.LoadScene("AvatarSelection");
                        },
                        error =>
                        {
                            isWaitingRequest = false;
                            logDisplayer.displayLog(logDisplayer.logStatus.error, error.ToString());
                        }));
            }
        }

        // Game initialisation when offline mode
        public void initOffline()
        {
            Session.setAppOrientation();

            // Instantiate defaultAvatar
            Session.SaveAvatar(Instantiate(defaultAvatar.transform), "metalman");

            try
            {
                Session.isDefaultAvatar = true;
                AutoFade.LoadLevel(Session.IdNextScene, 1, 1, Color.white);
            }
            catch
            {
                logDisplayer.displayLog(logDisplayer.logStatus.error, "You forget to inquire the next scene on the build settings.");
            }
        }

        // Get Login and password from inputfields
        private Dictionary<string, string> GetLoginArgs()
        {
            Dictionary<string, string> args = new Dictionary<string, string> { { "email", "" }, { "password", "" } };

            args["email"] = id.text != "" ? id.text : args["email"];
            args["password"] = pw.text != "" ? pw.text : args["password"];
            return args;
        }

        // Click out of [Cartouche]
        public void ClickOut()
        {
            if (cartouche.activeSelf && cartouche.GetComponent<Animator>().GetBool("isFocus"))
            {
                cartouche.GetComponent<Animator>().SetBool("isFocus", false);
                fullLogo.GetComponent<Image>().CrossFadeAlpha(1, 0.1f, true);
            }
        }

        void FocusHandler()
        {
            if (Platform.currentPlatform != RuntimePlatform.IPhonePlayer)
              return;

            if (cartouche.activeSelf)
            {
                bool isFocus = id.isFocused | pw.isFocused | email.isFocused | zipcode.isFocused;

                if (isFocus && !cartouche.GetComponent<Animator>().GetBool("isFocus"))
                {
                    cartouche.GetComponent<Animator>().SetBool("isFocus", true);
                    fullLogo.GetComponent<Image>().CrossFadeAlpha(0, 0.1f, true);
                }
            }
        }

        public void checkBoxHandler()
        {
            checkBox.targetGraphic.GetComponent<Image>().sprite = checkBox.isOn ? checkBoxSprite[0] : checkBoxSprite[1];
        }

        private void initCountry()
        {
            StartCoroutine(API.GET("countries", null,
                result =>
                {
                    for (int i = 0; i < result["data"].Count; i++)
                    {
                        Dropdown.OptionData data = new Dropdown.OptionData();
                        data.text = result["data"][i][1].ToString().ToUpper();
                        countryISO.Add(data);
                    }

                    if (countryISO.Count > 0)
                    {
                        regisPanel.transform.Find("Country").GetComponent<Dropdown>().options = countryISO;
						regisPanel.transform.Find("Country").GetChild(0).GetComponent<Text>().text = "FRANCE";
                    }
                },
                // onFailure
                error =>
                {
                    logDisplayer.displayLog(logDisplayer.logStatus.error, error.ToString());
                },
                // successLog
                statusLog =>
                {
                    Debug.Log(statusLog);
                },
                // errorLog
                errorLog =>
                {
                    Debug.Log(errorLog);
                }));
        }

        public void registration()
        {
            if (email.text == "" || zipcode.text == "")
                logDisplayer.displayLog(logDisplayer.logStatus.success, "You forget to enter some informations...");
            else
            {
                StartCoroutine(API.POST("account", null,
                                  new Dictionary<string, string>
                                  {
                                   { "email", email.text },
                                   { "country", country.text },
                                   { "zip", zipcode.text }
                                  },
                                  success =>
                                  {
                                      logDisplayer.displayLog(logDisplayer.logStatus.success, "Registration success. Check your email");
                                  },
                                  error =>
                                  {
                                      logDisplayer.displayLog(logDisplayer.logStatus.error, "Registration error");
                                  },
                                  statusLog =>
                                  {
                                      Debug.Log(statusLog);
                                  },
                                  errorLog =>
                                  {
                                      Debug.Log(errorLog);
                                  }));
                loginPanel.SetActive(true);
                regisPanel.SetActive(false);
            }
        }
    }
}