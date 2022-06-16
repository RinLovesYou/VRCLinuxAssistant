using System;
using VRCLinuxAssistant.Pages;

namespace VRCLinuxAssistant.Classes
{
    public class Mod
    {
        public int _id;
        public string uploaddate;
        public string category;
        public string[] aliases;
        public ModVersion[] versions;
        public Mods.ModListItem ListItem;
        public string installedFilePath;
        public string installedVersion;
        public bool installedInBrokenDir;
        public bool installedInRetiredDir;

        public class ModVersion
        {
            public int _version;
            public string name;
            public string modVersion;
            public string modType;
            public string author;
            public string description;
            public string downloadLink;
            public string sourceLink;
            public string hash;
            public string updateDate;
            public string vrchatVersion;
            public string loaderVersion;
            public int approvalStatus;

            public bool IsBroken => approvalStatus == 2;
            public bool IsRetired => approvalStatus == 3;
            public bool IsPlugin => modType.Equals("plugin", StringComparison.InvariantCultureIgnoreCase);
        }
    }
}