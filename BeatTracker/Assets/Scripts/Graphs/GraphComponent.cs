using System;
using UnityEngine;

namespace Graphs
{
    public class GraphComponent : MonoBehaviour
    {
        private Graph _graph;

        protected Graph Graph
        {
            get
            {
                if (!_graph) _graph = FindGraph();
                return _graph;
            }
        }

        private Graph FindGraph()
        {
            Exception exception = new("The graph component must be a child of the Graph");

            Transform parent = transform.parent;
            while (parent != null)
            {
                if (parent.TryGetComponent(out Graph graph))
                    return graph;

                parent = parent.parent;
            }

            throw exception;
        }
    }
}