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
        [SerializeField]
        public int ID;
        // TODO: Use string with type name insstead of enum
        [SerializeField]
        public readonly string type;
        [SerializeField]
        public string baseType;
        [SerializeField]
        public readonly string name;
        [SerializeField]
        public string description;
        [SerializeField]
        public Dictionary<string, List<string>> InVariableParams;
        [SerializeField]
        public Dictionary<string, List<string>> OutVariableParams;
        [SerializeField]
        public Rect box;
        [SerializeField]
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
            InVariableParams = new Dictionary<string, List<string>>();
            OutVariableParams = new Dictionary<string, List<string>>();
            Type myType = Type.GetType(type);
            baseType = myType.BaseType.Name;
            FieldInfo[] myFields = myType.GetFields(BindingFlags.Instance |
                BindingFlags.NonPublic|
                BindingFlags.Public);

            foreach (var field in myFields)
            {
                string type = field.FieldType.Name;
                if (type.Contains("InVariable") || type.Contains("OutVariable"))
                {
                    string innerType = field.FieldType.GetTypeInfo().GetGenericArguments()[0].AssemblyQualifiedName;
                    int index1 = field.Name.IndexOf('<');
                    int index2 = field.Name.IndexOf('>');
                    string fieldName = field.Name.Substring(index1 + 1, index2 - index1 - 1);
                    List<string> fieldDescription = new List<string>();
                    if (type.Contains("InVariable"))
                    {
                        fieldDescription.Add(innerType); // type of value
                        fieldDescription.Add("False"); // isExternal
                        fieldDescription.Add(""); // value
                        fieldDescription.Add(""); // variableName

                        InVariableParams.Add(fieldName, fieldDescription);
                    }
                    if (type.Contains("OutVariable"))
                    {
                        fieldDescription.Add(innerType); // type of value
                        fieldDescription.Add(""); // outVariableName
                        OutVariableParams.Add(fieldName, fieldDescription);
                    }
                }
            }
            box = new Rect(mousPos, new Vector2( 10,10));

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
    public List<Node> nodes;

    BTree()
    {
        nodes = new List<Node>();
        Node root = new Node(new Vector2(50,10), typeof(RepeatForever).AssemblyQualifiedName, "Root", 0);
        nodes.Add(root);
    }



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
    
