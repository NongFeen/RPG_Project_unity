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
    [SerializeField]public SkillLogic SkillV;
    [SerializeField]public SkillLogic SkillQ;
    [SerializeField]public SkillLogic SkillF;

    
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
        SkillV = CreateSkill(classSkillData.skillV);
        SkillQ = CreateSkill(classSkillData.skillQ);
        SkillF = CreateSkill(classSkillData.skillF);
        UIManager.Instance.SetUpSkill(gameObject);
    }
    SkillLogic CreateSkill(SkillDefinition def)
    {
        //switch back to use database
        switch(def.skillType)
        {
            case SkillBehaviourType.Dash:
                return new DashSkillLogic(this, def);
            case SkillBehaviourType.ExampleProjectile:
                return new ProjectileSkillLogic(this, def);
            case SkillBehaviourType.DinenDash:
                return new DineAndDashSkillLogic(this, def);
        }

        return null;
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
            SkillV.Tick(Time.deltaTime);
            SkillQ.Tick(Time.deltaTime);
            SkillF.Tick(Time.deltaTime);
        }
    }
    void OnSkillUse(int skillIndex)
    {
        if (!IsOwner) return;

        switch (skillIndex)
        {
            case 0:
                // TryUseSkill(skillV);
                SkillV.TryActivate(Camera.main.ScreenToWorldPoint(inputReader.AimPosition));
                // skillV.ActivateSkill(Camera.main.ScreenToWorldPoint(inputReader.AimPosition));
                break;
            case 1:
                SkillQ.TryActivate(Camera.main.ScreenToWorldPoint(inputReader.AimPosition));
                // skillQ.ActivateSkill(Camera.main.ScreenToWorldPoint(inputReader.AimPosition));
                break;
            case 2:
                // TryUseSkill(skillF);
                SkillF.TryActivate(Camera.main.ScreenToWorldPoint(inputReader.AimPosition));
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
}
