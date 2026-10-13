using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LagosLifestyle.Economy;

namespace LagosLifestyle.Missions
{
    [Serializable]
    public struct BusStop
    {
        public string stopName;
        public Vector3 worldPosition;
        public int expectedFare;
    }

    /// <summary>
    /// Danfo route and passenger fare mission manager.
    /// Handles popular Lagos routes (e.g. Oshodi to CMS, Ojuelegba to Ikeja).
    /// </summary>
    public class DanfoMissionManager : MonoBehaviour
    {
        public static DanfoMissionManager Instance { get; private set; }

        [Header("Route Definition")]
        [SerializeField] private List<BusStop> routeStops = new List<BusStop>()
        {
            new BusStop { stopName = "Oshodi Underbridge", worldPosition = new Vector3(50, 0, 100), expectedFare = 300 },
            new BusStop { stopName = "Ojuelegba Roundabout", worldPosition = new Vector3(250, 0, 300), expectedFare = 400 },
            new BusStop { stopName = "CMS Marina", worldPosition = new Vector3(500, 0, 750), expectedFare = 500 }
        };

        [Header("Mission Status")]
        [SerializeField] private int currentStopIndex = 0;
        [SerializeField] private int passengerCount = 0;
        [SerializeField] private int maxCapacity = 14; // Standard Lagos Danfo capacity
        [SerializeField] private float stopArrivalRadius = 8f;

        public event Action<string, int> OnStopReached;
        public event Action<int> OnPassengersUpdated;

        public BusStop? CurrentTargetStop => (currentStopIndex < routeStops.Count) ? routeStops[currentStopIndex] : null;
        public int PassengerCount => passengerCount;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        public void CheckArrival(Vector3 vehiclePosition)
        {
            if (currentStopIndex >= routeStops.Count) return;

            var target = routeStops[currentStopIndex];
            float dist = Vector3.Distance(vehiclePosition, target.worldPosition);

            if (dist <= stopArrivalRadius)
            {
                CompleteCurrentStop(target);
            }
        }

        private void CompleteCurrentStop(BusStop stop)
        {
            // Drop off existing passengers and collect fares
            int faresCollected = passengerCount * stop.expectedFare;
            if (EconomyManager.Instance != null && faresCollected > 0)
            {
                EconomyManager.Instance.AddMoney(faresCollected, $"Danfo Fares at {stop.stopName}");
            }

            // Pick up new passengers
            int newBoarding = UnityEngine.Random.Range(4, maxCapacity);
            passengerCount = Mathf.Clamp(newBoarding, 0, maxCapacity);

            OnStopReached?.Invoke(stop.stopName, faresCollected);
            OnPassengersUpdated?.Invoke(passengerCount);

            currentStopIndex++;
            if (currentStopIndex >= routeStops.Count)
            {
                Debug.Log("[DanfoMission] Route completed! All passengers dropped off.");
            }
        }
    }
}
