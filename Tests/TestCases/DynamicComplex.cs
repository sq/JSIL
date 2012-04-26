using System;
using JSIL;

public static class Program {
    public static void Main (string[] args) {
        dynamic document = Builtins.Global["document"];
        var canvas = document.createElement("canvas");
        var ctx = canvas.getContext("2d");
        var body = document.getElementsByTagName("body")[0];

        body.appendChild(canvas);

        ctx.fillStyle = "red";
        ctx.fillRect(10, 10, 20, 20);
    }
}