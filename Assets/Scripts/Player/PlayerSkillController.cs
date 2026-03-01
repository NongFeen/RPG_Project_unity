using Unity.Netcode;
using Unity.VisualScripting;
using UnityEditor.Experimental.GraphView;
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

    
    // [SerializeField]private SkillInstance skillV;
    // [SerializeField]private SkillInstance skillQ;
    // [SerializeField]private SkillInstance skillF;
    // [SerializeField]private SkillBehaviour skillV;
    // [SerializeField]private SkillBehaviour skillQ;
    // [SerializeField]private SkillBehaviour skillF;
    [SerializeField]private SkillLogic skillV;
    [SerializeField]private SkillLogic skillQ;
    [SerializeField]private SkillLogic skillF;

    
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
        // skillV = AddSkill(classSkillData.skillV);
        // skillQ = AddSkill(classSkillData.skillQ);
        // skillF = AddSkill(classSkillData.skillF);
        skillV = CreateSkill(classSkillData.skillV);
        skillQ = CreateSkill(classSkillData.skillQ);
        skillF = CreateSkill(classSkillData.skillF);
    }
    SkillLogic CreateSkill(SkillDefinition def)
    {
        switch(def.skillType)
        {
            case SkillBehaviourType.Dash:
                return new DashSkillLogic(this, def);

            case SkillBehaviourType.ExampleProjectile:
                return new ProjectileSkillLogic(this, def);
        }

        return null;
    }
    // SkillBehaviour AddSkill(SkillDefinition def)
    // {
    //     var behaviour = gameObject.AddComponent(def.GetBehaviourType()) as SkillBehaviour;
    //     behaviour.Initialize(player, def);
    //     return behaviour;
    // }
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
                // TryUseSkill(skillV);
                skillV.TryActivate(Camera.main.ScreenToWorldPoint(inputReader.AimPosition));
                // skillV.ActivateSkill(Camera.main.ScreenToWorldPoint(inputReader.AimPosition));
                break;
            case 1:
                skillQ.TryActivate(Camera.main.ScreenToWorldPoint(inputReader.AimPosition));
                // skillQ.ActivateSkill(Camera.main.ScreenToWorldPoint(inputReader.AimPosition));
                break;
            case 2:
                // TryUseSkill(skillF);
                skillF.TryActivate(Camera.main.ScreenToWorldPoint(inputReader.AimPosition));
                // skillF.ActivateSkill(Camera.main.ScreenToWorldPoint(inputReader.AimPosition));
                break;
        }
    }
        // controller.SpawnProjectileServerRpc(dir, definition.magicNumber1);
    [ServerRpc]
    public void SpawnProjectileServerRpc(float damage, Vector2 direction, SkillBehaviourType skillId)
    {
        // This check is the authoritative gate to ensure this is only done on the server.
        if (!IsServer) return;
        SkillDefinition def = classSkillDataBase.GetSkillDefinition(skillId);
        GameObject prefabToUse = def.projectilePrefab;
        prefabToUse.TryGetComponent<NetworkObject>(out NetworkObject netObjToUse);
        if (prefabToUse == null) return;
        
        // Calculate rotation
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        Quaternion rot = Quaternion.Euler(0, 0, angle);

        NetworkObject netObj = NetworkManager.Singleton.SpawnManager.InstantiateAndSpawn(netObjToUse, NetworkManager.Singleton.LocalClientId,false,false,false,player.transform.position,rot);

        GameObject proj = netObj.gameObject;

        if (proj.TryGetComponent<ServerProjectile>(out ServerProjectile serverProjectile))
        {
            serverProjectile.OnSpawn(direction, damage ,false, 1f);
        }
    }
    // void TryUseSkill(SkillInstance skill)
    // {
        // if (!skill.CanUse) return;
        // skill.TriggerCooldown();
        // ActivateSkillServerRpc(skill.definition.skillId, Camera.main.ScreenToWorldPoint(inputReader.AimPosition));
    // }

    // [ServerRpc]
    // void ActivateSkillServerRpc(SkillId skillId, Vector3 targetPos)
    // {
    //     if (classSkillDataBase == null)
    //     {
    //         Debug.LogError("ClassSkillDatabase is NULL on server", this);
    //         return;
    //     }

    //     if (player == null)
    //     {
    //         Debug.LogError("PlayerStats is NULL on server", this);
    //         return;
    //     }

    //     // 1️⃣ Resolve owner
    //     PlayerStats ownerStats = player;

    //     // 2️⃣ Get skill definition
    //     SkillDefinition def = classSkillDataBase.GetSkillDefinition(skillId);
    //     if (def == null)
    //     {
    //         Debug.LogError($"Skill not found: {skillId}", this);
    //         return;
    //     }

    //     if (def.skillPrefab == null)
    //     {
    //         Debug.LogError($"Skill prefab missing for {skillId}", this);
    //         return;
    //     }

    //     // 3️⃣ Spawn skill object
    //     GameObject skillObj = Instantiate(def.skillPrefab);
    //     NetworkObject netObj = skillObj.GetComponent<NetworkObject>();

    //     if (netObj == null)
    //     {
    //         Debug.LogError($"Skill prefab {def.skillPrefab.name} has no NetworkObject", this);
    //         Destroy(skillObj);
    //         return;
    //     }

    //     netObj.Spawn(true);

    //     // 4️⃣ Initialize & activate
    //     SkillBehaviour behaviour = skillObj.GetComponent<SkillBehaviour>();
    //     behaviour.Initialize(ownerStats);
    //     behaviour.ActivateSkill(targetPos);
    // }

}
