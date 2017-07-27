﻿﻿//
// Copyright (c) Antmicro
//
// This file is part of the Emul8 project.
// Full license details are defined in the 'LICENSE' file.
//
using System;
using Xwt;
using System.Text;

namespace Emul8.CLI
{
    public partial class TerminalWidget
    {
        private void CopyMarkedField()
        {
            Clipboard.SetText(terminal.CollectClipboardData().Text);
        }

        private void PasteMarkedField()
        {
            var text = Clipboard.GetText();
            if(string.IsNullOrEmpty(text))
            {
                return;
            }
            var textAsBytes = Encoding.UTF8.GetBytes(text);
            foreach(var b in textAsBytes)
            {
                terminalInputOutputSource.HandleInput(b);
            }
        }

        private void FontSizeUp()
        {
            var newSize = terminal.CurrentFont.Size + 1;
            terminal.CurrentFont = terminal.CurrentFont.WithSize(newSize);
        }

        private void FontSizeDown()
        {
            var newSize = Math.Max(terminal.CurrentFont.Size - 1, 1.0);
            terminal.CurrentFont = terminal.CurrentFont.WithSize(newSize);
        }

        private void SetDefaultFontSize()
        {
            var newSize = defaultFontSize;
            terminal.CurrentFont = terminal.CurrentFont.WithSize(newSize);
        }
    }
}