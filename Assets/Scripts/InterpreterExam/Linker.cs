using System.Collections;
using System.Collections.Generic;
using InterpreterExam;
using UnityEngine;

public class Linker {
    public Evaluator e;
    public Tokenizer t;

    public Linker(Evaluator e, Tokenizer t) {
        this.e = e;
        this.t = t;
    }

    private LibraryClassBase GetLibrary(string libraryName) {
        switch (libraryName) {
            case "Arduino.h":
                return new ArduinoLibrary();
            case "LedControl.h":
                return new LedControl();
            default:
                return null;
        }
    }

    public bool LinkLibrary(string libraryName) {
        var library = GetLibrary(libraryName);
        if (library == null) {
            return false;
        }

        library.Initialize();
        e.AddLibrary(library);
        t.AddHeader(library);

        return true;
    }
}
