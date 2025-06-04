using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ResourceManagerUI_Single : MonoBehaviour
{
    [SerializeField] private Image iconImage;
    [SerializeField] private TextMeshProUGUI amountText;

    public void Setup(ResourceDataSO resourceDataSO)
    {
        iconImage.sprite = resourceDataSO.sprite;
        amountText.text = "0";
    }

    public void UpdateAmount(int amount)
    {
        amountText.text = amount.ToString();
    }
}
