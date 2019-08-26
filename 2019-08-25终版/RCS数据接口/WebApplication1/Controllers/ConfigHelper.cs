using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Command
{
    public class ConfigHelper
    {
        private string _FileName = string.Empty;
        private string _FilePath = string.Empty;
        private string _DirectoryName = string.Empty;
        private ConcurrentDictionary<string, SettingMember> _Content =new ConcurrentDictionary<string, SettingMember>();

        private string FileName { get { return _FileName; } }
        private string FilePath { get { return _FilePath; } }
        private string DirectoryName { get { return _DirectoryName; } }
        public ConcurrentDictionary<string, SettingMember> Content { get { return _Content; } }

        public ConfigHelper(string filename,bool pathtype)
        {
            try
            {
                if (pathtype)//相对路径
                {
                    _FilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, filename);
                }
                else//绝对路径
                {
                    _FilePath = filename;

                }
                _DirectoryName = Path.GetDirectoryName(FilePath);
                _FileName = Path.GetFileNameWithoutExtension(FilePath);
                CreateDirectory();
                WriteFlagName();
                Read();
            }
            catch{ }
        }

        private void WriteFlagName()
        {
            if (!File.Exists(FilePath))
            {
                CreateFile();
                Write();
            }
        }

        public ConfigHelper()
        { }

        public string GetBaseDirectory()
        {
            return AppDomain.CurrentDomain.BaseDirectory;
        }

        private void CreateDirectory()
        {
            if (!Directory.Exists(DirectoryName))
            {
                Directory.CreateDirectory(DirectoryName);
            }
        }

        public void CreateDirectory(string path)
        {
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
        }

        public string GetDriectoryName(string name)
        {
            return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, name);
        }

        private void CreateFile()
        {
            if (!File.Exists(FilePath))
            {
                File.Create(FilePath).Close();
            }
        }

        public void CreateFile(string path)
        {
            if (!File.Exists(path))
            {
                File.Create(path).Close();
            }
        }

        public bool LoadSetting(string name,out string content)
        {
            content = string.Empty;
            if (Content.ContainsKey(name))
            {
                if (Content[name].Count>0)
                {
                    content = Content[name].GetContent();
                    return true;
                }
            }
            return false;
        }

        public bool LoadSetting(string name, out string[] contents)
        {
            contents = new string[] { };
            if (Content.ContainsKey(name))
            {
                if (Content[name].Count>0)
                {
                    contents = Content[name].GetContents();
                    return true;
                }
            }
            return false;
        }

        public void Clear(string name)
        {
            if (!Content.ContainsKey(name))
            {
                Content[name].Clear();
            }
        }

        public bool SaveSetting(string name, string content)
        {
            if (!Content.ContainsKey(name))
            {
                Content.TryAdd(name, new SettingMember(name));
                Write();
                return true;
            }
            else
            {
                Content[name].Clear();
                Content[name].Add(content);
                Write();
                return true;
            }
        }

        public bool SaveSetting(string name, List<string> contents)
        {
            SettingMember Setting = new SettingMember(name);
            if (!Content.ContainsKey(name))
            {
                foreach (var content in contents)
                {
                    Setting.Add(content);
                }
                Content.TryAdd(name, Setting);
                Write();
                return true;
            }
            else
            {
                Content[name].Clear();
                foreach (var content in contents)
                {
                    Setting.Add(content);
                }
                Content[name] = Setting;
                Write();
                return true;
            }
            
        }

        private void Read()
        {
            FileStream FS = new FileStream(FilePath, FileMode.Open);
            StreamReader SR = new StreamReader(FS);
            string message = string.Empty;
            int state = 0;
            SettingMember SettingContent = null;
            while ((message=SR.ReadLine())!=null)
            {
                switch (state)
                {
                    case 0:
                        if (message.Equals(string.Format("[{0}]",FileName)))
                        {
                            state = 1;
                        }
                        break;
                    case 1:
                        if (message.StartsWith("--"))
                        {
                            if (SettingContent!=null&&Content.ContainsKey(SettingContent.Name))
                            {
                                string content = message.Substring(2).Trim();
                                SettingContent.Add(content);
                            }
                        }
                        else if (message.StartsWith("-"))
                        {
                            string name = message.Substring(1).Trim();
                            SettingMember Setting = new SettingMember(name);
                            if (!Content.ContainsKey(name))
                            {
                                Content.TryAdd(name, Setting);
                            }
                            SettingContent = Setting;
                        }
                        break;
                    default:
                        break;
                }
            }
            SR.Close();
            FS.Close();
        }

        private void Write()
        {
            FileStream FS = new FileStream(FilePath, FileMode.Create);
            StreamWriter SW = new StreamWriter(FS);
            SW.WriteLine(string.Format("[{0}]",FileName));
            foreach (var content in Content.Values)
            {
                string name = string.Format("{0}{1}", "-", content.Name);
                SW.WriteLine(name);
                foreach (var item in content.ContentList)
                {
                    string con = string.Format("{0}{1}","--",item);
                    SW.WriteLine(con);
                }
            }
            SW.Close();
            FS.Close();
        }
    }
}
