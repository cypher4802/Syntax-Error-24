using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Button : MonoBehaviour
{
  
    public void Onclick()
    {
        SceneManager.LoadScene(2);
    }
    public void Onclick1() 
    { 
        SceneManager.LoadScene(0);
    }  
}
