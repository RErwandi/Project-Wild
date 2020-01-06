namespace GameCreator.Variables
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;

    [Serializable]
    public class IntProperty : BaseProperty<int>
    {
        public IntProperty() : base() { }
        public IntProperty(int value) : base(value) { }
    }
}