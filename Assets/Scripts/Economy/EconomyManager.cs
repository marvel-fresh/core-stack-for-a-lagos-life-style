using System;
using UnityEngine;

namespace LagosLifestyle.Economy
{
    /// <summary>
    /// In-game Naira (₦) economy manager for Lagos Life-style.
    /// Manages passenger fares, fuel costs, market shopping, and vehicle repairs.
    /// </summary>
    public class EconomyManager : MonoBehaviour
    {
        public static EconomyManager Instance { get; private set; }

        [Header("Starting Wallet (Naira ₦)")]
        [SerializeField] private int currentBalance = 5000;

        public event Action<int> OnBalanceChanged;

        public int CurrentBalance => currentBalance;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        public void AddMoney(int amount, string reason = "")
        {
            if (amount <= 0) return;
            currentBalance += amount;
            Debug.Log($"[Economy] Earned ₦{amount:N0} ({reason}). Total: ₦{currentBalance:N0}");
            OnBalanceChanged?.Invoke(currentBalance);
        }

        public bool SpendMoney(int amount, string reason = "")
        {
            if (amount <= 0) return true;

            if (currentBalance >= amount)
            {
                currentBalance -= amount;
                Debug.Log($"[Economy] Spent ₦{amount:N0} on {reason}. Balance: ₦{currentBalance:N0}");
                OnBalanceChanged?.Invoke(currentBalance);
                return true;
            }

            Debug.LogWarning($"[Economy] Insufficient funds! Needed ₦{amount:N0}, have ₦{currentBalance:N0}");
            return false;
        }
    }
}
