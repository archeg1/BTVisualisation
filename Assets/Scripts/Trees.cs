using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BTree : ScriptableObject
{
    public enum NodeType { REPEATFOREVAR , COMPOSITENODE, TESKNODE, DECORATORNODE, SERVICENODE}

    public class Node
    {
        // TODO: Use string with type name insstead of enum
        public NodeType type;
        public string nodeName;
        public string description;
        public List<int>? childNodes;
    }

    // TODO: Add variables

    [SerializeField]
    public string TreesName;
    [SerializeField]
    public List<Node> nodes;

    // TODO: Write function that creates a runtime-ready tree from description
    // public IBehaviourTreeNode CreateRuntimeTree() {  }
}
    
