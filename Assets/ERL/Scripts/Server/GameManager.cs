using System.Collections.Generic;
using UnityEngine;
using Fusion.Sockets;
using System;
using cryptokartz.Scripts.Player;
using Fusion;
using M2MqttUnity;
using System.Collections;
using Newtonsoft.Json;
using uPLibrary.Networking.M2Mqtt.Messages;

namespace cryptokartz.Scripts.GameControllers
{

    [SimulationBehaviour(Modes = SimulationModes.Server)]
    public class GameManager : M2MqttUnityClientNetwork, INetworkRunnerCallbacks
    {

        [SerializeField] private NetworkObject _playerPrefab;
        [SerializeField] private NetworkObject _carPrefab;
        [SerializeField] private List<NetworkObject> _carPrefabs = new List<NetworkObject>();
        [SerializeField] private List<NetworkObject> _trackPrefabs = new List<NetworkObject>();
        private readonly Dictionary<PlayerRef, NetworkObject> _playerMap = new Dictionary<PlayerRef, NetworkObject>();
        private readonly Dictionary<PlayerRef, NetworkObject> _playerCarMap = new Dictionary<PlayerRef, NetworkObject>();
        private readonly Dictionary<string, NetworkObject> _playerCarTagMap = new Dictionary<string, NetworkObject>();
        private Dictionary<PlayerRef, PlayerDataNetwork> _playerDataMap = new Dictionary<PlayerRef, PlayerDataNetwork>();
        private List<string> eventMessages = new List<string>();


        private int _playerId;
        private int _playerCount;
        private PlayerRef _player;


        public SessionProperty TrackId { get; private set; }


        #region Publish SessionInfo

        public IEnumerator SessionInfoPublish(SessionInfo sessionInfo)
        {
            var jsonSessionInfo = JsonConvert.SerializeObject(sessionInfo);
            client.Publish(string.Format($"ckgame.sessioninfo.{sessionInfo.Name}"), System.Text.Encoding.UTF8.GetBytes(jsonSessionInfo));
            yield return new WaitForSecondsRealtime(.033f);
        }

        public IEnumerator SessionInfoRemove(SessionInfo sessionInfo)
        {
            var jsonSessionInfo = JsonConvert.SerializeObject(sessionInfo);
            client.Publish(string.Format($"ckgame.sessioninfo.remove.{sessionInfo.Name}"), System.Text.Encoding.UTF8.GetBytes(jsonSessionInfo));
            yield return new WaitForSecondsRealtime(.033f);
        }

        void INetworkRunnerCallbacks.OnSessionListUpdated(NetworkRunner runner, List<SessionInfo> sessionList)
        {
            if (runner.IsServer)
            {
                Log.Info($"sessionList: {sessionList}");
            }


        }


        #endregion

        #region MQTT Client

        #region Broker Settings
        /// <summary>
        /// Set ClientId.
        /// </summary>
        /// <param name="clientId">The clientId.</param>
        public void SetClientId(string clientId)
        {
            this.clientId = clientId;
        }
        /// <summary>
        /// Set broker address.
        /// </summary>
        /// <param name="brokerAddress">The broker address.</param>
        public void SetBrokerAddress(string brokerAddress)
        {
            this.brokerAddress = brokerAddress;
        }

        /// <summary>
        /// Set broker port.
        /// </summary>
        /// <param name="brokerPort">The broker port.</param>
        public void SetBrokerPort(string brokerPort)
        {
            int.TryParse(brokerPort, out this.brokerPort);
        }

        /// <summary>
        /// Set the encrypted.
        /// </summary>
        /// <param name="isEncrypted">If true, is encrypted.</param>
        public void SetEncrypted(bool isEncrypted)
        {
            this.isEncrypted = isEncrypted;
        }
        #endregion

        #region Connection Methods
        protected override void OnConnecting()
        {
            base.OnConnecting();
            Debug.Log("Connecting to broker on " + brokerAddress + ":" + brokerPort.ToString() + "...\n");
        }

        protected override void OnConnected()
        {
            base.OnConnected();
            Debug.Log("Connected to broker on " + brokerAddress + "\n");
        }

        protected override void OnConnectionFailed(string errorMessage)
        {
            Debug.Log("CONNECTION FAILED! " + errorMessage);
        }

        protected override void OnDisconnected()
        {
            Debug.Log("Disconnected.");
        }

        protected override void OnConnectionLost()
        {
            Debug.Log("CONNECTION LOST!");
        }
        #endregion

        #region Subscription/Unsubscription
        protected override void SubscribeTopics()
        {
            client.Subscribe(new string[] { string.Format("game/manager/*", clientId) }, new byte[] { MqttMsgBase.QOS_LEVEL_EXACTLY_ONCE });
        }

        protected override void UnsubscribeTopics()
        {
            client.Unsubscribe(new string[] { string.Format("game/manager/{0}", clientId) });
        }

        #endregion

        #endregion

        protected override void DecodeMessage(string topic, byte[] message)
        {
            try
            {
                string msg = System.Text.Encoding.UTF8.GetString(message);
                //string msg = "{"type": "1", "vid": "grlv0telemetry", "posX": "0.85", "posZ": "-0.018", "velX": "-0.0", "velZ": "-0.003", "rotW": "0.987", "rotX": "-0.117", "rotY": "0.014", "rotZ": "0.105", "strAngle": "0.0", "strThrottle": "0.0"}"
                //Debug.Log("msg: " + msg);
                if (topic.Contains("game/manager/spawncar"))
                {
                    grlCarSpawn(_carPrefab);

                }



                StoreMessage(msg);
            }
            catch (Exception)
            {
                //Debug.Log("EXCEPTION: " + e.Message);
            }

        }

        private float getVelocity(float velx, float velz)
        {
            var velocity = Math.Sqrt(Math.Pow(velx, 2) + Math.Pow(velz, 2));
            return (float)velocity;
        }

        private void StoreMessage(string eventMsg)
        {
            eventMessages.Add(eventMsg);
        }

        private void ProcessMessage(string msg)
        {
            //Debug.Log("Received: " + msg);
        }

        /// <summary>
        /// Fixed update network.
        /// </summary>
        public override void FixedUpdateNetwork()
        {
            base.Update(); // call ProcessMqttEvents()

            /*if (CurrentGamePhase is GamePhase.InGame)
            {*/
            if (eventMessages.Count > 0)
            {
                foreach (string msg in eventMessages)
                {
                    ProcessMessage(msg);
                }
                eventMessages.Clear();
            }

            //}




        }

        void INetworkRunnerCallbacks.OnPlayerJoined(NetworkRunner runner, PlayerRef player)
        {

            Debug.Log($"Entered OnPlayerJoined");


            if (runner.IsServer && _playerPrefab != null)
            {

                _playerId = player.PlayerId;
                _playerCount = Runner.SessionInfo.PlayerCount - 1;
                _player = player;
                TrackId = Runner.SessionInfo.Properties["trackid"];

                if (_playerCount == 0) return;

                NetworkObject character, car;

                Debug.Log($"_playerId: {_playerId}");
                Debug.Log($"PlayerCount: {_playerCount}");
                Debug.Log($"TrackId: {TrackId.PropertyValue.ToString()}");

                character = grlAvatarSpawn(_playerPrefab, player);
                car = grlCarSpawn(GetCar(), player);

                _playerMap[player] = character;
                _playerCarMap[player] = car;
                runner.SetPlayerObject(player, character);


                Log.Info($"Spawn for Player: {player}");


            }

        }

        private NetworkObject GetCar()
        {
            int carIndex = UnityEngine.Random.Range(0, 4);
            NetworkObject newCar = _carPrefabs[carIndex];

            if (!_playerCarTagMap.ContainsKey(newCar.gameObject.tag))
            {
                _playerCarTagMap[newCar.gameObject.tag] = newCar;
                return newCar;
            }

            return GetCar();

        }

        private NetworkObject grlCarSpawn(NetworkObject _objPrefab)
        {
            return grlCarSpawn(_objPrefab, PlayerRef.None);
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

        private NetworkObject grlCarSpawn(NetworkObject _objPrefab, PlayerRef player)
        {
            return Runner.Spawn(
                _objPrefab,
                Vector3.zero,
                Quaternion.identity,
                inputAuthority: player
                );
        }

        private void InitializeAvatarBeforeSpawn(NetworkRunner runner, NetworkObject obj)
        {
            var objPlayerData = obj.GetComponentInChildren<PlayerDataNetwork>();
            var copy = objPlayerData;

            copy.PlayerId = _player.PlayerId;
            copy.PlayerTag = $"Player{_playerCount}";

            copy.AvatarIndex = UnityEngine.Random.Range(1, 31);



            if (_playerCount == 1)
            {
                copy.PlayerTag = "Handler";
            }
            else
            {
                copy.PlayerTag = $"Player{_playerCount}";
            }

            objPlayerData = copy;

            _playerDataMap[_player] = objPlayerData;

            Debug.Log($"PlayerId for Player: {objPlayerData.PlayerId}");



        }

        void INetworkRunnerCallbacks.OnPlayerLeft(NetworkRunner runner, PlayerRef player)
        {
            if (_playerMap.TryGetValue(player, out var character))
            {
                // Despawn Player
                runner.Despawn(character);

                // Remove player from mapping
                _playerMap.Remove(player);
                var car = _playerCarMap[player];
                runner.Despawn(car);
                _playerCarMap.Remove(player);

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
        #endregion
    }
}