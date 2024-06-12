using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Game.Character.Action
{
    public partial class ActionDataEditor
    {
        private class ActionConnectionInfo
        {
            public static GUIStyle guiStyle_Label = null;
            public static Texture2D tex = null;
            public static Color color_Air = new Color(0f, 0.55f, 1f);
            public static Color color_Ground = new Color(0f, 1f, 0.55f);

            public ActionData ActionData_A;
            public ActionData ActionData_B;

            public NextAction next_A;
            public NextAction next_B;

            public static string GetActionConnectorName(string nameA, string nameB)
            {
                if (nameA.CompareTo(nameB) < 0)
                    return $"{nameA}&{nameB}";
                else
                    return $"{nameB}&{nameA}";
            }

            public void SetHandleColorBySpaceType(NextActionSpaceType spaceType, float alpha)
            {
                switch (spaceType)
                {
                    case NextActionSpaceType.Air:
                        Handles.color = new Color(color_Air.r, color_Air.g, color_Air.b, alpha);
                        break;
                    case NextActionSpaceType.Ground:
                        Handles.color = new Color(color_Ground.r, color_Ground.g, color_Ground.b, alpha);
                        break;
                    case NextActionSpaceType.AirAndGround:
                        Handles.color = Color.white;
                        break;
                    default:
                        Handles.color = Color.white;
                        break;
                }
            }

            public void DrawArrow(Vector2 vPos_Start, Vector2 vPos_End, string mark)
            {
                Handles.DrawAAPolyLine(5f, vPos_Start, vPos_End);

                Vector2 vPos_Center = (vPos_Start + vPos_End) * 0.5f;
                Vector2 vDir = (vPos_End - vPos_Start).normalized;

                if (tex != null)
                {
                    var rt_Arrow = new Rect(vPos_Center.x - 7.5f, vPos_Center.y - 7.5f, 15f, 15f);
                    rt_Arrow.x += vDir.x * 7.5f;
                    rt_Arrow.y += vDir.y * 7.5f;

                    var angle = Vector2.Angle(Vector2.down, vDir);
                    if (vDir.x < 0f)
                        angle *= -1f;

                    GUIUtility.RotateAroundPivot(angle, rt_Arrow.center);
                    GUI.DrawTexture(rt_Arrow, tex, ScaleMode.ScaleToFit, true, 1f, Handles.color, 0f, 0f);
                    GUI.matrix = Matrix4x4.identity;
                }

                if (!string.IsNullOrWhiteSpace(mark))
                { 
                    var rt_Mark = new Rect(vPos_Center.x - 7.5f, vPos_Center.y - 7.5f, 15f, 15f);
                    rt_Mark.x -= vDir.x * 7.5f;
                    rt_Mark.y -= vDir.y * 7.5f;

                    guiStyle_Label.normal.textColor = Color.black;

                    --rt_Mark.x;
                    GUI.Label(rt_Mark, mark, guiStyle_Label);
                    rt_Mark.x += 2;
                    GUI.Label(rt_Mark, mark, guiStyle_Label);
                    --rt_Mark.x;

                    --rt_Mark.y;
                    GUI.Label(rt_Mark, mark, guiStyle_Label);
                    rt_Mark.y += 2;
                    GUI.Label(rt_Mark, mark, guiStyle_Label);
                    --rt_Mark.y;

                    guiStyle_Label.normal.textColor = Color.white;
                    GUI.Label(rt_Mark, mark, guiStyle_Label);
                }
            }

            public string GetTimeTypeToMark(NextAction nextAction)
            {
                switch (nextAction.TimeType)
                {
                    case NextActionTimeType.All:
                        return "A";
                    case NextActionTimeType.Custom:
                        if (nextAction.isEqualLength)
                            return "~E";
                        else
                            return "C";
                    case NextActionTimeType.End:
                        return "E";
                }

                return null;
            }

            public void OnGUI(Vector2 offset, bool bFocus)
            {
                if (tex == null)
                    tex = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/GameContents/Scripts/Character/Editor/arrow.png");

                if (guiStyle_Label == null)
                {
                    guiStyle_Label = new GUIStyle("BoldLabel");
                    guiStyle_Label.fontSize += 2;
                    guiStyle_Label.alignment = TextAnchor.MiddleCenter;
                }

                Handles.BeginGUI();

                var vPos_A = ActionData_A.nextData.EditorNodeCenterPosition + offset;
                var vPos_B = ActionData_B.nextData.EditorNodeCenterPosition + offset;

                var alpha = bFocus ? 1f : 0.1f;
                if (next_A != null)
                {
                    SetHandleColorBySpaceType(next_A.SpaceType, alpha);
                    var mark = bFocus ? GetTimeTypeToMark(next_A) : null;

                    if (next_B == null)
                    {
                        DrawArrow(vPos_A, vPos_B, mark);
                    }
                    else
                    {
                        var vDir = (vPos_B - vPos_A).normalized;
                        var offsetPos = new Vector2(vDir.y, -vDir.x) * 10f;
                        DrawArrow(vPos_A + offsetPos, vPos_B + offsetPos, mark);
                    }

                    Handles.color = Color.white;
                }

                if (next_B != null)
                {
                    SetHandleColorBySpaceType(next_B.SpaceType, alpha);
                    var mark = bFocus ? GetTimeTypeToMark(next_B) : null;

                    if (next_A == null)
                    {
                        DrawArrow(vPos_B, vPos_A, mark);
                    }
                    else
                    {
                        var vDir = (vPos_A - vPos_B).normalized;
                        var offsetPos = new Vector2(vDir.y, -vDir.x) * 10f;
                        DrawArrow(vPos_B + offsetPos, vPos_A + offsetPos, mark);
                    }

                    Handles.color = Color.white;
                }

                Handles.EndGUI();
            }
        }

        private SortedDictionary<string, ActionData> _dic_ActionSet = new SortedDictionary<string, ActionData>();
        private SortedDictionary<string, ActionConnectionInfo> _dic_ActionConnectionInfo = new SortedDictionary<string, ActionConnectionInfo>();

        public GUIStyle defaultNodeStyle;
        public GUIStyle selectedNodeStyle;

        private Vector2 offset;

        private void OnEnable_NextActionNode()
        {
            defaultNodeStyle = new GUIStyle();
            defaultNodeStyle.normal.background = EditorGUIUtility.Load("builtin skins/darkskin/images/node1.png") as Texture2D;
            defaultNodeStyle.border = new RectOffset(12, 12, 12, 12);

            selectedNodeStyle = new GUIStyle();
            selectedNodeStyle.normal.background = EditorGUIUtility.Load("builtin skins/darkskin/images/node1 on.png") as Texture2D;
            selectedNodeStyle.border = new RectOffset(12, 12, 12, 12);
        }

        private void OnGUI_NextActionNode()
        {
            var rect = EditorGUILayout.BeginVertical();
            {
                AutoFlexibleSpace();

                if (rect != Rect.zero)
                {
                    GUI.BeginGroup(rect);
                    {
                        var rt_Grid = rect;
                        rt_Grid.x = 0f;
                        rt_Grid.y = 0f;

                        // 그리드 드리기
                        DrawGrid(rt_Grid, 20, 0.2f, Color.gray);
                        DrawGrid(rt_Grid, 100, 0.4f, Color.gray);

                        // 화살표 그리기
                        if (_dic_ActionConnectionInfo != null && _dic_ActionConnectionInfo.Count > 0)
                        {
                            foreach (var pair in _dic_ActionConnectionInfo)
                            {
                                var connectionInfo = pair.Value;

                                bool bFocus = connectionInfo.ActionData_A == _selectedActionData || connectionInfo.ActionData_B == _selectedActionData;
                                connectionInfo.OnGUI(offset, bFocus);
                            }
                        }

                        // 노드 그리기
                        if (_dic_ActionSet != null && _dic_ActionSet.Count > 0)
                        {
                            foreach (var pair in _dic_ActionSet)
                            {
                                var actionData = pair.Value;
                                actionData.OnGUI_NextActionNode(offset, _selectedActionData == actionData, defaultNodeStyle, selectedNodeStyle);

                                System.Action selectedCallback = () =>
                                {
                                    _roList_ActionData.index = _roList_ActionData.list.IndexOf(actionData);
                                    _roList_ActionData.onSelectCallback?.Invoke(_roList_ActionData);

                                };

                                // 노드 입력 처리
                                if (actionData.ProcessEvents(Event.current, offset, defaultNodeStyle, selectedNodeStyle, selectedCallback))
                                {
                                    GUI.changed = true;
                                }
                            }
                        }

                        // 입력 처리
                        ProcessEvents(Event.current);
                    }
                    GUI.EndGroup();

                    // 그리드 Offset 표기
                    var guiContent_offset = new GUIContent($"Scroll : {-offset.x}, {-offset.y}");
                    var size_offset = GUI.skin.button.CalcSize(guiContent_offset);
                    var rt_Lable_Offset = new Rect(rect.x, rect.yMax - size_offset.y, size_offset.x, size_offset.y);
                    if (GUI.Button(rt_Lable_Offset, guiContent_offset))
                        offset = Vector2.zero;
                }
            }
            EditorGUILayout.EndVertical();

            if (GUI.changed) 
                Repaint();
        }

        private void DrawGrid(Rect bounds, float gridSpacing, float gridOpacity, Color gridColor)
        {
            int widthDivs = Mathf.CeilToInt(bounds.width / gridSpacing);
            int heightDivs = Mathf.CeilToInt(bounds.height / gridSpacing);

            Handles.BeginGUI();
            Handles.color = new Color(gridColor.r, gridColor.g, gridColor.b, gridOpacity);

            Vector3 newOffsetWidth = new Vector3(bounds.x + (offset.x % gridSpacing), bounds.y, 0);
            if (newOffsetWidth.x < bounds.x)
                newOffsetWidth.x += gridSpacing;

            Vector3 newOffsetHeight = new Vector3(bounds.x, bounds.y + (offset.y % gridSpacing), 0);
            if (newOffsetHeight.y < bounds.y)
                newOffsetHeight.y += gridSpacing;

            for (int i = 0; i < widthDivs; ++i)
            {
                var startPos = newOffsetWidth;
                startPos.x += gridSpacing * i;

                var endPos = startPos;
                endPos.y += bounds.height;

                Handles.DrawLine(startPos, endPos);
            }

            for (int i = 0; i < heightDivs; ++i)
            {
                var startPos = newOffsetHeight;
                startPos.y += gridSpacing * i;

                var endPos = startPos;
                endPos.x += bounds.width;

                Handles.DrawLine(startPos, endPos);
            }

            Handles.color = Color.white;
            Handles.EndGUI();
        }

        private void ProcessEvents(Event e)
        {
            switch (e.type)
            {
                case EventType.MouseDown:
                    //if (e.button == 0)
                    //{
                    //    ClearConnectionSelection();
                    //}
                    //
                    //if (e.button == 1)
                    //{
                    //    ProcessContextMenu(e.mousePosition);
                    //}
                    //break;
                case EventType.MouseDrag:
                    if (e.button == 0)
                    {
                        OnDrag(e.delta);
                    }
                    break;
            }
        }


        private void OnDrag(Vector2 delta)
        {
            offset += delta;

            //if (nodes != null)
            //{
            //    for (int i = 0; i < nodes.Count; i++)
            //    {
            //        nodes[i].Drag(delta);
            //    }
            //}

            GUI.changed = true;
        }

        private void RefreshActionSet(bool bForce)
        {
            if (_selectedActionData == null)
            {
                _dic_ActionSet.Clear();
                _dic_ActionConnectionInfo.Clear();
                return;
            }

            if (!bForce && _dic_ActionSet.ContainsKey(_selectedActionData.Name))
                return;

            _dic_ActionSet.Clear();
            _dic_ActionConnectionInfo.Clear();

            Vector2 minBounds = Vector2.positiveInfinity;
            Vector2 maxBounds = Vector2.negativeInfinity;
            CollectActionByNextInfo(_selectedActionData, ref _dic_ActionSet, ref minBounds, ref maxBounds);

            if (minBounds.x > float.MaxValue)
                minBounds.x = 0f;
            if (minBounds.y > float.MaxValue)
                minBounds.y = 0f;
            if (maxBounds.x < float.MinValue)
                maxBounds.x = 0f;
            if (maxBounds.y < float.MinValue)
                maxBounds.y = 0f;

            var list_UninitializedNode = new List<ActionData>(_dic_ActionSet.Count);
            foreach (var pair in _dic_ActionSet)
            {
                var actionData = pair.Value;
                if (!actionData.nextData.isInitNodePositioin)
                    list_UninitializedNode.Add(actionData);
            }

            var startPos = new Vector2(maxBounds.x, minBounds.y);

            for (int i = 0; i < list_UninitializedNode.Count; ++i)
            {
                startPos.y += (ActionNextData.EditorNodeSize.y + 50f);

                list_UninitializedNode[i].nextData.isInitNodePositioin = true;
                list_UninitializedNode[i].nextData.EditorNodePosition = startPos;
            }
        }

        private void CollectActionByNextInfo(ActionData actionData, ref SortedDictionary<string, ActionData> result, ref Vector2 minBounds, ref Vector2 maxBounds)
        {
            if (result == null)
                result = new SortedDictionary<string, ActionData>();

            if (result.ContainsKey(actionData.Name))
                return;

            result.Add(actionData.Name, actionData);

            var nodePos = actionData.nextData.EditorNodePosition;
            if (actionData.nextData.isInitNodePositioin)
            {
                var nodeBoundMax = nodePos + ActionNextData.EditorNodeSize;

                if (maxBounds.x < nodeBoundMax.x)
                    maxBounds.x = nodeBoundMax.x;
                if (maxBounds.y < nodeBoundMax.y)
                    maxBounds.y = nodeBoundMax.y;

                if (minBounds.x > nodePos.x)
                    minBounds.x = nodePos.x;
                if (minBounds.y > nodePos.y)
                    minBounds.y = nodePos.y;
            }

            if (actionData.nextData == null
                || actionData.nextData.list_NextAction == null)
                return;

            for (int i = 0; i < actionData.nextData.list_NextAction.Count; ++i)
            {
                var nextAction = actionData.nextData.list_NextAction[i];
                if (nextAction == null || string.IsNullOrWhiteSpace(nextAction.NextActionName))
                    continue;

                var nextActionData = this._dic_ActionDatas.GetOrNull(nextAction.NextActionName);
                if (nextActionData == null)
                    continue;

                if (actionData != nextActionData)
                {
                    string connectorName = ActionConnectionInfo.GetActionConnectorName(actionData.Name, nextActionData.Name);
                    var connectionInfo = _dic_ActionConnectionInfo.GetOrCreate(connectorName);

                    if (actionData.Name.CompareTo(nextActionData.Name) < 0)
                    {
                        connectionInfo.ActionData_A = actionData;
                        connectionInfo.ActionData_B = nextActionData;
                        connectionInfo.next_A = nextAction;
                    }
                    else
                    {
                        connectionInfo.ActionData_A = nextActionData;
                        connectionInfo.ActionData_B = actionData;
                        connectionInfo.next_B = nextAction;
                    }
                }

                CollectActionByNextInfo(nextActionData, ref result, ref minBounds, ref maxBounds);
            }
        }
    }

}