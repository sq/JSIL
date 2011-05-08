"use strict";

if (typeof (JSIL) === "undefined")
  throw new Error("JSIL.Core required");

if ((typeof (Microsoft) === "undefined") || (typeof (Microsoft.Xna) === "undefined")) {
  JSIL.DeclareNamespace(this, "Microsoft");
  JSIL.DeclareNamespace(Microsoft, "Xna");
  JSIL.DeclareNamespace(Microsoft.Xna, "Framework");
  JSIL.DeclareNamespace(Microsoft.Xna.Framework, "Input");
  JSIL.DeclareNamespace(Microsoft.Xna.Framework, "Graphics");

  JSIL.MakeStruct(Microsoft.Xna.Framework, "Plane", "Microsoft.Xna.Framework.Plane");
  JSIL.MakeStruct(Microsoft.Xna.Framework, "Point", "Microsoft.Xna.Framework.Point");
  JSIL.MakeStruct(Microsoft.Xna.Framework, "Quaternion", "Microsoft.Xna.Framework.Quaternion");
  JSIL.MakeStruct(Microsoft.Xna.Framework, "Ray", "Microsoft.Xna.Framework.Ray");
  JSIL.MakeStruct(Microsoft.Xna.Framework, "Rectangle", "Microsoft.Xna.Framework.Rectangle");
  JSIL.MakeStruct(Microsoft.Xna.Framework, "Vector2", "Microsoft.Xna.Framework.Vector2");
  JSIL.MakeStruct(Microsoft.Xna.Framework, "Vector3", "Microsoft.Xna.Framework.Vector3");
  JSIL.MakeStruct(Microsoft.Xna.Framework, "Vector4", "Microsoft.Xna.Framework.Vector4");
  JSIL.MakeStruct(Microsoft.Xna.Framework, "Matrix", "Microsoft.Xna.Framework.Matrix");

  JSIL.MakeStruct(Microsoft.Xna.Framework.Graphics, "VertexPositionColor", "Microsoft.Xna.Framework.Graphics.VertexPositionColor");
  JSIL.MakeStruct(Microsoft.Xna.Framework.Graphics, "VertexPositionTexture", "Microsoft.Xna.Framework.Graphics.VertexPositionTexture");
  JSIL.MakeStruct(Microsoft.Xna.Framework.Graphics, "VertexPositionColorTexture", "Microsoft.Xna.Framework.Graphics.VertexPositionColorTexture");
  JSIL.MakeStruct(Microsoft.Xna.Framework.Graphics, "VertexPositionNormalTexture", "Microsoft.Xna.Framework.Graphics.VertexPositionNormalTexture");

  JSIL.MakeInterface(
	  Microsoft.Xna.Framework, "IGraphicsDeviceManager", "Microsoft.Xna.Framework.IGraphicsDeviceManager", {
	    "CreateDevice": Function,
	    "BeginDraw": Function,
	    "EndDraw": Function
	  });

  JSIL.MakeClass(System.Object, Microsoft.Xna.Framework, "GraphicsDeviceManager", "Microsoft.Xna.Framework.GraphicsDeviceManager");

  JSIL.MakeInterface(
	  Microsoft.Xna.Framework, "IGameComponent", "Microsoft.Xna.Framework.IGameComponent", {
	    "Initialize": Function
    });

  JSIL.MakeInterface(
	  Microsoft.Xna.Framework, "IUpdateable", "Microsoft.Xna.Framework.IUpdateable", {
	    "get_Enabled": Function,
	    "get_UpdateOrder": Function,
	    "add_EnabledChanged": Function,
	    "remove_EnabledChanged": Function,
	    "add_UpdateOrderChanged": Function,
	    "remove_UpdateOrderChanged": Function,
	    "Update": Function,
	    "Enabled": Property,
	    "UpdateOrder": Property
	  });

  JSIL.MakeClass(System.Object, Microsoft.Xna.Framework, "GameComponent", "Microsoft.Xna.Framework.GameComponent");

  JSIL.MakeInterface(
	  Microsoft.Xna.Framework, "IDrawable", "Microsoft.Xna.Framework.IDrawable", {
	    "get_Visible": Function,
	    "get_DrawOrder": Function,
	    "add_VisibleChanged": Function,
	    "remove_VisibleChanged": Function,
	    "add_DrawOrderChanged": Function,
	    "remove_DrawOrderChanged": Function,
	    "Draw": Function,
	    "Visible": Property,
	    "DrawOrder": Property
	  });

  JSIL.MakeClass(Microsoft.Xna.Framework.GameComponent, Microsoft.Xna.Framework, "DrawableGameComponent", "Microsoft.Xna.Framework.DrawableGameComponent");

  JSIL.MakeClass(System.Object, Microsoft.Xna.Framework, "Game", "Microsoft.Xna.Framework.Game");
}