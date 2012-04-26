using System;
using JSIL;
using JSIL.Meta;

public static class Program {
  public static int x = 10;
  public static int y = 20;
  
  public static void Main () {
    dynamic document = Builtins.Global["document"];
    dynamic window = Builtins.Global["window"];
    
    var canvas = document.createElement("canvas");
    var ctx = canvas.getContext("2d");
    var body = document.getElementsByTagName("body")[0];

    Console.WriteLine("Hello JSIL World!");

    body.appendChild(canvas);
    
    window.setInterval((Action)(() => {
      Redraw(ctx);
    }), 25);
  }
  
  public static void Redraw (dynamic ctx) {
    x += 2;
    
    ctx.clearRect(0, 0, 300, 300);
    
    ctx.fillStyle = "red";
    ctx.fillRect(x, y, 20, 20);
  }
}