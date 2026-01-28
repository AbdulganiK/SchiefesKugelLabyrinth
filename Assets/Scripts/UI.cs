using System;
using DefaultNamespace;
using UnityEngine;
using UnityEngine.UIElements;
using Vector3 = UnityEngine.Vector3;

public class UI : MonoBehaviour
{
    private String[] logBacklog = new String[3];
    
    private VisualElement main_window;
    private VisualElement settings_window;
    
    private Label time_text;
    private Button settings_button;
    private SliderInt x_axis;
    private SliderInt y_axis;
    private Button startstop_button;
    private Button reset_button;
    private Label position_text;
    private Label speed_text;
    private Label acceleration_text;
    private Label rollforce_text;
    private Label staticforce_text;
    private Label log;

    private Button back_button;
    private EnumField ball_material;
    private EnumField wall_material;
    private EnumField ground_material;
    private TextField mass;
    private TextField gravitation;
    
    public GameObject board;
    private BoardController boardController;
    public GameObject kugel;
    private KugelController kugelController;
    public GameObject gameManager;
    private TickManager tickManager;
    private ResetController resetController;

    void Awake()
    {
        boardController = board.GetComponent<BoardController>();
        kugelController = kugel.GetComponent<KugelController>();
        tickManager = gameManager.GetComponent<TickManager>();
        resetController = gameManager.GetComponent<ResetController>();
    }

    private void OnEnable()
    {
        VisualElement root = GetComponent<UIDocument>().rootVisualElement;

        main_window = root.Q<VisualElement>("main_window");
        settings_window = root.Q<VisualElement>("settings_window");

        time_text = root.Q<Label>("time_text");
        settings_button = root.Q<Button>("settings_button");
        x_axis = root.Q<SliderInt>("x_axis");
        y_axis = root.Q<SliderInt>("y_axis");
        startstop_button = root.Q<Button>("startstop_button");
        reset_button = root.Q<Button>("reset_button");
        position_text = root.Q<Label>("position_text");
        speed_text = root.Q<Label>("speed_text");
        acceleration_text = root.Q<Label>("acceleration_text");
        rollforce_text = root.Q<Label>("rollforce_text");
        staticforce_text = root.Q<Label>("staticforce_text");
        log = root.Q<Label>("log");

        back_button = root.Q<Button>("back_button");
        ball_material = root.Q<EnumField>("ball_material");
        wall_material = root.Q<EnumField>("wall_material");
        ground_material = root.Q<EnumField>("ground_material");
        mass = root.Q<TextField>("mass");
        mass.value = kugelController.masse.ToString();
        gravitation = root.Q<TextField>("gravitation");
        gravitation.value = kugelController.gravitation.ToString();

        settings_button.clicked += () => switchTab(false);
        back_button.clicked += () => switchTab(true);
        startstop_button.clicked += () => tickManager.TogglePause();
        reset_button.clicked += () => resetController.ResetAll();

        ball_material.Init(CollisionMaterial.STAHL);
        wall_material.Init(CollisionMaterial.HOLZ);
        ground_material.Init(CollisionMaterial.HOLZ);

        ball_material.RegisterCallback<ChangeEvent<Enum>>(evt =>
        {
            updateMaterials();
        });
        wall_material.RegisterCallback<ChangeEvent<Enum>>(evt =>
        {
            updateMaterials();
        });
        ground_material.RegisterCallback<ChangeEvent<Enum>>(evt =>
        {
            updateMaterials();
        });
    }

    private void updateMaterials()
    {
        kugelController.rueckprallWand = (float)((int) (CollisionMaterial) ball_material.value + (int) (CollisionMaterial) wall_material.value) / 100;
        kugelController.rueckprallBrett = (float)((int) (CollisionMaterial) ball_material.value + (int) (CollisionMaterial) ground_material.value) / 100;
    }

    private void switchTab(bool isMainWindow)
    {
        main_window.SetEnabled(isMainWindow);
        main_window.visible = isMainWindow;
        settings_window.SetEnabled(!isMainWindow);
        settings_window.visible = !isMainWindow;
        if (isMainWindow)
        {
            kugelController.masse = Convert.ToSingle(mass.value);
            kugelController.gravitation = Convert.ToSingle(gravitation.value);
        }
        else
        {
            mass.value = kugelController.masse.ToString();
            gravitation.value = kugelController.gravitation.ToString();
        }
    }

    private void Update()
    {
        time_text.text = kugelController.getTicks() + " ticks";
        
        x_axis.value = (int) boardController.getRotationX();
        y_axis.value = (int) boardController.getRotationY();
        x_axis.label = "X-Achse " + (int) boardController.getRotationX() + "°";
        y_axis.label = "Y-Achse " + (int) boardController.getRotationY() + "°";

        Vector3 position = kugelController.getPosition();
        position_text.text = Math.Round(position.x, 2) + "/" + Math.Round(position.y, 2) + "/" + Math.Round(position.z, 2);

        speed_text.text = Math.Round(kugelController.getVelocity(), 2) + "m/s";
        acceleration_text.text = Math.Round(kugelController.getAccelleration(), 2) + "m/s²";
        rollforce_text.text = Math.Round(kugelController.getFhaftValue(), 2) + "N";
        staticforce_text.text = Math.Round(kugelController.getFgleitValue(), 2) + "N";
    }

    public void setLogText(String logText)
    {
        log.text = "";
        
        logBacklog[2] = logBacklog[1];
        logBacklog[1] = logBacklog[0];
        logBacklog[0] = logText;
        
        log.text += "<size=20>" + logBacklog[0] + "</size>\n" + logBacklog[1] + "\n" + logBacklog[2];
    }
}