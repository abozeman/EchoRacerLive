using Assets.CryptoKartz.Scripts.Managers;
using Fusion;
using UnityEngine;
namespace Assets.CryptoKartz.Scripts
{
    public class CarManagerLive : NetworkBehaviour
    {

        public override void Spawned()
        {
            if(!Runner.IsServer)
            {
                GetComponentInChildren<CarInputManagerLive>().enabled = false;
                GetComponentInChildren<CarPositionLiveSubscriber>().enabled = false;
                GetComponentInChildren<CarControlDataLivePublisher>().enabled = false;
            }
        }

    }

}
