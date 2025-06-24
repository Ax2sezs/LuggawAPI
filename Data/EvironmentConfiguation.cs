using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Web;

public enum API_MODE
{
    ADMIN,
    BACKOFFICE,
    DOCKER,
    LOCAL
}

public class EnvironmentConfiguration
{
    // requires using Microsoft.Extensions.Configuration;
    private IConfiguration Configuration;

    private const string CONFIGFILE = @"Environment\ConfigFile.json";
    private const string CONFIGFILE_LINUX = @"Environment/ConfigFile.json";
    private string m_configPathAndFileName;
    private JObject m_config;

    public API_MODE m_mode;// = API_MODE.LOCAL;
    public delegate void NotifileConfigChange(JObject config);

    private static EnvironmentConfiguration m_instant;

    private NotifileConfigChange m_notifileList;

    private string m_branchID = "Empty";
    private string m_branchCode = "Empty";
    private int m_waitingTime = 10;
    public static EnvironmentConfiguration GetSingleton()
    {
        if (m_instant == null) m_instant = new EnvironmentConfiguration();
        return m_instant;
    }
    private EnvironmentConfiguration()
    {
        m_configPathAndFileName = $"{AppDomain.CurrentDomain.BaseDirectory}{CONFIGFILE}";
    }
    public string GetServerSyncURL(bool needProduction = false)
    {
        //return "https://apiposbranchau.mmm2007.net/"; 
        return Configuration.GetSection("SYNC_SERVER").Value;
    }
    public void AttachNotifiToMe(NotifileConfigChange notifyFunction)
    {
        m_notifileList += notifyFunction;
    }

    public void DetachNotifiFromMe(NotifileConfigChange notifyFunction)
    {
        m_notifileList -= notifyFunction;
    }

    public void Init(IConfiguration configuration)
    {
        try
        {
            Configuration = configuration;
            // Set API Mode
            var configAPIMode = configuration.GetSection("APIMODE").Value.ToUpper();
            switch (configAPIMode)
            {
                case "ADMIN":
                    m_mode = API_MODE.ADMIN;
                    break;
                case "BACKOFFICE":
                    m_mode = API_MODE.BACKOFFICE;
                    break;
                case "DOCKER":
                    m_mode = API_MODE.DOCKER;
                    break;
                case "LOCAL":
                    m_mode = API_MODE.LOCAL;
                    break;
                default:
                    m_mode = API_MODE.LOCAL;
                    break;
            }
            //  Logger.GetSingleton().LogEvent("EnvironmentConfiguration.txt", m_configPathAndFileName);

            // Set Branch Code
            //if (m_mode == API_MODE.BACKOFFICE) 
            m_branchCode = configuration.GetSection("BRANCHCODE").Value.ToUpper();
            // m_branchID = "6EF8075A-32D0-4FD7-ACFD-E977F8FCAED4";
            // Get Config Mode
            using (StreamReader r = new StreamReader(m_configPathAndFileName))
            {
                string json = r.ReadToEnd();
                m_config = JObject.Parse(json);
            }
        }
        catch (Exception)
        {
            try
            {
                m_configPathAndFileName = $"{AppDomain.CurrentDomain.BaseDirectory}{CONFIGFILE_LINUX}";
                Logger.GetSingleton().LogEvent("EnvironmentConfiguration.txt", m_configPathAndFileName);
                using (StreamReader r = new StreamReader(m_configPathAndFileName))
                {
                    string json = r.ReadToEnd();
                    m_config = JObject.Parse(json);
                }
            }
            catch (Exception)
            {
                //fallback to dev1 environment
                m_config = new JObject();
                JObject falToPro = new JObject();
                falToPro.Add("TARGET_DB", @"localhost\SQLEXPRESS");
                falToPro.Add("USER", "sa");
                falToPro.Add("PASS", "Mymind01-");
                falToPro.Add("CATALOG", "AUPOS");
                m_config.Add("PRODUCTION", falToPro);
                falToPro = new JObject();
                falToPro.Add("TARGET_DB", @"mydb");
                falToPro.Add("USER", "sa");
                falToPro.Add("PASS", "Mymind01-");
                falToPro.Add("CATALOG", "AUPOS");
                m_config.Add("DOCKER", falToPro);
            }
        }
    }


    public void ReLoadConfig()
    {
        Init(Configuration);
        m_notifileList(m_config);
    }
    public JObject GetConfig()
    {
        return m_config;
    }
    public void SetBranchID(string id, string code)
    {
        m_branchID = id;
        m_branchCode = code;
    }

    public void SetBranchID(string id)
    {
        m_branchID = id;
    }
    public string GetBranchID()
    {
        return m_branchID;
    }
    public string GetBranchCode()
    {
        return m_branchCode;
    }


    public void SetWaitingTime(string no)
    {
        try
        {
            m_waitingTime = int.Parse(no);
        }
        catch (Exception)
        {
            m_waitingTime = 10;
        }

    }
    public int WaitingTime()
    {
        return m_waitingTime;
    }

    public string NileconKEY
    {
        get
        {
            return "380bf1ee-1da0-45a7-bf77-6ee94822ccbd";
        }
    }

    public string GetKeyEnCrypt
    {
        get
        {
            return "vkag9viNp%MVP";
        }
    }


    public string GetEtaxURL
    {
        get
        {
            return Configuration.GetSection("inet_etax_url").Value;
        }
    }


    public string GetEtaxToken
    {
        get
        {
            return Configuration.GetSection("inet_etax_token_authen").Value;
        }
    }


    public string GetBranchCodeConfig
    {
        get
        {
            return Configuration.GetSection("BRANCHCODE").Value;
        }
    }


    public bool GetSwaggerVisible
    {
        get
        {
            return Boolean.Parse(Configuration.GetSection("SwaggerVisible").Value.ToString());
        }
    }

    public string Encrypt256(string text)
    {
        string AesIV256 = @"nlc0grfe0atu24fg";
        string AesKey256 = "after-you@2020-ndjkiiei38di0w09k";

        // AesCryptoServiceProvider
        AesCryptoServiceProvider aes = new AesCryptoServiceProvider();
        aes.BlockSize = 128;
        aes.KeySize = 256;
        aes.IV = Encoding.UTF8.GetBytes(AesIV256);
        aes.Key = Encoding.UTF8.GetBytes(AesKey256);
        aes.Mode = CipherMode.CBC;
        aes.Padding = PaddingMode.PKCS7;

        // Convert string to byte array
        byte[] src = Encoding.UTF8.GetBytes(text);

        // encryption
        using (ICryptoTransform encrypt = aes.CreateEncryptor())
        {
            byte[] dest = encrypt.TransformFinalBlock(src, 0, src.Length);

            // Convert byte array to Base64 strings
            return Convert.ToBase64String(dest);
        }
    }

    public string Decrypt256(string text)
    {
        string AesIV256 = @"nlc0grfe0atu24fg";
        string AesKey256 = "after-you@2020-ndjkiiei38di0w09k";

        // AesCryptoServiceProvider
        AesCryptoServiceProvider aes = new AesCryptoServiceProvider();
        aes.BlockSize = 128;
        aes.KeySize = 256;
        aes.IV = Encoding.UTF8.GetBytes(AesIV256);
        aes.Key = Encoding.UTF8.GetBytes(AesKey256);
        aes.Mode = CipherMode.CBC;
        aes.Padding = PaddingMode.PKCS7;

        // Convert Base64 strings to byte array
        byte[] src = System.Convert.FromBase64String(@text);

        // decryption
        using (ICryptoTransform decrypt = aes.CreateDecryptor())
        {
            byte[] dest = decrypt.TransformFinalBlock(src, 0, src.Length);
            return Encoding.UTF8.GetString(dest);
        }
    }

}