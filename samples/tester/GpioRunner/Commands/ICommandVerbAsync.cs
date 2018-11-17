using System.Threading.Tasks;

namespace GpioRunner
{
    public interface ICommandVerbAsync
    {
        Task<int> ExecuteAsync();
    }
}
