using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PCG
{
    public class Loom : MonoBehaviour
    {
        private static readonly List<Action> queuedActions = new List<Action>();
        private static GameObject loomGo;


        public static void Init()
        {
            if (loomGo == null)
            {
                loomGo = new GameObject("Loom");
                loomGo.AddComponent<Loom>();
            }
        }

        private void Update()
        {
            Action[] actionsToRun;
            lock (queuedActions)
            {
                actionsToRun = queuedActions.ToArray();
                queuedActions.Clear();
            }

            RunActions(actionsToRun);
        }

        /// <summary>
        /// Run only 1 action per frame to reduce performance drops
        /// </summary>
        /// <param name="actions"></param>
        /// <returns></returns>
        private IEnumerator RunActionsAsync(Action[] actions)
        {
            int actionsNum = actions.Length;
            for (int i = 0; i < actionsNum; i++)
            {
                actions[i]();
                yield return null;
            }
        }

        private void RunActions(Action[] actions)
        {
            int actionsNum = actions.Length;
            for (int i = 0; i < actionsNum; i++)
            {
                actions[i]();
            }
        }

        /// <summary>
        /// Run action on main Unity thread
        /// </summary>
        /// <param name="action"></param>
        public static void QueueOnMainThread(Action action)
        {
            lock (queuedActions)
            {
                queuedActions.Add(action);
            }
        }
    }
}
