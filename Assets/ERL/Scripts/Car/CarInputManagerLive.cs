using Assets.CryptoKartz.Scripts.Managers;
using Fusion;
using UnityEngine;
namespace Assets.CryptoKartz.Scripts
{
    public class CarInputManagerLive : NetworkBehaviour
    {

        private float steeringInput;
        private float throttleInput;
        [SerializeField] public CarControlDataLivePublisher _carControlPublisher;

        
        public override void Spawned()
        {

        }

        /// <summary>
        /// Fixed update network.
        /// </summary>
        public override void FixedUpdateNetwork()
        {
            if (!Runner.IsServer) return;

            if(_carControlPublisher == null)
            {
                Debug.Log("CarControllerLive: CarControlPublisher is null");
                return;
            }

            if (!Object.HasInputAuthority) return;



            if (GetInput<Player.PlayerInputProvider.CarInput>(out var input) == false) return;

            throttleInput = input.carControlValue.y;
            steeringInput = input.carControlValue.x;

            _carControlPublisher.setControl(steeringInput, throttleInput);

        }

    }

}
