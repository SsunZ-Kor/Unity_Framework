using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Game.Character.Action;

namespace Game.Character
{
    public class ActionController
    {
        public CharacterObject Owner { get; private set; } = null;

        List<ActionRuntime> _list_ActionRuntime = new List<ActionRuntime>();
        private Dictionary<string, ActionRuntime> _dic_ActionRuntime = new Dictionary<string, ActionRuntime>();

        // 현재 실행중인 액션의 데이터
        public ActionData CurrActionData => _currActionRuntime?.Data;

        // 현재 실행중인 액션
        private ActionRuntime _currActionRuntime;
        public ActionRuntimeSharedInfo ActionRuntimeShared { get; private set; } = new ActionRuntimeSharedInfo();

        public ActionController(CharacterObject owner)
        {
            Owner = owner;
        }

        public void AddActionData(ActionData actionData, float atkFactor)
        {
            if (actionData == null)
                return;

            if (!_dic_ActionRuntime.ContainsKey(actionData.Name))
            {
                var newActionRuntime = new ActionRuntime(Owner, actionData, atkFactor, _list_ActionRuntime.Count);
                _list_ActionRuntime.Add(newActionRuntime);
                _dic_ActionRuntime.Add(actionData.Name, newActionRuntime);
            }
        }

        public void Init()
        {
            foreach (var pair in _dic_ActionRuntime)
                pair.Value.Init();
        }

        public void PlayAction(int actionIdx)
        {
            if (_list_ActionRuntime.CheckIndex(actionIdx))
                PlayAction(_list_ActionRuntime[actionIdx].Data.Name);
        }

        public void PlayAction(string actionName, bool syncPrevActionTime = false)
        {
#if UNITY_EDITOR
            if (string.IsNullOrWhiteSpace(CurrActionData?.Name) || CurrActionData.Name != actionName)
                Debug.Log($"PlayAction :: {actionName}");
#endif
            if (_currActionRuntime != null)
            {
                _currActionRuntime.OnEnd(ActionRuntimeShared, false);

                if (syncPrevActionTime)
                {
                    var prevElapsedTime = ActionRuntimeShared.ElapsedTime;
                    OnEnd();
                    ActionRuntimeShared.ElapsedTime = prevElapsedTime;
                }
                else
                {
                    OnEnd();
                }
            }

#if UNITY_EDITOR
            if (string.IsNullOrWhiteSpace(actionName))
            {
                Debug.LogError("ActionName is Null or White Space");
                return;
            }

            // 액션 찾기
            var actionRuntime = _dic_ActionRuntime.GetOrNull(actionName);
            if (actionRuntime == null)
            {
                Debug.LogError("ActionName is not found :: " + actionName);
                return;
            }
#endif

            // 찾은 액션을 현재 액션에 대입, 실행
            _currActionRuntime = actionRuntime;
            _currActionRuntime.OnStart(ActionRuntimeShared);

#if UNITY_EDITOR
            if (Managers.IsValid && Managers.Net != null)
            {
#endif
                if (!Owner.IsNetChar)
                {
                    if (IntroSceneController.UseUDP)
                    {
                        NetProcess.UDP_Send_BattleAction(_currActionRuntime.ActionIdx);
                    }
                    else
                    {
                        NetProcess.Send_BattleAction(_currActionRuntime.ActionIdx);
                    }
                }
#if UNITY_EDITOR
            }
#endif
        }

        public void OnUpdate()
        {
            if (_currActionRuntime == null)
                return;

           
            if (ActionRuntimeShared.ElapsedTime < _currActionRuntime.Data.eventData.Length)
            {
                if (_currActionRuntime.OnUpdate(ActionRuntimeShared))
                {
                    // 다음 이벤트가 실행 됬다면 호출 스택 종료
                    return;
                }

                ActionRuntimeShared.ElapsedTime += Time.deltaTime;
            }
            else
            {
                // 끝났다면
                ActionRuntimeShared.ElapsedTime = _currActionRuntime.Data.eventData.Length;
                if (_currActionRuntime.OnUpdate(ActionRuntimeShared))
                {
                    // 다음 이벤트가 실행 됬다면 호출 스택 종료
                    return;
                }

                if (_currActionRuntime.OnEnd(ActionRuntimeShared, true))
                {
                    // 다음 이벤트가 실행 됬다면 호출 스택 종료
                    return;
                }

                Debug.Log("OnEnd :: " + _currActionRuntime.Data.Name);
                OnEnd();
            }
        }



        public void OnFixedUpdate()
        {
            if (_currActionRuntime == null)
                return;

            _currActionRuntime.OnFixedUpdate(ActionRuntimeShared);
        }

        public void OnEnd()
        {
            if (_currActionRuntime != null)
            {
                _currActionRuntime = null;
                ActionRuntimeShared.Clear();
            }
        }

        public void OnDestroy()
        {
            foreach (var pair in _dic_ActionRuntime)
                pair.Value?.OnFinalized();

            _dic_ActionRuntime.Clear();
        }

        public bool OnTriggerEvent(TriggerEventType triggerEventType)
        {
            if (_currActionRuntime == null)
                return false;

            return _currActionRuntime.OnTriggerEvent(ActionRuntimeShared, triggerEventType);
        }
    }
}