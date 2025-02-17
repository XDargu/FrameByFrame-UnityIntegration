using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FbF;

public class RecordAnimations : MonoBehaviour
{
    private Animator animator;

    void Start()
    {
        FbFManager.RegisterRecordingOption("Animations");
        animator = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        Record();
    }

    [System.Diagnostics.Conditional("DEBUG")]
    void Record()
    {
        if (FbFManager.IsRecordingOptionEnabled("Animations"))
        {
            PropertyGroup animations = FbFManager.RecordProperties(gameObject, "Animations");
            animations.AddProperty("Enabled", animator.enabled, animator.enabled ? new Icon("check-circle", "green") : new Icon("times-circle", "red"));
            animations.AddProperty("Speed", animator.speed, new Icon("tachometer-alt"));
            animations.AddProperty("Has Root Motion", animator.hasRootMotion, new Icon("map-marker"));

            PropertyGroup layers = animations.AddPropertyGroup("Layers", new Icon("layer-group"));

            for (int i=0; i<animator.layerCount; ++i)
            {
                AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(i);

                PropertyGroup layer = layers.AddPropertyGroup(animator.GetLayerName(i), new Icon("layer-group"));
                layer.AddProperty("Weight", animator.GetLayerWeight(i), new Icon("weight-hanging"));
                layer.AddProperty("Loop", stateInfo.loop, new Icon("undo"));
                layer.AddProperty("Speed", stateInfo.speed, new Icon("tachometer-alt"));
                layer.AddProperty("Norm Time", stateInfo.normalizedTime, new Icon("clock"));

                float normTime = stateInfo.normalizedTime % 1f;

                AnimatorClipInfo[] clipInfo = animator.GetCurrentAnimatorClipInfo(i);

                for (int j = 0; j < clipInfo.Length; ++j)
                {
                    PropertyGroup clip = layer.AddPropertyGroup(clipInfo[j].clip.name, new Icon("walking"));
                    clip.AddProperty("Weight", clipInfo[j].weight, new Icon("weight-hanging"));
                    clip.AddProperty("Length", clipInfo[j].clip.length, new Icon("history"));
                    clip.AddProperty("Time", clipInfo[j].clip.length * normTime, new Icon("clock"));

                }
            }

            PropertyGroup parameters = animations.AddPropertyGroup("Parameters", new Icon("sliders-h"));

            foreach (AnimatorControllerParameter param in animator.parameters)
            {
                switch (param.type)
                {
                    case AnimatorControllerParameterType.Int:
                        parameters.AddProperty(param.name, animator.GetInteger(param.name));
                        break;
                    case AnimatorControllerParameterType.Float:
                        parameters.AddProperty(param.name, animator.GetFloat(param.name));
                        break;
                    case AnimatorControllerParameterType.Bool:
                        parameters.AddProperty(param.name, animator.GetBool(param.name));
                        break;
                    case AnimatorControllerParameterType.Trigger:
                        parameters.AddProperty(param.name, "Trigger (no value)");
                        break;
                    default:
                        parameters.AddProperty(param.name, "Unknown");
                        break;
                }

            }
        }
    }
}
