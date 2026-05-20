using likefeature.Models;

namespace likefeature.Services;

public interface IClickHouseViewEventWriter
{
    Task AppendAsync(ViewEvent viewEvent);
}
