using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Command
{
    public class SettingMember
    {
        private string _Name = string.Empty;
        private List<string> _ContentList = new List<string>();

        public string Name { get { return _Name; } }
        public List<string> ContentList { get { return _ContentList; } }
        public int Count { get { return _ContentList.Count; } }

        public SettingMember(string name)
        {
            this._Name = name;
        }

        public void Add(string content)
        {
            ContentList.Add(content);
        }

        public void Remove(string content)
        {
            ContentList.Remove(content);
        }

        public void Clear()
        {
            ContentList.Clear();
        }

        public string GetContent()
        {
            if (Count>0)
            {
                return ContentList[0];
            }
            return null;
        }

        public string[] GetContents()
        {
            if (Count>0)
            {
                return ContentList.ToArray();
            }
            return null;
        }

        //public List<>
    }
}
