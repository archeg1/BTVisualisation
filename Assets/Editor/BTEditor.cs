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
    BTree curBTrees;
    List<string> behaviorList;
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
        windowRect = new Rect(rootVisualElement.layout.width*0.75f, 0, rootVisualElement.layout.width * 0.25f, rootVisualElement.layout.height);
        windowRect = GUILayout.Window(10, windowRect, DoWindow, "������ ����/��� ��������");
        EndWindows();

        using (var scrollView = new EditorGUILayout.ScrollViewScope(scrollPos, GUILayout.Width(1000), GUILayout.Height(1000)))
        {
            scrollPos = scrollView.scrollPosition;
        }
        // Set up a scroll view
        //scrollPos = GUI.BeginScrollView(new Rect(0, 0, position.width, position.height), scrollPos, new Rect(0, 0, 1000, 1000));
        
        //// Close the scroll view
        //GUI.EndScrollView();

        Rect clickArea = GUILayoutUtility.GetLastRect();
        Event curEv = Event.current;

        if (clickArea.Contains(curEv.mousePosition) && curEv.type == EventType.ContextClick)
        {
            //Do a thing, in this case a drop down menu

            GenericMenu menu = new GenericMenu();
            behaviorList = new List<string>();
            //GetComponentInChildren<BehaviourTreeNode>()
            foreach (Type type in Assembly.GetAssembly(typeof(BehaviourTreeNode)).GetTypes())
            {
                if (type.IsClass && type.IsSubclassOf(typeof(BehaviourTreeNode))&& type.IsAbstract==false)
                {
                    behaviorList.Add(type.Name);
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
        if(GUILayout.Button("������� ����� ������ ���������"))
        {
            string result = EditorUtility.SaveFilePanel("��������� ������", "Assets", "BehaviorTree", "asset");
            result = result.Substring(result.IndexOf("Assets"),result.Length- result.IndexOf("Assets"));
            var temp = ScriptableObject.CreateInstance<BTree>();
            //temp.name = result.Replace(".asset","");

            AssetDatabase.CreateAsset(temp , result);

        }
        GUI.DragWindow();
        
    }
    public void CreateGUI()
    {
        // Each editor window contains a root VisualElement object
        VisualElement root = rootVisualElement;
        float temp = root.layout.width;
        
    }

    public void Update()
    {
        
    }

    void OnAppendNodeSelected(object nodeName)
    {

    }


    void AddMenuItemForNode(GenericMenu menu, string menuPath)
    {
        // the menu item is marked as selected if it matches the current value of m_Color
        menu.AddItem(new GUIContent(menuPath), false, OnAppendNodeSelected, menuPath);
    }
}