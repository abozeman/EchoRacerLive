using System.Collections.Generic;
using UnityEngine;
using Fusion.Sockets;
using System;
using Fusion;
using M2MqttUnity;
using System.Collections;
using Newtonsoft.Json;

namespace cryptokartz.Scripts.GameControllers
{

    [SimulationBehaviour(Modes = SimulationModes.Server)]
    public class ServerCKMRLiveGameController : Fusion.SimulationBehaviour, INetworkRunnerCallbacks
    {

        [SerializeField] private NetworkObject _playerPrefab;
        [SerializeField] private NetworkObject _liveCarPrefab;
        private readonly Dictionary<PlayerRef, NetworkObject> _playerMap = new Dictionary<PlayerRef, NetworkObject>();
        private readonly Dictionary<PlayerRef, NetworkObject> _liveCarMap = new Dictionary<PlayerRef, NetworkObject>();
        private readonly Dictionary<PlayerRef, NetworkObject> _simulatedCarMap = new Dictionary<PlayerRef, NetworkObject>();


        private int _playerId;
        private int _playerCount;
        private PlayerRef _player;


        public SessionProperty TrackId { get; private set; }

        void INetworkRunnerCallbacks.OnPlayerJoined(NetworkRunner runner, PlayerRef player)
        {

            Debug.Log($"Entered OnPlayerJoined");


            if (runner.IsServer && _playerPrefab != null)
            {
                Debug.Log($"runner.IsServer && _playerPrefab != null");

                _playerId = player.PlayerId;
                _playerCount = Runner.SessionInfo.PlayerCount - 1;
                _player = player;
                TrackId = Runner.SessionInfo.Properties["trackid"];

                Debug.Log($"TrackId : { TrackId}");
                Debug.Log($"_playerCount : {_playerCount}");
                Debug.Log($"_playerId : {_playerId}");


                if (_playerCount == 0) return;

                NetworkObject character;
                NetworkObject liveCar;
                NetworkObject simulatedCar;

                //Debug.Log($"_playerId: {_playerId}");
                //Debug.Log($"PlayerCount: {_playerCount}");
                //Debug.Log($"TrackId: {TrackId.PropertyValue.ToString()}");

                character = grlAvatarSpawn(_playerPrefab, player);
                liveCar = grlLiveCarSpawn(_liveCarPrefab, player);
                runner.SetPlayerObject(player, character);
                _playerMap[player] = character;
                _liveCarMap[player] = liveCar;


                Log.Info($"Spawn for Player: {player}");


            }

        }

        private NetworkObject grlAvatarSpawn(NetworkObject _objPrefab, PlayerRef player)
        {
            return Runner.Spawn(
                _objPrefab,
                Vector3.zero,
                Quaternion.identity,
                inputAuthority: player,
                InitializeAvatarBeforeSpawn
                );
        }

        private NetworkObject grlLiveCarSpawn(NetworkObject _objPrefab, PlayerRef player)
        {
            return Runner.Spawn(
                _objPrefab,
                Vector3.zero,
                Quaternion.identity,
                inputAuthority: player,
                InitializeLiveCarBeforeSpawn
                );
        }

        private NetworkObject grlTrackSpawn(NetworkObject _trackPrefab, PlayerRef player)
        {
            return Runner.Spawn(
                _trackPrefab,
                Vector3.zero,
                Quaternion.identity,
                inputAuthority: player,
                InitializeTrackBeforeSpawn
                );
        }

        private void InitializeAvatarBeforeSpawn(NetworkRunner runner, NetworkObject obj)
        {
        }

        private void InitializeTrackBeforeSpawn(NetworkRunner runner, NetworkObject obj)
        {
        }
        private void InitializeLiveCarBeforeSpawn(NetworkRunner runner, NetworkObject obj)
        {
        }

        void INetworkRunnerCallbacks.OnPlayerLeft(NetworkRunner runner, PlayerRef player)
        {
            if (_playerMap.TryGetValue(player, out var character))
            {
                // Despawn Player
                runner.Despawn(character);

                // Remove player from mapping
                _playerMap.Remove(player);

                Log.Info($"Despawn for Player: {player}");
            }

            if (_playerMap.Count == 0)
            {
                Log.Info("Last player left, shutdown...");
                // Shutdown Server after the last player leaves
                //runnerServer.Shutdown();
            }
        }

        #region Unused Callbacks


        /// <summary>
        /// On user simulation message.
        /// </summary>
        /// <param name="runner">The runnerServer.</param>
        /// <param name="message">The message.</param>
        public void OnUserSimulationMessage(NetworkRunner runner, SimulationMessagePtr message) { }

        void INetworkRunnerCallbacks.OnObjectExitAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player)
        {
        }

        void INetworkRunnerCallbacks.OnObjectEnterAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player)
        {

        }



        void INetworkRunnerCallbacks.OnInput(NetworkRunner runner, NetworkInput input)
        {

        }

        void INetworkRunnerCallbacks.OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input)
        {

        }

        void INetworkRunnerCallbacks.OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason)
        {
            Application.Quit(0);

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

        public void OnSessionListUpdated(NetworkRunner runner, List<SessionInfo> sessionList)
        {
        }
        #endregion
    }
}