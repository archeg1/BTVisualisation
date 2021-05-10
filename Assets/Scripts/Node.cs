using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Reflection;

[Serializable]
public class Node
{
    public int ID;
    // TODO: Use string with type name insstead of enum
    public string type;
    public string baseType;
    public string name;
    public string description;
    public List<string> InVariableParams;
    public List<string> OutVariableParams;
    public Rect box;
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
        InVariableParams = new List<string>();
        OutVariableParams = new List<string>();
        Type myType = Type.GetType(type);
        baseType = myType.BaseType.Name;
        FieldInfo[] myFields = myType.GetFields(BindingFlags.Instance |
            BindingFlags.NonPublic |
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
                if (type.Contains("InVariable"))
                {
                    InVariableParams.Add(fieldName); // display name
                    InVariableParams.Add(innerType); // type of value
                    InVariableParams.Add("False"); // isExternal
                    InVariableParams.Add(""); // value
                    InVariableParams.Add(""); // variableName
                    
                }
                if (type.Contains("OutVariable"))
                {
                    OutVariableParams.Add(fieldName);
                    OutVariableParams.Add(innerType); // type of value
                    OutVariableParams.Add(""); // outVariableName
                }
            }
        }
        box = new Rect(mousPos, new Vector2(10, 10));

    }

}
