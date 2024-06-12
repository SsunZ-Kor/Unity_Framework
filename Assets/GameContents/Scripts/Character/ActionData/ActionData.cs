using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Game.Character.Action
{
    public class ActionData
    {
        public string Name => eventData.ActionName;
        public string Desc => eventData.ActionDesc;
        public float Length => eventData.Length;

        public string eventFileName => eventData.ActionName;
        public string triggerFileName => eventData.ActionName + "_TRG";
        public string conditionFileName => eventData.ActionName + "_CDT";
        public string nextFileName => eventData.ActionName + "_NXT";

        public ActionEventData eventData;
        public ActionTriggerData triggerData;
        public ActionConditionData conditionData;
        public ActionNextData nextData;

#if UNITY_EDITOR

        static GUIStyle guiStyle_Label = null;
        bool isDragged = false;

        public void OnGUI_Detail()
        {
            EditorGUILayout.BeginVertical("Box");
            {
                EditorGUILayout.LabelField(Name);
                eventData.ActionDesc = EditorGUILayout.TextField("ActionDesc", eventData.ActionDesc);

                EditorGUILayout.BeginHorizontal();
                {
                    eventData.Length = EditorGUILayout.DelayedFloatField("Length", eventData.Length);
                    eventData.Length = EditorGUILayout.DelayedFloatField(eventData.Length * 30f, GUILayout.Width(50f)) / 30f;
                    if (eventData.Length < 0f)
                        eventData.Length = 0f;
                }
                EditorGUILayout.EndHorizontal();
            }
            EditorGUILayout.EndVertical();
        }

        public void OnGUI_NextActionNode(Vector2 offset, bool isSelected, GUIStyle defaultNodeStyle, GUIStyle selectedNodeStyle)
        {
            if (guiStyle_Label == null)
            {
                guiStyle_Label = new GUIStyle("BoldLabel");
                guiStyle_Label.fontSize += 2;
                guiStyle_Label.alignment = TextAnchor.MiddleCenter;
            }

            var rect = new Rect(this.nextData.EditorNodePosition + offset, ActionNextData.EditorNodeSize);

            GUI.Box(rect, string.Empty, isSelected ? selectedNodeStyle : defaultNodeStyle);
            GUI.Label(rect, Name, guiStyle_Label);
        }

        public bool ProcessEvents(Event e, Vector2 offset, GUIStyle defaultNodeStyle, GUIStyle selectedNodeStyle, System.Action SelectedCallback)
        {
            var rect = new Rect( this.nextData.EditorNodePosition + offset, ActionNextData.EditorNodeSize);

            switch (e.type)
            {
                case EventType.MouseDown:
                    if (e.button == 0)
                    {
                        if (rect.Contains(e.mousePosition))
                        {
                            isDragged = true;
                            GUI.changed = true;
                            SelectedCallback?.Invoke();
                        }
                    }
                    else if (e.button == 1)
                    {
                        if (rect.Contains(e.mousePosition))
                        {
                            isDragged = true;
                            GUI.changed = true;
                        }
                    }

                    //if (e.button == 1 && isSelected && rect.Contains(e.mousePosition))
                    //{
                    //    ProcessContextMenu();
                    //    e.Use();
                    //}
                    break;

                case EventType.MouseUp:
                    if (isDragged)
                    {
                        isDragged = false;
                        if (this.nextData.EditorNodePosition.x > 0f)
                        {
                            var remainder = this.nextData.EditorNodePosition.x % 20f;
                            if (remainder < 10f)
                                this.nextData.EditorNodePosition.x -= remainder;
                            else
                                this.nextData.EditorNodePosition.x -= remainder - 20f;
                        }
                        else
                        {
                            var remainder = this.nextData.EditorNodePosition.x % 20f;
                            if (remainder > -10f)
                                this.nextData.EditorNodePosition.x -= remainder;
                            else
                                this.nextData.EditorNodePosition.x -= remainder + 20f;
                        }

                        if (this.nextData.EditorNodePosition.y > 0f)
                        {
                            var remainder = this.nextData.EditorNodePosition.y % 20f;
                            if (remainder < 10f)
                                this.nextData.EditorNodePosition.y -= remainder;
                            else
                                this.nextData.EditorNodePosition.y -= remainder - 20f;
                        }
                        else
                        {
                            var remainder = this.nextData.EditorNodePosition.y % 20f;
                            if (remainder > -10f)
                                this.nextData.EditorNodePosition.y -= remainder;
                            else
                                this.nextData.EditorNodePosition.y -= remainder + 20f;
                        }

                        return true;
                    }

                    break;

                case EventType.MouseDrag:
                    if ((e.button == 0 || e.button == 1) && isDragged)
                    {
                        this.nextData.EditorNodePosition += e.delta;
                        e.Use();
                        return true;
                    }
                    break;
            }

            return false;
        }
#endif
    }

    public class ActionRuntimeSharedInfo
    {
        public float ElapsedTime;
        public string LastAnimStateName;
        public int CurrActiveEventIdx;
        public int CurrActiveNextIdx;

        public LinkedList<IActionEventRuntime> Llist_ActionEventRuntime_Active = new LinkedList<IActionEventRuntime>();
        public LinkedList<NextAction> Llist_ActionNext_Active = new LinkedList<NextAction>();

        public void Clear()
        {
            ElapsedTime = 0f;
            CurrActiveEventIdx = 0;
            CurrActiveNextIdx = 0;

            Llist_ActionEventRuntime_Active.Clear();
            Llist_ActionNext_Active.Clear();
        }
    }

    public class ActionRuntime
    {
        public CharacterObject Owner { get; private set; }
        public ActionData Data = null;
        public int ActionIdx { get; private set; }
        
        private float _atkFactor = 0f;

        public List<IActionEventRuntime> _list_ActionEventRuntime;

        public ActionRuntime(CharacterObject owner, ActionData actionDataSet, float atkFactor, int actionIdx)
        {
            Owner = owner;
            Data = actionDataSet;
            ActionIdx = actionIdx;
            _atkFactor = atkFactor;

            if (Owner == null || Data == null)
                throw new System.Exception("ActionDataRuntime->Owner or Data is Null");

            _list_ActionEventRuntime = new List<IActionEventRuntime>(Data.eventData.list_ActionEvents.Count);

            for (int i = 0; i < Data.eventData.list_ActionEvents.Count; ++i)
            {
                var eventData = Data.eventData.list_ActionEvents[i];
                if (eventData == null)
                    continue;

                var actionRuntime = Data.eventData.list_ActionEvents[i].CreateRuntime(owner, this);
                _list_ActionEventRuntime.Add(actionRuntime);
            }
        }

        public void Init()
        {
            if (_list_ActionEventRuntime != null)
            {
                for (int i = 0; i < _list_ActionEventRuntime.Count; ++i)
                    _list_ActionEventRuntime[i].Init();
            }
        }

        public void OnStart(ActionRuntimeSharedInfo runtimeInfo)
        {
            // 이미 경과한 이벤트 스킵
            if (runtimeInfo.ElapsedTime > 0f) 
            {
                while (runtimeInfo.CurrActiveEventIdx < _list_ActionEventRuntime.Count)
                {
                    var eventRuntime = _list_ActionEventRuntime[runtimeInfo.CurrActiveEventIdx];
                    if (!eventRuntime.CheckStart(runtimeInfo.ElapsedTime))
                        break;

                    ++runtimeInfo.CurrActiveEventIdx;
                    if (eventRuntime.IgnoreOnNetCharCtrl && Owner.CharCtrl.CtrlType == CharacterController.ControllerType.Net)
                        continue;

                    if (eventRuntime.isDurationType)
                    {
                        eventRuntime.OnStart(runtimeInfo.ElapsedTime);
                        runtimeInfo.Llist_ActionEventRuntime_Active.AddLast(eventRuntime);
                    }
                    else if (eventRuntime.isStartOnSyncTime)
                    {
                        eventRuntime.OnStart(runtimeInfo.ElapsedTime);
                    }
                }
            }

            OnUpdate_StartEvent(runtimeInfo);
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="runtimeInfo"></param>
        /// <returns>Play Next Event</returns>
        public bool OnUpdate(ActionRuntimeSharedInfo runtimeInfo)
        {
            OnUpdate_StartEvent(runtimeInfo);
            OnUpdate_ActiveEvent(runtimeInfo);

            OnUpdate_StartTrigger(runtimeInfo);
            return OnUpdate_ActiveTrigger(runtimeInfo);
        }

        public void OnFixedUpdate(ActionRuntimeSharedInfo runtimeInfo)
        {
            // 실행 중인 액션 업데이트
            runtimeInfo.Llist_ActionEventRuntime_Active.ForEachAllNode((node) =>
            {
                var eventRuntime = node.Value;
                eventRuntime.OnFixedUpdate();
            });
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="runtimeInfo"></param>
        /// <returns>Play Next Action</returns>
        public bool OnEnd(ActionRuntimeSharedInfo runtimeInfo, bool CheckTrigger)
        {
            OnEnd_ActiveEvent(runtimeInfo);
            return CheckTrigger && OnEnd_EndTrigger();
        }

        private void OnUpdate_StartEvent(ActionRuntimeSharedInfo runtimeInfo)
        {
            // 실행가능한 액션 체크 및 실행
            while (runtimeInfo.CurrActiveEventIdx < _list_ActionEventRuntime.Count)
            {
                var eventRuntime = _list_ActionEventRuntime[runtimeInfo.CurrActiveEventIdx];
                if (!eventRuntime.CheckStart(runtimeInfo.ElapsedTime))
                    break;

                ++runtimeInfo.CurrActiveEventIdx;
                eventRuntime.OnStart(runtimeInfo.ElapsedTime);
                if (eventRuntime.isDurationType)
                {
                    eventRuntime.OnUpdate(runtimeInfo.ElapsedTime);
                    if (eventRuntime.IsEnd)
                        eventRuntime.OnEnd();
                    else
                        runtimeInfo.Llist_ActionEventRuntime_Active.AddLast(eventRuntime);
                }
            }
        }

        private void OnUpdate_ActiveEvent(ActionRuntimeSharedInfo runtimeInfo)
        {
            // 실행 중인 액션 업데이트 및 완료된 이벤트 삭제
            runtimeInfo.Llist_ActionEventRuntime_Active.ForEachAllNode((node) =>
            {
                var eventRuntime = node.Value;
                eventRuntime.OnUpdate(runtimeInfo.ElapsedTime);
                if (eventRuntime.IsEnd)
                {
                    eventRuntime.OnEnd();
                    node.List.Remove(node);
                }
            });
        }

        private void OnUpdate_StartTrigger(ActionRuntimeSharedInfo runtimeInfo)
        {
            // 체크 가능한 트리거 체크 및 실행
            while (runtimeInfo.CurrActiveNextIdx < Data.nextData.list_NextAction_OnUpdate.Count)
            {
                var nextData = Data.nextData.list_NextAction_OnUpdate[runtimeInfo.CurrActiveNextIdx];
                if (!nextData.CheckStartTime(runtimeInfo.ElapsedTime, this.Data.Length))
                    break;

                ++runtimeInfo.CurrActiveNextIdx;
                runtimeInfo.Llist_ActionNext_Active.AddLast(nextData);
            }
        }

        private bool OnUpdate_ActiveTrigger(ActionRuntimeSharedInfo runtimeInfo)
        {
            // 트리거 체크 및 만료된 트리거 삭제
            var node_Next = runtimeInfo.Llist_ActionNext_Active.First;
            while (node_Next != null)
            {
                var currNode = node_Next;
                var nextAction = node_Next.Value;

                node_Next = node_Next.Next;

                if (nextAction.CheckSpace(Owner.IsAir ? NextActionSpaceType.Air : NextActionSpaceType.Ground)
                    && nextAction.CheckTrigger(Owner)
                    && nextAction.CheckCondition(Owner))
                {
                    Owner.ActionCtrl.PlayAction(nextAction.NextActionName, nextAction.SyncPrevElapsedTime);
                    return true;
                }

                if (nextAction.CheckEndTime(runtimeInfo.ElapsedTime, this.Data.Length))
                    currNode.RemoveSelf();
            }

            return false;
        }

        private void OnEnd_ActiveEvent(ActionRuntimeSharedInfo runtimeInfo)
        {
            // 실행 중인 액션 업데이트 및 완료된 이벤트 삭제
            runtimeInfo.Llist_ActionEventRuntime_Active.ForEachAllNode((node) =>
            {
                var eventRuntime = node.Value;
                eventRuntime.OnEnd();
            });

            runtimeInfo.Llist_ActionEventRuntime_Active.Clear();
        }

        private bool OnEnd_EndTrigger()
        {
            for (int i = 0; i < Data.nextData.list_NextAction_OnEnd.Count; ++i)
            {
                var nextAction = Data.nextData.list_NextAction_OnEnd[i];

                if (nextAction.CheckSpace(Owner.IsAir ? NextActionSpaceType.Air : NextActionSpaceType.Ground)
                    && nextAction.CheckTrigger(Owner)
                    && nextAction.CheckCondition(Owner))
                {
                    Owner.ActionCtrl.PlayAction(nextAction.NextActionName, nextAction.SyncPrevElapsedTime);
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="runtimeInfo"></param>
        /// <param name="eventType"></param>
        /// <returns>Find And Play</returns>
        public bool OnTriggerEvent(ActionRuntimeSharedInfo runtimeInfo, TriggerEventType eventType)
        {
            var list_NextAction = Data.nextData.dic_NextAction_Event.GetOrNull(eventType);
            if (list_NextAction == null)
                return false;

            for (int i = 0; i < list_NextAction.Count; ++i)
            {
                var nextAction = list_NextAction[i];
                if (nextAction.CheckSpace(Owner.IsAir ? NextActionSpaceType.Air : NextActionSpaceType.Ground)
                    && nextAction.CheckTime(runtimeInfo.ElapsedTime, Data.Length)
                    && nextAction.CheckTrigger(Owner)
                    && nextAction.CheckCondition(Owner))
                {
                    Owner.ActionCtrl.PlayAction(nextAction.NextActionName, nextAction.SyncPrevElapsedTime);
                    return true;
                }
            }

            return false;
        }

        public void OnFinalized()
        {
            if (_list_ActionEventRuntime != null)
            {
                for (int i = 0; i < _list_ActionEventRuntime.Count; ++i)
                    _list_ActionEventRuntime[i].OnFinalize();
            }
        }
    }
}