using UnityEngine;

public class ForceOrientation : MonoBehaviour
{
    public enum OrientacaoDesejada { Retrato, Paisagem }
    public OrientacaoDesejada orientacaoDaCena;

    void Awake()
    {
        switch (orientacaoDaCena)
        {
            case OrientacaoDesejada.Retrato:
                Screen.orientation = ScreenOrientation.Portrait;
                break;
            case OrientacaoDesejada.Paisagem:
                Screen.orientation = ScreenOrientation.LandscapeLeft;
                break;
        }
    }
}