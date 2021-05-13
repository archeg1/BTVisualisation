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

    string path = "";

    static Node parentNode = null;

    Node selectedNode = null;
    

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

    static SerializedObject so; 
    void OnGUI()
    {
        if(Event.current.type == EventType.MouseMove)
        {
            Repaint();
        }
        mainRect = new Rect(0, 0, rootVisualElement.layout.width * 0.7f, rootVisualElement.layout.height);
        subRect = new Rect(rootVisualElement.layout.width * 0.70f, 0, rootVisualElement.layout.width * 0.3f, rootVisualElement.layout.height);
        GUILayout.BeginArea(mainRect);
        scrollPos = GUI.BeginScrollView(mainRect, scrollPos, new Rect(0, 0, 1000, 1000));
        wantsMouseMove = true;

        BeginWindows();

        if (curBTrees != null)
        {
            if (GUI.changed)
            {
                EditorUtility.SetDirty(curBTrees);
            }

            HandleEvents(Event.current);

            PaintNodes();
            PaintCurves();

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
                path = EditorUtility.SaveFilePanel("Save behavior tree", "Assets", "BehaviorTree", "asset");
                if (path.Length > 0)
                {
                    path = path.Substring(path.IndexOf("Assets"), path.Length - path.IndexOf("Assets"));
                    curBTrees = ScriptableObject.CreateInstance<BTree>();
                    curBTrees.TreesName = path.Replace(".asset", "");
                    curBTrees.nodes = new List<Node>();
                    Node root = new Node(new Vector2(50, 10), typeof(RepeatForever).AssemblyQualifiedName, "Root", 0);
                    curBTrees.nodes.Add(root);
                    EditorUtility.SetDirty(curBTrees);
                    AssetDatabase.CreateAsset(curBTrees, path);
                    AssetDatabase.SaveAssets();
                    Selection.activeObject = curBTrees;
                    so = new SerializedObject(curBTrees);
                    parentNode = null;
                }
            }
            if (GUILayout.Button("Open \"Behavior tree\""))
            {
                path = EditorUtility.OpenFilePanel("Open behavior tree", "Assets", "asset");
                if (path.Length > 0)
                {
                    path = path.Substring(path.IndexOf("Assets"), path.Length - path.IndexOf("Assets"));
                    curBTrees = (BTree)AssetDatabase.LoadAssetAtPath(path, typeof(BTree));
                    so = new SerializedObject(curBTrees);
                    parentNode = null;
                }
            }
            if (curBTrees != null)
            {
                if (GUILayout.Button("Close \"Behavior tree\""))
                {
                    AssetDatabase.SaveAssets();
                    AssetDatabase.Refresh();
                    curBTrees = null;
                }
            }
        }
        else
        {
            if(selectedNode == null)
            {
                GUILayout.Label("Choose one of node(if there exist)");
            }
            else
            {
                var InVariableParams = selectedNode.InVariableParams;
                if (InVariableParams.Count > 0)
                {
                    GUILayout.Label("Inner values");
                    int countInnerValues = InVariableParams.Count / 6;
                    for (int i = 0; i< countInnerValues; i++)
                    {
                        string type = InVariableParams[i * 6 + 1];
                        string isExternal = InVariableParams[i * 6 + 2];
                        string value = InVariableParams[i * 6 + 3];
                        string variableName = InVariableParams[i * 6 + 4];
                        GUILayout.BeginHorizontal();
                        GUILayout.Label(InVariableParams[i*6]);
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
                                vector3 = EditorGUILayout.Vector3Field("", vector3,GUILayout.MaxWidth(100));
                                value = vector3.ToString();
                            }
                            if (type.Contains("Int32"))
                            {
                                int j = 0;
                                int.TryParse(value, out j);
                                j = EditorGUILayout.IntField(j);
                                value = j.ToString();
                            }
                            //switch(InVariableParams[innerFieldName][0])
                            //{
                            //    case "System.Single":
                            //        InVariableParams[innerFieldName][2] = GUILayout.HorizontalSlider(float.Parse(InVariableParams[innerFieldName][2]), -100, 100).ToString();
                            //        break;
                            //}
                        }
                        GUILayout.EndHorizontal();
                        InVariableParams[i * 6 + 2] = isExternal;
                        InVariableParams[i * 6 + 3] = value;
                        InVariableParams[i * 6 + 4] = variableName;
                    }
                }
                var OutVariableParams = selectedNode.OutVariableParams;
                if (OutVariableParams.Count > 0)
                {
                    int countOutValues = OutVariableParams.Count / 4;
                    GUILayout.Label("Out values");
                    for (int i = 0; i<countOutValues;i++)
                    {
                        string outVariableName = OutVariableParams[i*3+2];
                        GUILayout.BeginHorizontal();
                        GUIStyle style = new GUIStyle();
                        style.alignment = TextAnchor.MiddleLeft;
                        style.normal.textColor = Color.white;
                        GUILayout.Label(OutVariableParams[i * 4], style);
                        outVariableName = GUILayout.TextField(outVariableName);
                        GUILayout.EndHorizontal();
                        OutVariableParams[i * 4 + 2] = outVariableName;
                    }
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
        parentNode = null;
    }

    public static void Open(BTree bTree)
    {
        BTEditor window = GetWindow<BTEditor>("Behavior tree Editor");
        curBTrees = bTree;
        parentNode = null;
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
                if(e.button == 0)
                {
                    selectedNode = null;
                }
                break;

        }
    }

    void PaintCurves()
    {
        if (curBTrees != null)
        {
            foreach (var node in curBTrees.nodes)
            {
                foreach (var id in node.childNodesID)
                {
                    var tempNode = FindNodeByID(id);
                    DrawNodeCurve(node.box, tempNode.box);
                }                
            }
        }
        

        if (parentNode != null)
        {
            
            DrawNodeCurve(parentNode.box,Event.current.mousePosition);
            
        }
    }

    Node FindNodeByID(int id)
    {
        Node result;
        result = null;
        foreach(var node in curBTrees.nodes)
        {
            if (node.ID == id)
            {
                result = node;
            }
        }

        return result;

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
        Node node = new Node(mousPos, nodeType, nodeName, ID);
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

        Node windowNode = null;
        //foreach (var node in curBTrees.nodes)
        //{
        //    if (node.ID == unusedWindowID)
        //    {
        //        windowNode = node;
        //        break;
        //    }
        //}
        windowNode = FindNodeByID(unusedWindowID);
        int lenght = windowNode.name.Length;
        var text = " ";
        for (int i = 0; i <lenght;i++)
        {
            text += "   ";
        }
        if (selectedNode != null)
        {
            if (windowNode.ID == selectedNode.ID)
            {
                GUI.backgroundColor = Color.green;
                GUI.color = Color.green;
            }
        }
        GUILayout.Label(text);
        Event e = Event.current;
        if (e.type == EventType.MouseDown)
        {
            if (e.button == 1)
            {
                OpenNodeContextMenu(windowNode.baseType, windowNode);
            }
            if (e.button == 0 && parentNode != null && unusedWindowID!=0)
            {
                if(!parentNode.childNodesID.Contains(windowNode.ID))
                    parentNode.childNodesID.Add(windowNode.ID);
                parentNode = null;
            }
            if (e.button == 0)
            {
                selectedNode = windowNode;
            }
        }
        GUI.DragWindow();
    }

    void OpenNodeContextMenu(string rootType, Node windowNode)
    {
        menu = new GenericMenu();
        menu.AddItem(new GUIContent("Remove node"), false, () => OnDeleteNodeSelected(windowNode));
        if(!windowNode.baseType.Contains("BehaviourTreeNode"))
        {
            menu.AddItem(new GUIContent("AddChild"), false, () => OnAddChildSelected(windowNode));
        }
        menu.AddItem(new GUIContent("Remove connections"), false, () => OnRemoveParentSelected(windowNode.ID));
        menu.ShowAsContext();

    }

    void OnRemoveParentSelected(int ID)
    {
        foreach(var node in curBTrees.nodes)
        {
            node.childNodesID.Remove(ID);
        }
    }
    void OnAddChildSelected( Node p)
    {
        parentNode = p;
    }
    void OnDeleteNodeSelected(Node windowNode)
    {
        if (windowNode.ID != 0)
        {
            int deletedNodeID = windowNode.ID;
            foreach (var node in curBTrees.nodes)
            {
                node.childNodesID.Remove(deletedNodeID);
            }
            curBTrees.nodes.Remove(windowNode);
        }
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

    void DrawNodeCurve(Rect start, Rect end)
    {
        Vector3 startPos = new Vector3(start.x + (start.width/2), start.y + start.height, 0);
        Vector3 endPos = new Vector3(end.x + (end.width / 2), end.y, 0);
        Vector3 startTan = startPos + Vector3.right * 50;

        Vector3 endTan = endPos + Vector3.left * 50;
        Color shadowCol = new Color(0, 0, 0, 0.06f);
        for (int i = 0; i < 3; i++) // Draw a shadow
            Handles.DrawBezier(startPos, endPos, startTan, endTan, shadowCol, null, (i + 1) * 5);
        Handles.DrawBezier(startPos, endPos, startTan, endTan, Color.black, null, 1);
    }

    void DrawNodeCurve(Rect start, Vector2 end)
    {
        Vector3 startPos = new Vector3(start.x + (start.width / 2), start.y + start.height, 0);
        Vector3 endPos = new Vector3(end.x, end.y, 0);
        Vector3 startTan = startPos + Vector3.right * 50;
        Vector3 endTan = endPos + Vector3.left * 50;
        Color shadowCol = new Color(0, 0, 0, 0.06f);
        for (int i = 0; i < 3; i++) // Draw a shadow
            Handles.DrawBezier(startPos, endPos, startTan, endTan, shadowCol, null, (i + 1) * 5);
        Handles.DrawBezier(startPos, endPos, startTan, endTan, Color.black, null, 1);
    }
}
