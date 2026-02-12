using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem.Composites;
[RequireComponent(typeof(PlayerStats))]
public class PlayerSkillController : NetworkBehaviour
{
    [SerializeField] public ClassDataBase classSkillDataBase;
    [SerializeField] private ClassSkillData classSkillData;
    [SerializeField] private PlayerStats player;
    [SerializeField] private InputReader inputReader;
    [SerializeField] private bool canCooldown = true;

    
    [SerializeField]private SkillInstance skillV;
    [SerializeField]private SkillInstance skillQ;
    [SerializeField]private SkillInstance skillF;
    
    void Start()
    {
        // player = GetComponent<PlayerStats>();
        if (IsOwner)
        {
            inputReader.SkillUseEvents += OnSkillUse;
        }
        SetUpSkill(player.GetClassType());
        // print($"Skills Set Up for {player.GetClassType()}");
    //V Q F
    //1 2 3
    }
    public void SetUpSkill(ClassType classType)
    {
        // print("Setting up skills");
        classSkillData = classSkillDataBase.GetClassSkillData(classType);
        skillV = new SkillInstance { definition = classSkillData.skillV };
        skillQ = new SkillInstance { definition = classSkillData.skillQ };  
        skillF = new SkillInstance { definition = classSkillData.skillF };
    }
    public override void OnDestroy()
    {
        if (!IsOwner) return;
        inputReader.SkillUseEvents -= OnSkillUse;
        base.OnDestroy();
    }
    void Update()
    {
        if (!IsOwner) return;
        if (canCooldown)
        {
            skillV.Tick(Time.deltaTime);
            skillQ.Tick(Time.deltaTime);
            skillF.Tick(Time.deltaTime);
        }
    }
    void OnSkillUse(int skillIndex)
    {
        if (!IsOwner) return;

        switch (skillIndex)
        {
            case 0:
                TryUseSkill(skillV);
                break;
            case 1:
                TryUseSkill(skillQ);
                break;
            case 2:
                TryUseSkill(skillF);
                break;
        }
    }
    void TryUseSkill(SkillInstance skill)
    {
        if (!skill.CanUse) return;
        skill.TriggerCooldown();
        ActivateSkillServerRpc(skill.definition.skillId, Camera.main.ScreenToWorldPoint(inputReader.AimPosition));
    }

    [ServerRpc]
    void ActivateSkillServerRpc(SkillId skillId, Vector3 targetPos)
    {
        if (classSkillDataBase == null)
        {
            Debug.LogError("ClassSkillDatabase is NULL on server", this);
            return;
        }

        if (player == null)
        {
            Debug.LogError("PlayerStats is NULL on server", this);
            return;
        }

        // 1️⃣ Resolve owner
        PlayerStats ownerStats = player;

        // 2️⃣ Get skill definition
        SkillDefinition def = classSkillDataBase.GetSkillDefinition(skillId);
        if (def == null)
        {
            Debug.LogError($"Skill not found: {skillId}", this);
            return;
        }

        if (def.skillPrefab == null)
        {
            Debug.LogError($"Skill prefab missing for {skillId}", this);
            return;
        }

        // 3️⃣ Spawn skill object
        GameObject skillObj = Instantiate(def.skillPrefab);
        NetworkObject netObj = skillObj.GetComponent<NetworkObject>();

        if (netObj == null)
        {
            Debug.LogError($"Skill prefab {def.skillPrefab.name} has no NetworkObject", this);
            Destroy(skillObj);
            return;
        }

        netObj.Spawn(true);

        // 4️⃣ Initialize & activate
        SkillBehaviour behaviour = skillObj.GetComponent<SkillBehaviour>();
        behaviour.Initialize(ownerStats);
        behaviour.ActivateSkill(targetPos);
    }
}
