using UnityEngine;

namespace Silkke
{
    [CreateAssetMenu(fileName = "AppConfigData", menuName = "AppConfiguration", order = 1)]
    public class AppConfiguration : ScriptableObject
    {
        public string applicationName;
        public string clientID;
        public string secretID;
    }
}