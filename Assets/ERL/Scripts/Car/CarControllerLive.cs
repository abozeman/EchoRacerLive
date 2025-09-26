using Assets.CryptoKartz.Scripts.Managers;
using Fusion;
using UnityEngine;
namespace Assets.CryptoKartz.Scripts
{
    public class CarControllerLive : NetworkBehaviour
    {

        private float steeringInput;
        private float throttleInput;
        [SerializeField] public CarManager _carManager;

        
        public override void Spawned()
        {
        }

        /// <summary>
        /// Fixed update network.
        /// </summary>
        public override void FixedUpdateNetwork()
        {
            if (!Runner.IsServer) return;

            if(_carManager == null)
            {
                Debug.Log("CarControllerLive: CarManager is null");
                return;
            }

            if (!Object.HasInputAuthority) return;



            if (GetInput<Player.PlayerInputProvider.CarInput>(out var input) == false) return;

            throttleInput = input.carControlValue.y;
            steeringInput = input.carControlValue.x;

            _carManager.setControl(steeringInput, throttleInput);

        }

    }

}
