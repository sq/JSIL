using System;
using System.Collections.Generic;
using System.Text;

namespace WebGL {
    public static class Page {
        public static dynamic GL;
        public static dynamic Document;

        public static void Load () {
            Document = JSIL.Builtins.Global["document"];
            InitGL(Document.getElementById("canvas"));
        }

        public static void Alert (string text) {
            dynamic alert = JSIL.Builtins.Global["alert"];
            alert(text);
        }

        public static void InitGL (dynamic canvas) {
            try {
                GL = canvas.getContext("experimental-webgl");
            } catch {
            }

            if (GL == null)
                Alert("Could not initialize WebGL");
            else
                Console.WriteLine("Initialized WebGL");
        }
    }
}
