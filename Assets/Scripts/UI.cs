using UnityEngine;
using UnityEngine.UIElements;

public class UI : MonoBehaviour
{
    private Label time_text;
    private Label position_text;
    private Label speed_text;
    private Label acceleration_text;
    private Label rollforce_text;
    private Label staticforce_text;
    private Label log;

    private void OnEnable()
    {
        VisualElement root = GetComponent<UIDocument>().rootVisualElement;

        time_text = root.Q<Label>("time_text");
        Button setting_button = root.Q<Button>("settings_button");
        //missing reference to sliders cause they weird
        Button startstop_button = root.Q<Button>("startstop_button");
        Button reset_button = root.Q<Button>("reset_button");
        position_text = root.Q<Label>("position_text");
        speed_text = root.Q<Label>("speed_text");
        acceleration_text = root.Q<Label>("acceleration_text");
        rollforce_text = root.Q<Label>("rollforce_text");
        staticforce_text = root.Q<Label>("staticforce_text");
        log = root.Q<Label>("log");

        startstop_button.clicked += () => temp();
    }

    private void temp()
    {
        log.text = "Test";
    }
}