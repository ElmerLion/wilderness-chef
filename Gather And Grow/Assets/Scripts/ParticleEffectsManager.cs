using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum EffectType {
    SmokePuff,
    Sizzle,
    HitSmoke,
    Boiling,
    CoinGain,
    DirtPickup,
    WaterSplash,
    AnimalDeath,
    StationCut,
    Confetti,
    Hearts,
}

[Serializable]
public struct EffectEntry {
    public EffectType type;
    public GameObject prefab;
}

/// <summary>
/// Manages spawning, pooling, and recycling of visual effects.
/// </summary>
public class ParticleEffectsManager : MonoBehaviour {
    public static ParticleEffectsManager Instance { get; private set; }

    [Header("Mapping of effect types to prefabs")]
    [SerializeField]
    private EffectEntry[] entries;

    private Dictionary<EffectType, GameObject> prefabMap;
    private Dictionary<EffectType, Queue<GameObject>> pools;

    private void Awake() {
        Instance = this;

        // Build prefab lookup and initialize pools
        prefabMap = new Dictionary<EffectType, GameObject>();
        pools = new Dictionary<EffectType, Queue<GameObject>>();

        foreach (var entry in entries) {
            if (!prefabMap.ContainsKey(entry.type) && entry.prefab != null) {
                prefabMap.Add(entry.type, entry.prefab);
                pools.Add(entry.type, new Queue<GameObject>());
            }
        }
    }

    /// <summary>
    /// Spawns (or reuses) an effect of the given type at the specified position and rotation.
    /// Returns an EffectInstance handle for optional control or callbacks.
    /// </summary>
    public EffectInstance Play(EffectType type, Vector3 position, bool init = true, Quaternion rotation = default) {
        if (!prefabMap.TryGetValue(type, out GameObject prefab)) {
            Debug.LogWarning($"[EffectManager] No prefab registered for effect type '{type}'");
            return null;
        }

        GameObject instanceGO;
        var pool = pools[type];

        if (pool.Count > 0) {
            instanceGO = pool.Dequeue();
            instanceGO.transform.SetPositionAndRotation(position, rotation);
            instanceGO.SetActive(true);
        } else {
            instanceGO = Instantiate(prefab, position, rotation, transform);
        }

        // Ensure an EffectInstance component is present
        EffectInstance inst = instanceGO.GetComponent<EffectInstance>();
        if (inst == null)
            inst = instanceGO.AddComponent<EffectInstance>();

        if (init) {
            inst.Init(type, this);
        }
        return inst;
    }

    /// <summary>
    /// Returns an effect instance to its pool. Called by EffectInstance.
    /// </summary>
    internal void ReturnToPool(EffectType type, EffectInstance inst) {
        GameObject go = inst.gameObject;
        go.SetActive(false);
        pools[type].Enqueue(go);
    }
}

public class EffectInstance : MonoBehaviour {
    public event Action OnComplete;

    private ParticleSystem[] _systems;
    private EffectType _type;
    private ParticleEffectsManager _manager;
    private bool _hasFired;

    public void Init(EffectType type, ParticleEffectsManager manager) {
        _type = type;
        _manager = manager;
        _hasFired = false;

        _systems = GetComponentsInChildren<ParticleSystem>(true);

        if (_systems == null || _systems.Length == 0) {
            Debug.LogWarning($"[{nameof(EffectInstance)}] No ParticleSystems found on {gameObject.name}");
        } else {
            foreach (ParticleSystem ps in _systems) {
                if (!ps.gameObject.activeInHierarchy)
                    ps.gameObject.SetActive(true);

                ps.Play();
            }
        }

        StartCoroutine(WatchForEnd());
    }

    private IEnumerator WatchForEnd() {
        yield return new WaitUntil(() => {
            foreach (ParticleSystem ps in _systems) {
                if (ps != null && ps.IsAlive(true))
                    return false;
            }
            return true;
        });
        FireComplete();
    }

    private void FireComplete() {
        if (_hasFired) return;
        _hasFired = true;
        OnComplete?.Invoke();
        _manager.ReturnToPool(_type, this);
    }

    public void Stop() {
        if (_systems != null) {
            foreach (var ps in _systems) {
                if (ps != null && ps.isEmitting)
                    ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            }
        }
        FireComplete();
    }
}
