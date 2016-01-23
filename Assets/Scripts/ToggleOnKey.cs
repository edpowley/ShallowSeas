using UnityEngine;
using System.Collections;

public class ToggleOnKey : MonoBehaviour
{
    public KeyCode m_key;
    private bool m_enabled;
    public bool m_startEnabled;

    void Start()
    {
        setEnabled(m_startEnabled);
    }

    private void setEnabled(bool enabled)
    {
        m_enabled = enabled;

        for(int i=0;i< transform.childCount; i++)
        {
            Transform child = transform.GetChild(i);
            child.gameObject.SetActive(enabled);
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(m_key))
            setEnabled(!m_enabled);
    }
}
