// ResourceBank.cs
// MonoBehaviour that tracks the player's resource stockpile.
// Attach to a dedicated "Bank" GameObject in the scene.
//
// Capacity: 10,000 units per resource type (TODO: scale with player level).

using System;
using System.Collections.Generic;
using UnityEngine;

namespace Evetero
{
    public class ResourceBank : MonoBehaviour
    {
        public const int MaxCapacity = 10_000;

        // ── Singleton ─────────────────────────────────────────────────────────

        private static ResourceBank _instance;
        public static ResourceBank Instance => _instance;

        private void Awake()
        {
            if (_instance == null) _instance = this;
        }

        private void OnDestroy()
        {
            if (_instance == this) _instance = null;
        }

        // ── Events ────────────────────────────────────────────────────────────

        /// <summary>Fired after a successful deposit; carries the new total for that resource.</summary>
        public event Action<ResourceType, int> OnResourceDeposited;

        // ── Storage ───────────────────────────────────────────────────────────

        private readonly Dictionary<ResourceType, int> _resources = new();

        // ── Public API ────────────────────────────────────────────────────────

        /// <summary>Add up to MaxCapacity units of <paramref name="type"/>.</summary>
        public void Deposit(ResourceType type, int amount)
        {
            if (amount <= 0) return;

            _resources.TryGetValue(type, out int current);
            int newTotal   = Mathf.Min(current + amount, MaxCapacity);
            _resources[type] = newTotal;

            OnResourceDeposited?.Invoke(type, newTotal);
        }

        /// <summary>Current stored amount of <paramref name="type"/>.</summary>
        public int GetAmount(ResourceType type)
        {
            _resources.TryGetValue(type, out int amount);
            return amount;
        }

        /// <summary>
        /// Deduct <paramref name="amount"/> of <paramref name="type"/> if available.
        /// Returns false (and makes no change) when funds are insufficient.
        /// </summary>
        public bool Spend(ResourceType type, int amount)
        {
            if (amount <= 0) return true;

            _resources.TryGetValue(type, out int current);
            if (current < amount) return false;

            _resources[type] = current - amount;
            return true;
        }
    }
}
