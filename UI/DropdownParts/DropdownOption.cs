using System;
using UnityEngine;
using UnityEngine.Localization;

[Serializable]
public class DropdownOption
{
#region Fields

    [SerializeField] bool _useLocalizedText;
    [SerializeField] string _usualTxt;
    [SerializeField] LocalizedString _localizedTxt;
    [SerializeField] Sprite _sprite;

    public string Txt => _useLocalizedText ? _localizedTxt.GetLocalizedString() : _usualTxt;
    public Sprite Sprite => _sprite;
    public bool IsOn { get; private set; }
    public Guid GUID { get; private set; }
    
    public DropdownOption() => GUID = Guid.NewGuid();

#endregion

    public class Builder
    {
        bool _useLocalizedText;
        string _usualTxt;
        LocalizedString _localizedTxt;
        Sprite _img;
        bool _isOptionOn;

        public Builder SetText(string txt)
        {
            _usualTxt = txt;
            return this;
        }

        public Builder SetText(LocalizedString localizedTxt = null)
        {
            _useLocalizedText = true;
            _localizedTxt = localizedTxt;
            return this;
        }

        public Builder SetImg(Sprite img)
        {
            _img = img;
            return this;
        }

        public Builder SetIsOptionOn(bool isOptionOn)
        {
            _isOptionOn = isOptionOn;
            return this;
        }

        public DropdownOption Build() => new DropdownOption()
        {
            _useLocalizedText = _useLocalizedText,
            _usualTxt = _usualTxt,
            _localizedTxt = _localizedTxt,
            _sprite = _img,
            IsOn = _isOptionOn,
        };
    }
}