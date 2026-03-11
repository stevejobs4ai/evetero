// PlayerWallet.cs
// Singleton MonoBehaviour that tracks the player's currency balances.
// Survives scene loads via DontDestroyOnLoad.

using System;
using System.Collections.Generic;
using UnityEngine;

namespace Evetero
{
    public class PlayerWallet : MonoBehaviour
    {
        // ── Singleton ─────────────────────────────────────────────────────────

        private static PlayerWallet _instance;
        public static PlayerWallet Instance => _instance;

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }
            _instance = this;
            DontDestroyOnLoad(gameObject);

            _balances = new Dictionary<CurrencyType, int>
            {
                { CurrencyType.HardCurrency, 0 },
                { CurrencyType.SoftCurrency, 100 }
            };
        }

        private void OnDestroy()
        {
            if (_instance == this) _instance = null;
        }

        // ── Events ────────────────────────────────────────────────────────────

        /// <summary>Fired when a balance changes; carries type and new balance.</summary>
        public event Action<CurrencyType, int> OnBalanceChanged;

        // ── Storage ───────────────────────────────────────────────────────────

        private Dictionary<CurrencyType, int> _balances;

        // ── Public API ────────────────────────────────────────────────────────

        public int GetBalance(CurrencyType type)
        {
            _balances.TryGetValue(type, out int amount);
            return amount;
        }

        public void Earn(CurrencyType type, int amount)
        {
            if (amount <= 0) return;
            _balances.TryGetValue(type, out int current);
            _balances[type] = current + amount;
            OnBalanceChanged?.Invoke(type, _balances[type]);
        }

        /// <summary>
        /// Deduct <paramref name="amount"/> of <paramref name="type"/> if available.
        /// Returns false (and makes no change) when funds are insufficient.
        /// </summary>
        public bool Spend(CurrencyType type, int amount)
        {
            if (amount <= 0) return true;
            _balances.TryGetValue(type, out int current);
            if (current < amount) return false;
            _balances[type] = current - amount;
            OnBalanceChanged?.Invoke(type, _balances[type]);
            return true;
        }

        // ── Serialization helpers ─────────────────────────────────────────────

        public Dictionary<CurrencyType, int> GetAllBalances() =>
            new Dictionary<CurrencyType, int>(_balances);

        public void LoadBalances(Dictionary<CurrencyType, int> data)
        {
            foreach (var kvp in data)
                _balances[kvp.Key] = kvp.Value;
        }
    }
}
