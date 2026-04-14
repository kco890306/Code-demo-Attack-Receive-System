using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackReceiverDemo : MonoBehaviour
{
    //接收狀態、攻擊
    /// <summary>
    /// <para>此戰鬥單位接收攻擊方法，傳入對方的攻擊資料，傳回計算好的傷害資料</para>
    /// <para>會自行判斷為物傷or法傷，並自動帶入對應的減傷公式</para>
    /// <para>計算完成將直接變更此單位HP值，並觸發多個事件delegate</para>
    /// </summary>
    /// <param name="attackData">對方的攻擊資料，於其技能出手瞬間生成</param>
    public DamageData ReceiveAttackData(AttackData attackData)
    {
        if (CurrentState == StateType.Knockout) return null;//沒死才能接收傷害

        //生成一個空的傷害Data模板
        DamageData newDamageData = new DamageData(attackData.SourceUnit, attackData.AttackType, attackData.DamageType, ReceivedDamageType.TureDamage, 0);
        float attackDamage = attackData.AttackDamage;

        //計算傷害量
        if (attackData.DamageType == DamageType.TrueDamage)//真實傷害，無法減傷
        {
            newDamageData.ReceivedDamageType = ReceivedDamageType.TureDamage;
        }
        else if (attackData.DamageType == DamageType.AD)//物理傷害
        {
            if (CurrentAttributes.AMR > attackDamage * 4)//我方防禦值比對方攻擊傷害高4倍以上
            {
                //僅受到12.5%傷害
                newDamageData.ReceivedDamageType = ReceivedDamageType.AD_Deflected;
                attackDamage *= 0.125f;
            }
            else if (CurrentAttributes.AMR < attackDamage / 2)//對方攻擊傷害比我方防禦值高2倍以上
            {
                //受到全額傷害
                newDamageData.ReceivedDamageType = ReceivedDamageType.AD_Overwhelming;
            }
            else//對方攻擊傷害與我方防禦值接近
            {
                //公式計算
                newDamageData.ReceivedDamageType = ReceivedDamageType.AD_Normal;
                attackDamage = (attackDamage / CurrentAttributes.AMR) * (attackDamage / 2);
            }
        }
        else if (attackData.DamageType == DamageType.AP)//魔法傷害，乘法計算，可超過100%(無傷)
        {
            newDamageData.ReceivedDamageType = ReceivedDamageType.AP;
            float ratio = 1 - ((float)CurrentAttributes.RES * 0.01f);
            ratio = ratio < 0 ? 0 : ratio;//ratio不會低於0
            attackDamage *= ratio;
        }
        else if (attackData.DamageType == DamageType.Heal)//治療，其值為負
        {
            newDamageData.ReceivedDamageType = ReceivedDamageType.TureDamage;
            attackDamage *= 1f;
        }

        newDamageData.DamageAmount = (int)attackDamage;//帶入計算好的傷害量

        foreach (var effectTypeAndInt in attackData.AdditionBuffDic)//施加所有此次攻擊附加的Buff
        {
            unitBuffCtrl.ReceiveEffectStackChange(this, effectTypeAndInt.Key, effectTypeAndInt.Value);
        }

        if (attackData.SourceUnit)//如果有來源
        {
            attackData.SourceUnit.Delegate_DealDamage?.Invoke(newDamageData);//通知來源已經成功造成傷害
        }

        Delegate_ReceiveDamage?.Invoke(newDamageData);//通知:受到傷害事件
        battleCtrl.StartCoro_SummonDamagePopUp(this, newDamageData);//生成傷害數字

        ReceiveHpChange(-newDamageData.DamageAmount);//變更HP

        return newDamageData;
    }
}


[SerializeField]
public class AttackData//攻擊成功時，把此攻擊資料傳給目標，由對方來計算最終傷害
{
    //一個傷害為基礎單位。一次攻擊可能由多個傷害組成，因此會造成多次傷害

    //以下為必要基礎數據
    public BattleUnit SourceUnit;//攻擊來源，若攻擊者隱匿or已死亡，則此攻擊無來源
    public LayerMask TargetFactionLayers;//傷害目標的陣營Layers，給投射物過濾unit用
    public AttackType AttackType;//攻擊方式 : 近戰攻擊 or 投射物攻擊 or AOE攻擊
    public DamageType DamageType;//傷害形式 : 物理 or 法術 or 真實 or 治療
    public int AttackDamage;//基礎傷害量，可能因目標防禦而減傷，也可能因目標脆弱而增傷  //傷害為正，治療為負

    //以下為可選附加數據
    public Dictionary<BuffType, int> AdditionBuffDic = new();//此次攻擊將對目標施加的所有Buff類型、及Buff層數


    public AttackData(BattleUnit nSourceUnit, LayerMask nTargetFactionLayers, AttackType nAttackType, DamageType nDamageType, int nAttackDamage)
    {
        SourceUnit = nSourceUnit;
        TargetFactionLayers = nTargetFactionLayers;
        AttackType = nAttackType;
        DamageType = nDamageType;
        AttackDamage = nAttackDamage;
    }
}

[SerializeField]
public class DamageData//攻擊成功時，把此攻擊資料傳給目標，由對方來計算最終傷害，之後傳回此傷害資料
{
    public BattleUnit SourceUnit;//攻擊來源單位
    public AttackType AttackType;//攻擊方式 : 近戰攻擊 or 投射物攻擊 or AOE攻擊
    public DamageType DamageType;//傷害形式 : 物理 or 法術 or 真實 or 治療
    public ReceivedDamageType ReceivedDamageType;//依據受到的傷害類型與減傷多寡，將區分為幾種受傷類型  //主要影響跳傷數字等視覺效果
    public int DamageAmount;//最終造成的傷害量  //傷害為正，治療為負


    public DamageData(BattleUnit nSourceUnit, AttackType nAttackType, DamageType nDamageType, ReceivedDamageType nReceivedDamageType, int nDamageAmount)
    {
        SourceUnit = nSourceUnit;
        AttackType = nAttackType;
        DamageType = nDamageType;
        ReceivedDamageType = nReceivedDamageType;
        DamageAmount = nDamageAmount;
    }
}

public enum AttackType//攻擊方式，部分道具有針對特定類型攻擊的減傷or易傷，e.g.盾牌機率擋投射物，重甲防咬傷
{
    Direct,//腳本直接指定(dot、反甲、真實傷害)
    Melee,//近戰攻擊
    Projectile,//投射物
    Splash,//範圍濺射、地圖砲
}
public enum DamageType//傷害形式
{
    TrueDamage,//真實傷害     //無法減傷 
    AD,//物理傷害             //對應: 護甲AMR
    AP,//法術傷害             //對應: 抗性RES
    Heal,//治療(其值為負數)   
}