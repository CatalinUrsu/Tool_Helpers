using Helpers.StateMachine;

namespace Source.StateMachine
{
public struct SimpleSplashScreenInfo : ISplashScreenInfo
{
    public bool SkipAnimation { get;}
    
    public SimpleSplashScreenInfo(bool skipAnimation) => SkipAnimation = skipAnimation;
}
}