using System;
using System.Collections;
using System.Linq;
using StaticData;
using UnityEngine;
using UnityEngine.AI;
using Random = UnityEngine.Random;

namespace Tools
{
    public static class Extensions
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

            agent.ResetPath();
        }

        public static IEnumerator WaitCurrentAnimation(this Animator anim)
        {
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

        public static void TurnBackOn(this Transform trans, Transform target)
        {
            trans.rotation = Quaternion.LookRotation(trans.position - target.position);
        }

        public static IEnumerator LookAtSmoothly(this Transform trans, Transform target, float speed)
        {
            var targetRotation = Quaternion.LookRotation(target.transform.position - trans.position);
            var currentAngle = Quaternion.Angle(trans.rotation, target.rotation);
            while (currentAngle >= 1)
            {
                trans.rotation = Quaternion.Slerp(trans.rotation, targetRotation, speed * Time.deltaTime);
                currentAngle = Quaternion.Angle(trans.rotation, target.rotation);
                yield return null;
            }
        }

        public static IEnumerator MoveUntilArrive(this Transform trans, Transform target, float speed, float tolerance = .5f)
        {
            while (!trans.position.IsCloseEnough(target.position, tolerance))
            {
                var step = speed * Time.deltaTime;
                trans.position = Vector3.MoveTowards(trans.position, target.position, step);
                yield return null;
            }
        }

        public static void PlayClip(this AudioSource audioSource, AudioClip clip)
        {
            audioSource.clip = clip;
            audioSource.Play();
        }

        public static void PlayRandomClip(this AudioSource audioSource, AudioClip[] clips)
        {
            var rndClip = clips[Random.Range(0, clips.Length)];
            audioSource.PlayClip(rndClip);
        }
    }
}