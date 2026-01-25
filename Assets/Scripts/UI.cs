using System;
using UnityEngine;
using UnityEngine.UIElements;
using Vector3 = UnityEngine.Vector3;

public class UI : MonoBehaviour
{
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
    
    public GameObject board;
    private BoardController boardController;
    public GameObject kugel;
    private KugelController kugelController;

    void Awake()
    {
        boardController = board.GetComponent<BoardController>();
        kugelController = kugel.GetComponent<KugelController>();
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

        settings_button.clicked += () => switchTab(false);
        back_button.clicked += () => switchTab(true);
    }

    private void switchTab(bool isMainWindow)
    {
        main_window.SetEnabled(isMainWindow);
        main_window.visible = isMainWindow;
        settings_window.SetEnabled(!isMainWindow);
        settings_window.visible = !isMainWindow;
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
    }
}