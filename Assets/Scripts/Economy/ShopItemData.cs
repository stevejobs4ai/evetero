// ShopItemData.cs
// ScriptableObject representing a single item in the shop.

using UnityEngine;

namespace Evetero
{
    public enum ShopItemType
    {
        HeroSkin,
        AllianceBanner,
        VoicePack,
        FoundersPack
    }

    [CreateAssetMenu(menuName = "Evetero/Shop/ShopItem", fileName = "NewShopItem")]
    public class ShopItemData : ScriptableObject
    {
        public string       itemName;
        public string       description;
        public CurrencyType currencyType;
        public int          price;
        public ShopItemType itemType;
        public Sprite       icon;
    }
}
