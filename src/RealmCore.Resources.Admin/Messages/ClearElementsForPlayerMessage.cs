﻿using RealmCore.Resources.Base.Interfaces;
using SlipeServer.Server.Elements;

namespace RealmCore.Resources.Admin.Messages;

internal record struct ClearElementsForPlayerMessage(Player player) : IMessage;