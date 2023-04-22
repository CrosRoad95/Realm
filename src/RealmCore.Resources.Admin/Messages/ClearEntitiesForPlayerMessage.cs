using RealmCore.Resources.Base.Interfaces;
using SlipeServer.Server.Elements;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RealmCore.Resources.Admin.Messages;

internal record struct ClearEntitiesForPlayerMessage(Player player) : IMessage;