﻿using UnityEngine.SceneManagement;

namespace Helpers.StateMachine
{
public class SceneLoadResult
{
    public Scene LoadedScene;
    public SceneLoadProgress SceneLoadProgress;

    public SceneLoadResult() => SceneLoadProgress = new SceneLoadProgress();
}
}