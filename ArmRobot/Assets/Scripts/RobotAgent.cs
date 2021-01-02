using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using System;

public class RobotAgent : Agent
{
    public GameObject endEffector;
    public GameObject cube;
    public GameObject robot;

    RobotController robotController;
    TouchDetector touchDetector;
    TablePositionRandomizer tablePositionRandomizer;

    PincherController pincherController;
    PinchDetector pinchDetector;


    void Start()
    {
        robotController = robot.GetComponent<RobotController>();
        touchDetector = cube.GetComponent<TouchDetector>();
        tablePositionRandomizer = cube.GetComponent<TablePositionRandomizer>();
        
        pinchDetector = cube.GetComponent<PinchDetector>();
        pincherController = endEffector.GetComponent<PincherController>();
    }


    // AGENT

    public override void OnEpisodeBegin()
    {
        //float[] defaultRotations = { 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f };
        float[] defaultRotations = { 0.0f, 0.0f, -91.5f, 0.0f, 40.8f, -90.0f, 0.0f };
        robotController.ForceJointsToRotations(defaultRotations);
        touchDetector.hasTouchedTarget = false;
        pinchDetector.hasPinchedTarget = false; // Added
        tablePositionRandomizer.Move();

        Debug.Log(Vector3.Distance(robot.transform.position, pincherController.CurrentGraspCenter()));
        Debug.Log("CurrentGraspCenter: " + pincherController.CurrentGraspCenter());
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        if (robotController.joints[0].robotPart == null)
        {
            // No robot is present, no observation should be added
            return;
        }

        // relative cube position
        Vector3 cubePosition = cube.transform.position - robot.transform.position;
        sensor.AddObservation(cubePosition);

        // relative end position
        Vector3 endPosition = endEffector.transform.position - robot.transform.position;
        sensor.AddObservation(endPosition);
        sensor.AddObservation(cubePosition - endPosition);
    }

    public override void OnActionReceived(float[] vectorAction)
    {
        //Debug.Log(vectorAction.Length);
        // move
        for (int jointIndex = 0; jointIndex < vectorAction.Length - 1; jointIndex++)
        {
            //Debug.Log(vectorAction[jointIndex]); 
            
            RotationDirection rotationDirection = ActionIndexToRotationDirection((int) vectorAction[jointIndex]);
            robotController.RotateJoint(jointIndex, rotationDirection, false);
            

        }
       
        // grip
        if(vectorAction[vectorAction.Length-1] == 1)
        {
            pincherController.CloseGrip();
        }
        else if(vectorAction[vectorAction.Length-1] == 2)
        {
            pincherController.OpenGrip();
        }
        else if(vectorAction[vectorAction.Length-1] == 0) 
        {
            pincherController.FixGrip();
        }

        /*
         * Energy used penalization
         * Used every 2 trainings to compare performance
         */
        AddReward(-0.001f);

        /* DISTANCE BASED REWARD */
        //float distanceToCube = Vector3.Distance(endEffector.transform.position, cube.transform.position); // roughly 0.7f
        float distanceToCube = Vector3.Distance(pincherController.CurrentGraspCenter(), cube.transform.position);

        var jointHeight = 0f; // This is to reward the agent for keeping high up // max is roughly 3.0f
        for (int jointIndex = 0; jointIndex < robotController.joints.Length; jointIndex++)
        {
            jointHeight += robotController.joints[jointIndex].robotPart.transform.position.y - cube.transform.position.y;
        }
        var reward = -distanceToCube + jointHeight / 100f;

        //AddReward(reward * 0.01f);
        SetReward(reward * 0.1f);

        // Knocked the cube off the table
        if (cube.transform.position.y < 0.5f)
        {
            Debug.Log("Cube off the table");
            SetReward(-1f);
            EndEpisode();
        }

        if (touchDetector.hasTouchedTarget)
        {
            AddReward(0.05f);
        }

        //if (pincherController.LocatedIn2D(cube.transform.position, pincherController.CurrentGraspCenter(), 0.01f))
        //{
        //    AddReward(0.1f);
        //    EndEpisode();
        //}

        if (pinchDetector.hasPinchedTarget)
        {
            Debug.Log("Target pinched");
            SetReward(1.0f);
            EndEpisode();
        }
    }

    // HELPERS

    static public RotationDirection ActionIndexToRotationDirection(int actionIndex)
    {
        return (RotationDirection)(actionIndex - 1);
    }
}