using UnityEngine;
using System.Threading;
using Cysharp.Threading.Tasks;

namespace Helpers.UI
{
public class TabPanel : MonoBehaviour
{
 public virtual UniTask Init(CancellationTokenSource cts) => UniTask.CompletedTask;
 
 public virtual UniTask Show(bool skipAnimation, CancellationTokenSource cts) => UniTask.CompletedTask;

 public virtual UniTask Hide(bool skipAnimation, CancellationTokenSource cts) => UniTask.CompletedTask;
}
}