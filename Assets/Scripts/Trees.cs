using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Reflection;
using SimpleProto.AI.BehaviourTrees;

public class BTree : ScriptableObject
{
    public class Node
    {
        public int ID;
        // TODO: Use string with type name insstead of enum
        public readonly string type;
        public readonly string name;
        public string description;
        public Dictionary<string, string> InVariableParams;
        public Dictionary<string, string> OutVariableParams;

        public Rect box;
        public List<int>? childNodesID;
        public Node(Vector2 mousPos, string nodeType, string nodeName, int nodeID)
        {
            ID = nodeID;
            type = nodeType;
            name = nodeName;
            Init(mousPos);
        }
        public void Init(Vector2 mousPos)
        {
            InVariableParams = new Dictionary<string, string>();
            OutVariableParams = new Dictionary<string, string>();
            Type myType = Type.GetType(type);
            FieldInfo[] myFields = myType.GetFields(BindingFlags.Instance |
                BindingFlags.NonPublic|
                BindingFlags.Public);

            foreach (var field in myFields)
            {
                string type = field.FieldType.Name;
                string innerType = field.FieldType.GetTypeInfo().GetGenericArguments()[0].AssemblyQualifiedName;
                int index1 = field.Name.IndexOf('<');
                int index2 = field.Name.IndexOf('>');
                string fieldName = field.Name.Substring(index1 + 1, index2 - index1 - 1);
                if (type.Contains("InVariable"))
                {
                    InVariableParams.Add(fieldName, innerType);
                }
                if (type.Contains("OutVariable"))
                {
                    OutVariableParams.Add(fieldName, innerType);
                }
            }
            box = new Rect(mousPos, new Vector2( 300, InVariableParams.Count * 50 + OutVariableParams.Count * 50 + 100));

        }
       
        public void HandleEvent(Event e)
        {
            switch(e.type)
            {
                case EventType.MouseDown:
                    if(box.Contains(e.mousePosition))
                    {
                        box.position += e.delta;
                    }
                    break;
            }
        }
    }

    // TODO: Add variables

    [SerializeField]
    public string TreesName;
    [SerializeField]
    public List<Node> nodes = new List<Node>();



    // TODO: Write function that creates a runtime-ready tree from description
    //public IBehaviourTreeNode CreateRuntimeTree()
    //{
    //    IBehaviourTreeNode resultTree;

    //    return resultTree;

    //}
    public IBehaviourTreeNode CreateNode(Node node)
    {
        IBehaviourTreeNode resultNode;
        Type nodeType = Type.GetType(node.type);
        resultNode = (IBehaviourTreeNode)Activator.CreateInstance(nodeType);
        return resultNode;

    }
}
    
