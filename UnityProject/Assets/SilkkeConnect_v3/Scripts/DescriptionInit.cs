using UnityEngine;
using UnityEngine.UI;

public class DescriptionInit : MonoBehaviour
{
    public Text line0;
    public Text line1;
    public Text line2;
    public Text line3;
    public Text line4;

    void Start()
    {
        line0.text = Language.jsonReader.ReadValue("DescriptionPage", "Line0");
        line1.text = Language.jsonReader.ReadValue("DescriptionPage", "Line1");
        line2.text = Language.jsonReader.ReadValue("DescriptionPage", "Line2");
        line3.text = Language.jsonReader.ReadValue("DescriptionPage", "Line3");
        line4.text = Language.jsonReader.ReadValue("DescriptionPage", "Line4");
    }
}