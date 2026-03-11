// WalletTests.cs
// Play Mode tests for PlayerWallet.

using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Evetero.Tests
{
    public class WalletTests
    {
        private PlayerWallet _wallet;

        [SetUp]
        public void SetUp()
        {
            var go = new GameObject("Wallet");
            _wallet = go.AddComponent<PlayerWallet>();
        }

        [TearDown]
        public void TearDown()
        {
            if (_wallet != null)
                Object.Destroy(_wallet.gameObject);
        }

        [UnityTest]
        public IEnumerator Earn_IncreasesBalance()
        {
            int before = _wallet.GetBalance(CurrencyType.SoftCurrency);
            _wallet.Earn(CurrencyType.SoftCurrency, 50);
            yield return null;
            Assert.AreEqual(before + 50, _wallet.GetBalance(CurrencyType.SoftCurrency));
        }

        [UnityTest]
        public IEnumerator Spend_SucceedsAndDecreasesBalance()
        {
            _wallet.Earn(CurrencyType.SoftCurrency, 200);
            yield return null;
            int before = _wallet.GetBalance(CurrencyType.SoftCurrency);
            bool result = _wallet.Spend(CurrencyType.SoftCurrency, 50);
            yield return null;
            Assert.IsTrue(result);
            Assert.AreEqual(before - 50, _wallet.GetBalance(CurrencyType.SoftCurrency));
        }

        [UnityTest]
        public IEnumerator Spend_ReturnsFalseAndBalanceUnchangedWhenInsufficient()
        {
            // HardCurrency starts at 0
            int before = _wallet.GetBalance(CurrencyType.HardCurrency);
            bool result = _wallet.Spend(CurrencyType.HardCurrency, 100);
            yield return null;
            Assert.IsFalse(result);
            Assert.AreEqual(before, _wallet.GetBalance(CurrencyType.HardCurrency));
        }

        [UnityTest]
        public IEnumerator OnBalanceChanged_FiresOnEarnAndSuccessfulSpend()
        {
            int eventCount = 0;
            _wallet.OnBalanceChanged += (_, __) => eventCount++;

            _wallet.Earn(CurrencyType.SoftCurrency, 10);
            yield return null;
            Assert.AreEqual(1, eventCount);

            _wallet.Spend(CurrencyType.SoftCurrency, 5);
            yield return null;
            Assert.AreEqual(2, eventCount);

            // Failed spend should NOT fire
            _wallet.Spend(CurrencyType.HardCurrency, 999);
            yield return null;
            Assert.AreEqual(2, eventCount);
        }
    }
}
