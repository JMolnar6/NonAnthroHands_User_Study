using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StartButtonHandler : MonoBehaviour
{
    public Button m_PlayButton;
    public Button m_RecordButton;

    // Start is called before the first frame update
    void Start()
    {
        m_PlayButton.onClick.AddListener(TaskOnClickPlay);
        m_RecordButton.onClick.AddListener(TaskOnClickRecord);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void TaskOnClickPlay()
    {
        // 

    }
    private void TaskOnClickRecord()
    {
        // 

    }
}
