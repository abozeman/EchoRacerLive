using Assets.CryptoKartz.Scripts.Managers;
using Fusion;
using UnityEngine;
namespace Assets.CryptoKartz.Scripts
{
    public class CarManagerLive : NetworkBehaviour
    {

        public override void Spawned()
        {
            if(Runner.IsServer)
            {
                GetComponentInChildren<CarInputManagerLive>().enabled = true;
                GetComponentInChildren<CarPositionLiveSubscriber>().enabled = true;
                GetComponentInChildren<CarControlDataLivePublisher>().enabled = true;
            }
        }

    }

}
