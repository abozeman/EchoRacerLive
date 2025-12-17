using Assets.CryptoKartz.Scripts;
using Assets.CryptoKartz.Scripts.Managers;
using Fusion;
using Unity.XR.CoreUtils;
using UnityEngine;
using static Unity.Collections.Unicode;

public class CarSpawnManager : NetworkBehaviour
{
    public override void Spawned()
    {
        if (!Runner.IsServer)
        {
            GetComponent<CarInputManagerLive>().enabled = true;
            GetComponent<CarPositionLiveSubscriber>().enabled = false;
            GetComponent<CarControlDataLivePublisher>().enabled = false;
        }
        else
        {
            GetComponent<CarInputManagerLive>().enabled = false;
            GetComponent<CarPositionLiveSubscriber>().enabled = true;
            GetComponent<CarControlDataLivePublisher>().enabled = true;
        }
    }
}
