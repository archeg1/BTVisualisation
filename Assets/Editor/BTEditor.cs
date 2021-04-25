using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using SimpleProto.AI.BehaviourTrees;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Reflection;
using System.Globalization;

public class BTEditor : EditorWindow
{
    static BTree curBTrees = null;
    GenericMenu menu;
    List<string> behaviorList;
    Dictionary<String, String> behaviors;
    int lastWindowId = 0;
    int toolbarInt = 0;

    [MenuItem("Window/BTEditor")]
    public static void ShowExample()
    {
        BTEditor wnd = GetWindow<BTEditor>();
        wnd.titleContent = new GUIContent("BTEditor");

    }

    public Rect windowRect;
    Rect mainRect;
    Rect subRect;

    // Scroll position
    public Vector2 scrollPos = Vector2.zero;
    void OnGUI()
    {
        mainRect = new Rect(0, 0, rootVisualElement.layout.width * 0.75f, rootVisualElement.layout.height);
        subRect = new Rect(rootVisualElement.layout.width * 0.75f, 0, rootVisualElement.layout.width * 0.25f, rootVisualElement.layout.height);
        GUILayout.BeginArea(mainRect);
        scrollPos = GUI.BeginScrollView(mainRect, scrollPos, new Rect(0, 0, 1000, 1000));


        BeginWindows();

        if (curBTrees != null)
        {
            HandleEvents(Event.current);

            PaintNodes();
            //PaintCurves();

        }

        EndWindows();

        GUI.EndScrollView();
        GUILayout.EndArea();

        GUILayout.BeginArea(subRect);
        string[] toolbarString = { "Main", "Node" };
        toolbarInt = GUILayout.Toolbar(toolbarInt, toolbarString);
        if (toolbarInt == 0)
        {
            if (GUILayout.Button("Create new \"Behavior tree\""))
            {
                string result = EditorUtility.SaveFilePanel("Save behavior tree", "Assets", "BehaviorTree", "asset");
                result = result.Substring(result.IndexOf("Assets"), result.Length - result.IndexOf("Assets"));
                curBTrees = ScriptableObject.CreateInstance<BTree>();
                curBTrees.name = result.Replace(".asset", "");
                AssetDatabase.CreateAsset(curBTrees, result);
            }
            if (GUILayout.Button("Open \"Behavior tree\""))
            {
                string result = EditorUtility.OpenFilePanel("Open behavior tree", "Assets", "asset");
                result = result.Substring(result.IndexOf("Assets"), result.Length - result.IndexOf("Assets"));
                curBTrees = (BTree)AssetDatabase.LoadAssetAtPath(result, typeof(BTree));
                int a = 0;
            }
            if (curBTrees != null)
            {
                if (GUILayout.Button("Close \"Behavior tree\""))
                {
                    curBTrees = null;
                }
            }
        }
        GUILayout.EndArea();


    }
    public void CreateGUI()
    {
        // Each editor window contains a root VisualElement object
        VisualElement root = rootVisualElement;
        float temp = root.layout.width;

    }

    public static void Open(BTree bTree)
    {
        BTEditor window = GetWindow<BTEditor>("Behavior tree Editor");
        curBTrees = bTree;
    }

    void HandleEvents(Event e)
    {
        switch (e.type)
        {
            case EventType.MouseDown:
                if (e.button == 1)
                {
                    OpenContextMenu(e.mousePosition);
                }
                break;
        }
    }

    void OpenContextMenu(Vector2 mousPos)
    {
        menu = new GenericMenu();
        behaviors = new Dictionary<string, string>();
        behaviorList = new List<string>();
        foreach (Type type in Assembly.GetAssembly(typeof(BehaviourTreeNode)).GetTypes())
        {
            var parent = type.BaseType;
            bool abstr = type.IsAbstract;
            if (type.IsClass && (type.IsSubclassOf(typeof(BehaviourTreeNode)) || type.IsSubclassOf(typeof(SimpleProto.AI.BehaviourTrees.BehaviourTreeNode<UnityEngine.GameObject>))) && abstr == false)
            {
                behaviorList.Add(type.Name);
                String fullName = type.AssemblyQualifiedName;
                behaviors.Add(type.Name, fullName);
            }
        }
        foreach (var item in behaviorList)
        {
            AddMenuItemForNode(menu, item, mousPos);
        }
        menu.ShowAsContext();
    }


    void OnAppendNodeSelected(string nodeName, Vector2 mousPos)
    {
        var nodeType = behaviors[nodeName.ToString()];
        var ID = lastWindowId + 1;
        BTree.Node node = new BTree.Node(mousPos, nodeType, nodeName, ID);
        curBTrees.nodes.Add(node);
    }

    void AddMenuItemForNode(GenericMenu menu, string menuPath, Vector2 mousPos)
    {
        menu.AddItem(new GUIContent(menuPath), false, () => OnAppendNodeSelected(menuPath, mousPos));
    }

    public void PaintNodes()
    {
        if (curBTrees != null)
        {
            foreach (var node in curBTrees.nodes)
            {
                node.box = GUILayout.Window(node.ID, node.box, nodeFunc, node.name);
            }
            if (curBTrees.nodes.Count > 0)
            {
                lastWindowId = curBTrees.nodes[curBTrees.nodes.Count - 1].ID;
            }
        }
    }

    void nodeFunc(int unusedWindowID)
    {

        BTree.Node windowNode = null;
        foreach (var node in curBTrees.nodes)
        {
            if (node.ID == unusedWindowID)
            {
                windowNode = node;
                break;
            }
        }

        var InVariableParams = windowNode.InVariableParams;
        if (InVariableParams.Count > 0)
        {
            GUILayout.Label("Inner values");
            foreach (var innerFieldName in InVariableParams.Keys)
            {
                string type = InVariableParams[innerFieldName][0];
                string isExternal = InVariableParams[innerFieldName][1];
                string value = InVariableParams[innerFieldName][2];
                string variableName = InVariableParams[innerFieldName][3];
                GUILayout.BeginHorizontal();
                GUILayout.Label(innerFieldName);
                isExternal = GUILayout.Toggle(bool.Parse(isExternal), new GUIContent("isExternal")).ToString();
                if (bool.Parse(isExternal))
                {
                    variableName = GUILayout.TextField(variableName);
                }
                else
                {
                    if (type.Contains("System.Single"))
                    {
                        float f = 0;
                        float.TryParse(value, out f);
                        Rect position = EditorGUILayout.GetControlRect(false, 2 * EditorGUIUtility.singleLineHeight); // Get two lines for the control
                        position.height *= 0.5f;
                        value = EditorGUI.Slider(position, f, -100, 100).ToString();
                        position.y += position.height;
                        position.x += EditorGUIUtility.labelWidth;
                        position.width -= EditorGUIUtility.labelWidth + 54;
                        GUIStyle style = GUI.skin.label;

                        style.alignment = TextAnchor.UpperLeft; EditorGUI.LabelField(position, "Min", style);
                        style.alignment = TextAnchor.UpperRight; EditorGUI.LabelField(position, "Max", style);
                    }
                    if (type.Contains("Vector3"))
                    {
                        Vector3 vector3 = StringToVector3(value);
                        vector3 = EditorGUILayout.Vector3Field("",vector3);
                        value = vector3.ToString();
                    }
                    if (type.Contains("Int32"))
                    {
                        int i = 0;
                        int.TryParse(value, out i);
                        i = EditorGUILayout.IntField(i);
                        value = i.ToString();
                    }
                    //switch(InVariableParams[innerFieldName][0])
                    //{
                    //    case "System.Single":
                    //        InVariableParams[innerFieldName][2] = GUILayout.HorizontalSlider(float.Parse(InVariableParams[innerFieldName][2]), -100, 100).ToString();
                    //        break;
                    //}
                }
                GUILayout.EndHorizontal();
                InVariableParams[innerFieldName][1] = isExternal;
                InVariableParams[innerFieldName][2] = value;
                InVariableParams[innerFieldName][3] = variableName;
            }
        }
        var OutVariableParams = windowNode.OutVariableParams;
        if(OutVariableParams.Count>0)
        {
            GUILayout.Label("Out values");
            foreach(var outFieldName in OutVariableParams.Keys)
            {
                string outVariableName = OutVariableParams[outFieldName][1];
                GUILayout.BeginHorizontal();
                GUIStyle style = new GUIStyle();
                style.alignment = TextAnchor.MiddleLeft;
                style.normal.textColor = Color.white;
                GUILayout.Label(outFieldName,style);
                outVariableName = GUILayout.TextField(outVariableName);
                GUILayout.EndHorizontal();
                OutVariableParams[outFieldName][1] = outVariableName;
            }
        }

        Event e = Event.current;
        if (e.type == EventType.MouseDown)
        {
            if (e.button == 1)
            {
                OpenNodeContextMenu(windowNode.baseType, windowNode);
            }
        }
        GUI.DragWindow(new Rect(0, 0, 10000, 20));
    }

    void OpenNodeContextMenu(string rootType, BTree.Node windowNode)
    {
        menu = new GenericMenu();
        menu.AddItem(new GUIContent("Remove node"), false, () => OnDeleteNodeSelected(windowNode));
        menu.ShowAsContext();

    }

    void OnDeleteNodeSelected(BTree.Node windowNode)
    {
        curBTrees.nodes.Remove(windowNode);
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
