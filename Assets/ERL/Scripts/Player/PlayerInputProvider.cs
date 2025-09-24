using Fusion;
using System;
using System.Collections.Generic;
using Fusion.Sockets;
using static Assets.CryptoKartz.Scripts.CarController;

namespace Assets.CryptoKartz.Scripts.Player
{
    public class PlayerInputProvider : NetworkBehaviour, INetworkRunnerCallbacks, IBeforeUpdate
    {

        // Local variable to store the input polled.
        CarInput carInput = new CarInput();

        public override void Spawned()
        {
            Object.Runner.AddCallbacks(this);
        }

        public void BeforeUpdate()
        {
            OVRInput.FixedUpdate();
            carInput.steeringValue = OVRInput.Get(OVRInput.Axis2D.SecondaryThumbstick);
            carInput.throttleValue = OVRInput.Get(OVRInput.Axis1D.PrimaryIndexTrigger);

            if (OVRInput.Get(OVRInput.Axis1D.PrimaryHandTrigger) > .5f) carInput.throttleValue *= -1;

        }

        void INetworkRunnerCallbacks.OnInput(NetworkRunner runner, NetworkInput input)
        {
            if(Object.HasInputAuthority)
            {
                input.Set(carInput);

                var carOutput = new CarInput();
                input.TryGet(out carOutput);
                //Debug.Log($"carOutput (x,y): ({carOutput.steeringValue.x},{carOutput.steeringValue.y}) ");
                //carInput = default;
            }

        }

        #region Unused Callbacks

        void INetworkRunnerCallbacks.OnObjectExitAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player)
        {
            
        }

        void INetworkRunnerCallbacks.OnObjectEnterAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player)
        {
            
        }

        void INetworkRunnerCallbacks.OnPlayerJoined(NetworkRunner runner, PlayerRef player)
        {
            
        }

        void INetworkRunnerCallbacks.OnPlayerLeft(NetworkRunner runner, PlayerRef player)
        {
            
        }

        void INetworkRunnerCallbacks.OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input)
        {
            
        }

        void INetworkRunnerCallbacks.OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason)
        {
            
        }

        void INetworkRunnerCallbacks.OnConnectedToServer(NetworkRunner runner)
        {
            
        }

        void INetworkRunnerCallbacks.OnDisconnectedFromServer(NetworkRunner runner, NetDisconnectReason reason)
        {
            
        }

        void INetworkRunnerCallbacks.OnConnectRequest(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request, byte[] token)
        {
            
        }

        void INetworkRunnerCallbacks.OnConnectFailed(NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason)
        {
            
        }

        void INetworkRunnerCallbacks.OnUserSimulationMessage(NetworkRunner runner, SimulationMessagePtr message)
        {
            
        }

        void INetworkRunnerCallbacks.OnSessionListUpdated(NetworkRunner runner, List<SessionInfo> sessionList)
        {
            
        }

        void INetworkRunnerCallbacks.OnCustomAuthenticationResponse(NetworkRunner runner, Dictionary<string, object> data)
        {
            
        }

        void INetworkRunnerCallbacks.OnHostMigration(NetworkRunner runner, HostMigrationToken hostMigrationToken)
        {
            
        }

        void INetworkRunnerCallbacks.OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ReliableKey key, ArraySegment<byte> data)
        {
            
        }

        void INetworkRunnerCallbacks.OnReliableDataProgress(NetworkRunner runner, PlayerRef player, ReliableKey key, float progress)
        {
            
        }

        void INetworkRunnerCallbacks.OnSceneLoadDone(NetworkRunner runner)
        {
            
        }

        void INetworkRunnerCallbacks.OnSceneLoadStart(NetworkRunner runner)
        {
            
        }

        #endregion
    }
}
