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
    [HideInInspector] public GameObject currentDrag;

 

    public static GameManager instance;
    public string GenerateRandomKey(int length = 8)
    {
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
        System.Random random = new System.Random();
        char[] result = new char[length];

        for (int i = 0; i < length; i++)
        {
            result[i] = chars[random.Next(chars.Length)];
        }

        return new string(result);
    }

    private void Awake()
    {
        instance = this;
    }

    private void Update()
    {
    }

}
