using GameCreator.Core.Hooks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;

namespace NJG.PUN
{
    public class LookAtCamera : MonoBehaviour
    {
        private void Update()
        {
            if (HookCamera.Instance && enabled) Setup();
        }

        private void Setup()
        {
            LookAtConstraint constraint = GetComponent<LookAtConstraint>();
            if (constraint == null) constraint = gameObject.AddComponent<LookAtConstraint>();
            constraint.rotationOffset = new Vector3(0, 180, 0);
            constraint.SetSources(new List<ConstraintSource>()
                {
                    new ConstraintSource()
                    {
                        sourceTransform = HookCamera.Instance.transform,
                        weight = 1.0f
                    }
                });

            constraint.constraintActive = true;

            enabled = false;
            Destroy(this);
        }
    }
}
