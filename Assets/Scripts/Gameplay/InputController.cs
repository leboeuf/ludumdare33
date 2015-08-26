using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Assets.Scripts.Gameplay
{
    [Serializable]
    public abstract class InputController
    {
        public abstract void Init(int playerNum);
        public abstract void Update();

        public abstract bool IsSwimming();
        public abstract Vector3 GetMovementAxis();
    }
}
