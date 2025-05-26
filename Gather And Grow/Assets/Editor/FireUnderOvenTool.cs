using UnityEngine;
using UnityEditor;

public class FireUnderOvenTool : EditorWindow {
    [SerializeField] private GameObject ovenRoot;
    [SerializeField] private Vector3 offset = new Vector3(0f, -0.5f, 0f);
    [SerializeField] private string fireObjectName = "OvenFire";

    [MenuItem("Tools/Fire Under Oven")]
    public static void ShowWindow() {
        FireUnderOvenTool window = GetWindow<FireUnderOvenTool>("Fire Under Oven");
        window.minSize = new Vector2(300f, 150f);
    }

    private void OnGUI() {
        GUILayout.Label("Fire Under Oven Tool", EditorStyles.boldLabel);

        ovenRoot = (GameObject)EditorGUILayout.ObjectField(
            "Oven Root",
            ovenRoot,
            typeof(GameObject),
            true
        );

        offset = EditorGUILayout.Vector3Field("Offset", offset);
        fireObjectName = EditorGUILayout.TextField("Fire Object Name", fireObjectName);

        EditorGUI.BeginDisabledGroup(ovenRoot == null);
        if (GUILayout.Button("Create Fire Effect")) {
            CreateFireEffect();
        }
        EditorGUI.EndDisabledGroup();
    }

    private void CreateFireEffect() {
        GameObject fireGO = new GameObject(fireObjectName);
        Undo.RegisterCreatedObjectUndo(fireGO, "Create Fire Effect");

        fireGO.transform.SetParent(ovenRoot.transform, false);
        fireGO.transform.localPosition = offset;

        ParticleSystem ps = fireGO.AddComponent<ParticleSystem>();

        // Configure main module
        ParticleSystem.MainModule mainModule = ps.main;
        mainModule.startLifetime = 1.0f;
        mainModule.startSpeed = 1.5f;
        mainModule.startSize = new ParticleSystem.MinMaxCurve(0.3f, 0.6f);
        mainModule.loop = true;
        mainModule.playOnAwake = true;
        mainModule.simulationSpace = ParticleSystemSimulationSpace.World;

        // Emission
        ParticleSystem.EmissionModule emission = ps.emission;
        emission.rateOverTime = 50f;

        // Shape
        ParticleSystem.ShapeModule shape = ps.shape;
        shape.shapeType = ParticleSystemShapeType.Cone;
        shape.angle = 25f;
        shape.radius = 0.2f;

        // Color over lifetime
        ParticleSystem.ColorOverLifetimeModule col = ps.colorOverLifetime;
        col.enabled = true;
        Gradient grad = new Gradient();
        grad.SetKeys(
            new GradientColorKey[] {
                new GradientColorKey(new Color(1f, 0.5f, 0f), 0f),
                new GradientColorKey(new Color(1f, 0f, 0f), 1f)
            },
            new GradientAlphaKey[] {
                new GradientAlphaKey(1f, 0f),
                new GradientAlphaKey(0f, 1f)
            }
        );
        col.color = grad;

        // Add a subtle point-light
        Light fireLight = fireGO.AddComponent<Light>();
        fireLight.type = LightType.Point;
        fireLight.range = 2f;
        fireLight.intensity = 1f;
        fireLight.color = new Color(1f, 0.5f, 0f);

        // Select the new fire object in the hierarchy
        Selection.activeGameObject = fireGO;
    }
}
