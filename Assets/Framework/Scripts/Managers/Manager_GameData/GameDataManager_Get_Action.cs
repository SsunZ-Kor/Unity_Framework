using Game;
using Game.Character.Action;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Game
{
    public partial class GameDataManager
    {

        public List<ActionData> GetActionData(string rootActionName)
        {
            Dictionary<string, ActionData> dic_Result = null;
            GetActionData(rootActionName, ref dic_Result);

            return dic_Result.Values.ToList();
        }

        private void GetActionData(string actionName, ref Dictionary<string, ActionData> dic_Result)
        {
            if (string.IsNullOrWhiteSpace(actionName))
                return;

            if (dic_Result != null && dic_Result.ContainsKey(actionName))
                return;

            var actionData = LoadActionData(actionName);
            if (actionData == null)
                return;

            if (dic_Result == null)
                dic_Result = new Dictionary<string, ActionData>();

            dic_Result.Add(actionName, actionData);

            if (actionData.nextData.list_NextAction == null)
                return;

            for (int i = 0; i < actionData.nextData.list_NextAction.Count; ++i)
            {
                var nextAction = actionData.nextData.list_NextAction[i];
                if (nextAction == null)
                    continue;

                GetActionData(nextAction.NextActionName, ref dic_Result);
                nextAction.NextActionData = dic_Result.GetOrNull(nextAction.NextActionName);
            }

            actionData.nextData.InitNextAction();
        }

        private ActionData LoadActionData(string ActionDataName)
        {
            var actionData = Resources.Load<ActionEventData>($"Data/ActionDatas/{ActionDataName}");
            if (actionData == null)
            {
                Debug.LogError("Not Found ActionData :: " + ActionDataName);
                return null;
            }

            var triggerData = Resources.Load<ActionTriggerData>($"Data/ActionDatas/{ActionDataName}_TRG");
            if (triggerData == null)
            {
                Debug.LogError("Not Found ActionTriggerData :: " + ActionDataName);
                return null;
            }

            var conditionData = Resources.Load<ActionConditionData>($"Data/ActionDatas/{ActionDataName}_CDT");
            if (conditionData == null)
            {
                Debug.LogError("Not Found ActionConditionData :: " + ActionDataName);
                return null;
            }

            var nextData = Resources.Load<ActionNextData>($"Data/ActionDatas/{ActionDataName}_NXT");
            if (nextData == null)
            {
                Debug.LogError("Not Found ActionNextData :: " + ActionDataName);
                return null;
            }


            var newActionData = new ActionData();
            newActionData.eventData = actionData;
            newActionData.triggerData = triggerData;
            newActionData.conditionData = conditionData;
            newActionData.nextData = nextData;

            return newActionData;
        }
    }
}