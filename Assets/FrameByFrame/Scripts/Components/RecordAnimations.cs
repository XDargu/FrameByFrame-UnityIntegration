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
            animations.AddProperty("Speed", animator.speed);
            animations.AddProperty("Has Root Motion", animator.hasRootMotion);

            PropertyGroup layers = animations.AddPropertyGroup("Layers");

            for (int i=0; i<animator.layerCount; ++i)
            {
                AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(i);

                PropertyGroup layer = layers.AddPropertyGroup(animator.GetLayerName(i));
                layer.AddProperty("Weight", animator.GetLayerWeight(i));
                layer.AddProperty("Loop", stateInfo.loop);
                layer.AddProperty("Speed", stateInfo.speed);
                layer.AddProperty("Norm Time", stateInfo.normalizedTime);

                float normTime = stateInfo.normalizedTime % 1f;

                AnimatorClipInfo[] clipInfo = animator.GetCurrentAnimatorClipInfo(i);

                for (int j = 0; j < clipInfo.Length; ++j)
                {
                    PropertyGroup clip = layer.AddPropertyGroup(clipInfo[j].clip.name);
                    clip.AddProperty("Weight", clipInfo[j].weight);
                    clip.AddProperty("Length", clipInfo[j].clip.length);
                    clip.AddProperty("Time", clipInfo[j].clip.length * normTime);

                }
            }
        }
    }
}
