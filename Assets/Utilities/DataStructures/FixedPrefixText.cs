using System.Text;
using UnityEngine;

namespace Utilities.DataStructures
{
    public class FixedPrefixText
    {
        private readonly StringBuilder _stringBuilder;

        public string Prefix { get; private set; }
        public string Content { get; private set; }

        public FixedPrefixText(string prefix = "", string content = "")
        {
            _stringBuilder = new StringBuilder(prefix);
            _stringBuilder.Append(content);
            Prefix = prefix;
            Content = content;
        }

        public void ChangeContent(string content)
        {
            ClearContent();
            Content = content;
            _stringBuilder.Append(content);
        }

        public void ChangePrefix(string prefix)
        {
            _stringBuilder.Clear();
            Prefix = prefix;
            _stringBuilder.Append(prefix);
            _stringBuilder.Append(Content);
        }

        public void ClearContent()
        {
            int st = Prefix.Length;
            int len = Content.Length;
            if (st != _stringBuilder.Length)
            {
                _stringBuilder.Remove(st, len);
            }
        }

        public override string ToString()
        {
            return _stringBuilder.ToString();
        }
    }
}