﻿using DotLiquid;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nessie
{
    public class FileOutput
    {
        public FileOutput(FileLocation name, string output, Hash variables)
        {
            this.Name = name;
            this.Variables = variables;
            this.Output = output;
        }

        public FileLocation Name { get; private set; }
        public string Output { get; private set; }
        public Hash Variables { get; private set; }
    }
}