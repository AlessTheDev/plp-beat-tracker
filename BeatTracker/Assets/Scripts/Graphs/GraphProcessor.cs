using System;
using UnityEngine;

namespace Graphs
{
    [RequireComponent(typeof(Graph))]
    public abstract class GraphProcessor<T> : MonoBehaviour
    {
        public Graph Graph { get; private set; }
        private void Awake()
        {
            Graph = GetComponent<Graph>();
        }
        
        public abstract void Process(T input);
    }
}