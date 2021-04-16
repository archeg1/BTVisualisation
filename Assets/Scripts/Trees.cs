using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Reflection;

public class BTree : ScriptableObject
{
    public class Node
    {
        // TODO: Use string with type name insstead of enum
        public string type;
        public string nodeName;
        public string description;
        public Dictionary<string, object> innerParams;
        public List<int>? childNodes;
        public void Init()
        {
            Type myType = Type.GetType(type);
            FieldInfo[] myFields = myType.GetFields();
        }
    }

    // TODO: Add variables

    [SerializeField]
    public string TreesName;
    [SerializeField]
    public List<Node> nodes;

    // TODO: Write function that creates a runtime-ready tree from description
    // public IBehaviourTreeNode CreateRuntimeTree() {  }
}
    
