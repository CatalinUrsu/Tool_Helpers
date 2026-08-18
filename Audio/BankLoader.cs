using UnityEngine;
using Cysharp.Threading.Tasks;
using UnityEngine.AddressableAssets;

namespace Helpers.Audio
{
public class BankLoader : MonoBehaviour, IBankLoader
{
    [SerializeField] AssetReference _fmodAssetRef;

    public async UniTask Init() => await _fmodAssetRef.LoadBank();

    public void Deinit() => _fmodAssetRef.UnloadBank();
}
}