using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using SimpleProto.AI.BehaviourTrees;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Reflection;

public class BTEditor : EditorWindow
{
    Vector2 scrollPosition;
    string openedBTPath;
    static BTree curBTrees = null;
    GenericMenu menu;
    bool drawComposite = false;
    List<string> behaviorList;
    Dictionary<String, String> behaviors;
    Dictionary<Rect, String> windows = new Dictionary<Rect, string>();
    //List<Rect> windows = new List<Rect>();
    int lastWindowId = 0;

    [MenuItem("Window/BTEditor")]
    public static void ShowExample()
    {
        BTEditor wnd = GetWindow<BTEditor>();
        wnd.titleContent = new GUIContent("BTEditor");

    }

    public Rect windowRect;

    // Scroll position
    public Vector2 scrollPos = Vector2.zero;
    void OnGUI()
    {
        BeginWindows();
        windowRect = new Rect(rootVisualElement.layout.width * 0.75f, 0, rootVisualElement.layout.width * 0.25f, rootVisualElement.layout.height);
        windowRect = GUILayout.Window(10, windowRect, DoWindow, "������ ����/��� ��������");

        EndWindows();

        if (curBTrees != null)
        {
            HandleEvents(Event.current);

            //PaintNodes();
            //PaintCurves();

        }

        
        /*
        using (var scrollView = new EditorGUILayout.ScrollViewScope(scrollPos, GUILayout.Width(1000), GUILayout.Height(1000)))
        {
            scrollPos = scrollView.scrollPosition;
        }
        // Set up a scroll view
        scrollPos = GUI.BeginScrollView(new Rect(0, 0, position.width, position.height), scrollPos, new Rect(0, 0, 1000, 1000));
        foreach(var key in windows.Keys)
        {
            windows[key] = 
        }
        //// Close the scroll view
        GUI.EndScrollView();

        Rect clickArea = GUILayoutUtility.GetLastRect();
        Event curEv = Event.current;
        behaviors = new Dictionary<string, string>();

        if (clickArea.Contains(curEv.mousePosition) && curEv.type == EventType.ContextClick)
        {
            //Do a thing, in this case a drop down menu

            menu = new GenericMenu();

            behaviorList = new List<string>();
            //GetComponentInChildren<BehaviourTreeNode>()
            foreach (Type type in Assembly.GetAssembly(typeof(BehaviourTreeNode)).GetTypes())
            {
                if (type.IsClass && type.IsSubclassOf(typeof(BehaviourTreeNode))&& type.IsAbstract==false)
                {
                    behaviorList.Add(type.Name);
                    String parent = type.BaseType.Name;
                    //����� ��������
                    behaviors.Add(type.Name, parent);

                }
            }
            foreach (var item in behaviorList)
            {
                AddMenuItemForNode(menu, item);
            }
            menu.ShowAsContext();
            curEv.Use();
        }
        */

    }
    void DoWindow(int unusedWindowID)
    {
        if (GUILayout.Button("������� ����� ������ ���������"))
        {
            string result = EditorUtility.SaveFilePanel("��������� ������", "Assets", "BehaviorTree", "asset");
            result = result.Substring(result.IndexOf("Assets"), result.Length - result.IndexOf("Assets"));
            curBTrees = ScriptableObject.CreateInstance<BTree>();
            curBTrees.name = result.Replace(".asset","");

            AssetDatabase.CreateAsset(curBTrees, result);

        }
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
        foreach (var node in curBTrees.nodes)
        {
            node.HandleEvent(e);
        }

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
            if (type.IsClass && (type.IsSubclassOf(typeof(BehaviourTreeNode))||type.IsSubclassOf(typeof(SimpleProto.AI.BehaviourTrees.BehaviourTreeNode<UnityEngine.GameObject>))) && abstr == false)
            {
                behaviorList.Add(type.Name);
                String fullName = type.AssemblyQualifiedName;
                //����� ��������

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
        BTree.Node node = new BTree.Node(mousPos, nodeName);
        node.type = nodeType;
        node.nodeName = nodeName;
        node.Init();
        curBTrees.nodes.Add(node);

    }

    void AddMenuItemForNode(GenericMenu menu, string menuPath, Vector2 mousPos)
    {
        // the menu item is marked as selected if it matches the current value of m_Color
        menu.AddItem(new GUIContent(menuPath), false, () => OnAppendNodeSelected(menuPath, mousPos));
    }

    void OnEnable()
    {
        AssemblyReloadEvents.beforeAssemblyReload += OnBeforeAssemblyReload;
    }

    void OnDisable()
    {
        AssemblyReloadEvents.beforeAssemblyReload -= OnBeforeAssemblyReload;
    }

    public void OnBeforeAssemblyReload()
    {
        
    }

}
