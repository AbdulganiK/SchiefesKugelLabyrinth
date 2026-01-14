using UnityEngine;


    public class KugelController : MonoBehaviour
{
    [Header("Physik")]
    public float masse = 1f;
    public float gravitation = 9.81f;
    public float rollReibungsKoef = 0.05f;

    [Header("Umgebung")]
    public Transform brett; // hier im Inspector die Labyrinth-Platte reinziehen

    private Vector2 velocity;   // aktuelle Geschwindigkeit in X/Z
    private Vector2 position;   // aktuelle Position in X/Z

    void Start()
    {
        // Startposition aus Transform holen
        position = new Vector2(transform.position.x, transform.position.z);
    }

    void FixedUpdate()
    {
        float dt = Time.fixedDeltaTime;

        // 1. Brett-Winkel auslesen
        // Neigung links/rechts (für Bewegung in X-Richtung)
        float winkelLinksRechts = GetSignedAngle(brett.localEulerAngles.z);   // Grad

        // Neigung vor/zurück (für Bewegung in Z-Richtung)
        float winkelVorZurueck  = -GetSignedAngle(brett.localEulerAngles.x);  // Grad, Vorzeichen umdrehen

        // 2. Hangabtriebskraft (Vektor in Brett-Ebene)
        Vector2 Fhang = Physik.berechneHangabtriebskraft(
            masse, gravitation,
            winkelLinksRechts,   // wirkt auf X
            winkelVorZurueck     // wirkt auf Z
        );
        
        Fhang = -Fhang;


        // 3. "Gesamtneigung" (für Normalkraft/Reibung – grobe Näherung)
        float gesamtWinkel = Mathf.Sqrt(
            winkelLinksRechts * winkelLinksRechts +
            winkelVorZurueck  * winkelVorZurueck
        );

        float Fn = Physik.berechneNormalenKraft(masse, gravitation, gesamtWinkel);
        float FrollBetrag = Physik.berechneRollReibungsKraft(
            rollReibungsKoef, masse, gravitation, gesamtWinkel);

        // 4. Reibung: entgegengesetzt zur Geschwindigkeit
        Vector2 Freib = Vector2.zero;
        if (velocity.sqrMagnitude > 0.0001f)
        {
            Vector2 reibDir = -velocity.normalized;
            Freib = reibDir * FrollBetrag;
        }

        // 5. Gesamtkraft & Beschleunigung
        Vector2 Fgesamt = Fhang + Freib;
        Vector2 a = Fgesamt / masse;

        // 6. Geschwindigkeit & Position updaten
        velocity += a * dt;
        position += velocity * dt;

        // TODO: später Kollisionen mit Wänden hier einbauen

        // 7. Auf Transform anwenden (X/Z -> Weltkoordinaten)
        transform.position = new Vector3(position.x, transform.position.y, position.y);
    }


    // Hilfsfunktion, um Winkel von 0..360 in -180..180 umzuwandeln
    private float GetSignedAngle(float angle)
    {
        if (angle > 180f)
            angle -= 360f;
        return angle;
    }
}

