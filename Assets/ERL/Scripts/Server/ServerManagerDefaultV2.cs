using Assets.CryptoKartz.Scripts.managers;
using Assets.CryptoKartz.Scripts.Utils;
using Fusion;
using Meta.WitAi;
using Oculus.Platform;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;
using uPLibrary.Networking.M2Mqtt.Messages;
using Application = UnityEngine.Application;

namespace Assets.CryptoKartz.Scripts.Managers
{

    public class ServerManagerDefaultV2 : ServerManagerBaseNetwork
    {
        private List<string> eventMessages = new List<string>();

        Fusion.NetworkRunner runnerServer;
        Fusion.NetworkRunner runnerClient;

        #region MQTT Client

        #region Broker Settings
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
            SubscribeTopics();
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
            client.Subscribe(new string[] { string.Format("gameserver.agent.{0}", "*") }, new byte[] { MqttMsgBase.QOS_LEVEL_EXACTLY_ONCE });
            client.Subscribe(new string[] { string.Format("gameclient.agent.{0}", "*") }, new byte[] { MqttMsgBase.QOS_LEVEL_EXACTLY_ONCE });
        }

        protected override void UnsubscribeTopics()
        {
            client.Unsubscribe(new string[] { string.Format("gameserver.agent.{0}", "*") });
            client.Unsubscribe(new string[] { string.Format("gameclient.agent.{0}", "*") });
        }

        #endregion

        #endregion

        protected override async void DecodeMessage(string topic, byte[] message)
        {
            try
            {
                string msg = System.Text.Encoding.UTF8.GetString(message);
                //string msg = "{"type": "1", "vid": "grlv0telemetry", "posX": "0.85", "posZ": "-0.018", "velX": "-0.0", "velZ": "-0.003", "rotW": "0.987", "rotX": "-0.117", "rotY": "0.014", "rotZ": "0.105", "strAngle": "0.0", "strThrottle": "0.0"}"
                Debug.Log("msg: " + msg);
                if (topic.Contains("gameserver"))
                {
                    var result = await StartServer(msg);
                    Debug.Log("StartServer Result: " + result);
                }

                if (topic.Contains("gameclient"))
                {
                    var result = await StartClient();
                    Debug.Log("StartClient Result: " + result);
                }

                StoreMessage(msg);
            }
            catch (Exception e)
            {
                Console.WriteLine("DecodeMessage Exception: " + e.StackTrace);
            }

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
            base.Update();

            if (eventMessages.Count > 0)
            {
                foreach (string msg in eventMessages)
                {
                    ProcessMessage(msg);
                }
                eventMessages.Clear();
            }

        }

        public async Task<StartGameResult> StartServer(string msg)
        {
            await Task.Yield();
            Application.targetFrameRate = 30;
            AgentConfig agentConfig = new AgentConfig();
            runnerServer = GetRunner("Server");

            DedicatedServerConfig config = new DedicatedServerConfig();
            try
            {
                agentConfig = new AgentConfig(msg);
                Debug.Log($"AgentConfig {agentConfig.ToString()}");
            }
            catch (Exception e)
            {
                Debug.Log($"AgentConfig Failure {e.StackTrace}");
            }

            config = DedicatedServerConfig.AgentResolve(agentConfig);
            var result = await StartSimulation(runnerServer, config);

            // Check if all went fine
            if (result.Ok)
            {
                //Do Playfab GSDK Start Here
                //Do Playfab Logging here also
                Log.Debug($"Runner Start DONE");
            }
            else
            {
                // Quit the application if startup fails
                Log.Debug($"Error while starting Server: {result.ShutdownReason}");

                // it can be used any error code that can be read by an external application
                // using 0 means all went fine
                runnerServer.DestroySafely();
                Application.Quit(1);
            }

            return result;


        }

        public async Task<StartGameResult> StartClient()
        {
            int loadERLTask = await LoadERLAsync();
            StartGameResult startERLTask = await StartSessionAsync("ERLRace", SceneRef.FromIndex((int)SceneDefs.ERLRace));
            return startERLTask;
        }

        public async Task<int> LoadERLAsync() // assume we return an int from this long running operation 
        {
            await SceneManager.LoadSceneAsync((int)SceneDefs.ERLRace, LoadSceneMode.Additive);
            return 1;
        }

        public async Task<StartGameResult> StartSessionAsync(string sessionName, SceneRef scene) // assume we return an int from this long running operation 
        {
            runnerClient = GetRunner("Client");

            var result = await StartSession(runnerClient, GameMode.Client, sessionName, "ERLOrlandoDev", scene);

            // Check if all went fine
            if (result.Ok)
            {

                Log.Debug($"Runner Start session success");
                //_instanceRunner.DestroySafely();
            }
            else
            {
                Log.Debug($"Runner Start Session failed");
            }

            return result;
        }

        private NetworkRunner GetRunner(string name)
        {
            if(name.Contains("Server") && _runnerServerPrefab == null)
            {
                Debug.LogError("ServerManagerDefaultV2: _runnerServerPrefab is not assigned in the inspector.");
                return null;
            }
            if (name.Contains("Client") && _runnerClientPrefab == null)
            {
                Debug.LogError("ServerManagerDefaultV2: _runnerClientPrefab is not assigned in the inspector.");
                return null;
            }
            var runner = name.Contains("Server") ? Instantiate(_runnerServerPrefab) : Instantiate(_runnerClientPrefab);
            runner.name = name;
            runner.ProvideInput = true;

            return runner;
        }

        /// <summary>
        /// Start the simulation.
        /// </summary>
        /// <param name="runner">The runnerServer.</param>
        /// <param name="gameMode">The game mode.</param>
        /// <param name="sessionName">The session name.</param>
        /// <returns><![CDATA[Task<StartGameResult>]]></returns>
        public Task<StartGameResult> StartSession(
            NetworkRunner runner,
            GameMode gameMode,
            string sessionName,
            string lobbyName,
            SceneRef scene
          )
        {

            return runner.StartGame(new StartGameArgs()
            {
                CustomLobbyName = lobbyName,
                SessionName = sessionName,
                GameMode = gameMode,
                SceneManager = runner.gameObject.AddComponent<NetworkSceneManagerDefault>(),
                Scene = scene,
            });



        }

    }

    
}