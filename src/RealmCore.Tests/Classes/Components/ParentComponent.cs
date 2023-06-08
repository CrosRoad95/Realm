using RealmCore.Server.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RealmCore.Tests.Classes.Components;

internal class ParentComponent : Component
{
    public override void Dispose()
    {
        Entity.TryDestroyComponent<ChildComponent>();
    }
}
