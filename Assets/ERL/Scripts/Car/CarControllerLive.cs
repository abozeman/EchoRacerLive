using Assets.CryptoKartz.Scripts.Managers;
using Fusion;
using Fusion.Sockets;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Assets.CryptoKartz.Scripts.CarController;

namespace Assets.CryptoKartz.Scripts
{
    public class CarControllerLive : NetworkBehaviour, INetworkRunnerCallbacks
    {

        private float steeringInput;
        private float throttleInput;
        [SerializeField] private CarManager _carManager;

        private float currentSteerAngle;
        private float currentbreakForce;
        private bool zeroInputDetected;

        
        /*public struct CarInput : INetworkInput
        {
            public Vector2 steeringValue;
        }*/

        public override void Spawned()
        {
            Object.Runner.AddCallbacks(this);
            zeroInputDetected = false;
        }

        /// <summary>
        /// Fixed update network.
        /// </summary>
        public override void FixedUpdateNetwork()
        {

            if (GetInput<CarInput>(out var input) == false) return;

            throttleInput = input.throttleValue;
            steeringInput = input.steeringValue.x;

            //Debug.Log($"throttleInput : {throttleInput}");
            //Debug.Log($"steeringInput : {steeringInput}");

            //We need to not send zero values to the car controller after we set a zeroInputDetected flag.
            if(throttleInput == 0 && steeringInput == 0 && !zeroInputDetected)
            {
                zeroInputDetected = true;
                _carManager.setControl(steeringInput, throttleInput);
            }
            else if ((throttleInput != 0 || steeringInput != 0) && zeroInputDetected)
            {
                zeroInputDetected = false;
                _carManager.setControl(steeringInput, throttleInput);

            } else if (throttleInput != 0 || steeringInput != 0)
            {
                _carManager.setControl(steeringInput, throttleInput);
            }


        }

        public void OnPlayerJoined(NetworkRunner runner, PlayerRef player)
        {
            if (Object.Runner.IsServer && !Object.InputAuthority.IsRealPlayer) { Object.AssignInputAuthority(player); }

        }

        public void OnPlayerLeft(NetworkRunner runner, PlayerRef player)
        {
            Object.AssignInputAuthority(PlayerRef.None);
        }



        #region Unused Callbacks

        public void OnObjectExitAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player)
        {

        }

        public void OnObjectEnterAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player)
        {

        }

        

        public void OnInput(NetworkRunner runner, NetworkInput input)
        {

        }

        public void OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input)
        {

        }

        public void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason)
        {

        }

        public void OnConnectedToServer(NetworkRunner runner)
        {

        }

        public void OnDisconnectedFromServer(NetworkRunner runner, NetDisconnectReason reason)
        {

        }

        public void OnConnectRequest(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request, byte[] token)
        {

        }

        public void OnConnectFailed(NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason)
        {

        }

        public void OnUserSimulationMessage(NetworkRunner runner, SimulationMessagePtr message)
        {

        }

        public void OnSessionListUpdated(NetworkRunner runner, List<SessionInfo> sessionList)
        {

        }

        public void OnCustomAuthenticationResponse(NetworkRunner runner, Dictionary<string, object> data)
        {

        }

        public void OnHostMigration(NetworkRunner runner, HostMigrationToken hostMigrationToken)
        {

        }

        public void OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ReliableKey key, ArraySegment<byte> data)
        {

        }

        public void OnReliableDataProgress(NetworkRunner runner, PlayerRef player, ReliableKey key, float progress)
        {

        }

        public void OnSceneLoadDone(NetworkRunner runner)
        {

        }

        public void OnSceneLoadStart(NetworkRunner runner)
        {

        }

        #endregion
    }

}
