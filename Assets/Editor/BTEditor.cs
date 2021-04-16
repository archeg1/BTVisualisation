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
    static BTree? curBTrees = null;
    GenericMenu menu;
    bool drawComposite = false;
    List<string> behaviorList;
    Dictionary<String, String> behaviors;
    List<Rect> windows = new List<Rect>();
    int lastWindowId = 0;

    [MenuItem("Window/BTEditor")]
    public static void ShowExample()
    {
        BTEditor wnd = GetWindow<BTEditor>();
        wnd.titleContent = new GUIContent("BTEditor");

    }
    []
    public Rect windowRect;

    // Scroll position
    public Vector2 scrollPos = Vector2.zero;
    void OnGUI()
    {
        BeginWindows();
        windowRect = new Rect(rootVisualElement.layout.width * 0.75f, 0, rootVisualElement.layout.width * 0.25f, rootVisualElement.layout.height);
        windowRect = GUILayout.Window(10, windowRect, DoWindow, "Панель созд/ред деревьев");

        EndWindows();

        using (var scrollView = new EditorGUILayout.ScrollViewScope(scrollPos, GUILayout.Width(1000), GUILayout.Height(1000)))
        {
            scrollPos = scrollView.scrollPosition;
        }
        // Set up a scroll view
        scrollPos = GUI.BeginScrollView(new Rect(0, 0, position.width, position.height), scrollPos, new Rect(0, 0, 1000, 1000));
        if(drawComposite)
        {
            BeginWindows();
            lastWindowId += 1;
            windowRect = new Rect(100, 100, 90, 50);
            windowRect = GUILayout.Window(lastWindowId, windowRect, DoWindow, "Панель созд/ред деревьев");
            EndWindows();
            drawComposite = false;
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
                    //Найти родителя
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

    }
    void DoWindow(int unusedWindowID)
    {
        if(GUILayout.Button("Создать новое дерево поведения"))
        {
            string result = EditorUtility.SaveFilePanel("Сохранить дерево", "Assets", "BehaviorTree", "asset");
            result = result.Substring(result.IndexOf("Assets"),result.Length- result.IndexOf("Assets"));
            var temp = ScriptableObject.CreateInstance<BTree>();
            //temp.name = result.Replace(".asset","");

            AssetDatabase.CreateAsset(temp , result);

        }
        GUI.DragWindow(new Rect(0, 0, 10000, 20));

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

    void OnAppendNodeSelected(object nodeName)
    {
        var nodeType = behaviors[nodeName.ToString()];
        if(nodeType == "Composite")
        {
            drawComposite= true;
        }
        if(nodeType == "BehaviorTreeNode")
        {
            //drawAction=true;
        }
        if(nodeType == "Decorator")
        {
            //drawDecorator=true;
        }
    }


    void AddMenuItemForNode(GenericMenu menu, string menuPath)
    {
        // the menu item is marked as selected if it matches the current value of m_Color
        menu.AddItem(new GUIContent(menuPath), false, OnAppendNodeSelected, menuPath);
    }
    
    
}