using FMOD.Studio;
using UnityEngine;

namespace Helpers.Audio
{
public struct BankData
{
    public Bank Bank {get; private set;}
    public TextAsset TextAsset {get; private set;}

    public BankData(Bank bank, TextAsset textAsset)
    {
        Bank = bank;
        TextAsset = textAsset;
    }
}
}