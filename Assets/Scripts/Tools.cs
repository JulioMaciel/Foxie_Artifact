using System;
using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;

public static class Tools
{
    public static IEnumerator MoveAnimating(this NavMeshAgent agent, Animator anim, Vector3 destination)
    {
        if (anim.parameters.All(p => p.nameHash != AnimParam.MoveSpeed))
            throw new UnityException("The animator must have a MoveSpeed parameter.");

        agent.SetDestination(destination);
        
        while (!agent.HasPracticallyArrived())
        {
            var velocity = agent.velocity.magnitude/agent.speed;
            anim.SetFloat(AnimParam.MoveSpeed, velocity);
            yield return null;
        }
        anim.SetFloat(AnimParam.MoveSpeed, 0);
    }
    
    public static bool HasPracticallyArrived(this NavMeshAgent agent, float tolerance = .5f)
    {
        return IsCloseEnough(agent.transform.position, agent.destination, tolerance);
    }

    public static bool IsCloseEnough(this Vector3 original, Vector3 other, float tolerance = .5f)
    {
        return Vector3.Distance(original, other) <= tolerance;
    }

    public static IEnumerator WaitToArrive(this NavMeshAgent agent, float tolerance = .5f)
    {
        while (!HasPracticallyArrived(agent, tolerance))
            yield return null;
    }

    public static IEnumerator WaitCurrentAnimation(this Animator anim)
    {
        // lenght = 2
        // if speed < 1 => (0.5)
            // increase length to 4
            // so -> 2 ___ 0.5 = 4
            // 2/.5 = 4
        // if speed > 1 => (2)
            // decrease length to 1
            // so -> 2 ___ 2 = 2
            // 2/2 => 1
        var clip = anim.GetCurrentAnimatorStateInfo(0);
        var fullLength = clip.length / clip.speed;
        yield return new WaitForSecondsRealtime(fullLength);
    }

    public static void TrySetWorldDestination(this NavMeshAgent agent, Vector3 worldDestination)
    {
        NavMesh.SamplePosition(worldDestination, out var hit, 1.0f, NavMesh.AllAreas);

        if (hit.hit) agent.SetDestination(hit.position);
        else Debug.DrawLine(agent.transform.position, worldDestination, Color.red, 10000);
    }

    public static IEnumerator WaitAnimationStart(this Animator anim, AnimClip clip)
    {
        var clipName = Enum.GetName(typeof(AnimClip), clip);
        var clipHashId = Animator.StringToHash(clipName);
        
        if (!anim.HasState(0, clipHashId))
            throw new UnityException("The animator does not contain this clip");
        
        while (!anim.GetCurrentAnimatorStateInfo(0).IsName(clipName))
            yield return null;
    }

    public static IEnumerator WaitAnimationFinish(this Animator anim, AnimClip clip)
    {
        yield return anim.WaitAnimationStart(clip);
        yield return anim.WaitCurrentAnimation();
    }
}