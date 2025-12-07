using Fusion;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.CryptoKartz.Scripts
{

    public class ERLSceneManger : NetworkBehaviour
    {

        public List<GameObject> sceneObjects;
        public bool isTesting = false;

        // Start is called once before the first execution of Update after the MonoBehaviour is created
        public override void Spawned()
        {
            foreach (var gameObj in sceneObjects)
            {
                if (!Runner.IsServer || isTesting)
                {
                    Instantiate(gameObj);
                }
            }
        }

    }

}
