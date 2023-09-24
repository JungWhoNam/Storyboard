using System;

namespace VMail.Utils.Web
{
    public class VMailData
    {
        public int ID { get; private set; }
        public string name { get; private set; }
        public DateTime lastModifiedDesktop;
        public DateTime lastModifiedMobile;
        public DateTime lastModifiedServer;


        public VMailData(int ID, string name)
        {
            this.ID = ID;
            this.name = name;
            this.lastModifiedDesktop = DateTime.UtcNow;
            this.lastModifiedMobile = DateTime.UtcNow;
            this.lastModifiedServer = DateTime.UtcNow;
        }

        public VMailData(int ID, string name, string lastModifiedDesktop, string lastModifiedMobile, string lastModifiedServer)
        {
            this.ID = ID;
            this.name = name;
            this.lastModifiedDesktop = DateTime.Parse(lastModifiedDesktop);
            this.lastModifiedMobile = DateTime.Parse(lastModifiedMobile);
            this.lastModifiedServer = DateTime.Parse(lastModifiedServer);
        }

        public string GetDirectoryURL()
        {
            return WebIntegration.ServerURL + WebIntegration.ServerDataDir + this.ID.ToString();
        }

        public override string ToString()
        {
            string res = ID + " " + name;
            res += " " + lastModifiedDesktop.ToString("yyyy-MM-dd HH:mm:ss");
            res += " " + lastModifiedMobile.ToString("yyyy-MM-dd HH:mm:ss");
            res += " " + lastModifiedServer.ToString("yyyy-MM-dd HH:mm:ss");

            return res;
        }

    }
}