using EsmatPlastic.Domain.Entities;

namespace EsmatPlastic.API.Services;

public interface IFirebaseCustomTokenService
{
    Task<string> CreateAsync(User user, IReadOnlyCollection<string> permissions);
}