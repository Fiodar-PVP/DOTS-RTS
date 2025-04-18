using UnityEngine;

public class UnitSelectionManagerUI : MonoBehaviour
{
    [SerializeField] private RectTransform selectionAreaRectTransform;

    private void Start()
    {
        UnitSelectionManager.Instance.OnSelectionStart += UnitSelectionManager_OnSelectionStart;
        UnitSelectionManager.Instance.OnSelectionEnd += UnitSelectionManager_OnSelectionEnd;

        selectionAreaRectTransform.gameObject.SetActive(false);
    }

    private void Update()
    {
        if (selectionAreaRectTransform.gameObject.activeSelf)
        {
            UpdateVisual();
        }
    }

    private void UnitSelectionManager_OnSelectionStart(object sender, System.EventArgs e)
    {
        selectionAreaRectTransform.gameObject.SetActive(true);

        UpdateVisual();
    }
    private void UnitSelectionManager_OnSelectionEnd(object sender, System.EventArgs e)
    {
        selectionAreaRectTransform.gameObject.SetActive(false);
    }

    private void UpdateVisual()
    {
        Rect selectionRect = UnitSelectionManager.Instance.GetSelectionArea();

        selectionAreaRectTransform.anchoredPosition = selectionRect.position;
        selectionAreaRectTransform.sizeDelta = selectionRect.size;
    }
}
