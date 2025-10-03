using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VehicleController : MonoBehaviour
{
    Rigidbody m_rigidbody;
    public float speed = 1f;
    public float turn = 1f;
    private void Awake()
    {
        m_rigidbody = gameObject.GetComponent<Rigidbody>();
    }

    void FixedUpdate()
    {
        if (Input.GetKey(KeyCode.W))
        {
            m_rigidbody.AddForce(transform.forward * speed);
        }
        else if (Input.GetKey(KeyCode.S))
        {
            m_rigidbody.AddForce(-transform.forward * speed);
        }
        if (Input.GetKey(KeyCode.A))
        {
            m_rigidbody.rotation *= Quaternion.Euler(-transform.up * turn);
        }
        if (Input.GetKey(KeyCode.D))
        {
            m_rigidbody.rotation *= Quaternion.Euler(transform.up * turn);
        }
    }
}
