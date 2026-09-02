using Domain.Entities;
using Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace ChaosTest;

internal static class Utils
{
    public static IEnumerable<Order> Gerar10MilOrdensAleatorias()
    {
        return Enumerable
            .Range(1, 10_000)
            .Select(_ => new Order()
            {
                Id = Guid.NewGuid(),
                Price = RandomNumberGenerator.GetInt32(1, 11),
                Quantity = RandomNumberGenerator.GetInt32(1, 11),
                Side = (Side)RandomNumberGenerator.GetInt32(0, 2),
                Timestamp = RandomNumberGenerator.GetInt32(1, 2000)
            });
    }
}
