using UnityEngine;
using System.Collections;

public class BoatRocking : MonoBehaviour
{
    public enum TargetProperty { Position, Rotation };
    public enum TargetAxis { X, Y, Z };

    public TargetProperty m_targetProperty;
    public TargetAxis m_targetAxis;
    public float m_centre;
    public float m_amount;
    public float m_speed;

    // Update is called once per frame
    void Update()
    {
        float value = m_centre + Mathf.Sin(Time.time * m_speed) * m_amount;

        Vector3 vector = Vector3.zero;
        switch (m_targetProperty)
        {
            case TargetProperty.Position:
                vector = transform.position;
                break;

            case TargetProperty.Rotation:
                vector = transform.rotation.eulerAngles;
                break;
        }

        switch(m_targetAxis)
        {
            case TargetAxis.X: vector.x = value; break;
            case TargetAxis.Y: vector.y = value; break;
            case TargetAxis.Z: vector.z = value; break;
        }

        switch (m_targetProperty)
        {
            case TargetProperty.Position:
                transform.position = vector;
                break;

            case TargetProperty.Rotation:
                transform.rotation = Quaternion.Euler(vector);
                break;
        }
    }
}
