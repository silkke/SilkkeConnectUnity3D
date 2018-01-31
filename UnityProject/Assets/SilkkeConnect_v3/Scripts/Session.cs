using System;
using UnityEngine;
using System.Collections.Generic;

namespace Silkke
{
    // Game orientation (after the login / AvatarSelection scene)
    public enum applicationOrientation
    {
        Landscape,
        Portrait,
        Both
    };

    // Variables proper to app configuration |------------------------------------------------------------------
    static public class appConfig
    {
        static public string applicationName;
        static public string clientID;
        static public string secretID;
    }
    //----------------------------------------------------------------------------------------------------------

    // Variables and methods proper to user account |-----------------------------------------------------------
    public static class UserAccount
    {
        static public string accountName = null;
        static public string accountPassword = null;
        static public int avatarNumber = 0;
        static public bool checkBoxStatus = false;
        static public string defaultAvatarID = null;

        public static void saveDefaultAvatar(string avatarID)
        {
            defaultAvatarID = avatarID;
            PlayerPrefs.SetString("defaultAvatarID", avatarID);
        }

        public static void Save()
        {
            PlayerPrefs.SetString("accountName", accountName);
            PlayerPrefs.SetString("accountPassword", accountPassword);
            PlayerPrefs.SetInt("checkBoxStatus", (checkBoxStatus) ? 1 : 0);
            PlayerPrefs.SetInt("avatarNumber", avatarNumber);
        }

        public static bool Load()
        {
            try
            {
                accountName     = PlayerPrefs.GetString("accountName");
                accountPassword = PlayerPrefs.GetString("accountPassword");
                checkBoxStatus  = (PlayerPrefs.GetInt("checkBoxStatus") == 1) ? true : false;
                avatarNumber    = PlayerPrefs.GetInt("avatarNumber");
                defaultAvatarID = PlayerPrefs.GetString("defaultAvatarID");
                return true;
            }
            catch (NullReferenceException e)
            {
                Debug.LogError(e);
                return false;
            }
        }

        public static void Clear()
        {
            PlayerPrefs.DeleteAll();
        }
    }
    //----------------------------------------------------------------------------------------------------------

    public class AvatarInfos
    {
        public string avatarID;
        public string avatarImgUrl;
        public string avatarPictureDate;
        public string eventName;
        public Sprite avatarImg;

        public string sponsorName;
        public Sprite sponsorLogo;
        public string sponsorLogoUrl;
    } 

    // Variable proper to user session
    static class Session
    {
        static public bool initialized = false;

        // Screen orientation - modification during avatar choice depending on the application
        static public applicationOrientation appOrientation;

        static public string avatarURL = null;

        // Avatar selected
        static public Transform avatar = null;

        // Avatar instance
        static public Transform avatarInstance = null;

        // If defaultAvatar
        static public bool isDefaultAvatar = false;

        // ID of the avatar selected
        static public int avatarID = -1;

        // Know if the user is connected to an account or not.
        static public bool isConnected = false;

        // This is the scene to load after login (if offline) or after avatar selection
        static public int IdNextScene = 2;

        // If disconnect, reset all variables.
        static public void CleanAll()
        {
            if (avatar)
                GameObject.Destroy(avatar.gameObject);

            // Disconnect
            isConnected = false;

            // Reset isDefaultAvatar boolean
            isDefaultAvatar = false;

            // Unload all unused assets
            Resources.UnloadUnusedAssets();
        }

        // Saving avatar on the choice screen
        static public void SaveAvatar(Transform obj, string suffix)
        {
            avatar = obj;
            avatar.name = "avatar_" + suffix;
            avatar.gameObject.SetActive(false);
            GameObject.DontDestroyOnLoad(avatar.gameObject);
        }

        // Instantiate selected avatar
        static public Transform InstantiateAvatar()
        {
            if (avatar == null)
                return null;
            GameObject clone = GameObject.Instantiate(avatar).gameObject;
            clone.gameObject.SetActive(true);
            return clone.transform;
        }

        // Reset orientation to all portrait AND landscape
        static public void resetAppOrientation()
        {
            Screen.orientation = ScreenOrientation.AutoRotation;
            Screen.autorotateToPortrait = true;
            Screen.autorotateToPortraitUpsideDown = true;
            Screen.autorotateToLandscapeLeft = true;
            Screen.autorotateToLandscapeRight = true;
        }

        // Set application orientation after avatar choice
        static public void setAppOrientation()
        {
            if (appOrientation == applicationOrientation.Landscape)
            {
                Debug.Log("[Landscape mode]");
                Screen.orientation = ScreenOrientation.Landscape;
                Screen.orientation = ScreenOrientation.AutoRotation;
                Screen.autorotateToPortrait = false;
                Screen.autorotateToPortraitUpsideDown = false;
                Screen.autorotateToLandscapeLeft = true;
                Screen.autorotateToLandscapeRight = true;
            }
            else if (appOrientation == applicationOrientation.Portrait)
            {
                Debug.Log("[Portrait mode]");
                Screen.orientation = ScreenOrientation.Portrait;
                Screen.orientation = ScreenOrientation.AutoRotation;
                Screen.autorotateToPortrait = true;
                Screen.autorotateToPortraitUpsideDown = true;
                Screen.autorotateToLandscapeLeft = false;
                Screen.autorotateToLandscapeRight = false;
            }
            else
            {
                Debug.Log("[AutoRotation mode]");
                Screen.orientation = ScreenOrientation.AutoRotation;
            }
        }
    }
    //----------------------------------------------------------------------------------------------------------

    // Variable proper to platform
    static public class Platform
    {
        static public RuntimePlatform currentPlatform;

        static private Dictionary<RuntimePlatform, int> platformID = new Dictionary<RuntimePlatform, int> { { RuntimePlatform.WindowsEditor, -1 },
                                                                                                              { RuntimePlatform.OSXEditor, -1 },
                                                                                                              { RuntimePlatform.Android, 1 },
                                                                                                              { RuntimePlatform.IPhonePlayer, 2 },
                                                                                                              { RuntimePlatform.WSAPlayerX64, 3 },
                                                                                                              { RuntimePlatform.WSAPlayerX86, 3 },
                                                                                                              { RuntimePlatform.WSAPlayerARM, 3 },
                                                                                                              { RuntimePlatform.OSXPlayer, 4 },
                                                                                                              { RuntimePlatform.LinuxPlayer, 4 },
                                                                                                              { RuntimePlatform.WindowsPlayer, 4 },
                                                                                                              { RuntimePlatform.WebGLPlayer, 5 }};

        // Get platform ID in Dictionary
        static public string GetPlatformID()
        {
            foreach (KeyValuePair<RuntimePlatform, int> pl in platformID)
            {
                if (pl.Key == Application.platform)
                    return pl.Value.ToString();
            }
            return null;
        }

        // There is actually only three platform where avatars files are availables.
        static public string GetPlatformName()
        {
            switch (Application.platform)
            {
                case RuntimePlatform.Android:
                    return "android";
                case RuntimePlatform.IPhonePlayer:
                    return "ios";
                case RuntimePlatform.WebGLPlayer:
                    return "webgl";
                case RuntimePlatform.WSAPlayerX64 | RuntimePlatform.WSAPlayerX86 | RuntimePlatform.WSAPlayerARM:
                    return "wsa";
                default:
                    return "windows";
            }
        }
    }
}