﻿//
// Copyright (c) Antmicro
//
// This file is part of the Emul8 project.
// Full license details are defined in the 'LICENSE' file.
//
using System;
using System.Threading.Tasks;
using Antmicro.OptionsParser;
using Emul8.CLI;
using Emul8.Core;
using Emul8.Peripherals.UART;
using Emul8.Robot;
using Emul8.Utilities;

namespace Emul8.RobotFrontend
{
    public class RobotFrontend
    {
        public static void Main(string[] args)
        {
            var options = new Emul8.Robot.Options();
            var optionsParser = new OptionsParser();
            if(!optionsParser.Parse(options, args))
            {
                return;
            }

            var keywordManager = new KeywordManager();
            TypeManager.Instance.AutoLoadedType += keywordManager.Register;

            var processor = new XmlRpcServer(keywordManager);
            server = new HttpServer(processor);

            Task.Run(() => 
            {
                XwtProvider xwt = null;
                try
                {
                    if(!options.DisableX11)
                    {
                        var preferredUARTAnalyzer = typeof(UARTWindowBackendAnalyzer);
                        EmulationManager.Instance.CurrentEmulation.BackendManager.SetPreferredAnalyzer(typeof(UARTBackend), preferredUARTAnalyzer);
                        EmulationManager.Instance.EmulationChanged += () =>
                        {
                            EmulationManager.Instance.CurrentEmulation.BackendManager.SetPreferredAnalyzer(typeof(UARTBackend), preferredUARTAnalyzer);
                        };
                        xwt = new XwtProvider(new WindowedUserInterfaceProvider());
                    }

                    server.Run(options.Port);
                    server.Dispose();
                }
                finally
                {
                    if(xwt != null)
                    {
                        xwt.Dispose();
                    }
                }
            });

            Emulator.ExecuteAsMainThread();
        }

        public static void ExecuteKeyword(string name, string[] arguments)
        {
            server.Processor.RunKeyword(name, arguments);
        }

        public static void Shutdown()
        {
            server.Shutdown();
            Emulator.FinishExecutionAsMainThread();
        }

        private static HttpServer server;
    }
}
