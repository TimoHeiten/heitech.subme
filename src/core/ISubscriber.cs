using System.Threading.Tasks;

namespace heitech.subme.core
{
    public interface ISubscriber
    {
        Task ReceiveAsync<T>(T msg);
    }
}
