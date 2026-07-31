using UnityEngine;
using System.Threading;
using Cysharp.Threading.Tasks;

namespace Helpers.UI
{
public abstract class TabPanel : MonoBehaviour
{
    public abstract UniTask Init(CancellationToken cancelToken, object config = null);
    public abstract void Deinit();
    public abstract UniTask Show(bool skipAnimation, CancellationToken cancelToken);
    public abstract UniTask Hide(bool skipAnimation, CancellationToken cancelToken);
}
}