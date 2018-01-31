using UnityEngine;

namespace Silkke
{
    /*
        This class is used for 
        - download ans assetBundle
        - load tha avatar in the assetBundle
        - And instantiate this avatar in the current scene
        from an URL pointing on a assetBundle 
    */
    public class AvatarBundle
    {
        public GameObject avatar { get; private set; }
        public string error { get; private set; }

        private string downloadingURL;

        public delegate void LoadCallback(AvatarBundle avatar);

        public AvatarBundle(string url)
        {
            downloadingURL = url;
        }

        public void AsyncLoad(
          LoadCallback onSuccess
          , LoadCallback onFailure
          , DownloadManager.DLCallback onUpdate)
        {
            DownloadManager.Request(
              downloadingURL,
              // onSuccess
              www =>
              {
                  foreach (var str in www.assetBundle.GetAllAssetNames())
                  {
                      if (str.EndsWith(".prefab"))
                      {
                          avatar = (GameObject)GameObject.Instantiate(www.assetBundle.LoadAsset(str));

                          www.assetBundle.Unload(false);

                          onSuccess(this);
                          return;
                      }
                  }
                  error = "There is a problem with this silkke";
                  onFailure(this);
                  www.assetBundle.Unload(false);
                  return;
              },
              // onFailure
              www =>
              {
                  error = "network failure";
                  Debug.LogError("Error : " + www.error);
                  onFailure(this);
              },
              onUpdate
            );
        }

        public void Cancel()
        {
            DownloadManager.Cancel(downloadingURL);
        }
    }
}
