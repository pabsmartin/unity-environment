using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PinchDetector : MonoBehaviour
{
    public string pinchTargetTag;
    public bool hasPinchedTarget = false;
    public GameObject hand;

    PincherController pincherController;

    int touching = 0;

    void Start()
    {
        pincherController = hand.GetComponent<PincherController>();
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.transform.gameObject.tag == pinchTargetTag)
        {
            Debug.Log("Touching with " + collision.transform.name);
            touching++;
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        if(collision.transform.gameObject.tag == pinchTargetTag)
        {
            Debug.Log("Not touching anymore with "+ collision.transform.name);
            touching--;
        }
    }

    public void HasPinched()
    {
        if (touching == 2 && pincherController.IsOnCube() && pincherController.grip > 0)
            hasPinchedTarget = true;
    }
}
