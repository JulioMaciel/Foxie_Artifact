using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;

public static class Tools
{
    public static IEnumerator MoveWhileAnimating(this NavMeshAgent agent, Animator anim, Vector3 destination)
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

    public static IEnumerator WaitAnimationFinishes(this Animator anim)
    {
        var animationLength = anim.GetCurrentAnimatorStateInfo(0).length * anim.GetCurrentAnimatorStateInfo(0).speed;
        yield return new WaitForSecondsRealtime(animationLength);
    }

    public static void TrySetWorldDestination(this NavMeshAgent agent, Vector3 worldDestination)
    {
        NavMesh.SamplePosition(worldDestination, out var hit, 1.0f, NavMesh.AllAreas);

        if (hit.hit) agent.SetDestination(hit.position);
        else Debug.DrawLine(agent.transform.position, worldDestination, Color.red, 10000);
    }
}