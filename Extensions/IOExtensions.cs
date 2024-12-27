using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace MqttBrokerWithDashboard.Extensions
{
    public static class IOExtensions
    {
        public static async Task<string> ReadAllTextSharedAsync(string path)
        {
            using (var fileStream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            using (var reader = new StreamReader(fileStream))
            {
                return await reader.ReadToEndAsync();
            }
        }

        public static string ReadAllTextShared(string path)
        {
            using (var fileStream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            using (var reader = new StreamReader(fileStream))
            {
                return reader.ReadToEnd();
            }
        }
    }
}
