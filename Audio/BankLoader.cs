using FMOD.Studio;
using UnityEngine;
using Cysharp.Threading.Tasks;
using UnityEngine.AddressableAssets;

namespace Helpers.Audio
{
public class BankLoader : MonoBehaviour
{
    [SerializeField] AssetReference _fmodAssetRef;
    
    Bank _bank;
    TextAsset _bankTextAsset;

    public async UniTask Init()
    {
        _bankTextAsset = await _fmodAssetRef.LoadTextAsset();
        _bank = _bankTextAsset.LoadBank();
    }

    public void Deinit()
    {
        _bank.UnloadBank(_bankTextAsset);
    }
}
}