using System.Text;
using UnityEngine;

namespace Utilities.StaticMethodClass
{
    public static class RichText
    {
            private static readonly StringBuilder _builder = new StringBuilder();
            
            public static string Cat(string left, string right)
            {
                _builder.Clear();
                _builder.Append(left);
                _builder.Append(right);
                return _builder.ToString();
            }
    
            public static string Cat(string left, string mid, string right)
            {
                _builder.Clear();
                _builder.Append(left);
                _builder.Append(mid);
                _builder.Append(right);
                return _builder.ToString();
            }
            
            private static string AddTag(string tagL, string text, string tagR)
            {
                return Cat(tagL, text, tagR);
            }
    
            public static string Color(string text, Color color)
            {
                _builder.Clear();
                _builder.Append("<color=#");
                _builder.Append(ColorUtility.ToHtmlStringRGBA(color));
                _builder.Append(">");
                return AddTag(_builder.ToString(), text, "</color>");
            }
    
            public static string Size(string text, int size)
            {
                _builder.Clear();
                _builder.Append("<size=");
                _builder.Append(size.ToString());
                _builder.Append(">");
                return AddTag(_builder.ToString(), text, "</size>");
            }
    
            public static string Bold(string text)
            {
                return AddTag("<b>", text, "</b>");
            }
            
            public static string Italic(string text)
            {
                return AddTag("<i>", text, "</i>");
            }
    
            public static string Brackets(string text)
            {
                return Cat("[", text, "]");
            }
    }
}