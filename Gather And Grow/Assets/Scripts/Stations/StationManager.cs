using UnityEngine;
using System.Collections.Generic;
using UnityEditor;
using System.Linq;

public class StationManager : MonoBehaviour {
    public static StationManager Instance { get; private set; }

    [System.Serializable]
    public struct SpawnPoint {
        public GameObject prefab;
        public Transform spawnPos;
    }

    [System.Serializable]
    public struct StationPrefabEntry {
        public StationType type;
        public GameObject prefab;
    }

    [Header("References")]
    [SerializeField] private Transform stationParent;
    [SerializeField] private Transform plotParent;
    [SerializeField] private StationPrefabEntry[] stationPrefabs;

    [Header("Initial Spawn Points")]
    [Tooltip("Define where and which station prefabs should appear on a new game")]
    [SerializeField] private List<SpawnPoint> stationSpawnPoints;
    [Tooltip("Define where and which plot prefabs should appear on a new game")]
    [SerializeField] private List<SpawnPoint> plotSpawnPoints;
    [SerializeField] private GameObject[] stationPlotPrefabs;

    // Runtime lists of the actual scene instances
    private List<GameObject> allStations;
    private List<GameObject> allPlots;

    // ES3 keys
    private const string StationsKey = "allStations";
    private const string PlotsKey = "allPlots";

    private void Awake() {
        Instance = this;

        allStations = new List<GameObject>();
        allPlots = new List<GameObject>();
    }

    private void Start() {
        string path = SaveManager.CurrentSaveFilePath;
        if (ES3.KeyExists(StationsKey, path))
            LoadAll(path);
        else
            SpawnInitial();

        SaveManager.OnGameSaved += OnGameSaved;
    }

    // Called whenever a station is created at runtime
    public void AddStation(GameObject station) {
        if (station == null) {
            Debug.LogError("Attempted to add a null station.");
            return;
        }
        if (allStations.Contains(station) == false)
            allStations.Add(station);
    }

    // Called when a plot is “used up” (purchased)
    public void AddStationPlot(GameObject plot) {
        if (plot == null) {
            Debug.LogError("Attempted to add a null station plot.");
            return;
        }
        if (allPlots.Contains(plot) == false)
            allPlots.Add(plot);
    }

    private void OnDestroy() {
        SaveManager.OnGameSaved -= OnGameSaved;
        SaveManager.OnGameLoaded -= LoadAll;
    }

    private void SpawnInitial() {
        foreach (SpawnPoint sp in stationSpawnPoints) {
            if (sp.prefab == null) continue;
            GameObject go = Instantiate(sp.prefab, sp.spawnPos.position, sp.spawnPos.rotation, stationParent);
            allStations.Add(go);
        }
        // Plots
        foreach (SpawnPoint sp in plotSpawnPoints) {
            if (sp.prefab == null) continue;
            GameObject go = Instantiate(sp.prefab, sp.spawnPos.position, sp.spawnPos.rotation, plotParent);
            allPlots.Add(go);
        }
    }

    private void OnGameSaved(string path) {
        var stationData = allStations
            .Select(go => go.GetComponent<BaseStation>().GetSaveData())
            .ToList();
        ES3.Save(StationsKey, stationData, path);

        var plotData = allPlots
            .Select(go => go.GetComponent<StationPlot>().GetSaveData())
            .ToList();
        ES3.Save(PlotsKey, plotData, path);
    }

    private void LoadAll(string path) {
        foreach (GameObject go in allStations) Destroy(go);
        allStations.Clear();

        var dataList = ES3.Load<List<StationSaveData>>(StationsKey, path);
        foreach (StationSaveData data in dataList) {
            GameObject prefab = PrefabForType(data.type);
            GameObject go = Instantiate(prefab, stationParent);
            BaseStation station = go.GetComponent<BaseStation>();
            station.RestoreFromData(data);
            allStations.Add(go);
        }

        foreach (GameObject go in allPlots) Destroy(go);
        allPlots.Clear();

        var plotDataList = ES3.Load<List<StationPlotSaveData>>(PlotsKey, path);

        foreach (StationPlotSaveData data in plotDataList) {
            GameObject go = Instantiate(stationPlotPrefabs[data.prefabIndex], plotParent);
            go.transform.position = data.position;
            go.transform.localScale = data.scale;
            StationPlot plot = go.GetComponent<StationPlot>();
            plot.LoadSaveData(data);
            allPlots.Add(go);
        }
    }

    private GameObject PrefabForType(StationType t) {
        return stationPrefabs.First(e => e.type == t).prefab;
    }
}
