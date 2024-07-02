using System.Management.Automation;

namespace PowerShellToolsPro
{
    public class Certificate 
    {

        public Certificate() { }

        public Certificate(PSObject psobject)
        {
            Path = psobject.Properties["PSPath"].Value.ToString();
            Thumbprint = psobject.Properties["Thumbprint"].Value.ToString();
            Subject = psobject.Properties["Subject"].Value.ToString();
            Expiration = psobject.Properties["NotAfter"].Value.ToString();
        }

        public string Path { get; set;  }
        public string Thumbprint { get; set; }
        public string Subject { get; set;  }
        public string Expiration { get; set;  }
    }
}