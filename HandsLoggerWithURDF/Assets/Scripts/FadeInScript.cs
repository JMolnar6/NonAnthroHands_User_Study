using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FadeInScript : MonoBehaviour
{
    MeshRenderer rend;

    // Start is called before the first frame update
    void Start()
    {
        rend = GetComponent<MeshRenderer>();   
        Color c = rend.material.color;
        c.a = 0f;
        rend.material.color = c;

    } 

    // Update is called once per frame
    void Update()
    {
        
    }

    IEnumerator FadeIn(){
        for (float f = 0.1f; f<=1f; f+=0.1f){
            Color c = rend.material.color;
            c.a = f;
            rend.material.color = c;
            yield return(new WaitForSecondsRealtime(0.05f));
        }
    }

    public void StartFading(){
        StartCoroutine(FadeIn());
    }
}
