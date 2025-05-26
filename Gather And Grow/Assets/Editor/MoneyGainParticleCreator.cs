using UnityEngine;
using UnityEditor;

public class MoneyGainParticleCreator : EditorWindow {
    [SerializeField] private Material coinMaterial;
    [SerializeField] private Material sparkleMaterial;

    [SerializeField] private int coinBurstCount = 20;
    [SerializeField] private float coinStartSpeed = 2f;
    [SerializeField] private float coinStartLifetime = 1.5f;
    [SerializeField] private float coinStartSize = 0.5f;

    [SerializeField] private int sparkleBurstCount = 30;
    [SerializeField] private float sparkleStartSpeed = 1f;
    [SerializeField] private float sparkleStartLifetime = 1f;
    [SerializeField] private float sparkleStartSize = 0.2f;

    [SerializeField] private float emitterRadius = 0.5f;

    [MenuItem("Tools/Money Gain Particle Creator")]
    public static void ShowWindow() {
        MoneyGainParticleCreator window = GetWindow<MoneyGainParticleCreator>(false, "Money Gain Particle");
        window.minSize = new Vector2(350, 260);
    }

    private void OnGUI() {
        GUILayout.Label("Money Gain Particle Settings", EditorStyles.boldLabel);

        coinMaterial = (Material)EditorGUILayout.ObjectField("Coin Material", coinMaterial, typeof(Material), false);
        sparkleMaterial = (Material)EditorGUILayout.ObjectField("Sparkle Material", sparkleMaterial, typeof(Material), false);

        GUILayout.Space(5);
        GUILayout.Label("Coin Emitter", EditorStyles.miniBoldLabel);
        coinBurstCount = EditorGUILayout.IntField(" Burst Count", coinBurstCount);
        coinStartSpeed = EditorGUILayout.FloatField(" Start Speed", coinStartSpeed);
        coinStartLifetime = EditorGUILayout.FloatField(" Lifetime", coinStartLifetime);
        coinStartSize = EditorGUILayout.FloatField(" Start Size", coinStartSize);

        GUILayout.Space(5);
        GUILayout.Label("Sparkle Emitter", EditorStyles.miniBoldLabel);
        sparkleBurstCount = EditorGUILayout.IntField(" Burst Count", sparkleBurstCount);
        sparkleStartSpeed = EditorGUILayout.FloatField(" Start Speed", sparkleStartSpeed);
        sparkleStartLifetime = EditorGUILayout.FloatField(" Lifetime", sparkleStartLifetime);
        sparkleStartSize = EditorGUILayout.FloatField(" Start Size", sparkleStartSize);

        GUILayout.Space(10);
        emitterRadius = EditorGUILayout.FloatField("Emitter Radius", emitterRadius);

        GUILayout.Space(15);
        bool canCreate = (coinMaterial != null && sparkleMaterial != null);
        EditorGUI.BeginDisabledGroup(!canCreate);
        if (GUILayout.Button("Create Money Gain Effect", GUILayout.Height(30))) {
            CreateMoneyGainParticleSystem();
        }
        EditorGUI.EndDisabledGroup();

        if (!canCreate) {
            EditorGUILayout.HelpBox("Please assign both a Coin Material and a Sparkle Material.", MessageType.Warning);
        }
    }

    private void CreateMoneyGainParticleSystem() {
        // Parent root
        GameObject root = new GameObject("MoneyGainEffect");
        Undo.RegisterCreatedObjectUndo(root, "Create MoneyGainEffect");

        // --- Coin Particle System ---
        GameObject coinGO = new GameObject("CoinParticleSystem");
        coinGO.transform.parent = root.transform;
        ParticleSystem coinPS = coinGO.AddComponent<ParticleSystem>();
        ParticleSystem.MainModule mainModuleCoin = coinPS.main;
        mainModuleCoin.loop = false;
        mainModuleCoin.playOnAwake = false;
        mainModuleCoin.duration = coinStartLifetime;
        mainModuleCoin.startLifetime = coinStartLifetime;
        mainModuleCoin.startSpeed = coinStartSpeed;
        mainModuleCoin.startSize = coinStartSize;

        ParticleSystem.EmissionModule emissionModuleCoin = coinPS.emission;
        emissionModuleCoin.rateOverTime = 0f;
        ParticleSystem.Burst coinBurst = new ParticleSystem.Burst(0f, (short)coinBurstCount);
        emissionModuleCoin.SetBursts(new ParticleSystem.Burst[] { coinBurst });

        ParticleSystem.ShapeModule shapeModuleCoin = coinPS.shape;
        shapeModuleCoin.shapeType = ParticleSystemShapeType.Sphere;
        shapeModuleCoin.radius = emitterRadius;

        ParticleSystemRenderer rendererCoin = coinPS.GetComponent<ParticleSystemRenderer>();
        rendererCoin.material = coinMaterial;
        rendererCoin.renderMode = ParticleSystemRenderMode.Billboard;

        // --- Sparkle Particle System ---
        GameObject sparkleGO = new GameObject("SparkleParticleSystem");
        sparkleGO.transform.parent = root.transform;
        ParticleSystem sparkPS = sparkleGO.AddComponent<ParticleSystem>();
        ParticleSystem.MainModule mainModuleSpark = sparkPS.main;
        mainModuleSpark.loop = false;
        mainModuleSpark.playOnAwake = false;
        mainModuleSpark.duration = sparkleStartLifetime;
        mainModuleSpark.startLifetime = sparkleStartLifetime;
        mainModuleSpark.startSpeed = sparkleStartSpeed;
        mainModuleSpark.startSize = sparkleStartSize;

        ParticleSystem.EmissionModule emissionModuleSpark = sparkPS.emission;
        emissionModuleSpark.rateOverTime = 0f;
        ParticleSystem.Burst sparkleBurst = new ParticleSystem.Burst(0f, (short)sparkleBurstCount);
        emissionModuleSpark.SetBursts(new ParticleSystem.Burst[] { sparkleBurst });

        ParticleSystem.ShapeModule shapeModuleSpark = sparkPS.shape;
        shapeModuleSpark.shapeType = ParticleSystemShapeType.Sphere;
        shapeModuleSpark.radius = emitterRadius;

        ParticleSystemRenderer rendererSpark = sparkPS.GetComponent<ParticleSystemRenderer>();
        rendererSpark.material = sparkleMaterial;
        rendererSpark.renderMode = ParticleSystemRenderMode.Billboard;

        // Select the parent so the user can move it into the scene
        Selection.activeGameObject = root;
    }
}
