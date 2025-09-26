using Fusion;
using Fusion.Sockets;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.CryptoKartz.Scripts
{
    public class CarController : NetworkBehaviour
    {

        private float steeringInput;
        private float throttleInput;
        private float currentSteerAngle;

        [SerializeField] private float motorForce;
        [SerializeField] private float breakForce;
        [SerializeField] private float maxSteerAngle;

        [SerializeField] private WheelCollider frontLeftWheelCollider;
        [SerializeField] private WheelCollider frontRightWheelCollider;
        [SerializeField] private WheelCollider rearLeftWheelCollider;
        [SerializeField] private WheelCollider rearRightWheelCollider;

        [SerializeField] private Transform frontLeftWheelTransform;
        [SerializeField] private Transform frontRightWheeTransform;
        [SerializeField] private Transform rearLeftWheelTransform;
        [SerializeField] private Transform rearRightWheelTransform;

       

        public override void Spawned()
        {
        }

        /// <summary>
        /// Fixed update network.
        /// </summary>
        public override void FixedUpdateNetwork()
        {
            if (GetInput<Player.PlayerInputProvider.CarInput>(out var input) == false) return;

            throttleInput = input.carControlValue.y;
            steeringInput = input.carControlValue.x;

            HandleMotor();
            HandleSteering();
            UpdateWheels();

            if(transform.position.y < -.1)
            {
                transform.position = new Vector3(0, 0, 0);
                transform.rotation = Quaternion.identity;
            }
        }

        private void HandleMotor()
        {
            frontLeftWheelCollider.motorTorque = throttleInput * motorForce;
            frontRightWheelCollider.motorTorque = throttleInput * motorForce;

        }

        private void HandleSteering()
        {
            currentSteerAngle = maxSteerAngle * steeringInput;
            frontLeftWheelCollider.steerAngle = currentSteerAngle;
            frontRightWheelCollider.steerAngle = currentSteerAngle;

        }

        private void UpdateWheels()
        {
            UpdateSingleWheel(frontLeftWheelCollider, frontLeftWheelTransform);
            UpdateSingleWheel(frontRightWheelCollider, frontRightWheeTransform);
            UpdateSingleWheel(rearRightWheelCollider, rearRightWheelTransform);
            UpdateSingleWheel(rearLeftWheelCollider, rearLeftWheelTransform);
        }

        private void UpdateSingleWheel(WheelCollider wheelCollider, Transform wheelTransform)
        {
            Vector3 pos;
            Quaternion rot; 
            wheelCollider.GetWorldPose(out pos, out rot);
            //wheelTransform.rotation = rot;
            //wheelTransform.position = pos; 
            wheelTransform.localRotation = rot;
            wheelTransform.localPosition = pos;
        }

    }

}
