using UnityEngine;
using UnityEngine.UI;

// This simple class creates a Graphic with zero overhead to drawing system,
// but still capable of capturing user input event. Usefull for lage fulscreen
// areas to capture swipe/tap gestures without drawing transparent image and
// wasting fillrate.
namespace Helpers
{
[AddComponentMenu("UI/Helpers/Dummy Image")]
[RequireComponent(typeof(CanvasRenderer))]
public class DummyImage : Graphic
{
    // Doing nothing to draw nothing, clearing mesh given
    protected override void OnPopulateMesh(VertexHelper m) => m.Clear();
}
}