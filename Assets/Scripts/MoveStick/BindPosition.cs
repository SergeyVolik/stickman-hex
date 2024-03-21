using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BindPosition : MonoBehaviour
{
    public Transform from;
    public Transform to;

    private void Update()
    {
        to.position = from.position;
    }
}
