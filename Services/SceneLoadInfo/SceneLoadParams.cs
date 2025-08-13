using UnityEngine.SceneManagement;

namespace Helpers.Services
{
public struct SceneLoadParams
{
    public string SceneName { get; private set; }
    public string LoadingTip { get; private set; }
    public bool IsAddressable { get; private set; }
    public bool TrackProgress { get; private set; }
    public bool SetSceneActive { get; private set; }
    public LoadSceneParameters LoadParameters { get; private set; }

    public class Builder
    {
        readonly string _sceneName;
        string _loadingTip = string.Empty;
        bool _isAddressable;
        bool _trackProgress = true;
        bool _setSceneActive;
        LoadSceneParameters _loadParameters = new(LoadSceneMode.Additive);

        public Builder(string sceneName)
        {
            _sceneName = sceneName;
        }

        public Builder SetTip(string tip = "")
        {
            _loadingTip = tip;
            return this;
        }

        public Builder SetIsAddressable(bool isAddressable)
        {
            _isAddressable = isAddressable;
            return this;
        }

        public Builder SetActiveOnLoad(bool setActive)
        {
            _setSceneActive = setActive;
            return this;
        }

        public Builder SetTrackProgress(bool trackProgress)
        {
            _trackProgress = trackProgress;
            return this;
        }

        public Builder SetLoadParameters(LoadSceneParameters loadParameters)
        {
            _loadParameters = loadParameters;
            return this;
        }

        public SceneLoadParams Build()
        {
            return new SceneLoadParams()
            {
                SceneName = _sceneName,
                LoadingTip = _loadingTip,
                IsAddressable = _isAddressable,
                TrackProgress = _trackProgress,
                SetSceneActive = _setSceneActive,
                LoadParameters = _loadParameters
            };
        }
    }
}
}