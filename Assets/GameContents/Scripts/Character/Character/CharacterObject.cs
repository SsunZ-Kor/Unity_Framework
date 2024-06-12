using BubbleFighter.Network.Protocol;
using System.Collections.Generic;
using UnityEngine;
using Game.Character.Action;

namespace Game.Character
{
    public partial class CharacterObject : AttackableObject
    {
        public virtual bool IsNetChar => CharCtrl.CtrlType == CharacterController.ControllerType.Net;

        [SerializeField]
        protected Transform _trRoot_Model;
        
        [SerializeField]
        protected CapsuleCollider _capsuleCol;
        public CapsuleCollider CapsuleCol => _capsuleCol;
        
        [SerializeField]
        private Transform _tr_CamPos = null;
        public Transform Tr_CamPos => _tr_CamPos == null ? this.transform : _tr_CamPos;
        public Vector3 CamPos => Tr_CamPos.position;

        public Animator AnimCtrl { get; protected set; }
        public CharacterController CharCtrl { get; protected set; }
        public ActionController ActionCtrl { get; protected set; }
        public StatController StatCtrl { get; protected set; }
        public GameObject Model { get; protected set; }

        public ST_GameRoomUser UserInfo { get; protected set;}
        public CharacterData CharacterData { get; protected set; }
        public WeaponData WeaponData { get; protected set;}
        public List<ActionData> List_ActionData { get; protected set; }

        public SortedDictionary<string, CharacterAttackCollider> _dic_CharAtkCols = new SortedDictionary<string, CharacterAttackCollider>();


        protected virtual void Awake()
        {
            OnAwake_Move();

            StatCtrl = new StatController(this);
            CharCtrl = new CharacterController(this);
            ActionCtrl = new ActionController(this);
        }

        protected virtual void Update()
        {
            StatCtrl.OnUpdate();
            CharCtrl.OnUpdate();
            ActionCtrl.OnUpdate();
        }
        protected virtual void FixedUpdate()
        {
            ActionCtrl.OnFixedUpdate();
        }

        protected virtual void OnDisable()
        {
            ActionCtrl.OnEnd();
        }

        protected virtual void OnDestroy()
        {
            CharCtrl.OnDestroy();
            ActionCtrl.OnDestroy();
        }

        public virtual void Init(ST_GameRoomUser userInfo, CharacterData characterData, WeaponData weaponData, GameObject goModel, List<ActionData> list_ActionDataSet)
        {
            // 유저 정보 세팅(추후 AI 캐릭터가 생긴다면 없을 수도 있음)
            UserInfo = userInfo;

            // 데이터 세팅
            CharacterData = characterData;
            WeaponData = weaponData;

            // 캐릭터 모델 세팅
            if (_trRoot_Model == null)
                _trRoot_Model = this.transform;

            goModel.transform.parent = _trRoot_Model;
            goModel.transform.Reset();

            // 캐릭터 모델 세팅 :: AtkCollider 세팅
            _dic_CharAtkCols.Clear();

            var atkCols = goModel.GetComponentsInChildren<CharacterAttackCollider>();
            if (atkCols != null)
            {
                for (int i = 0; i < atkCols.Length; ++i)
                {
                    var atkCol = atkCols[i];
                    _dic_CharAtkCols.AddOrRefresh(atkCol.name, atkCol);
                    atkCol.gameObject.SetActive(false);
                }
            }

            // 캐릭터 모델 세팅 :: Animoator 캐싱
            AnimCtrl = goModel.GetComponentInChildren<Animator>();

            // 액션 데이터 세팅
            List_ActionData = list_ActionDataSet;

            if (list_ActionDataSet == null || list_ActionDataSet.Count == 0)
                Debug.LogError("Chracter->Init_Async :: 테스트 할 액션이 비어있습니다.");

            for (int i = 0; i < list_ActionDataSet.Count; ++i)
                ActionCtrl.AddActionData(list_ActionDataSet[i], 1f);

            RemoveAllProjInfo();
            ActionCtrl.Init();

            // 스텟 세팅
            StatCtrl.Init();

        }

        public override void SetLayer(int userTeam, int team, int layerMask_Mine, int layerMask_Enemy)
        {
            base.SetLayer(userTeam, team, layerMask_Mine, layerMask_Enemy);

            if (userTeam == team)
            {
                this.Motor.RefreshLayer(LayerMask.NameToLayer("MyCompany_Character"));
            }
            else
            {
                this.Motor.RefreshLayer(LayerMask.NameToLayer("EnemyCompany_Character"));
            }

            if (_dic_CharAtkCols != null && _dic_CharAtkCols.Count > 0)
            {
                var atkColLayer = LayerMask.NameToLayer("MyAttackCollider");
                foreach (var pair in _dic_CharAtkCols)
                {
                    var atkCol = pair.Value;
                    atkCol.gameObject.layer = atkColLayer;
                }
            }
        }

        public CharacterAttackCollider GetCharAtkCol(string gameObjectName)
        {
            return _dic_CharAtkCols?.GetOrNull(gameObjectName);
        }
    }
}