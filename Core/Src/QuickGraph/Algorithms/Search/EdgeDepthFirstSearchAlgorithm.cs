﻿using System;
using System.Collections.Generic;

namespace Topology.Graph.Algorithms.Search
{
    /// <summary>
    /// A edge depth first search algorithm for directed graphs
    /// </summary>
    /// <remarks>
    /// This is a variant of the classic DFS algorithm where the
    /// edges are color marked instead of the vertices.
    /// </remarks>
    /// <reference-ref
    ///     idref="gross98graphtheory"
    ///     chapter="4.2"
    ///     />
    [Serializable]
    public sealed class EdgeDepthFirstSearchAlgorithm<TVertex, TEdge> :
        RootedAlgorithmBase<TVertex,IEdgeListAndIncidenceGraph<TVertex, TEdge>>,
        IEdgeColorizerAlgorithm<TVertex,TEdge>,
        IEdgePredecessorRecorderAlgorithm<TVertex,TEdge>,
        ITreeBuilderAlgorithm<TVertex, TEdge>
        where TEdge : IEdge<TVertex>
    {
        private IDictionary<TEdge,GraphColor> colors;
        private int maxDepth = int.MaxValue;

        public EdgeDepthFirstSearchAlgorithm(IEdgeListAndIncidenceGraph<TVertex, TEdge> g)
            :this(g, new Dictionary<TEdge, GraphColor>())
        {
        }

        public EdgeDepthFirstSearchAlgorithm(
            IEdgeListAndIncidenceGraph<TVertex, TEdge> visitedGraph,
            IDictionary<TEdge, GraphColor> colors
            )
            :base(visitedGraph)
        {
            if (colors == null)
                throw new ArgumentNullException("VertexColors");

            this.colors = colors;
        }

        public IDictionary<TEdge, GraphColor> EdgeColors
        {
            get
            {
                return this.colors;
            }
        }

        public int MaxDepth
        {
            get
            {
                return this.maxDepth;
            }
            set
            {
                this.maxDepth = value;
            }
        }

        public event EdgeEventHandler<TVertex,TEdge> InitializeEdge;
        private void OnInitializeEdge(TEdge e)
        {
            if (InitializeEdge != null)
                InitializeEdge(this, new EdgeEventArgs<TVertex,TEdge>(e));
        }

        public event VertexEventHandler<TVertex> StartVertex;
        private void OnStartVertex(TVertex v)
        {
            if (StartVertex != null)
                StartVertex(this, new VertexEventArgs<TVertex>(v));
        }

        public event EdgeEventHandler<TVertex, TEdge> StartEdge;
        private void OnStartEdge(TEdge e)
        {
            if (StartEdge != null)
                StartEdge(this, new EdgeEventArgs<TVertex, TEdge>(e));
        }

        public event EdgeEdgeEventHandler<TVertex, TEdge> DiscoverTreeEdge;
        private void OnDiscoverTreeEdge(TEdge e, TEdge targetEge)
        {
            if (DiscoverTreeEdge != null)
                DiscoverTreeEdge(this, new EdgeEdgeEventArgs<TVertex, TEdge>(e, targetEge));
        }

        public event EdgeEventHandler<TVertex, TEdge> ExamineEdge;
        private void OnExamineEdge(TEdge e)
        {
            if (ExamineEdge != null)
                ExamineEdge(this, new EdgeEventArgs<TVertex, TEdge>(e));
        }

        public event EdgeEventHandler<TVertex, TEdge> TreeEdge;
        private void OnTreeEdge(TEdge e)
        {
            if (TreeEdge != null)
                TreeEdge(this, new EdgeEventArgs<TVertex, TEdge>(e));
        }

        public event EdgeEventHandler<TVertex, TEdge> BackEdge;
        private void OnBackEdge(TEdge e)
        {
            if (BackEdge != null)
                BackEdge(this, new EdgeEventArgs<TVertex, TEdge>(e));
        }

        public event EdgeEventHandler<TVertex, TEdge> ForwardOrCrossEdge;
        private void OnForwardOrCrossEdge(TEdge e)
        {
            if (ForwardOrCrossEdge != null)
                ForwardOrCrossEdge(this, new EdgeEventArgs<TVertex, TEdge>(e));
        }

        public event EdgeEventHandler<TVertex,TEdge> FinishEdge;
        private void OnFinishEdge(TEdge e)
        {
            if (FinishEdge != null)
                FinishEdge(this, new EdgeEventArgs<TVertex,TEdge>(e));
        }
        
        protected override void  InternalCompute()
        {
            Initialize();
            if (this.IsAborting)
                return;

            // start whith him:
            if (this.RootVertex != null)
            {
                OnStartVertex(this.RootVertex);

                // process each out edge of v
                foreach (TEdge e in VisitedGraph.OutEdges(this.RootVertex))
                {
                    if (this.IsAborting)
                        return;
                    if (EdgeColors[e] == GraphColor.White)
                    {
                        OnStartEdge(e);
                        Visit(e, 0);
                    }
                }
            }

            // process the rest of the graph edges
            foreach (TEdge e in VisitedGraph.Edges)
            {
                if (this.IsAborting)
                    return;
                if (EdgeColors[e] == GraphColor.White)
                {
                    OnStartEdge(e);
                    Visit(e, 0);
                }
            }
        }

        public void Initialize()
        {
            // put all vertex to white
            foreach (TEdge e in VisitedGraph.Edges)
            {
                if (this.IsAborting)
                    return;
                EdgeColors[e] = GraphColor.White;
                OnInitializeEdge(e);
            }
        }

        public void Visit(TEdge se, int depth)
        {
            if (depth > this.maxDepth)
                return;
            if (this.IsAborting)
                return;

            // mark edge as gray
            EdgeColors[se] = GraphColor.Gray;
            // add edge to the search tree
            OnTreeEdge(se);

            // iterate over out-edges
            foreach (TEdge e in VisitedGraph.OutEdges(se.Target))
            {
                // check edge is not explored yet,
                // if not, explore it.
                if (EdgeColors[e] == GraphColor.White)
                {
                    OnDiscoverTreeEdge(se, e);
                    Visit(e, depth + 1);
                }
                else if (EdgeColors[e] == GraphColor.Gray)
                {
                    // edge is being explored
                    OnBackEdge(e);
                }
                else
                    // edge is black
                    OnForwardOrCrossEdge(e);
            }

            // all out-edges have been explored
            EdgeColors[se] = GraphColor.Black;
            OnFinishEdge(se);
        }
    }
}
