using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class RebindButton : MonoBehaviour
{
    [SerializeField] private InputReader inputReader;
    [SerializeField] private string actionName;
    [SerializeField] private int bindingIndex;

    [SerializeField] private TextMeshProUGUI keyText;
    [SerializeField] private Button button;

    private void Awake()
    {
        button.onClick.AddListener(OnClick);
    }

    private void OnEnable()
    {
        inputReader.OnBindingsChanged += UpdateUI;
        inputReader.OnRebindStarted += OnRebindStarted;

        UpdateUI();
    }

    private void OnDisable()
    {
        inputReader.OnBindingsChanged -= UpdateUI;
        inputReader.OnRebindStarted -= OnRebindStarted;
    }

    private void OnClick()
    {
        inputReader.StartRebind(actionName, bindingIndex);
    }

    private void UpdateUI()
    {
        keyText.text = inputReader.GetBindingName(actionName, bindingIndex);
    }

    private void OnRebindStarted(string action)
    {
        if (action == actionName)
        {
            keyText.text = "...";
        }
    }
}