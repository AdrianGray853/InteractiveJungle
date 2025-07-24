using UnityEngine;

public class GameManager : MonoBehaviour
{
   [HideInInspector] public House house;
    public Background background;
    public Animals animal;
    public Birds birds;
    [HideInInspector] public Corals corals;
    public Decoration decoration;
    [HideInInspector] public Vehicle vehicle;

 

    public static GameManager instance;

    private void Awake()
    {
        instance = this;
    }

    private void Update()
    {
    }

}
