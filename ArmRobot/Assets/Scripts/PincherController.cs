using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum GripState { Fixed = 0, Opening = -1, Closing = 1 };

public class PincherController : MonoBehaviour
{
    public GameObject fingerA;
    public GameObject fingerB;
    public GameObject pinchTarget;

    //Used to visualize CurrentGraspCenter()
    public GameObject sphere;

    PincherFingerController fingerAController;
    PincherFingerController fingerBController;

    // Grip - the extent to which the pincher is closed. 0: fully open, 1: fully closed.
    public float grip;
    public float gripSpeed = 3.0f;
    public GripState gripState = GripState.Fixed;



    void Start()
    {
        fingerAController = fingerA.GetComponent<PincherFingerController>();
        fingerBController = fingerB.GetComponent<PincherFingerController>();
    }

    void FixedUpdate()
    {
        UpdateGrip();
        UpdateFingersForGrip();
        //Debug.Log("PincherController grip = " + CurrentGrip());

    }


    // READ

    public float CurrentGrip()
    {
        // TODO - we can't really assume the fingers agree, need to think about that
        float meanGrip = (fingerAController.CurrentGrip() + fingerBController.CurrentGrip()) / 2.0f;
        return meanGrip;
    }


    public Vector3 CurrentGraspCenter()
    {
        /* Gets the point directly between the middle of the pincher fingers,
         * in the global coordinate system.      
         */
        Vector3 localCenterPoint = (fingerAController.GetOpenPosition() + fingerBController.GetOpenPosition()) / 2.0f;
        Vector3 globalCenterPoint = transform.TransformPoint(localCenterPoint);
        sphere.transform.position = Vector3.MoveTowards(sphere.transform.position, globalCenterPoint, 1.0f * Time.deltaTime);

        return globalCenterPoint;
    }

    public bool IsOnCube()
    {
        /* Checks if the cube's position is between the fingers of the pincher */
        bool locatedOk = false;

        Vector3 pos1 = CurrentGraspCenter();
        Vector3 pos2 = pinchTarget.transform.position;

        if (V3Equal(pos1, pos2))
        {
            locatedOk = true;
        }
        return locatedOk;
    }


    // CONTROL

    public void ResetGripToOpen()
    {
        grip = 0.0f;
        fingerAController.ForceOpen(transform);
        fingerBController.ForceOpen(transform);
        gripState = GripState.Fixed;
    }


    // GRIP HELPERS
    void UpdateGrip()
    {
        if (gripState != GripState.Fixed)
        {
            float gripChange = (float)gripState * gripSpeed * Time.fixedDeltaTime;
            float gripGoal = CurrentGrip() + gripChange;
            grip = Mathf.Clamp01(gripGoal);
        }
    }

    void UpdateFingersForGrip()
    {
        fingerAController.UpdateGrip(grip);
        fingerBController.UpdateGrip(grip);
    }

    public void CloseGrip()
    {
        /* Changes gripState so that the fingers close */
        gripState = GripState.Closing;
    }

    public void FixGrip()
    {
        /* Changes gripState so that the fingers close */
        gripState = GripState.Fixed;
    }
    /*UNUSED*/
    public void OpenGrip()
    {
        /* Changes gripState so that the fingers open */
        gripState = GripState.Opening;
    }

    bool V3Equal(Vector3 pos1, Vector3 pos2)
    {
        //Debug.Log("Pos1: " + pos1);
        //Debug.Log("Pos2: " + pos2);

        /* Compares if both positions are the same */
        return Vector3.SqrMagnitude(pos1 - pos2) < 0.01;

        //return Vector3.Distance(pos1, pos2) < 0.01;
    }

    public bool LocatedIn2D(Vector3 pos1, Vector3 pos2, float allowedDiff)
    {
        var dx = Mathf.Abs(pos1.x - pos2.x);
        if (dx > allowedDiff)
            return false;
        var dz = Mathf.Abs(pos1.z - pos2.z);
        if (dz > allowedDiff)
            return false;
        //var dy = Mathf.Abs(pos1.y - pos2.y);
        //return dy < allowedDiff;
        return true;
    }
}
