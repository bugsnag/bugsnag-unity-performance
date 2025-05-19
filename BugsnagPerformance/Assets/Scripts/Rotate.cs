using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rotate : MonoBehaviour
{

    public float speed = 10f;
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        var rotation = transform.rotation.eulerAngles;
        rotation.y += Time.deltaTime * speed;
        transform.rotation = Quaternion.Euler(rotation);
    }
}
