using System.Reflection;

namespace Safepot.Web.Api
{
    public class LogWriter
    {
        //private static string logsPath = "C:\\Working Projects\\Safepot\\Safepot.Web.Api\\Logs\\";
        private static string logsPath = "C:\\WebSite\\Safept\\28-02-2024\\ErrorLogs\\";
        public static async Task LogWrite(string logMessage)
        {
            try
            {
                using (StreamWriter w = File.AppendText(logsPath + "\\" + "log.txt"))
                {
                    await Log(logMessage, w);
                }
            }
            catch (Exception ex)
            {
            }
        }

        public static async Task Log(string logMessage, TextWriter txtWriter)
        {
            try
            {
                await txtWriter.WriteLineAsync(DateTime.Now.ToString("dd-MM-yyyy hh:mm:ss") + " -- " + logMessage);
            }
            catch (Exception ex)
            {
            }
        }
    }
}
