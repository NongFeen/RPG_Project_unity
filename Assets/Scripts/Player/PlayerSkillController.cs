using Unity.Netcode;
using UnityEngine;
[RequireComponent(typeof(PlayerStats))]
public class PlayerSkillController : NetworkBehaviour
{
    [SerializeField] public ClassDataBase classSkillDataBase;
    [SerializeField] private ClassSkillData classSkillData;
    [SerializeField] private PlayerStats player;
    [SerializeField] private InputReader inputReader;
    [SerializeField] private PlayerAiming playerAiming;
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
        if (player == null)
        {
            player = GetComponent<PlayerStats>();
        }
        if (playerAiming == null)
        {
            playerAiming = GetComponent<PlayerAiming>();
        }
        if (IsOwner)
        {
            inputReader.SkillUseEvents += OnSkillUse;
        }

        SetUpSkill(player.GetClassType());

        if (player != null)
            player.playerClass.OnValueChanged += OnClassChanged;

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
        if(classSkillData.skillV != null)
        {
            SkillV = CreateSkill(classSkillData.skillV);
        }
        if(classSkillData.skillQ != null)
        {
            SkillQ = CreateSkill(classSkillData.skillQ);
        }
        if(classSkillData.skillF != null)
        {
            SkillF = CreateSkill(classSkillData.skillF);
        }
        if (IsOwner && UIManager.Instance != null)
        {
            UIManager.Instance.SetUpSkill(gameObject);
        }
    }
    SkillLogic CreateSkill(SkillDefinition def)
    {
        return def.skillType switch
        {
            SkillBehaviourType.Dash => new DashSkillLogic(this, def),
            SkillBehaviourType.ExampleProjectile => new ProjectileSkillLogic(this, def),
            SkillBehaviourType.DinenDash => new DineAndDashSkillLogic(this, def),
            SkillBehaviourType.HolyLight => new HolyLightSkillLogic(this, def),
            SkillBehaviourType.HealAndCure => new HealAndCure(this, def),
            SkillBehaviourType.LockedIn => new LockedInSkillLogic(this, def),
            SkillBehaviourType.BoltDart => new BoltDartSkillLogic(this, def),
            SkillBehaviourType.SteelStrong => new SteelStrongSkillLogic(this, def),
            SkillBehaviourType.WellOfBlessing => new WellBlessingSkillLogic(this, def),
            SkillBehaviourType.ChadAura => new ChadAuraSkillLogic(this, def),
            SkillBehaviourType.QucikDraw => new QuickDrawSkillLogic(this, def),
            SkillBehaviourType.Arise => new AriseSkillLogic(this, def),
            SkillBehaviourType.Along => new AlongSkillLogic(this, def),
            SkillBehaviourType.ABigGuy => new ABigGuySkillLogic(this, def),
            _ => null,
        };
    }
    private void OnClassChanged(ClassType oldClass, ClassType newClass)
    {
        SetUpSkill(newClass);
    }

    public override void OnDestroy()
    {
        if (player != null)
            player.playerClass.OnValueChanged -= OnClassChanged;

        if (!IsOwner) return;
        inputReader.SkillUseEvents -= OnSkillUse;
        base.OnDestroy();
    }

    void Update()
    {
        if (!IsOwner) return;
        if (player.IsGhost) return;
        if (canCooldown)
        {
            SkillV?.Tick(Time.deltaTime);
            SkillQ?.Tick(Time.deltaTime);
            SkillF?.Tick(Time.deltaTime);
        }
    }
    void OnSkillUse(int skillIndex)
    {
        if (!IsOwner) return;
        if (player.IsGhost) return;
        if (playerAiming == null) return;

        Vector2 targetPosition = playerAiming.GetAimWorldPosition();

        switch (skillIndex)
        {
            case 0:
                // TryUseSkill(skillV);
                SkillV?.TryActivate(targetPosition);
                // skillV.ActivateSkill(Camera.main.ScreenToWorldPoint(inputReader.AimPosition));
                break;
            case 1:
                SkillQ?.TryActivate(targetPosition);
                // skillQ.ActivateSkill(Camera.main.ScreenToWorldPoint(inputReader.AimPosition));
                break;
            case 2:
                // TryUseSkill(skillF);
                SkillF?.TryActivate(targetPosition);
                // skillF.ActivateSkill(Camera.main.ScreenToWorldPoint(inputReader.AimPosition));
                break;
        }
    }
        // controller.SpawnProjectileServerRpc(dir, definition.magicNumber1);
    [ServerRpc]
    public void SpawnProjectileServerRpc(float damage,Vector2 position, Vector2 direction, SkillBehaviourType skillId, ServerRpcParams rpcParams = default)
    {
        if (!IsServer) return;
        SkillDefinition def = classSkillDataBase.GetSkillDefinition(skillId);
        GameObject prefabToUse = def.projectilePrefab;
        prefabToUse.TryGetComponent<NetworkObject>(out NetworkObject netObjToUse);
        if (prefabToUse == null) return;
        
        // Calculate rotation
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        Quaternion rot = Quaternion.Euler(0, 0, angle);

        ulong senderId = rpcParams.Receive.SenderClientId;
        NetworkObject netObj = NetworkManager.Singleton.SpawnManager.InstantiateAndSpawn(netObjToUse, senderId,false,false,false,position,rot);

        GameObject proj = netObj.gameObject;

        if (proj.TryGetComponent<ServerProjectile>(out ServerProjectile serverProjectile))
        {
            serverProjectile.OnSpawn(direction, damage ,false, 1f);
        }
    }
}
