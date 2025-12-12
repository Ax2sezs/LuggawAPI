using System;
using System.IO;
using System.Linq;

public enum LOG_STATUS
{
    ON,
    OFF
}

public enum LOG_INCOMING_TO_SERVER_SAVE_MODE
{
    KEEP_ENCRYPT,
    DECRYPT
}
public class Logger
{
    /// <summary>
    /// after xx minute last file is create add write log 
    /// if have new log message it will create new file
    /// prevent log file large size
    /// </summary>
    private const double CREATE_FILE_AFTER_LAST_FILE_WAS_CREATED = 10.0;

    private static Logger m_instant;

    public static Logger GetSingleton()
    {
        if (m_instant == null) m_instant = new Logger();
        return m_instant;
    }

    private Logger()
    {
        string getEnv = Environment.GetEnvironmentVariable("AFTERYOU_DOCKER_MODE");
        if (getEnv != null)
        {
            m_filePath = $@"{AppDomain.CurrentDomain.BaseDirectory}Logs/";
        }
        else
        {
            m_filePath = $@"{AppDomain.CurrentDomain.BaseDirectory}Logs\";
        }

        //m_filePath =  @"c:\Logs\";
        Directory.CreateDirectory(m_filePath);
        //AdjustFile();
    }
    private Object m_lockObjeect = new Object();
    private string m_filePath = "";
    private string m_fileName = "";
    private DateTime m_lastCreateFile;
    private LOG_STATUS m_status;
    private LOG_INCOMING_TO_SERVER_SAVE_MODE m_incomingLogStatus;

    public void SetLogSaveFileLocation(string location)
    {
        m_filePath = location;
    }

    public void SetLogStatus(LOG_STATUS status)
    {
        m_status = status;
    }

    public void SetIncomingLogStatus(LOG_INCOMING_TO_SERVER_SAVE_MODE status)
    {
        m_incomingLogStatus = status;
    }

    public LOG_INCOMING_TO_SERVER_SAVE_MODE GetIncomingLogStatus()
    {
        return m_incomingLogStatus;
    }

    public void LogEvent(string fileName, string message, string message2 = "")
    {
        if (EnvironmentConfiguration.GetSingleton().m_mode == API_MODE.BACKOFFICE)
        {
            if (m_status == LOG_STATUS.OFF)
                return;

            // ไม่ต้องเขียน log ไฟล์ เนื่องจาก txt เยอะเกินไป ทำให้ช้า
            if (fileName.Contains("ingredient_getbybranch")
                || fileName.Contains("promotionbuy_getall")
                || fileName.Contains("promotionstaff_getall")
                || fileName.Contains("branch_getbybranchcode")
                || fileName.Contains("invoice_getbybranch")
                || fileName.Contains("branchproduct_getbybranch"))
                return;

            string eachpath = m_filePath + DateTime.Now.ToString("dd_MM_yyyy");
            Directory.CreateDirectory(eachpath);
            File.AppendAllText($@"{eachpath}\{fileName}", message + Environment.NewLine);

        }
        else if (EnvironmentConfiguration.GetSingleton().m_mode == API_MODE.ADMIN)
        {
            if (m_status == LOG_STATUS.OFF)
                return;

            // ไม่ต้องเขียน log ไฟล์ เนื่องจาก txt เยอะเกินไป ทำให้ช้า
            if (!fileName.Contains("VoucherCode_Check") && !fileName.Contains("VoucherCodeSale_Check"))
                return;

            string eachpath = m_filePath + DateTime.Now.ToString("dd_MM_yyyy");
            Directory.CreateDirectory(eachpath);
            File.AppendAllText($@"{eachpath}\{fileName}", message + Environment.NewLine);
        }
    }

    public void CopyErrorStatusFile(string fileName, string prefixFileName = "")
    {
        string fileToCopy = $@"{m_filePath}{DateTime.Now.ToString("dd_MM_yyyy")}\{fileName}";

        string destinationDirectory = $@"{m_filePath}{DateTime.Now.ToString("dd_MM_yyyy")}\Error\";
        try
        {

            Directory.CreateDirectory(destinationDirectory);

            File.Copy(fileToCopy, $@"{destinationDirectory}{prefixFileName}_{Path.GetFileName(fileToCopy)}");
        }
        catch (Exception e)
        {

            File.AppendAllText(destinationDirectory + "errorwritelog.txt", e.Message + Environment.NewLine);
        }

    }

    public void LogEventETax(string fileName, string message, string message2 = "")
    {

        string eachpath = $@"{m_filePath}{DateTime.Now.ToString("dd_MM_yyyy")}\ETaxINet";

        if (!Directory.Exists(eachpath))
        {
            Directory.CreateDirectory(eachpath);
        }

        File.AppendAllText($@"{eachpath}\{fileName}", message + Environment.NewLine);
        if (message2 != "")
        {
            File.AppendAllText($@"{eachpath}\{fileName}", message2 + Environment.NewLine);
        }

    }
}
