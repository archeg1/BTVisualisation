using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Reflection;
using SimpleProto.AI.BehaviourTrees;
using System.Globalization;

public class BTree : ScriptableObject
{
    int TYPEOFVALUE = 0;
    int ISEXTERNAL = 1;
    int VALUE = 2;
    int VARIABLENAME = 3;
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
            childNodesID = new List<int>();
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



    //TODO: Write function that creates a runtime-ready tree from description
    public IBehaviourTreeNode CreateRuntimeTree()
    {
        BTree.Node rootNode = nodes[0];
        Dictionary<string, object> externalValues = new Dictionary<string, object>();
        IBehaviourTreeNode resultTree = CreateNode(rootNode, externalValues);
        return resultTree;
    }
    public IBehaviourTreeNode CreateNode(Node node, Dictionary<string, object> externalValues)
    {
        IBehaviourTreeNode resultNode;
        Type nodeType = Type.GetType(node.type);
        resultNode = (IBehaviourTreeNode)Activator.CreateInstance(nodeType);
        foreach (var field in node.InVariableParams.Keys)
        {
            var type = node.InVariableParams[field][TYPEOFVALUE];
            var isExternal = node.InVariableParams[field][ISEXTERNAL];
            var value = node.InVariableParams[field][VALUE];
            var variableName = node.InVariableParams[field][VARIABLENAME];
            FieldInfo tempField = nodeType.GetField(field, BindingFlags.Instance |
                BindingFlags.NonPublic |
                BindingFlags.Public);

            if (isExternal.Contains("True"))
            {
                tempField.SetValue(resultNode, externalValues[variableName]);
            }
            else
            {
                object setValue = null;
                if(type.Contains("System.Single"))
                {
                    float f = 0;
                    float.TryParse(value, out f);
                    setValue = f;
                }
                if (type.Contains("Vector3"))
                {
                    Vector3 vector3 = StringToVector3(value);
                    setValue = vector3;
                }
                if (type.Contains("Int32"))
                {
                    int i = 0;
                    int.TryParse(value, out i);
                    setValue = i;
                }
                tempField.SetValue(resultNode, setValue);
            }
            
        }
        foreach( var field in node.OutVariableParams.Keys)
        {
            var type = node.OutVariableParams[field][TYPEOFVALUE];
            var variableName = node.OutVariableParams[field][VARIABLENAME];
            FieldInfo tempField = nodeType.GetField(field, BindingFlags.Instance |
                BindingFlags.NonPublic |
                BindingFlags.Public);
            object setValue = null;
            if (type.Contains("System.Single"))
            {                
                setValue = new float();
            }
            if (type.Contains("Vector3"))
            {
                setValue = new Vector3();
            }
            if (type.Contains("Int32"))
            {
                setValue = new int();
            }
            tempField.SetValue(resultNode, setValue);
            externalValues.Add(variableName, setValue);
        }
        if(node.childNodesID.Count>0)
        {
            foreach(var nodeID in node.childNodesID)
            {
                BTree.Node innerNode = FindNodeByID(nodeID);
                MethodInfo addMethod = nodeType.GetMethod("Add", BindingFlags.Instance |
                BindingFlags.NonPublic |
                BindingFlags.Public);
                addMethod.Invoke(resultNode,new[] { CreateNode(innerNode, externalValues) });
            }
        }
        return resultNode;
    }

    BTree.Node FindNodeByID(int ID)
    {
        foreach( var node in nodes)
        {
            if (node.ID == ID)
            {
                return node;
            }
        }
        return null;
    }
    public static Vector3 StringToVector3(string sVector)
    {
        try
        {
            sVector = sVector.Replace(" ", string.Empty);
            // Remove the parentheses
            if (sVector.StartsWith("(") && sVector.EndsWith(")"))
            {
                sVector = sVector.Substring(1, sVector.Length - 2);
            }

            // split the items
            string[] sArray = sVector.Split(',');

            // store as a Vector3
            Vector3 result = new Vector3(
                float.Parse(sArray[0], CultureInfo.InvariantCulture.NumberFormat),
                float.Parse(sArray[1], CultureInfo.InvariantCulture.NumberFormat),
                float.Parse(sArray[2], CultureInfo.InvariantCulture.NumberFormat));

            return result;
        }
        catch
        {
            return Vector3.zero;
        }

    }
}
    
