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

    public class ClientManager : ClientManagerBase
    {

        Fusion.NetworkRunner runnerClient;

        protected void Start()
        {
            // DontDestroyOnLoad(this.gameObject);
            // Start the client
            _ = StartClient();
        }


        public async Task<StartGameResult> StartClient()
        {
            int loadERLTask = await LoadERLAsync();
            StartGameResult startERLTask = await StartSessionAsync("ERLRace", SceneRef.FromIndex((int)SceneDefs.ERLRace));
            return startERLTask;
        }

        public async Task<int> LoadERLAsync() // assume we return an int from this long running operation 
        {
            await SceneManager.LoadSceneAsync((int)SceneDefs.ERLRace, LoadSceneMode.Single);
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

            var runner = Instantiate(_runnerClientPrefab);
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