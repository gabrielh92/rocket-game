using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Oscillator : MonoBehaviour
{
    [SerializeField] Vector3 movementVector = new Vector3(10f, 10f, 10f);
    [SerializeField] float period = 2f;

    Vector3 startPos;
    void Start()
    {
        startPos = transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        if(period <= Mathf.Epsilon) return;
        float cycles = Time.time / period;
        float rawSin = Mathf.Sin(2 * Mathf.PI * cycles);
        float movementSpeed = rawSin / 2f + 0.5f;
        Vector3 offset = movementSpeed * movementVector;
        transform.position = startPos + offset;
    }
}
