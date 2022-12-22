using SlipeServer.Server.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Realm.Domain.Contextes;

public interface IFormContext
{
    string FormName { get; }

    TData GetData<TData>() where TData : ILuaValue, new();
}
