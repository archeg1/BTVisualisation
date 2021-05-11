using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Reflection;
using SimpleProto.AI.BehaviourTrees;
using System.Globalization;


[Serializable]
public class BTree : ScriptableObject
{

    public string TreesName;
    public List<Node> nodes;

    //BTree()
    //{
    //    nodes = new List<Node>();
    //    Node root = new Node(new Vector2(50,10), typeof(RepeatForever).AssemblyQualifiedName, "Root", 0);
    //    nodes.Add(root);
    //}

    //TODO: Write function that creates a runtime-ready tree from description
    public IBehaviourTreeNode CreateRuntimeTree()
    {
        Node rootNode = nodes[0];
        Dictionary<string, object> externalValues = new Dictionary<string, object>();
        IBehaviourTreeNode resultTree = CreateNode(rootNode, externalValues);
        return resultTree;
    }
    public IBehaviourTreeNode CreateNode(Node node, Dictionary<string, object> externalValues)
    {
        IBehaviourTreeNode resultNode;
        Type nodeType = Type.GetType(node.type);
        resultNode = (IBehaviourTreeNode)Activator.CreateInstance(nodeType);
        //int count
        int countInnerValues = node.InVariableParams.Count / 6;
        for (int i = 0; i < countInnerValues; i++)
        {
            string type = node.InVariableParams[i * 6 + 1];
            string isExternal = node.InVariableParams[i * 6 + 2];
            string value = node.InVariableParams[i * 6 + 3];
            string variableName = node.InVariableParams[i * 6 + 4];
            FieldInfo tempField = nodeType.GetField(node.InVariableParams[i * 6 + 5], BindingFlags.Instance |
                BindingFlags.NonPublic |
                BindingFlags.Public);
            if (isExternal.Contains("True"))
            {
                object setValue = null;
                var temp = Activator.CreateInstance(Type.GetType(type), externalValues[variableName]);
                setValue = temp;
                tempField.SetValue(resultNode, setValue);
            }
            else
            {
                object setValue = null;
                if (type.Contains("System.Single"))
                {
                    float f = 0;
                    float.TryParse(value, out f);
                    var temp = Activator.CreateInstance(Type.GetType(type),f);
                    setValue = temp;
                }
                if (type.Contains("Vector3"))
                {
                    Vector3 vector3 = StringToVector3(value);
                    var temp = Activator.CreateInstance(Type.GetType(type),vector3);
                    setValue = temp;
                }
                if (type.Contains("Int32"))
                {
                    int j = 0;
                    int.TryParse(value, out j);
                    var temp = Activator.CreateInstance(Type.GetType(type));
                    setValue = temp;
                    temp = j;
                }
                tempField.SetValue(resultNode, setValue);
            }

        }

        int countOutValues = node.OutVariableParams.Count / 4;
        for (int i = 0; i < countOutValues; i++)
        {
            var type = node.OutVariableParams[i * 4 + 1];
            string variableName = node.OutVariableParams[i * 4 + 2];
            FieldInfo tempField = nodeType.GetField(node.OutVariableParams[i * 4 + 3], BindingFlags.Instance |
                BindingFlags.NonPublic |
                BindingFlags.Public);
            object setValue = null;

            //Делаем промежуточную переменную
            var innerType = Type.GetType(type).GetGenericArguments()[0];
            var initVal = Activator.CreateInstance(innerType);
            var VariableType = typeof(Variable<>).MakeGenericType(innerType);
            var tempVariable = Activator.CreateInstance(VariableType);
            var setM = VariableType.GetMethod("Set");
            setM.Invoke(tempVariable,new[] { initVal });

            setValue = Activator.CreateInstance(Type.GetType(type),tempVariable);
            tempField.SetValue(resultNode, setValue);
            externalValues.Add(variableName, tempVariable);
        }

        if (node.childNodesID.Count>0)
        {
            foreach(var nodeID in node.childNodesID)
            {
                Node innerNode = FindNodeByID(nodeID);
                MethodInfo addMethod = nodeType.GetMethod("Add", BindingFlags.Instance |
                BindingFlags.NonPublic |
                BindingFlags.Public);
                addMethod.Invoke(resultNode,new[] { CreateNode(innerNode, externalValues) });
            }
        }
        return resultNode;
    }

    Node FindNodeByID(int ID)
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
    
