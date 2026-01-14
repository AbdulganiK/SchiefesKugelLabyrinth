using UnityEngine;

public class Physik
{
    public static float berechneGewichtskraft(float masse, float gravitation)
    {
        return masse * gravitation;
    }

    public static Vector2 berechneHangabtriebskraft(float masse, float gravitation,
        float neigungsWinkelXGrad, float neigungsWinkelYGrad)
    {
        if (neigungsWinkelXGrad is > -90 and < 90 && neigungsWinkelYGrad is > -90 and < 90)
        {
            float gx = Mathf.Sin(neigungsWinkelXGrad * Mathf.Deg2Rad);
            float gy = Mathf.Sin(neigungsWinkelYGrad * Mathf.Deg2Rad);

            float fg = berechneGewichtskraft(masse, gravitation);

            return new Vector2(fg * gx, fg * gy);
        }

        throw new System.Exception("Das ist kein gueltiger Winkel!");
    }

    // Normalkraft: Fn = m * g * cos(alpha)
    public static float berechneNormalenKraft(float masse, float gravitation, float gesamtNeigungsWinkelGrad)
    {
        float alpha = gesamtNeigungsWinkelGrad * Mathf.Deg2Rad;
        return berechneGewichtskraft(masse, gravitation) * Mathf.Cos(alpha);
    }

    public static float berechneHaftReibungsKraft(float haftReibungsKoefizient,
        float masse, float gravitation, float gesamtNeigungsWinkelGrad)
    {
        return haftReibungsKoefizient *
               berechneNormalenKraft(masse, gravitation, gesamtNeigungsWinkelGrad);
    }

    public static float berechneRollReibungsKraft(float rollReibungsKoefizient,
        float masse, float gravitation, float gesamtNeigungsWinkelGrad)
    {
        return rollReibungsKoefizient *
               berechneNormalenKraft(masse, gravitation, gesamtNeigungsWinkelGrad);
    }

    // 1D-Stoß: Vorzeichen umdrehen
    public static float berechneStossMitStarrerWand(float geschwindigkeitVorStoss)
    {
        return -geschwindigkeitVorStoss;
    }
}