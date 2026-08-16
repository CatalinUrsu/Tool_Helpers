using UnityEngine;
using System.Threading;
using Cysharp.Threading.Tasks;

namespace Helpers.UI
{
public class TabButton : MonoBehaviour
{
    [SerializeField] protected ButtonHelper _btnHelper;

    public ButtonHelper BtnHelper => _btnHelper;

    public void Init() => _btnHelper.Init();

    public virtual UniTask Select(bool skipAnimation, CancellationToken cancelToken) => UniTask.CompletedTask;

    public virtual UniTask Deselect(bool skipAnimation, CancellationToken cancelToken) => UniTask.CompletedTask;
}
}