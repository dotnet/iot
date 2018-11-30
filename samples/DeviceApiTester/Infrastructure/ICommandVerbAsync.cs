using System.Threading.Tasks;

namespace DeviceApiTester.Infrastructure
{
    public interface ICommandVerbAsync
    {
        Task<int> ExecuteAsync();
    }
}
