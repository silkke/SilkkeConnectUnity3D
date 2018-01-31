using System.IO;
using UnityEngine;
using UnityEngine.UI;

public class Sponsor : MonoBehaviour
{
    public GameObject sponsorTitle;
    public GameObject sponsorLogo;
    public GameObject sponsorName;

    private void Start()
    {
        sponsorTitle.GetComponent<Text>().text = Language.jsonReader.ReadValue("AvatarSelectionPage", "sponsorTxt");
        sponsorTitle.SetActive(false);

        sponsorLogo.SetActive(false);
        sponsorName.SetActive(false);
    }

    public void updateSponsorInfos(string name = null, Sprite logo = null, string logoUrl = null)
    {
        sponsorTitle.SetActive((name != null | logo != null) ? true : false);

        sponsorName.SetActive(name != null ? true : false);
        sponsorName.GetComponent<Text>().text = (name != null && logo == null) ? name : "";

        sponsorLogo.SetActive((logo != null && Path.GetExtension(logoUrl).CompareTo(".gif") != 0) ? true : false);
        sponsorLogo.GetComponent<Image>().sprite = logo != null ? logo : null;
    }
}
