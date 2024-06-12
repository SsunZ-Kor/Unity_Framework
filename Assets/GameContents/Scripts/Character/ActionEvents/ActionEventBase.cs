using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Game.Character.Action;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Game.Character.Action
{
    public abstract class ActionEventBase
    {
        public abstract bool IgnoreOnNetCharCtrl { get; }

        public virtual bool isStartOnSyncTime => false;
        public virtual bool isDurationType => false;
        public virtual float Length => 0f;

        public float StartTime;

        public abstract IActionEventRuntime CreateRuntime(CharacterObject owner, ActionRuntime actionDataRuntime);

        public abstract void GetAssetBundlePackageNames(ref HashSet<string> set_BundlePackageName);

#if UNITY_EDITOR

        [System.NonSerialized]
        public bool isSelected = false;

        // 경고 텍스트 컬러
        public static GUIStyle _titleStyle_Warning = null;
        protected static GUIStyle TitleStyle_Warning
        {
            get
            {
                if (_titleStyle_Warning == null)
                {
                    _titleStyle_Warning = new GUIStyle(GUI.skin.label);
                    _titleStyle_Warning.normal.textColor = Color.yellow;
                    _titleStyle_Warning.hover.textColor = Color.yellow;
                }

                return _titleStyle_Warning;
            }
        }

        public virtual void OnGUI(ActionData actionData, int index)
        {
            EditorGUILayout.BeginHorizontal();
            {
                isSelected = EditorGUILayout.Toggle(isSelected, GUILayout.Width(20f));

                EditorGUILayout.BeginVertical();
                {
                    EditorGUILayout.BeginHorizontal();
                    {
                        StartTime = EditorGUILayout.Slider(StartTime, 0f, actionData.Length);
                        StartTime = Mathf.Clamp(EditorGUILayout.FloatField(StartTime * 30f, GUILayout.Width(50f)) / 30f, 0f, actionData.Length);
                    }
                    EditorGUILayout.EndHorizontal();
                }
                EditorGUILayout.EndVertical();
            }
            EditorGUILayout.EndHorizontal();
        }
#endif

        public abstract ActionEventBase Clone();
    }

    public abstract class ActionEventDurationDataBase : ActionEventBase
    {
        public override bool isStartOnSyncTime => true;
        public override bool isDurationType => true;
        public override float Length => EndTime - StartTime;

        public float EndTime;

#if UNITY_EDITOR
        public override void OnGUI(ActionData actionData, int index)
        {
            EditorGUILayout.BeginHorizontal();
            {
                isSelected = EditorGUILayout.Toggle(isSelected, GUILayout.Width(20f));

                EditorGUILayout.BeginVertical();
                {
                    EditorGUILayout.BeginHorizontal();
                    {
                        StartTime = EditorGUILayout.Slider(StartTime, 0f, actionData.Length);
                        StartTime = Mathf.Clamp(EditorGUILayout.FloatField(StartTime * 30f, GUILayout.Width(50f)) / 30f, 0f, actionData.Length);
                    }
                    EditorGUILayout.EndHorizontal();
                    EditorGUILayout.BeginHorizontal();
                    {
                        EndTime = Mathf.Max(EditorGUILayout.Slider(EndTime, 0f, actionData.Length), StartTime);
                        EndTime = Mathf.Clamp(EditorGUILayout.FloatField(EndTime * 30f, GUILayout.Width(50f)) / 30f, StartTime, actionData.Length);
                    }
                    EditorGUILayout.EndHorizontal();
                }
                EditorGUILayout.EndVertical();
            }
            EditorGUILayout.EndHorizontal();
        }
#endif
    }

    public interface IActionEventRuntime
    {
        bool CheckStart(float actionElapsedTime);
        bool IsEnd { get; }
        bool isDurationType { get; }
        bool isStartOnSyncTime { get; }
        bool IgnoreOnNetCharCtrl { get; }


        void Init();

        void OnStart(float actionElapsedTime);
        void OnUpdate(float actionElapsedTime);
        void OnFixedUpdate();
        void OnEnd();
        void OnFinalize();
    }

    public abstract class ActionEventRuntimeBase<T> : IActionEventRuntime where T : ActionEventBase
    {
        protected CharacterObject _owner;
        protected ActionRuntime _actionDataRuntime;
        protected T _eventData;

        public virtual bool IsEnd => true;
        public virtual bool isDurationType => _eventData.isDurationType;
        public virtual bool isStartOnSyncTime => _eventData.isStartOnSyncTime;
        public virtual bool IgnoreOnNetCharCtrl => _eventData.IgnoreOnNetCharCtrl;


        public ActionEventRuntimeBase(CharacterObject owner, ActionRuntime actionRuntime, T data)
        {
            _owner = owner;
            _eventData = data;
            _actionDataRuntime = actionRuntime;
        }

        public abstract void Init();

        public bool CheckStart(float actionElapsedTime)
        {
            return actionElapsedTime >= _eventData.StartTime;
        }

        public virtual void OnStart(float actionElapsedTime) { }
        public virtual void OnUpdate(float actionElapsedTime) { }
        public virtual void OnFixedUpdate() { }
        public virtual void OnEnd() { }
        public virtual void OnFinalize() { }
    }


    public abstract class ActionEventRuntimeDurationBase<T> : ActionEventRuntimeBase<T> where T : ActionEventDurationDataBase
    {
        protected float deltaTime = 0f;
        protected float elapsedTime = 0f;
        protected float elapsedFactor = 0f;
        public override bool IsEnd => elapsedFactor >= 1f;

        public ActionEventRuntimeDurationBase(CharacterObject owner, ActionRuntime actionRuntime, T data) : base(owner, actionRuntime, data)
        {
        }

        public override void OnStart(float actionElapsedTime)
        {
            elapsedTime = actionElapsedTime - _eventData.StartTime;
            deltaTime = elapsedTime;

            if (_eventData.Length > 0f)
                elapsedFactor = Mathf.Clamp01(elapsedTime / _eventData.Length);
            else
                elapsedFactor = 1f;
        }

        public override void OnUpdate(float actionElapsedTime)
        {
            var prevElapsedtime = elapsedTime;
            elapsedTime = Mathf.Clamp(actionElapsedTime - _eventData.StartTime, 0f, _eventData.Length);
            deltaTime = elapsedTime - prevElapsedtime;

            if (_eventData.Length > 0f)
                elapsedFactor = Mathf.Clamp01(elapsedTime / _eventData.Length);
            else
                elapsedFactor = 1f;
        }

        public override void OnEnd()
        {
            elapsedTime = _eventData.Length;
            elapsedFactor = 1f;
        }
    }
}