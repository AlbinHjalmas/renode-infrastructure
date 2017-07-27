//
// Copyright (c) Antmicro
// Copyright (c) Realtime Embedded
//
// This file is part of the Emul8 project.
// Full license details are defined in the 'LICENSE' file.
//
using System;
using Emul8.Utilities.Collections;
using System.Collections.Generic;
using AntShell.Terminal;
using System.Threading.Tasks;
using Antmicro.Migrant;
using System.Threading;

namespace Emul8.Peripherals.UART
{
    public class UARTBackend : IAnalyzableBackend<IUART>
    {
        public UARTBackend()
        {
            history = new CircularBuffer<byte>(BUFFER_SIZE);
            actionsDictionary = new Dictionary<IOProvider, Action<byte>>();
        }

        public void Attach(IUART uart)
        {
            UART = uart;
            UART.CharReceived += b =>
            {
                lock(lockObject)
                {
                    history.Add(b);
                }
            };
        }

        public void BindAnalyzer(IOProvider io)
        {
            this.io = io;
            io.ByteRead += b => UART.WriteChar((byte)b);

            Action<byte> writeAction = (b =>
            {
                lock(lockObject)
                {
                    io.Write(b);
                }
            });

            var mre = new ManualResetEventSlim();
            Task.Run(() =>
            {
                lock(lockObject)
                {
                    mre.Set();
                    RepeatHistory();
                    UART.CharReceived += writeAction;
                    actionsDictionary.Add(io, writeAction);
                }
            });
            mre.Wait();
        }

        public void UnbindAnalyzer(IOProvider io)
        {
            lock(lockObject)
            {
                UART.CharReceived -= actionsDictionary[io];
                actionsDictionary.Remove(io);
            }
        }

        public IUART UART { get; private set; }

        public IAnalyzable AnalyzableElement { get { return UART; } }

        public void RepeatHistory(Action beforeRepeatingHistory = null)
        {
            lock(lockObject)
            {
                if(beforeRepeatingHistory != null)
                {
                    beforeRepeatingHistory();
                }

                foreach(var b in history)
                {
                    io.Write(b);
                }
            }
        }

        [Transient]
        private IOProvider io;
        private Dictionary<IOProvider, Action<byte>> actionsDictionary;
        private readonly CircularBuffer<byte> history;
        private object lockObject= new object();

        private const int BUFFER_SIZE = 100000;
    }
}

