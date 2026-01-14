using UnityEngine;
using System;

public static class Physik
{
    // ✅ Konstanten (kannst du anpassen)
    public const float HAFT_REIBUNG_KOEF = 0.30f; // µ_H
    public const float ROLL_REIBUNG_KOEF = 0.05f; // µ_R

    public static float berechneGewichtskraft(float masse, float gravitation)
        => masse * gravitation;

    // neigungsWinkelXGrad/YGrad: Grad
    public static Vector2 berechneHangabtriebskraft(float masse, float gravitation,
        float neigungsWinkelXGrad, float neigungsWinkelYGrad)
    {
        if (neigungsWinkelXGrad is > -90 and < 90 && neigungsWinkelYGrad is > -90 and < 90)
        {
            float fg = berechneGewichtskraft(masse, gravitation);

            float sx = Mathf.Sin(neigungsWinkelXGrad * Mathf.Deg2Rad);
            float sy = Mathf.Sin(neigungsWinkelYGrad * Mathf.Deg2Rad);

            return new Vector2(fg * sx, fg * sy);
        }
        throw new Exception("Das ist kein gueltiger Winkel!");
    }

    // alphaGrad: Gesamt-Neigungswinkel (Grad)
    public static float berechneNormalenKraft(float masse, float gravitation, float alphaGrad)
    {
        float fg = berechneGewichtskraft(masse, gravitation);
        return fg * Mathf.Cos(alphaGrad * Mathf.Deg2Rad);
    }

    public static float berechneHaftReibungsKraft(float haftReibungsKoefizient,
        float masse, float gravitation, float alphaGrad)
    {
        return haftReibungsKoefizient * berechneNormalenKraft(masse, gravitation, alphaGrad);
    }

    public static float berechneRollReibungsKraft(float rollReibungsKoefizient,
        float masse, float gravitation, float alphaGrad)
    {
        return rollReibungsKoefizient * berechneNormalenKraft(masse, gravitation, alphaGrad);
    }

    // 1D-Stoß (Normalrichtung): kehrt Vorzeichen um (perfekt elastisch ohne Verluste)
    public static float berechneStoßMitStarrenWand(float geschwindigkeitVorStoß)
        => -geschwindigkeitVorStoß;
}