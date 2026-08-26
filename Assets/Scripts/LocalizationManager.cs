using System;
using System.Collections.Generic;
using System.Xml;
using Core._Common;
using UnityEngine;

public class LocalizationManager : AbstractManager<LocalizationManager>
{
    public class LanguageData
    {
        public string name
        {
            get;
            private set;
        }

        public string index
        {
            get;
            private set;
        }

        public string fontFile
        {
            get;
            private set;
        }

        public LanguageData(XmlNode p_node)
        {
            name = p_node.Name;
            index = p_node.Attributes["Index"].ParseString();
            fontFile = p_node.Attributes["FontFile"].ParseString();
        }
    }

    private static LocalizationManager _localization;

    private List<LanguageData> _locales = new List<LanguageData>();

    private XmlNode _LocalizationTexts;

    private string _currentFontFile;

    public LanguageData CurrentLocale
    {
        get;
        private set;
    }

    public Font CurrentFont
    {
        get;
        private set;
    }

    public event Action<bool> UpdateEvent;

    protected override void InitInternal()
    {
        var listDoc = XmlUtils.OpenXMLDocument(VectorPaths.Localization, "localization_list.xml");
        foreach (XmlNode node in listDoc["List"])
        {
            _locales.Add(new LanguageData(node));
        }
        var loc = XmlUtils.OpenXMLDocument(VectorPaths.Localization, "localization_all.xml");
        _LocalizationTexts = loc["log"];
    }

    public string GetNextLocale()
    {
        if (_locales.Count - 1 < _locales.IndexOf(CurrentLocale) + 1)
        {
            return _locales[0].index;
        }
        return _locales[_locales.IndexOf(CurrentLocale) + 1].index;
    }

    public void ChangeLocale(string newLocale)
    {
        Predicate<LanguageData> match = data => data.index == newLocale;
        CurrentLocale = _locales.Find(match);
        bool flag = false;
        if (CurrentLocale == null)
        {
            CurrentLocale = _locales[0];
            return;
        }
        if (_currentFontFile != CurrentLocale.fontFile)
        {
            _currentFontFile = CurrentLocale.fontFile;
            CurrentFont = Resources.Load<Font>(_currentFontFile);
            flag = true;
        }
        UpdateEvent?.Invoke(flag);
    }

    public string GetTranslation(string key)
    {
        var translation = GetTranslation(key, CurrentLocale.index);
        if (translation == string.Empty)
        {
            translation = GetTranslation(key, "eng");
        }
        if (translation != string.Empty)
        {
            return translation;
        }

        // u suck - Floof

        //string[] values = {
        //    "%",
        //    CurrentLocale.index,
        //    "_",
        //    key,
        //    "%"
        //};
        //return string.Concat(values);

        return key;
    }

    public string GetTranslation(string key, string locale)
    {
        foreach (XmlNode node in _LocalizationTexts)
        {
            if (node.Name == key)
            {
                return node.Attributes[locale].Value;
            }
        }
        return string.Empty;
    }

    public string GetTranslationByID(string key)
    {
        string val = key.Replace("_HUNTER", "");
        val = "item_" + val;
        return GetTranslation(val);
    }

    public string GetSystem()
    {
        Predicate<LanguageData> match = data => data.name == Application.systemLanguage.ToString();
        var lang = _locales.Find(match);
        if (lang != null)
        {
            return lang.index;
        }
        return "eng";
    }
}
