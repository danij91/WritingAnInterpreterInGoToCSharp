using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace InterpreterExam {
    public abstract class LibraryClassBase {
        public Dictionary<string, HostFunction> Header = new Dictionary<string, HostFunction>();
        public Dictionary<string, Object> Fields = new Dictionary<string, Object>();

        public abstract void Initialize();
    }
}
