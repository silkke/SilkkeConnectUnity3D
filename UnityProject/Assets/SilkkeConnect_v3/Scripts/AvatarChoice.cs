using LitJson;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

namespace Silkke
{
    public class AvatarChoice : MonoBehaviour
    {
        [Header("Downloading Panel Elements")]
        public GameObject downloadingPanel;
        public GameObject downloadSpinner;
        public ProgressBar downloadProgressBar;

        private AvatarBundle downloadingAvatar;

        [Header("Default avatar")]
        public GameObject defaultAvatar = null;

        [Header("Avatar container")]
        public GameObject avatarPanel;
        public GameObject imgContainer;
        public Text dateTxt;
        public Text eventTxt;

        [Header("Carousel")]
        public GameObject carousel;
        public ScrollSnapRect ssRect;

        [Header("Sponsor")]
        public Sponsor sponsorObj;

        // Avatar ID + avatar infos corresponding to AvatarInfos structure
        private SortedDictionary<int, AvatarInfos> avatarList = new SortedDictionary<int, AvatarInfos>();
        // Avatar Images
        private SortedDictionary<string, Sprite> avatarImgList = new SortedDictionary<string, Sprite>();

        private int currentAvatarIndex = 0;

        private void Awake()
        {
            dateTxt.text = "";
            eventTxt.text = "";

            // Get list of avatars proper to userAccount
            getAvatarList();
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Escape))
                SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex - 1);
        }

        private void FixedUpdate()
        {
            // Update avatar info text
            if (currentAvatarIndex != ssRect.getCurrentPage())
            {
                currentAvatarIndex = ssRect.getCurrentPage();

                // Update event date and name
                updateEventDateAndName(currentAvatarIndex);

                // Update sponsor text and logo 
                updateSponsorTextAndLogo(currentAvatarIndex);
            }
        }

        private void updateEventDateAndName(int index)
        {
            // Set dateTxt on the first avatar Date
            if (!string.IsNullOrEmpty(avatarList[index].avatarPictureDate))
                dateTxt.text = ConvertDate(avatarList[index].avatarPictureDate);

            // Set eventTxt on the first avatar EventName
            if (!string.IsNullOrEmpty(avatarList[index].eventName))
                eventTxt.text = avatarList[index].eventName;
        }

        private void updateSponsorTextAndLogo(int index)
        {
            sponsorObj.updateSponsorInfos(avatarList[index].sponsorName, avatarList[index].sponsorLogo, avatarList[index].sponsorLogoUrl);
        }

        private string ConvertDate(string date)
        {
            string year = date.Substring(0, 4);
            string month = date.Substring(5, 2);
            string day = date.Substring(8, 2);

            month = Language.jsonReader.ReadValue("date", month);

            string fullDate = null;
            switch (Application.systemLanguage)
            {
                case SystemLanguage.French:
                    fullDate = int.Parse(day) + " " + month.ToLower() + " " + year;
                    break;
                default:
                    fullDate = month.ToLower() + " " + int.Parse(day) + " " + year;
                    break;
            }
            return fullDate;
        }

        // Called when a click is detected on avatar images/buttons
        private void choseAvatar(string avatarID)
        {
            // Get selected avatar ID
            Session.avatarID = int.Parse(avatarID);

            // Hide avatar panel
            avatarPanel.SetActive(false);

            // Clean unload unused assets
            Debug.Log("[Clean unused assets]");
            Resources.UnloadUnusedAssets();

            // Get avatar bundle depending on platform and download it
            StartCoroutine(API.GET(
            "avatars/" + avatarID,
            new Dictionary<string, string>
            {
                    { "platform", Platform.GetPlatformID() }
            },
            // onSuccess
            result =>
            {
                Session.avatarURL = result["meta"]["unity_" + Platform.GetPlatformName()].ToString();
                DownloadAvatar(Session.avatarURL);
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
            }
            ));
        }

        // Get avatar list available for the current account
        private void getAvatarList()
        {
            StartCoroutine(API.GET(
                "avatars", new Dictionary<string, string>
                {
                    { "includes", "capsuleLocation,queue.coupon.company" }
                },
                // onSuccess
                result =>
                {
                    // Get data table from Json
                    JsonData data = result["data"];

                    // Get avatar id and image url.
                    JsonData previewsList = result["meta"];

                    for (int i = 0; i < previewsList["previews"].Count; i++)
                    {
                        AvatarInfos ai = new AvatarInfos();

                        ai.avatarID = previewsList["previews"][i]["idPicture3D"].ToString();
                        ai.avatarImgUrl = previewsList["previews"][i]["preview"].ToString();

                        if (data[i]["queue"].Count > 1 &&
                            data[i]["queue"]["coupon"].Count > 1 &&
                            data[i]["queue"]["coupon"]["company"].Count > 1)
                        {
                            JsonData company = data[i]["queue"]["coupon"]["company"];

                            // Get sponsorName and sponsorLogoUrl to download it
                            ai.sponsorName = company["name"].ToString();
                            ai.sponsorLogoUrl = company["logo"].ToString();

                            // Download sponsorLogo from sponsorLogoUrl
                            StartCoroutine(API.Download(
                            ai.avatarID, ai.sponsorLogoUrl,
                            // onSuccess
                            (id, www) =>
                            {
                                ai.sponsorLogo = Sprite.Create(www.texture,
                                    new Rect(0, 0, www.texture.width, www.texture.height),
                                    new Vector2(0.5f, 0.5f), 100);

                                www.Dispose();
                            },
                            // onFailure
                            (id, onFailure) =>
                            {
                                logDisplayer.displayLog(logDisplayer.logStatus.error, onFailure.error);
                            }));
                        }

                        // Get avatarisation date
                        if (result["data"][i]["datePicture"] != null)
                        {
                            string fullDate = result["data"][i]["datePicture"].ToString();
                            ai.avatarPictureDate = fullDate.Substring(0, 10);
                        }

                        // Get avatarisation event name
                        if (result["data"][i]["capsuleLocation"].Count > 0)
                            ai.eventName = result["data"][i]["capsuleLocation"]["libel"].ToString().ToUpper();
                        else
                            ai.eventName = "";

                        avatarList.Add(i, ai);
                    }

                    // Get number of avatar
                    UserAccount.avatarNumber = avatarList.Count;

                    // If user account doesn't have an avatar - set the default one and load the next scene
                    if (UserAccount.avatarNumber == 0)
                    {
                        Session.SaveAvatar(defaultAvatar.transform, "metalman");
                        AutoFade.LoadLevel(Session.IdNextScene, 2, 2, Color.white);
                    }
                    else
                    {
                        // Downlod avatars images
                        foreach (KeyValuePair<int, AvatarInfos> info in avatarList)
                        {
                            StartCoroutine(API.Download(
                            info.Value.avatarID, info.Value.avatarImgUrl,
                            // onSuccess
                            (id, www) =>
                            {
                                // add avatar image in list
                                avatarImgList.Add(id, Sprite.Create(www.texture,
                                 new Rect(0, 0, (float)www.texture.width, (float)www.texture.height),
                                 new Vector2(0.5f, 0.5f)));

                                if (avatarImgList.Count == avatarList.Count)
                                {
                                    // Update event date and name
                                    updateEventDateAndName(0);

                                    // Update sponsor text and logo for the first avatar
                                    updateSponsorTextAndLogo(0);

                                    // Start displaying when all loaded
                                    DisplayAvatarImages();
                                }
                                www.Dispose();
                            },
                            // onFailure
                            (id, onFailure) =>
                            {
                                logDisplayer.displayLog(logDisplayer.logStatus.error, onFailure.error);
                            }));
                        }
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
                }
            ));
        }

        // Sort images and display them
        private void DisplayAvatarImages()
        {
            // Hide load spinner
            downloadSpinner.SetActive(false);

            SortedDictionary<string, AvatarInfos> tmpList = new SortedDictionary<string, AvatarInfos>();
            foreach (KeyValuePair<int, AvatarInfos> p in avatarList)
                tmpList.Add(p.Value.avatarID, p.Value);

            GameObject prefab = Resources.Load("Prefabs/carouselBtn") as GameObject;

            // Display images
            foreach (KeyValuePair<string, AvatarInfos> pair in tmpList.Reverse())
            {
                Sprite tmp;
                GameObject btnObj = Instantiate(prefab);

                btnObj.name = "avatar_" + pair.Value.avatarID;
                btnObj.transform.GetChild(1).name = pair.Value.avatarID;
                btnObj.transform.SetParent(imgContainer.transform);
                btnObj.transform.localScale = new Vector3(2.2f, 2.2f, 1);

                btnObj.transform.localPosition = Vector3.zero;
                btnObj.transform.GetChild(1).localScale = new Vector3(1.5f, 1.5f, 1);

                if (avatarImgList.TryGetValue(pair.Value.avatarID, out tmp))
                    btnObj.transform.GetChild(1).GetComponent<Image>().sprite = tmp;
                btnObj.transform.GetChild(1).GetComponent<Image>().preserveAspect = true;
                btnObj.GetComponent<Button>().onClick.AddListener(() => choseAvatar(btnObj.transform.GetChild(1).name));
            }
            ssRect.InitScroll();
        }

        // Download selected avatar & start next scene.
        private void DownloadAvatar(string path)
        {
            downloadProgressBar.gameObject.SetActive(true);
            downloadingPanel.SetActive(true);

            downloadingAvatar = new AvatarBundle(path);
            downloadingAvatar.AsyncLoad(
                // On Success
                bundle =>
                {
                    downloadingAvatar = null;
                    downloadProgressBar.gameObject.SetActive(false);

                    Session.setAppOrientation();

                    // Instantiate defaultAvatar
                    Session.SaveAvatar(Instantiate(bundle.avatar.transform), Session.avatarID.ToString());

                    try
                    {
                        AutoFade.LoadLevel(Session.IdNextScene, 2, 2, Color.white);
                    }
                    catch (System.IndexOutOfRangeException e)
                    {
                        logDisplayer.displayLog(logDisplayer.logStatus.error, "The next id scene is out of range.");
                        Debug.Log(e.ToString());
                    }
                },
                // On Failure
                bundle =>
                { downloadingAvatar = null; },
                // On WWW update
                www =>
                { downloadProgressBar.setProgress(www.progress); }
            );
        }
    }
}