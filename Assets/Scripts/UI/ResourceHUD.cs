// ResourceHUD.cs
// Canvas overlay MonoBehaviour that displays current resource counts.
// Wire one TMP_Text per resource type in the Inspector, attach to a UI Canvas.

using TMPro;
using UnityEngine;

namespace Evetero
{
    public class ResourceHUD : MonoBehaviour
    {
        [SerializeField] private TMP_Text woodText;
        [SerializeField] private TMP_Text stoneText;
        [SerializeField] private TMP_Text foodText;
        [SerializeField] private TMP_Text ironText;

        private void Start()
        {
            if (ResourceBank.Instance == null) return;

            RefreshLabel(ResourceType.Wood);
            RefreshLabel(ResourceType.Stone);
            RefreshLabel(ResourceType.Food);
            RefreshLabel(ResourceType.Iron);

            ResourceBank.Instance.OnResourceDeposited += OnDeposited;
        }

        private void OnDestroy()
        {
            if (ResourceBank.Instance != null)
                ResourceBank.Instance.OnResourceDeposited -= OnDeposited;
        }

        private void OnDeposited(ResourceType type, int newTotal)
        {
            RefreshLabel(type);
        }

        private void RefreshLabel(ResourceType type)
        {
            int amount = ResourceBank.Instance.GetAmount(type);
            switch (type)
            {
                case ResourceType.Wood:  if (woodText  != null) woodText.text  = $"Wood: {amount}";  break;
                case ResourceType.Stone: if (stoneText != null) stoneText.text = $"Stone: {amount}"; break;
                case ResourceType.Food:  if (foodText  != null) foodText.text  = $"Food: {amount}";  break;
                case ResourceType.Iron:  if (ironText  != null) ironText.text  = $"Iron: {amount}";  break;
            }
        }
    }
}
