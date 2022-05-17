using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace InterpreterExam {
    public class Environment {
        private Dictionary<string, Object> store = new Dictionary<string, Object>();
        public Environment outer { get; set; } = null;

        public Tuple<Object, bool> Get(string name) {
            var isExist = store.ContainsKey(name);

            if (!isExist && outer != null) {
                return outer.Get(name);
            }

            return new Tuple<Object, bool>(isExist ? store[name] : null, isExist);
        }

        public Object Set(string name, Object val) {
            store.Add(name, val);
            return val;
        }
    }
}
