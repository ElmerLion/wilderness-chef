using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;

public class HarvestableManager : MonoBehaviour {
    public static HarvestableManager Instance { get; private set; }

    private readonly List<Harvestable> harvestables = new List<Harvestable>();
    private readonly List<GardenPlot> gardenPlots = new List<GardenPlot>();

    private void Awake() {
        Instance = this;
    }

    private void Start() {
        SaveManager.OnGameSaved += OnGameSaved;
        SaveManager.OnGameLoaded += OnGameLoaded;
    }

    public void RegisterHarvestable(Harvestable h) {
        if (!harvestables.Contains(h))
            harvestables.Add(h);
    }

    public void RegisterGardenPlot(GardenPlot plot) {
        if (!gardenPlots.Contains(plot))
            gardenPlots.Add(plot);
    }

    private void OnGameSaved(string saveFilePath) {
        // build data list
        List<HarvestableData> dataList = new List<HarvestableData>();
        foreach (Harvestable h in harvestables) {
            HarvestableData d = new HarvestableData {
                saveID = h.SaveID,
                isDestroyed = h.IsDestroyed,
                currentHealth = h.CurrentHealth,
                regrownItemCount = h.RegrownItemCount,
                regrowTimer = h.RegrowTimer
            };
            dataList.Add(d);
        }

        ES3.Save("harvestables", dataList, saveFilePath);

        List<GardenPlotSaveData> gardenPlotDataList = new List<GardenPlotSaveData>();
        foreach (GardenPlot plot in gardenPlots) {
            gardenPlotDataList.Add(plot.GetSaveData());
        }

        ES3.Save("gardenPlots", gardenPlotDataList, saveFilePath);
    }

    private void OnGameLoaded(string saveFilePath) {
        if (!ES3.KeyExists("harvestables", saveFilePath))
            return;

        List<GardenPlotSaveData> gardenPlotDataList =
    ES3.Load<List<GardenPlotSaveData>>("gardenPlots", saveFilePath);

        Dictionary<string, GardenPlotSaveData> gardenPlotDataById = new Dictionary<string, GardenPlotSaveData>();
        foreach (var data in gardenPlotDataList) {
            if (gardenPlotDataById.ContainsKey(data.saveID)) {
                Debug.LogWarning($"Duplicate GardenPlotSaveData ID in save: {data.saveID}");
                continue;
            }
            gardenPlotDataById.Add(data.saveID, data);
        }

        foreach (GardenPlot plot in gardenPlots.ToList()) {
            if (gardenPlotDataById.TryGetValue(plot.SaveID, out GardenPlotSaveData d)) {
                plot.RestoreFromData(d);
            } else {
                Debug.LogWarning($"Garden plot with ID {plot.SaveID} not found in save data.");
            }
        }

        List<HarvestableData> dataList =
            ES3.Load<List<HarvestableData>>("harvestables", saveFilePath);

        Debug.Log($"Loaded {dataList.Count} harvestables from save file.");

        Dictionary<string, HarvestableData> map = new Dictionary<string, HarvestableData>();
        foreach (var data in dataList) {
            if (map.ContainsKey(data.saveID)) {
                Debug.LogWarning($"Duplicate HarvestableData ID in save: {data.saveID}");
                continue;
            }
            map.Add(data.saveID, data);
        }

        // apply to each scene instance
        foreach (Harvestable h in harvestables.ToList()) {
            if (map.TryGetValue(h.SaveID, out HarvestableData d))
                h.ApplyState(d);
        }
    }

    public Harvestable GetHarvestableById(string id) {
        return harvestables.FirstOrDefault(h => h.SaveID == id);
    }

    private void OnDestroy() {
        SaveManager.OnGameSaved -= OnGameSaved;
        SaveManager.OnGameLoaded -= OnGameLoaded;
    }
}
