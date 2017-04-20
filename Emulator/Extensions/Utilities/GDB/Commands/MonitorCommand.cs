﻿//
// Copyright (c) Antmicro
//
// This file is part of the Emul8 project.
// Full license details are defined in the 'LICENSE' file.
//
using Emul8.Core;
using Emul8.UserInterface;
using System.Text;
using System.Linq;

namespace Emul8.Utilities.GDB.Commands
{
    internal class MonitorCommand : Command
    {
        public MonitorCommand(CommandsManager m) : base(m)
        {
        }

        [Execute("qRcmd,")]
        public PacketData Run([Argument(Encoding = ArgumentAttribute.ArgumentEncoding.HexString)]string arg)
        {
            string result;
            var monitor = ObjectCreator.Instance.GetSurrogate<Monitor>();
            var eater = new CommandInteractionEater();
            if(!monitor.Parse(arg, eater))
            {
                result = eater.GetError();
            }
            else
            {
                result = eater.GetContents();
                if(string.IsNullOrEmpty(result))
                {
                    return PacketData.Success;
                }
            }
            return new PacketData(string.Join(string.Empty, Encoding.UTF8.GetBytes(result).Select(x => x.ToString("X2"))));
        }
    }
}

