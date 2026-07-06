using UnityEngine;
using Cysharp.Threading.Tasks;
using UnityEngine.AddressableAssets;

namespace Helpers.Audio
{
public class BankLoader : MonoBehaviour
{
    [SerializeField] AssetReference _fmodAssetRef;
    
    BankData _bankData;

    public async UniTask Init() => _bankData = await Addressables.LoadAssetAsync<BankData>(_fmodAssetRef);

    public void Deinit() => _bankData.UnloadBank();
}
}