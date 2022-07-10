using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;

public static class Tools
{
    public static IEnumerator MoveWhileAnimating(this NavMeshAgent agent, Animator anim, Transform destination)
    {
        if (anim.parameters.All(p => p.nameHash != Anim.MoveSpeed))
            throw new UnityException("The animator must have a MoveSpeed parameter.");

        agent.SetDestination(destination.position);
        
        while (agent.hasPath)
        {
            var velocity = agent.velocity.magnitude/agent.speed;
            anim.SetFloat(Anim.MoveSpeed, velocity);
            yield return null;
        }
    }
}