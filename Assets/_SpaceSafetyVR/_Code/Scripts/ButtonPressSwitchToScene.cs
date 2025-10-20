using UnityEngine;
using UnityEngine.SceneManagement;

public class ButtonPressSwitchToScene : MonoBehaviour
{

    public string debrisScene = "3_SpaceDebris_only";
    public string asteroidScene = "4_Asteroid_only";
    public string CMEScene = "2_CME_only_p1";

    public void SwitchToDebris()
    {
        SceneManager.LoadScene(debrisScene, LoadSceneMode.Single);
    }
    
    public void SwitchToAsteroid()
    {
        SceneManager.LoadScene(asteroidScene, LoadSceneMode.Single);
    }
    
    public void SwitchToCME()
    {
        SceneManager.LoadScene(CMEScene, LoadSceneMode.Single);
    }
}
