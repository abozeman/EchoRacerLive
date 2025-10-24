using Assets.CryptoKartz.Scripts.Managers;
using Fusion;
using Unity.XR.CoreUtils;
using UnityEngine;
namespace Assets.CryptoKartz.Scripts
{
    public class ERLRaceSceneManager : NetworkBehaviour
    {
        [SerializeField] public GameObject _xrOrigin;

        public override void Spawned()
        {
            if(!Runner.IsServer)
            {
                var _playerCount = Runner.SessionInfo.PlayerCount - 1;
                Debug.Log($"Player Count {_playerCount}");
                _xrOrigin.SetActive(true);
                GetComponentInChildren<CarInputManagerLive>().enabled = false;
                GetComponentInChildren<CarPositionLiveSubscriber>().enabled = false;
                GetComponentInChildren<CarControlDataLivePublisher>().enabled = false;

            } 
        }

    }

}
