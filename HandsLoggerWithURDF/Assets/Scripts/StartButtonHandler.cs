using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StartButtonHandler : MonoBehaviour
{
    public Button m_StartButton;

    // Start is called before the first frame update
    void Start()
    {
        m_StartButton.onClick.AddListener(TaskOnClick);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void TaskOnClick()
    {
        Button.Destroy(m_StartButton); //Should make the button disappear after being clicked

    }
}
