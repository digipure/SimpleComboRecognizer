using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[Serializable]
public class InputEntryInfo
{
    public string name;
    public string code;
    public string input;
    public int damage;
    public bool isCancelable;
    public bool isSpecialSkill;
    public string animationTrigger;
}
//________________________________________________________P____________________________P____________________________P_________________________________________________________________________________________________________________________________________________R_________D________R_________P______________________________________________________________________________________________________________________________________________________________R_______D_______R__________P____________________________________________________________________________R_______D________R____________P____________________________________________________________________________________________________________________D____R_________P______________________________________________________________________________________________________________________________________________________________________________________________________________________________________________________

[Serializable]
public class InputsInfo : MonoBehaviour
{
    public List<InputEntryInfo> BasicCommands = new List<InputEntryInfo>() { };
    public List<InputEntryInfo> Commands = new List<InputEntryInfo>() { };
    public List<InputEntryInfo> Sequences = new List<InputEntryInfo>() { };
}


