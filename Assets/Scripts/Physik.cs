using System;
using UnityEngine;

public class Physik
{
    public static float berechneGewichtskraft(float masse, float gravitation)
    {
        return masse * gravitation;
    }

    public static Vector2 berechneHangabtriebskraft(float masse, float gravitation, float neigungsWinkelX, float neigungsWinkelY)
    {
        if (neigungsWinkelX is > -90 and < 90 && neigungsWinkelY is > -90 and < 90)
        {
            return new Vector2((float)(berechneGewichtskraft(masse, gravitation) * Math.Sin(neigungsWinkelX)), 
                (float)(berechneGewichtskraft(masse, gravitation) * Math.Sin(neigungsWinkelY)));
        }
        throw new Exception("Das ist kein gueltiger Winkel!");
    }

    public static float berechneNormalenKraft(float masse, float gravitation, float gesamtNeigungsWinkelSchiefeEbene)
    {
        return (float)(berechneGewichtskraft(masse, gravitation) * Math.Sin(gesamtNeigungsWinkelSchiefeEbene));
    }

    public static float berechneHaftReibungsKraft(float haftReibungsKoefiziente, float masse, float gravitation, float gesamtNeigungsWinkelSchiefeEbene)
    {
        return haftReibungsKoefiziente * berechneNormalenKraft(masse, gravitation, gesamtNeigungsWinkelSchiefeEbene);
    }
    
    public static float berechneRollReibungsKraft(float rollReibungsKoefiziente, float masse, float gravitation, float gesamtNeigungsWinkelSchiefeEbene)
    {
        return rollReibungsKoefiziente * berechneNormalenKraft(masse, gravitation, gesamtNeigungsWinkelSchiefeEbene);
    }

    public static float berechneStoßMitStarrenWand(float geschwindigkeitVorStoß)
    {
        return -geschwindigkeitVorStoß;
    }
}
