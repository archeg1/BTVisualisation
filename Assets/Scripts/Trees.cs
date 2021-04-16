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
        // TODO: Use string with type name insstead of enum
        public string type;
        public string nodeName;
        public string description;
        public Dictionary<string, Type> innerParams;
        public Rect box;
        public List<int>? childNodesID;
        public Node(Vector2 mousPos, string Name)
        {

        }
        public void Init()
        {


            Type myType = 
            FieldInfo[] myFields = myType.GetFields();
            foreach(var field in myFields)
            {
                var temp = field.FieldType.Name;

            }
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
    // public IBehaviourTreeNode CreateRuntimeTree() {  }
}
    
