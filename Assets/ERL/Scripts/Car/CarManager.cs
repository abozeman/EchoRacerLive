using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using uPLibrary.Networking.M2Mqtt.Messages;
using M2MqttUnity;
using Fusion;
using Assets.CryptoKartz.Scripts.Utils;
using System.Runtime.ConstrainedExecution;

namespace Assets.CryptoKartz.Scripts.Managers
{
    [SimulationBehaviour(Modes = SimulationModes.Server)]
    public class CarManager : M2MqttUnityClientNetwork, ISpawned
    {
        private List<string> eventMessages = new List<string>();
        private bool firstTime = true;
        [SerializeField] private GameObject frontRightWheel;
        [SerializeField] private GameObject frontLeftWheel;
        [SerializeField] private GameObject rearRightWheel;
        [SerializeField] private GameObject rearLeftWheel;
        //public Vector3 telemetryScale = new Vector3(1f, 1f, 1f);

        private GameObject tmPro;
        private GameObject lapNumText;

        //public GameObject warningCube;
        //public TTSSpeaker ttsSpeaker;
        public Vector3 startLineOffset = new Vector3(-1.29799998f, 1.07f, 0.138999999f);

        //Car Transform Data
        private Vector3 newCarPosition;
        private Vector3 oldCarPosition;
        private Quaternion newCarRotation;
        private Quaternion oldCarRotation;

        //Car Metadata
        [SerializeField] public string vid;
        public int CurrentLap = 0;
        public bool IsOffTrack;
        public bool IsOverlapping;
        public float Velocity;
        private NetworkTransform carNetworkTransform;

        //Car Steering/Throttle
        private float maxSteering = .20f;
        private float maxThrottle = .6f;
        public float Steering = 0;
        public float Throttle = 0;
        //private Quaternion offsetRot = new Quaternion(0f, 1f, 0f, 0f);


        #region Publish Sterring/Throttle
        /// <summary>
        /// Set the steering.
        /// </summary>
        /// <param name="steering">The steering.</param>
        public void setSteering(float steering)
        {
            
            try
            {
                Debug.Log("CarManager setSteering : " + steering);
                StartCoroutine(ControlPublish(0, steering));

            }
            catch (Exception e)
            {
                Debug.Log("CarManager setSteering Exception: " + e); 
            }
            
            
            
        }

        /// <summary>
        /// Set the throttle.
        /// </summary>
        /// <param name="throttle">The throttle.</param>
        public void setThrottle(float throttle)
        {
            
            try
            {
                Debug.Log("CarManager setThrottle : " + throttle);
                StartCoroutine(ControlPublish(1, throttle));

            }
            catch (Exception e)
            {
                Debug.Log("CarManager setThrottle Exception: " + e);
            }

        }

        public void setControl(float steering, float throttle)
        {

            try
            {
                //Debug.Log($"CarManager setControl (steering, throttle): ({steering}, {throttle})");
                StartCoroutine(ControlPublish(steering, throttle));

            }
            catch (Exception e)
            {
                Debug.Log("CarManager setControl Exception: " + e);
            }

        }

        IEnumerator ControlPublish(float steering, float throttle)
        {
            try
            {
                client.Publish(string.Format("car.cc.{0}", vid), System.Text.Encoding.UTF8.GetBytes("{\"steering\": \"" + steering + "\",\"throttle\": \"" + throttle + "\"}"));
                //Debug.Log("ControlPublish: " + "{\"type\": \"" + type + "\",\"value\": \"" + value + "\"}");
                //yield return new WaitForSecondsRealtime(.033f);
            }
            catch (Exception)
            {

                throw;
            }

            yield return null;



        }

        IEnumerator ControlPublishOld(int type, float valueFloat)
        {
            var value = valueFloat / 90f;

            client.Publish(string.Format("car.cc.{0}", vid), System.Text.Encoding.UTF8.GetBytes("{\"type\": \"" + type + "\",\"value\": \"" + value + "\"}"));
            //Debug.Log("ControlPublish: " + "{\"type\": \"" + type + "\",\"value\": \"" + value + "\"}");
            yield return new WaitForSecondsRealtime(.033f);

        }
        #endregion

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
            Debug.Log("Connected to broker on " + brokerAddress + "\n");
            SubscribeTopics();
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
            UnsubscribeTopics();
        }
        #endregion

        #region Subscription/Unsubscription
        protected override void SubscribeTopics()
        {
            client.Subscribe(new string[] { string.Format("car.telemetry.vr.{0}", vid) }, new byte[] { MqttMsgBase.QOS_LEVEL_EXACTLY_ONCE });
            client.Subscribe(new string[] { string.Format("car.lapupdate.{0}", vid) }, new byte[] { MqttMsgBase.QOS_LEVEL_EXACTLY_ONCE });
            client.Subscribe(new string[] { string.Format("car.vracestate.{0}", vid) }, new byte[] { MqttMsgBase.QOS_LEVEL_EXACTLY_ONCE });

        }

        protected override void UnsubscribeTopics()
        {
            client.Unsubscribe(new string[] { string.Format("car.telemetry.vr.{0}", vid) });
            client.Unsubscribe(new string[] { string.Format("car.lapupdate.{0}", vid) });
            client.Unsubscribe(new string[] { string.Format("car.vracestate.{0}", vid) });
        }

        #endregion

        #endregion
        
        public void Spawned()
        {
            Debug.Log("Ready.");

            base.Start();

            this.tmPro = transform.GetComponent("TMPInfo").gameObject;
            this.lapNumText = transform.GetComponent("TMPInfo").gameObject;

        }


        private void handleLapUpdate(LapData lapData)
        {
            //Debug.Log("lap: " + lapData.lap);
            //Debug.Log("laptimes: " + lapData.lapTimes);

            foreach(string lapTime in lapData.lapTimes)
            {
                Debug.Log(lapTime);
            }

            var lapCube = transform.Find("LapCube");
            //lapCube.GetComponent<CarEventManager>().cubeOn = true;

        }
        private void handleTelemetryData(TelemetryData telemetryData)
        {
            //Get The Raw Measurement First
            var carPosition = new Vector3(telemetryData.posX, telemetryData.posY, telemetryData.posZ);

            //Apply Environment Offset
            carPosition += startLineOffset;

            //Change from Right Handed Coords to Left Handed Coords
            //var carPosition = new Vector3(rawCarPosition.x * -1, rawCarPosition.y, rawCarPosition.z);
            var carRotation = new Quaternion(0, telemetryData.rotY * -1, 0, telemetryData.rotW);



            if (firstTime)
            {
                oldCarPosition = carPosition;
                oldCarRotation = carRotation;

                transform.SetLocalPositionAndRotation(carPosition,carRotation);

                firstTime = false;

            }
            else
            {
                newCarPosition = carPosition;
                newCarRotation = carRotation;

                transform.SetLocalPositionAndRotation(carPosition, carRotation);


                //After that oldCarposition = carPosition
                oldCarPosition = carPosition;
                oldCarRotation = carRotation;

            }



            //Transform for Car
            var angles = transform.rotation.eulerAngles;
            angles.y += 180f;
            angles.z = 0.0f;
            angles.x = 0.0f;
            transform.rotation = Quaternion.Euler(angles.x, angles.y, angles.z);
            
            Velocity = getVelocity(telemetryData.velX, telemetryData.velZ);

            //Transform Front Left and Right Wheels for steering
            var frwAngles = frontRightWheel.transform.localEulerAngles;
            var flwAngles = frontLeftWheel.transform.localEulerAngles;
            var rrwAngles = rearRightWheel.transform.localEulerAngles;
            var rlwAngles = rearLeftWheel.transform.localEulerAngles;
            frwAngles.y = telemetryData.steeringAngle * 45f;
            flwAngles.y = telemetryData.steeringAngle * 45f;

            if (Velocity > 0.0f)
            {
                frwAngles = spinWheels(frwAngles);
                flwAngles = spinWheels(flwAngles);
                rrwAngles = spinWheels(rrwAngles);
                rlwAngles = spinWheels(rlwAngles);
            }

            frontRightWheel.transform.localEulerAngles = frwAngles;
            frontLeftWheel.transform.localEulerAngles = flwAngles;
            rearRightWheel.transform.localEulerAngles = rrwAngles;
            rearLeftWheel.transform.localEulerAngles = rlwAngles;

            tmPro.GetComponent<TMPro.TextMeshPro>().text = "vel: " + string.Format("{0:N2}", Velocity) + " m/s<br> Lap " + CurrentLap;
            this.lapNumText.GetComponent<TextMesh>().text = CurrentLap.ToString();

        }
        private void handleVRaceStateData(VRaceStateData vRaceStateData)
        {
            //Debug.Log($"Offtrack || Overlap: {vRaceStateData.overlapFlag || vRaceStateData.offtrackFlag}");

            IsOffTrack = vRaceStateData.offtrackFlag;
            IsOverlapping = vRaceStateData.overlapFlag;

            var warningCube = transform.Find("WarningCube");
            var warningPosition = new Vector3(vRaceStateData.px * -1, startLineOffset.y, vRaceStateData.pz);
            warningCube.transform.SetPositionAndRotation(warningPosition, warningCube.transform.rotation);
            //warningCube.GetComponent<CarEventManager>().cubeOn = true;
        }

        private Vector3 spinWheels(Vector3 wheelAngles)
        {

            //Transform All wheels for moving
            if (wheelAngles.x > 360f)
            {
                wheelAngles.x = 0f;
            }
            else
            {
                wheelAngles.x += 1f;
            }

            return wheelAngles;
        }

        protected override void DecodeMessage(string topic, byte[] message)
        {
            try
            {
                string msg = System.Text.Encoding.UTF8.GetString(message);
                //string msg = "{"type": "1", "vid": "grlv0telemetry", "posX": "0.85", "posZ": "-0.018", "velX": "-0.0", "velZ": "-0.003", "rotW": "0.987", "rotX": "-0.117", "rotY": "0.014", "rotZ": "0.105", "strAngle": "0.0", "strThrottle": "0.0"}"
                //Debug.Log("msg: " + msg);
                if (topic.Contains("vracestate"))
                {
                    IsOffTrack = false;
                    IsOverlapping = false;
                    //Debug.Log("msg: " + msg);
                    VRaceStateData vRaceStateData = new VRaceStateData(msg);
                    handleVRaceStateData(vRaceStateData);
                }

                if (topic.Contains("lapupdate"))
                {
                    LapData lapData = new LapData(msg);
                    CurrentLap = lapData.lap;

                    handleLapUpdate(lapData);
                }

                if (topic.Contains("telemetry"))
                {
                    TelemetryData telemetryData = new TelemetryData(msg);
                    Steering = telemetryData.steeringAngle;
                    Throttle = telemetryData.throttle;
                    handleTelemetryData(telemetryData);
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

        IEnumerator SendControl()
        {
            //Debug.Log("ControlPublish: "+ Steering);
            ControlPublish(Steering, Throttle);
            yield return new WaitForSecondsRealtime(.05f);
        }

        IEnumerator ShowAndHideWarning(float delay)
        {
            //this.warningCube.SetActive(true);
            yield return new WaitForSeconds(delay);
            //this.warningCube.SetActive(false);
        }

        IEnumerator ShowAndHide(float delay)
        {
            var lapCube = GameObject.FindGameObjectWithTag("LapCube");

            lapCube.GetComponent<MeshRenderer>().enabled = true;
            yield return new WaitForSeconds(delay);
            lapCube.GetComponent<MeshRenderer>().enabled = false;
            yield return new WaitForSeconds(delay - 2);


        }

        IEnumerator RaceControlStates()
        {
            RaceStatePublish("PRERACE");
            yield return new WaitForSeconds(5f);
            RaceStatePublish("COUNTDOWN");
            yield return new WaitForSeconds(20f);
            RaceStatePublish("FINALCOUNTDOWN");
            yield return new WaitForSeconds(3f);
            RaceStatePublish("INPROGRESS");
            yield return new WaitForSeconds(150f);
            RaceStatePublish("COMPLETE");

        }

        /// <summary>
        /// Race state publish.
        /// </summary>
        /// <param name="raceState">The race state.</param>
        public void RaceStatePublish(string raceState)
        {
            client.Publish("race.control.raceId", System.Text.Encoding.UTF8.GetBytes("{\"race_state\": \"" + raceState + "\"}"));
            //Debug.Log("race state message published");
        }

        private void OnDestroy()
        {
            Disconnect();
        }

        private void OnValidate()
        {

        }
    }
}