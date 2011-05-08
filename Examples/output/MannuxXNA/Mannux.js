JSIL.DeclareNamespace(this, "Input");
JSIL.MakeInterface(
	Input, "IInputDevice", "Input.IInputDevice", {
		"Poll": Function, 
		"Axis": Function, 
		"Button": Function
	});

JSIL.DeclareNamespace(this, "Entities");
JSIL.MakeClass(System.Object, Entities, "Entity", "Entities.Entity");

JSIL.DeclareNamespace(Entities, "Enemies");
JSIL.MakeClass(Entities.Entity, Entities.Enemies, "Enemy", "Entities.Enemies.Enemy");

JSIL.MakeClass(Entities.Enemies.Enemy, Entities.Enemies, "Hopper", "Entities.Enemies.Hopper");

JSIL.DeclareNamespace(this, "Editor");
JSIL.MakeInterface(
	Editor, "IEditorState", "Editor.IEditorState", {
		"MouseDown": Function, 
		"MouseUp": Function, 
		"MouseClick": Function, 
		"MouseWheel": Function, 
		"KeyPress": Function, 
		"RenderHUD": Function
	});

JSIL.MakeClass(System.Object, Editor, "TileSetMode", "Editor.TileSetMode");

JSIL.MakeClass(System.Object, Editor, "Tileset", "Editor.Tileset");

JSIL.MakeClass(System.Object, this, "Timer", "Timer");

JSIL.MakeClass(System.Object, Input, "X360Gamepad", "Input.X360Gamepad");

JSIL.DeclareNamespace(this, "Cataract");
JSIL.MakeClass(System.Object, Cataract, "XNAGraph", "Cataract.XNAGraph");

JSIL.MakeClass(System.MulticastDelegate, Editor, "ChangeTileHandler", "Editor.ChangeTileHandler");

JSIL.MakeClass(System.Windows.Forms.ScrollableControl, Editor, "TileSetPreview", "Editor.TileSetPreview");
JSIL.MakeClass(System.Windows.Forms.Panel, Editor.TileSetPreview, "DoubleBufferedPanel", "Editor.TileSetPreview/DoubleBufferedPanel");


JSIL.MakeClass(System.Windows.Forms.Control, Editor, "MapInfoView", "Editor.MapInfoView");

JSIL.MakeClass(System.Object, Editor, "EntityEditMode", "Editor.EntityEditMode");

JSIL.MakeClass(Entities.Enemies.Enemy, Entities.Enemies, "Skree", "Entities.Enemies.Skree");

JSIL.DeclareNamespace(this, "Sprites");
JSIL.MakeInterface(
	Sprites, "ISprite", "Sprites.ISprite", {
		"get_Width": Function, 
		"get_Height": Function, 
		"get_NumFrames": Function, 
		"get_HotSpot": Function, 
		"Draw": Function, 
		"Width": Property, 
		"Height": Property, 
		"NumFrames": Property, 
		"HotSpot": Property
	});

JSIL.DeclareNamespace(this, "Import");
JSIL.MakeClass(System.Object, Import, "MannuxMap", "Import.MannuxMap");

JSIL.MakeClass(System.Object, this, "AnimState", "AnimState");

JSIL.DeclareNamespace(this, "Mannux");
JSIL.MakeClass(Microsoft.Xna.Framework.Game, Mannux, "MannuxGame", "Mannux.MannuxGame");

JSIL.MakeInterface(
	this, "IUpdatable", "IUpdatable", {
		"Update": Function
	});

JSIL.MakeClass(System.Object, Editor, "Editor", "Editor.Editor");

JSIL.MakeClass(System.Object, Sprites, "BitmapSprite", "Sprites.BitmapSprite");

JSIL.MakeClass(System.Object, Import, "VectorIndexBuffer", "Import.VectorIndexBuffer");

JSIL.DeclareNamespace(Import, "Geo");
JSIL.MakeStruct(Import.Geo, "Vertex", "Import.Geo.Vertex");

JSIL.MakeClass(System.Object, this, "Controller", "Controller");
JSIL.MakeClass(System.Object, Controller, "Resource", "Controller/Resource");


JSIL.MakeClass(System.Windows.Forms.Control, Editor, "MapEntPropertiesView", "Editor.MapEntPropertiesView");

JSIL.MakeClass(System.Object, this, "Vector", "Vector");

JSIL.MakeClass(System.Object, Input, "InputHandler", "Input.InputHandler");

JSIL.MakeClass(Entities.Entity, Entities, "Door", "Entities.Door");

JSIL.MakeClass(System.Windows.Forms.Control, Editor, "AutoSelectionThing", "Editor.AutoSelectionThing");

JSIL.MakeClass(System.Object, Sprites, "ParticleSprite", "Sprites.ParticleSprite");
JSIL.MakeStruct(Sprites.ParticleSprite, "Particle", "Sprites.ParticleSprite/Particle");


JSIL.MakeClass(Entities.Entity, Entities, "Boom", "Entities.Boom");

JSIL.MakeClass(Microsoft.Xna.Framework.Game, this, "Engine", "Engine");

JSIL.MakeClass(System.Object, Editor, "ObstructionMode", "Editor.ObstructionMode");

JSIL.MakeClass(System.Object, Import, "VectorObstructionMap", "Import.VectorObstructionMap");

JSIL.MakeClass(System.Object, Import, "Map", "Import.Map");
JSIL.MakeClass(System.Object, Import.Map, "Layer", "Import.Map/Layer");


JSIL.MakeClass(System.Object, Import.Geo, "Line", "Import.Geo.Line");

JSIL.MakeClass(System.Object, this, "RLE", "RLE");

JSIL.MakeClass(System.Object, Import, "v2Map", "Import.v2Map");

JSIL.DeclareNamespace(Mannux, "Import");
JSIL.MakeClass(System.Object, Mannux.Import, "AutoArray$b1", "Mannux.Import.AutoArray`1");

JSIL.MakeClass(System.Windows.Forms.Form, Editor, "NewMapDlg", "Editor.NewMapDlg");

JSIL.DeclareNamespace(Mannux, "Program");

JSIL.MakeClass(System.Object, Import, "MapEnt", "Import.MapEnt");

JSIL.MakeEnum(
	Input, "MouseButton", "Input.MouseButton", {
		None: 0, 
		Left: 1, 
		Right: 2, 
		Middle: 4
	}, false
);

JSIL.MakeClass(System.Object, Input, "MouseDevice", "Input.MouseDevice");

JSIL.MakeClass(Entities.Entity, Entities, "Player", "Entities.Player");

JSIL.MakeClass(Entities.Enemies.Enemy, Entities.Enemies, "Ripper", "Entities.Enemies.Ripper");

JSIL.MakeClass(System.Windows.Forms.TextBox, Editor, "NumberEditBox", "Editor.NumberEditBox");

JSIL.MakeClass(System.Object, Editor, "Util", "Editor.Util");

JSIL.MakeClass(System.Object, Editor, "CopyPasteMode", "Editor.CopyPasteMode");
JSIL.MakeEnum(
	Editor.CopyPasteMode, "EditState", "Editor.CopyPasteMode/EditState", {
		DoingNothing: 0, 
		Copying: 1, 
		Pasting: 2
	}, false
);


JSIL.MakeClass(System.Object, Input, "KeyboardDevice", "Input.KeyboardDevice");

JSIL.MakeEnum(
	Entities, "Dir", "Entities.Dir", {
		left: 0, 
		right: 1, 
		up: 2, 
		down: 3, 
		up_left: 4, 
		up_right: 5, 
		down_left: 6, 
		down_right: 7
	}, false
);

JSIL.MakeClass(Entities.Entity, Entities, "Bullet", "Entities.Bullet");

Entities.Entity.prototype._ctor = function (e, s) {
	this.x = 0;
	this.y = 0;
	this.vx = 0;
	this.vy = 0;
	this.anim = new AnimState();
	this.direction = Entities.Dir.left;
	this.visible = true;
	System.Object.prototype._ctor.call(this);
	this.engine = e;
	this.sprite = s;
	this.width = this.sprite.HotSpot.Width;
	this.height = this.sprite.HotSpot.Height;
	this.UpdateState = JSIL.Delegate.New("System.Action", this, Entities.Entity.prototype.DoNothing);
};

Entities.Entity.prototype.DoNothing = function () {
};

Entities.Entity.prototype.Update = function () {
	this.UpdateState();
	this.anim.Update();
};

Entities.Entity.prototype.Touches = function (e) {
	return ((this.x <= (e.x + e.width)) && 
		(this.y <= (e.y + e.height)) && 
		(e.x <= (this.x + this.width)) && (e.y <= (this.y + this.height)));
};

Entities.Entity.prototype.Tick = function () {
	this.vy = Vector.Clamp(this.vy, 4);
	var x2 = (this.x + this.width);
	var y2 = (this.y + this.height);
	this.ceiling = this.engine.IsObs((this.x + 2), (this.y + this.vy), (x2 - 4), (this.y - this.vy));
	this.touchingceiling = (this.ceiling !== null);
	this.floor = this.engine.IsObs((this.x + 2), (y2 - this.vy - 2), (x2 - 4), Math.floor(y2));
	this.touchingground = (this.floor !== null);
	this.leftwall = this.engine.IsObs((this.x + this.vx), (this.y + 4), (this.x - this.vx), (y2 - 2));
	this.touchingleftwall = (this.leftwall !== null);
	this.rightwall = this.engine.IsObs((x2 + this.vx), (this.y + 4), ((x2 - this.vx) + 4), (y2 - 2));
	this.touchingrightwall = (this.rightwall !== null);
	this.Update();
	this.x += this.vx;
	this.y += this.vy;
};

Entities.Entity.prototype.HandleGravity = function () {

	if (!this.touchingground) {
		this.vy += 0.21699999272823334;
	} else {
		this.vy = 0;
	}
};

Entities.Entity.prototype.get_X = function () {
	return Math.floor(this.x);
};

Entities.Entity.prototype.set_X = function (value) {
	this.x = value;
};

Entities.Entity.prototype.get_Y = function () {
	return Math.floor(this.y);
};

Entities.Entity.prototype.set_Y = function (value) {
	this.y = value;
};

Entities.Entity.prototype.get_VX = function () {
	return Math.floor(this.vx);
};

Entities.Entity.prototype.set_VX = function (value) {
	this.vx = value;
};

Entities.Entity.prototype.get_VY = function () {
	return Math.floor(this.vy);
};

Entities.Entity.prototype.set_VY = function (value) {
	this.vy = value;
};

Entities.Entity.prototype.get_Width = function () {
	return this.Height;
};

Entities.Entity.prototype.set_Width = function (value) {
	this.height = value;
};

Entities.Entity.prototype.get_Height = function () {
	return this.height;
};

Entities.Entity.prototype.set_Height = function (value) {
	this.height = value;
};

Entities.Entity.prototype.get_Visible = function () {
	return this.visible;
};

Entities.Entity.prototype.set_Visible = function (value) {
	this.visible = value;
};

Entities.Entity.prototype.get_Facing = function () {
	return this.direction;
};

Entities.Entity.prototype.set_Facing = function (value) {
	this.direction = value;
};

Object.defineProperty(Entities.Entity.prototype, "X", {
		get: Entities.Entity.prototype.get_X, 
		set: Entities.Entity.prototype.set_X
	});
Object.defineProperty(Entities.Entity.prototype, "Y", {
		get: Entities.Entity.prototype.get_Y, 
		set: Entities.Entity.prototype.set_Y
	});
Object.defineProperty(Entities.Entity.prototype, "VX", {
		get: Entities.Entity.prototype.get_VX, 
		set: Entities.Entity.prototype.set_VX
	});
Object.defineProperty(Entities.Entity.prototype, "VY", {
		get: Entities.Entity.prototype.get_VY, 
		set: Entities.Entity.prototype.set_VY
	});
Object.defineProperty(Entities.Entity.prototype, "Width", {
		get: Entities.Entity.prototype.get_Width, 
		set: Entities.Entity.prototype.set_Width
	});
Object.defineProperty(Entities.Entity.prototype, "Height", {
		get: Entities.Entity.prototype.get_Height, 
		set: Entities.Entity.prototype.set_Height
	});
Object.defineProperty(Entities.Entity.prototype, "Visible", {
		get: Entities.Entity.prototype.get_Visible, 
		set: Entities.Entity.prototype.set_Visible
	});
Object.defineProperty(Entities.Entity.prototype, "Facing", {
		get: Entities.Entity.prototype.get_Facing, 
		set: Entities.Entity.prototype.set_Facing
	});
Entities.Entity.prototype.touchingground = false;
Entities.Entity.prototype.touchingceiling = false;
Entities.Entity.prototype.touchingleftwall = false;
Entities.Entity.prototype.touchingrightwall = false;
Entities.Entity.prototype.leftwall = null;
Entities.Entity.prototype.rightwall = null;
Entities.Entity.prototype.floor = null;
Entities.Entity.prototype.ceiling = null;
Entities.Entity.prototype.x = 0;
Entities.Entity.prototype.y = 0;
Entities.Entity.prototype.width = 0;
Entities.Entity.prototype.height = 0;
Entities.Entity.prototype.vx = 0;
Entities.Entity.prototype.vy = 0;
Entities.Entity.prototype.sprite = null;
Entities.Entity.prototype.anim = null;
Entities.Entity.prototype.direction = 0;
Entities.Entity.prototype.visible = false;
Entities.Entity.prototype.UpdateState = null;
Entities.Entity.prototype.engine = null;
Entities.Entity._cctor = function () {
	Object.defineProperty(Entities.Entity, "maxvelocity", {
			"value": 4}
	);
	Object.defineProperty(Entities.Entity, "gravity", {
			"value": 0.21699999272823334}
	);
};


Entities.Enemies.Enemy.prototype._ctor = function (e, s) {
	this.hp = 0;
	this.damage = 0;
	Entities.Entity.prototype._ctor.call(this, e, s);
};

Entities.Enemies.Enemy.prototype.Die = function () {
	this.hp = 0;
	this.engine.DestroyEntity(this);
};

Entities.Enemies.Enemy.prototype.get_HP = function () {
	return this.hp;
};

Entities.Enemies.Enemy.prototype.set_HP = function (value) {

	if (value <= 0) {
		this.Die();
	} else {
		this.hp = value;
	}
};

Entities.Enemies.Enemy.prototype.get_Damage = function () {
	return this.damage;
};

Entities.Enemies.Enemy.prototype.set_Damage = function (value) {
	this.damage = value;
};

Object.defineProperty(Entities.Enemies.Enemy.prototype, "HP", {
		get: Entities.Enemies.Enemy.prototype.get_HP, 
		set: Entities.Enemies.Enemy.prototype.set_HP
	});
Object.defineProperty(Entities.Enemies.Enemy.prototype, "Damage", {
		get: Entities.Enemies.Enemy.prototype.get_Damage, 
		set: Entities.Enemies.Enemy.prototype.set_Damage
	});
Entities.Enemies.Enemy.prototype.hp = 0;
Entities.Enemies.Enemy.prototype.damage = 0;

Entities.Enemies.Hopper.prototype._ctor = function (e, startx, starty) {
	this.delay = 0;
	Entities.Enemies.Enemy.prototype._ctor.call(this, e, e.RipperSprite);
	this.x = startx;
	this.y = starty;
	this.delay = 0;
	this.vx = 1;
	this.vy = 0;
	this.hp = 10;
	this.direction = Entities.Dir.right;
	this.anim.Set(2, 3, 10, true);
	this.UpdateState = JSIL.Delegate.New("System.Action", this, Entities.Enemies.Hopper.prototype.DoTick);
};

Entities.Enemies.Hopper.prototype.DoTick = function () {
	Entities.Entity.prototype.HandleGravity.call(this);

	if (this.touchingground) {
		this.y = (this.floor.atX((this.x + Math.floor(this.width / 2))) - this.height);

		if (this.delay === 0) {
			this.delay = 5;
			this.vx = 0;
			this.vy = 0;
		} else {

			if (this.delay > 0) {
				--this.delay;
			}

			if (this.delay === 0) {
				this.vy = -3;

				if (this.direction === Entities.Dir.right) {
					this.vx = 1;
				} else {
					this.vx = -1;
				}
			}
		}
	} else {

		if (this.touchingleftwall) {
			this.vx = 1;
			this.direction = Entities.Dir.right;
			this.anim.Set(2, 3, 10, true);
		}

		if (this.touchingrightwall) {
			this.vx = -1;
			this.direction = Entities.Dir.left;
			this.anim.Set(0, 1, 10, true);
		}
	}
};

Entities.Enemies.Hopper.prototype.delay = 0;

Editor.TileSetMode.prototype._ctor = function (e) {
	this.curtile = 0;
	this.curlayer = 0;
	System.Object.prototype._ctor.call(this);
	this.editor = e;
	this.engine = e.engine;
};

Editor.TileSetMode.prototype.MouseDown = function (e, b) {
	this.MouseClick(e, b);
};

Editor.TileSetMode.prototype.MouseUp = function (e, b) {
};

Editor.TileSetMode.prototype.MouseWheel = function (p, delta) {
	this.curtile += Math.floor(delta / 120);

	if (this.curtile < 0) {
		this.curtile += this.editor.tileset.NumTiles;
	}

	if (this.curtile >= this.editor.tileset.NumTiles) {
		this.curtile -= this.editor.tileset.NumTiles;
	}
};

Editor.TileSetMode.prototype.MouseClick = function (e, b) {

	if (b === Input.MouseButton.Left) {
		var tilex = Math.floor((e.X + this.engine.XWin) / this.engine.tileset.Width);
		var tiley = Math.floor((e.Y + this.engine.YWin) / this.engine.tileset.Height);
		this.editor.statbar.Panels.get_Item(0).Text = System.String.Format("Layer {0}", this.curlayer);
		this.editor.statbar.Panels.get_Item(1).Text = System.String.Format("({0},{1})", tilex, tiley);
		this.oldx = tilex;
		this.oldy = tiley;

		if ((System.Windows.Forms.Control.ModifierKeys & System.Windows.Forms.Keys.None | System.Windows.Forms.Keys.Shift) !== System.Windows.Forms.Keys.None) {
			this.curtile = this.engine.map.get_Item(this.curlayer).get_Item(tilex, tiley);
		} else {
			this.engine.map.get_Item(this.curlayer).set_Item(tilex, tiley, this.curtile);
		}
	}
};

Editor.TileSetMode.prototype.KeyPress = function (e) {
};

Editor.TileSetMode.prototype.RenderHUD = function () {
};

Editor.TileSetMode.prototype.__ImplementInterface__(Editor.IEditorState);
Editor.TileSetMode.prototype.editor = null;
Editor.TileSetMode.prototype.engine = null;
Editor.TileSetMode.prototype.curtile = 0;
Editor.TileSetMode.prototype.curlayer = 0;
Editor.TileSetMode.prototype.oldx = 0;
Editor.TileSetMode.prototype.oldy = 0;

Editor.Tileset.prototype._ctor = function (bitmap, tileWidth, tileHeight, tilesPerRow, numTiles) {
	System.Object.prototype._ctor.call(this);
	this.Bitmap = bitmap;
	this.TileWidth = tileWidth;
	this.TileHeight = tileHeight;
	this.TilesPerRow = tilesPerRow;
	this.NumTiles = numTiles;
	this.Padded = true;
};

Editor.Tileset.prototype.TileFromPoint = function (x, y) {
	x = Math.floor(x / (this.TileWidth + this.Padded ? 1 : 0));
	y = Math.floor(y / (this.TileHeight + this.Padded ? 1 : 0));
	return ((y * this.TilesPerRow) + x);
};

Editor.Tileset.prototype.PointFromTile = function (i) {
	var tx = (this.TileWidth + this.Padded ? 1 : 0);
	var ty = (this.TileHeight + this.Padded ? 1 : 0);
	var tilex = this.TilesPerRow;
	return new System.Drawing.Point((Math.floor(i / tilex) * ty), ((i % tilex) * tx));
};

Editor.Tileset.prototype.Bitmap = null;
Editor.Tileset.prototype.TileWidth = 0;
Editor.Tileset.prototype.TileHeight = 0;
Editor.Tileset.prototype.NumTiles = 0;
Editor.Tileset.prototype.Padded = false;
Editor.Tileset.prototype.TilesPerRow = 0;

Timer.prototype._ctor = function (r) {
	System.Object.prototype._ctor.call(this);
	this.rate = r;
};

Timer.prototype.get_Time = function () {
	return Math.floor((Math.floor(JSIL.Cast(Timer.timeGetTime(), System.Single)) * this.rate) / 1000);
};

Timer.op_Implicit = function (t) {
	return t.Time;
};

Object.defineProperty(Timer.prototype, "Time", {
		get: Timer.prototype.get_Time
	});
Timer.prototype.rate = 0;

Input.X360Gamepad.prototype.Poll = function () {
	this.ps = Microsoft.Xna.Framework.Input.GamePad.GetState(Microsoft.Xna.Framework.PlayerIndex.One);
};

Input.X360Gamepad.prototype.Axis = function (N) {

	switch (N) {
		case 0: 
			var thumbSticks = this.ps.ThumbSticks;
			var result = thumbSticks.Left.X;
			break;
		case 1: 
			thumbSticks = this.ps.ThumbSticks;
			result = thumbSticks.Left.Y;
			break;
		default: 
			throw new System.InvalidOperationException();
	}
	return result;
};

Input.X360Gamepad.prototype.Button = function (b) {

	switch (b) {
		case 0: 
			var buttons = this.ps.Buttons;
			var result = (buttons.A === Microsoft.Xna.Framework.Input.ButtonState.Pressed);
			break;
		case 1: 
			buttons = this.ps.Buttons;
			result = (buttons.X === Microsoft.Xna.Framework.Input.ButtonState.Pressed);
			break;
		default: 
			throw new System.InvalidOperationException();
	}
	return result;
};

Input.X360Gamepad.prototype._ctor = function () {
	System.Object.prototype._ctor.call(this);
};

Input.X360Gamepad.prototype.__ImplementInterface__(Input.IInputDevice);
Input.X360Gamepad.prototype.__StructFields__ = {
	ps: Microsoft.Xna.Framework.Input.GamePadState
};

Cataract.XNAGraph.prototype._ctor = function (device, contentManager) {
	System.Object.prototype._ctor.call(this);
	this.device = device;
	this.contentManager = contentManager;
	this.spriteBatch = new Microsoft.Xna.Framework.Graphics.SpriteBatch(device);
};

Cataract.XNAGraph.prototype.Begin = function () {
	this.spriteBatch.Begin();
};

Cataract.XNAGraph.prototype.End = function () {
	this.spriteBatch.End();
};

Cataract.XNAGraph.prototype.Blit$0 = function (img, pos, slice) {
	var destRect = new Microsoft.Xna.Framework.Rectangle();
	destRect._ctor(Math.floor(pos.X), Math.floor(pos.Y), slice.Width, slice.Height);
	this.spriteBatch.Draw(img, destRect.MemberwiseClone(), new System.Nullable$b1/* <Rectangle> */ (slice), Microsoft.Xna.Framework.Graphics.Color.White);
};

Cataract.XNAGraph.prototype.Blit$1 = function (src, x, y, trans) {
	this.spriteBatch.Draw(src, new Microsoft.Xna.Framework.Vector2(x, y), Microsoft.Xna.Framework.Graphics.Color.White);
};

Cataract.XNAGraph.prototype.ScaleBlit = function (src, x, y, w, h, trans) {
	this.spriteBatch.Draw(src, new Microsoft.Xna.Framework.Rectangle(x, y, w, h), Microsoft.Xna.Framework.Graphics.Color.White);
};

Cataract.XNAGraph.prototype.DrawParticle = function (x, y, size, r, g, b, a) {
};

Cataract.XNAGraph.prototype.DrawLine = function (x1, y1, x2, y2, r, g, b, a) {
};

Cataract.XNAGraph.prototype.DrawPoints$0 = function (points, color) {
};

Cataract.XNAGraph.prototype.DrawPoints$1 = function (points, color) {
	this.device.DrawUserPrimitives(Microsoft.Xna.Framework.Graphics.PrimitiveType.PointList, points, 0, points.length);
};

Cataract.XNAGraph.prototype.Clear = function () {
	this.device.Clear(Microsoft.Xna.Framework.Graphics.Color.Black);
};

Cataract.XNAGraph.prototype.LoadImage = function (fname) {
	return this.contentManager.Load(fname);
};

Cataract.XNAGraph.prototype.get_XRes = function () {
	return this.device.Viewport.Width;
};

Cataract.XNAGraph.prototype.get_YRes = function () {
	return this.device.Viewport.Height;
};

JSIL.OverloadedMethod(Cataract.XNAGraph.prototype, "Blit", [
		["Blit$0", [Microsoft.Xna.Framework.Graphics.Texture2D, Microsoft.Xna.Framework.Vector2, Microsoft.Xna.Framework.Rectangle]], 
		["Blit$1", [Microsoft.Xna.Framework.Graphics.Texture2D, System.Int32, System.Int32, System.Boolean]]
	]
);
JSIL.OverloadedMethod(Cataract.XNAGraph.prototype, "DrawPoints", [
		["DrawPoints$0", [System.Array.Of(Import.Geo.Vertex), Microsoft.Xna.Framework.Graphics.Color]], 
		["DrawPoints$1", [System.Array.Of(Microsoft.Xna.Framework.Graphics.VertexPositionColor), Microsoft.Xna.Framework.Graphics.Color]]
	]
);
Object.defineProperty(Cataract.XNAGraph.prototype, "XRes", {
		get: Cataract.XNAGraph.prototype.get_XRes
	});
Object.defineProperty(Cataract.XNAGraph.prototype, "YRes", {
		get: Cataract.XNAGraph.prototype.get_YRes
	});
Cataract.XNAGraph.prototype.device = null;
Cataract.XNAGraph.prototype.contentManager = null;
Cataract.XNAGraph.prototype.spriteBatch = null;


Editor.TileSetPreview.prototype._ctor = function (t) {
	this.panel = new Editor.TileSetPreview.DoubleBufferedPanel();
	System.Windows.Forms.ScrollableControl.prototype._ctor.call(this);
	this.Text = "Tile set";
	this.BackColor = System.Drawing.Color.Black;
	this.tileset = t;
	this.AutoScroll = true;
	this.DoubleBuffered = true;
	this.panel.Size = new System.Drawing.Size(t.Bitmap.Width, t.Bitmap.Height);
	this.Controls.Add(this.panel);
	this.panel.add_Paint(JSIL.Delegate.New("System.Windows.Forms.PaintEventHandler", this, Editor.TileSetPreview.prototype.Draw));
	this.panel.Show();
	this.ClientSize = this;
	this.Refresh();
	System.Windows.Forms.Control.prototype.add_Resize.call(this, JSIL.Delegate.New("System.EventHandler", this, Editor.TileSetPreview.prototype.OnResize));
	this.panel.add_MouseDown(JSIL.Delegate.New("System.Windows.Forms.MouseEventHandler", this, Editor.TileSetPreview.prototype.OnClick));
};

Editor.TileSetPreview.prototype.Draw = function (o, e) {
	e.Graphics.DrawImage(this.tileset.Bitmap, new System.Drawing.Rectangle(0, 0, this.tileset.Bitmap.Width, this.tileset.Bitmap.Height));
};

Editor.TileSetPreview.prototype.OnResize = function (o, e) {
	this.Refresh();
};

Editor.TileSetPreview.prototype.OnClick = function (o, e) {
	var i = this.tileset.TileFromPoint(e.X, e.Y);
	System.Console.WriteLine("Click {0},{1} on tile: {2}", e.X, e.Y, i);
	var p = this.tileset.PointFromTile(i);
	System.Console.WriteLine("{0},{1}", p.X, p.Y);

	if (this.ChangeTile !== null) {
		this.ChangeTile(i);
	}
};

Editor.TileSetPreview.prototype.add_ChangeTile = function (value) {
	this.ChangeTile = System.Delegate.Combine(this.ChangeTile, value);
};

Editor.TileSetPreview.prototype.remove_ChangeTile = function (value) {
	this.ChangeTile = System.Delegate.Remove(this.ChangeTile, value);
};

Editor.TileSetPreview.prototype.tileset = null;
Editor.TileSetPreview.prototype.panel = null;
Editor.TileSetPreview.prototype.ChangeTile = null;

Editor.TileSetPreview.DoubleBufferedPanel.prototype._ctor = function () {
	System.Windows.Forms.Panel.prototype._ctor.call(this);
	System.Windows.Forms.Control.prototype.SetStyle.call(this, System.Windows.Forms.ControlStyles.UserPaint | System.Windows.Forms.ControlStyles.AllPaintingInWmPaint | System.Windows.Forms.ControlStyles.OptimizedDoubleBuffer, true);
};


Editor.MapInfoView.prototype._ctor = function (e) {
	System.Windows.Forms.Control.prototype._ctor.call(this);
	this.editor = e;
	this.engine = e.engine;
	this.Text = "Map Properties";
	this.curlay = new System.Windows.Forms.ListBox();
	this.Controls.Add(this.curlay);
	this.curlay.Left = 10;
	this.curlay.Top = 10;
	this.curlay.add_SelectedIndexChanged(JSIL.Delegate.New("System.EventHandler", this, Editor.MapInfoView.prototype.SwitchLayer));
	var layerbuttons = [["Move &Up", JSIL.Delegate.New("System.EventHandler", this, Editor.MapInfoView.prototype.MoveLayerUp)], ["Move &Down", JSIL.Delegate.New("System.EventHandler", this, Editor.MapInfoView.prototype.MoveLayerDown)], ["&Remove", JSIL.Delegate.New("System.EventHandler", this, Editor.MapInfoView.prototype.RemoveLayer)], ["Insert &New", JSIL.Delegate.New("System.EventHandler", this, Editor.MapInfoView.prototype.InsertNewLayer)], ["&Add New", JSIL.Delegate.New("System.EventHandler", this, Editor.MapInfoView.prototype.AddNewLayer)], ["&Hide", JSIL.Delegate.New("System.EventHandler", this, Editor.MapInfoView.prototype.HideLayer)], ["&Show", JSIL.Delegate.New("System.EventHandler", this, Editor.MapInfoView.prototype.ShowLayer)], ["Show &Only", JSIL.Delegate.New("System.EventHandler", this, Editor.MapInfoView.prototype.ShowLayerOnly)], ["Show A&ll", JSIL.Delegate.New("System.EventHandler", this, Editor.MapInfoView.prototype.ShowAll)]];
	var x = 140;
	var y = 10;
	var array = layerbuttons;
	var j = 0;

__while0__: 
	while (j < array.length) {
		var o = array[j];
		var b = new System.Windows.Forms.Button();
		b.Text = JSIL.Cast(o[0], System.String);
		b.add_Click(o[1]);
		b.Location = new System.Drawing.Point(x, y);
		y += (b.Height + 5);
		this.Controls.Add(b);
		++j;
	}
	this.Height = this;
	x = 280;
	y = 10;
	var i = new System.Windows.Forms.Label();
	i.Text = "Name";
	i.Location = new System.Drawing.Point(x, y);
	this.Controls.Add(i);
	this.layername = new System.Windows.Forms.TextBox();
	this.layername.Location = new System.Drawing.Point((x + i.Width), y);
	this.Controls.Add(this.layername);
	var renamelay = new System.Windows.Forms.Button();
	renamelay.Text = "Rename";
	renamelay.add_Click(JSIL.Delegate.New("System.EventHandler", this, Editor.MapInfoView.prototype.RenameLayer));
	renamelay.Location = new System.Drawing.Point((this.layername.Right + 5), this.layername.Top);
	this.Controls.Add(renamelay);
	this.Width = this;
	y += (i.Height + 15);
	var lw = new System.Windows.Forms.Label();
	lw.Text = "Width";
	lw.Location = new System.Drawing.Point(x, y);
	this.Controls.Add(lw);
	this.widthbox = new System.Windows.Forms.TextBox();
	this.widthbox.Location = new System.Drawing.Point((x + lw.Width), y);
	var num = this.engine.map.Width;
	this.widthbox.Text = num.toString();
	this.Controls.Add(this.widthbox);
	var resize = new System.Windows.Forms.Button();
	resize.Text = "Resize";
	resize.add_Click(JSIL.Delegate.New("System.EventHandler", this, Editor.MapInfoView.prototype.ChangeDims));
	resize.Location = new System.Drawing.Point((this.widthbox.Right + 5), (this.widthbox.Top + 10));
	this.Controls.Add(resize);
	y += (i.Height + 5);
	var lh = new System.Windows.Forms.Label();
	lh.Text = "Height";
	lh.Location = new System.Drawing.Point(x, y);
	this.Controls.Add(lh);
	this.heightbox = new System.Windows.Forms.TextBox();
	this.heightbox.Location = new System.Drawing.Point((x + lh.Width), y);
	num = this.engine.map.Height;
	this.heightbox.Text = num.toString();
	this.Controls.Add(this.heightbox);
	this.Update();
};

Editor.MapInfoView.prototype.Update = function () {
	this.curlay.Items.Clear();
	var i = 0;

__while0__: 
	while (i < this.engine.map.NumLayers) {
		this.curlay.Items.Add(this.engine.map.get_Item(i).Name);
		++i;
	}

	if (this.editor.tilesetmode.curlayer >= this.engine.map.NumLayers) {
		this.editor.tilesetmode.curlayer = (this.engine.map.NumLayers - 1);
	}
	var num = this.engine.map.Height;
	this.heightbox.Text = num.toString();
	num = this.engine.map.Width;
	this.widthbox.Text = num.toString();
	this.curlay.SelectedIndex = this.editor.tilesetmode.curlayer;
	this.layername.Text = JSIL.Cast(this.curlay.SelectedItem, System.String);
};

Editor.MapInfoView.prototype.SwitchLayer = function (o, e) {
	this.editor.tilesetmode.curlayer = this.curlay.SelectedIndex;
	this.layername.Text = JSIL.Cast(this.curlay.SelectedItem, System.String);
};

Editor.MapInfoView.prototype.MoveLayerUp = function (o, e) {
	var lay = this.curlay.SelectedIndex;

	if (!(lay === 0)) {
		this.engine.map.SwapLayers(lay, (lay - 1));
		this.curlay.SelectedIndex -= 1;
		this.Update();
	}
};

Editor.MapInfoView.prototype.MoveLayerDown = function (o, e) {
	var lay = this.curlay.SelectedIndex;

	if (!(lay === (this.engine.map.NumLayers - 1))) {
		this.engine.map.SwapLayers(lay, (lay + 1));
		this.curlay.SelectedIndex += 1;
		this.Update();
	}
};

Editor.MapInfoView.prototype.RemoveLayer = function (o, e) {
	var result = System.Windows.Forms.MessageBox.Show("Are you sure?  There is no undoing this.", "MAYDAY MAYDAY WE ARE UNDER ATTACK", System.Windows.Forms.MessageBoxButtons.YesNo);

	if (result === System.Windows.Forms.DialogResult.Yes) {
		this.engine.map.RemoveLayer(this.curlay.SelectedIndex);
		this.Update();
	}
};

Editor.MapInfoView.prototype.InsertNewLayer = function (o, e) {
	this.engine.map.AddLayer(this.curlay.SelectedIndex);
	this.Update();
};

Editor.MapInfoView.prototype.AddNewLayer = function (o, e) {
	this.engine.map.AddLayer();
	this.Update();
};

Editor.MapInfoView.prototype.HideLayer = function (o, e) {
	this.engine.map.get_Item(this.curlay.SelectedIndex).visible = false;
};

Editor.MapInfoView.prototype.ShowLayer = function (o, e) {
	this.engine.map.get_Item(this.curlay.SelectedIndex).visible = true;
};

Editor.MapInfoView.prototype.ShowLayerOnly = function (o, e) {
	var i = 0;

__while0__: 
	while (i < this.engine.map.NumLayers) {
		this.engine.map.get_Item(i).visible = (i === this.curlay.SelectedIndex);
		++i;
	}
};

Editor.MapInfoView.prototype.ShowAll = function (o, e) {
	var i = 0;

__while0__: 
	while (i < this.engine.map.NumLayers) {
		this.engine.map.get_Item(i).visible = true;
		++i;
	}
};

Editor.MapInfoView.prototype.RenameLayer = function (o, e) {
	var i = this.curlay.SelectedIndex;
	this.curlay.Items.set_Item(i, this.layername.Text);
	this.engine.map.get_Item(i).Name = this.layername.Text;
};

Editor.MapInfoView.prototype.ChangeDims = function (o, e) {
	var result = System.Windows.Forms.MessageBox.Show("Are you sure?  There is no undoing this.", "MAYDAY MAYDAY WE ARE UNDER ATTACK", System.Windows.Forms.MessageBoxButtons.YesNo);

	if (result === System.Windows.Forms.DialogResult.Yes) {
		this.engine.map.Resize(
			System.Convert.ToInt32(this.widthbox.Text), 
			System.Convert.ToInt32(this.heightbox.Text)
		);
		this.Update();
	}
};

Editor.MapInfoView.prototype.editor = null;
Editor.MapInfoView.prototype.engine = null;
Editor.MapInfoView.prototype.curlay = null;
Editor.MapInfoView.prototype.layername = null;
Editor.MapInfoView.prototype.widthbox = null;
Editor.MapInfoView.prototype.heightbox = null;

Editor.EntityEditMode.prototype._ctor = function (e) {
	System.Object.prototype._ctor.call(this);
	this.editor = e;
	this.engine = e.engine;
};

Editor.EntityEditMode.prototype.FindEnt = function (x, y) {
	var enumerator = this.engine.map.Entities.GetEnumerator();

	try {

	__while0__: 
		while (enumerator.IEnumerator_MoveNext()) {
			var e = JSIL.Cast(enumerator.IEnumerator_Current, Import.MapEnt);

			if ((e.x <= x) && (e.y <= y)) {

				if ((x <= (e.x + 16)) && (y <= (e.y + 16))) {
					var result = e;
					return result;
				}
			}
		}
	} finally {
		var disposable = JSIL.TryCast(enumerator, System.IDisposable);

		if (disposable !== null) {
			disposable.IDisposable_Dispose();
		}
	}
	result = null;
	return result;
};

Editor.EntityEditMode.prototype.MouseDown = function (e, b) {
};

Editor.EntityEditMode.prototype.MouseUp = function (e, b) {
};

Editor.EntityEditMode.prototype.MouseClick = function (e, b) {
	var x = (e.X + this.engine.XWin);
	var y = (e.Y + this.engine.YWin);

	if ((System.Windows.Forms.Control.ModifierKeys & System.Windows.Forms.Keys.None | System.Windows.Forms.Keys.Shift) !== System.Windows.Forms.Keys.None) {
		var ent = new Import.MapEnt();
		ent.type = "Ripper";
		ent.x = x;
		ent.y = y;
		this.engine.map.Entities.Add(ent);
		this.CurEnt = ent;
	} else {
		ent = this.FindEnt(x, y);

		if (!(ent === null)) {
			this.CurEnt = ent;
		}
	}
};

Editor.EntityEditMode.prototype.KeyPress = function (e) {

	if (!(this.CurEnt === null)) {
		var size = 1;

		if ((System.Windows.Forms.Control.ModifierKeys & System.Windows.Forms.Keys.None | System.Windows.Forms.Keys.Shift) !== System.Windows.Forms.Keys.None) {
			size = 10;
		}
		var keyCode = e.KeyCode;

		if (keyCode !== System.Windows.Forms.Keys.None | System.Windows.Forms.Keys.RButton | System.Windows.Forms.Keys.MButton | System.Windows.Forms.Keys.XButton2 | System.Windows.Forms.Keys.Back | System.Windows.Forms.Keys.LineFeed | System.Windows.Forms.Keys.Clear | System.Windows.Forms.Keys.Space | System.Windows.Forms.Keys.Next | System.Windows.Forms.Keys.PageDown | System.Windows.Forms.Keys.Home | System.Windows.Forms.Keys.Up | System.Windows.Forms.Keys.Down | System.Windows.Forms.Keys.Print | System.Windows.Forms.Keys.Snapshot | System.Windows.Forms.Keys.PrintScreen | System.Windows.Forms.Keys.Delete) {

			switch (keyCode) {
				case 98: 
					this.CurEnt.y += size;
					break;
				case 100: 
					this.CurEnt.x -= size;
					break;
				case 102: 
					this.CurEnt.x += size;
					break;
				case 104: 
					this.CurEnt.y -= size;
					break;
			}
		} else {
			this.engine.map.Entities.Remove(this.CurEnt);
			this.CurEnt = null;
		}
	}
};

Editor.EntityEditMode.prototype.get_CurEnt = function () {
	return this.curent;
};

Editor.EntityEditMode.prototype.set_CurEnt = function (value) {
	this.editor.mapentpropertiesview.UpdateEnt(this.curent);
	this.editor.mapentpropertiesview.UpdateDlg(value);
	this.curent = value;
};

Editor.EntityEditMode.prototype.RenderHUD = function () {
};

Editor.EntityEditMode.prototype.MouseWheel = function (p, delta) {
};

Object.defineProperty(Editor.EntityEditMode.prototype, "CurEnt", {
		get: Editor.EntityEditMode.prototype.get_CurEnt, 
		set: Editor.EntityEditMode.prototype.set_CurEnt
	});
Editor.EntityEditMode.prototype.__ImplementInterface__(Editor.IEditorState);
Editor.EntityEditMode.prototype.editor = null;
Editor.EntityEditMode.prototype.engine = null;
Editor.EntityEditMode.prototype.curent = null;

Entities.Enemies.Skree.prototype._ctor = function (e, startx, starty) {
	this.flying = false;
	Entities.Enemies.Enemy.prototype._ctor.call(this, e, e.RipperSprite);
	this.x = startx;
	this.y = starty;
	this.hp = 10;
	this.direction = Entities.Dir.right;
	this.anim.Set(2, 3, 5, true);
	this.UpdateState = JSIL.Delegate.New("System.Action", this, Entities.Enemies.Skree.prototype.DoTick);
};

Entities.Enemies.Skree.prototype.DoTick = function () {

	if (!(this.flying || !JSIL.CheckType(this.engine.DetectInXCoords(this), Entities.Player))) {
		this.flying = true;
	}

	if (this.flying) {
		Entities.Entity.prototype.HandleGravity.call(this);
	}

	if (this.touchingground) {
		this.engine.SpawnEntity(new Entities.Boom(this.engine, this.x, this.y));
		this.engine.DestroyEntity(this);
	}
};

Entities.Enemies.Skree.prototype.flying = false;

Import.MannuxMap.ReadString = function (file) {
	var s = "";

__while0__: 
	while (true) {
		var c = file.ReadByte();

		if (c === 0) {
			break __while0__;
		}
		s = System.String.Concat(s, c);
	}
	return s;
};

Import.MannuxMap.Load = function (s) {
	var file = new System.IO.BinaryReader(s);
	var sig = Import.MannuxMap.ReadString(file);

	if (System.String.op_Inequality(sig, "Mannux Map wee!")) {
		throw new System.Exception("Not a Mannux map!");
	}
	var map = new Import.Map();
	var width = file.ReadInt32();
	var height = file.ReadInt32();
	map.Resize(width, height);
	var numlayers = file.ReadInt32();
	var layerdata = JSIL.Array.New(System.Int32, (width * height));
	var i = 0;

__while0__: 
	while (i < numlayers) {
		var j = 0;

	__while1__: 
		while (j < (width * height)) {
			layerdata[j] = file.ReadInt32();
			++j;
		}
		map.AddLayer(layerdata);
		map.get_Item(i).parx = JSIL.Cast(file.ReadDouble(), System.Single);
		map.get_Item(i).pary = JSIL.Cast(file.ReadDouble(), System.Single);
		map.get_Item(i).Name = Import.MannuxMap.ReadString(file);
		++i;
	}
	var numents = file.ReadInt32();
	i = 0;

__while2__: 
	while (i < numents) {
		var e = new Import.MapEnt();
		e.x = file.ReadInt32();
		e.y = file.ReadInt32();
		e.type = Import.MannuxMap.ReadString(file);
		var numprops = file.ReadInt32();
		e.data = JSIL.Array.New(System.String, numprops);
		j = 0;

	__while3__: 
		while (j < numprops) {
			e.data[j] = Import.MannuxMap.ReadString(file);
			++j;
		}
		map.Entities.Add(e);
		++i;
	}
	var numpoints = file.ReadInt32();
	i = 0;

__while4__: 
	while (i < numpoints) {
		var x = file.ReadInt32();
		map.Obs.Points.Add(new Import.Geo.Vertex(x, file.ReadInt32()));
		++i;
	}
	var numsegments = file.ReadInt32();
	i = 0;

__while5__: 
	while (i < numsegments) {
		var p = file.ReadInt32();
		map.Obs.Lines.Add(JSIL.Array.New(System.Int32, [p, file.ReadInt32()]));
		++i;
	}
	return map;
};

Import.MannuxMap.WriteString = function (file, s) {
	var i = 0;

__while0__: 
	while (i < s.length) {
		var c = s.get_Chars(i);
		file.Write(c);
		++i;
	}
	file.Write(0);
};

Import.MannuxMap.Save = function (map, s) {
	var file = new System.IO.BinaryWriter(s);
	Import.MannuxMap.WriteString(file, "Mannux Map wee!");
	file.Write(map.Width);
	file.Write(map.Height);
	file.Write(map.Layers.Count);
	var enumerator = map.Layers.GetEnumerator();

	try {

	__while0__: 
		while (enumerator.IEnumerator_MoveNext()) {
			var lay = JSIL.Cast(enumerator.IEnumerator_Current, Import.Map.Layer);
			var y = 0;

		__while1__: 
			while (y < map.Height) {
				var x = 0;

			__while2__: 
				while (x < map.Width) {
					file.Write(lay.get_Item(x, y));
					++x;
				}
				++y;
			}
			file.Write(lay.parx);
			file.Write(lay.pary);
			Import.MannuxMap.WriteString(file, lay.Name);
		}
	} finally {
		var disposable = JSIL.TryCast(enumerator, System.IDisposable);

		if (disposable !== null) {
			disposable.IDisposable_Dispose();
		}
	}
	file.Write(map.Entities.Count);
	enumerator = map.Entities.GetEnumerator();

	try {

	__while3__: 
		while (enumerator.IEnumerator_MoveNext()) {
			var ent = JSIL.Cast(enumerator.IEnumerator_Current, Import.MapEnt);
			file.Write(ent.x);
			file.Write(ent.y);
			Import.MannuxMap.WriteString(file, ent.type);

			if (ent.data !== null) {
				file.Write(ent.data.length);
				var data = ent.data;
				var j = 0;

			__while4__: 
				while (j < data.length) {
					Import.MannuxMap.WriteString(file, data[j]);
					++j;
				}
			} else {
				file.Write(0);
			}
		}
	} finally {
		disposable = JSIL.TryCast(enumerator, System.IDisposable);

		if (disposable !== null) {
			disposable.IDisposable_Dispose();
		}
	}
	file.Write(map.Obs.Points.Count);
	var enumerator2 = map.Obs.Points.GetEnumerator();

	try {

	__while5__: 
		while (enumerator2.MoveNext()) {
			var p = enumerator2.Current;
			file.Write(p.X);
			file.Write(p.Y);
		}
	} finally {
		enumerator2.IDisposable_Dispose();
	}
	file.Write(map.Obs.Lines.Count);
	enumerator = map.Obs.Lines.GetEnumerator();

	try {

	__while6__: 
		while (enumerator.IEnumerator_MoveNext()) {
			var i = JSIL.Cast(enumerator.IEnumerator_Current, System.Array.Of(System.Int32));
			file.Write(i[0]);
			file.Write(i[1]);
		}
	} finally {
		disposable = JSIL.TryCast(enumerator, System.IDisposable);

		if (disposable !== null) {
			disposable.IDisposable_Dispose();
		}
	}
};

Import.MannuxMap.prototype._ctor = function () {
	System.Object.prototype._ctor.call(this);
};

Import.MannuxMap._cctor = function () {
	Object.defineProperty(Import.MannuxMap, "mapsig", {
			"value": "Mannux Map wee!"}
	);
};


AnimState.prototype._ctor$0 = function () {
	this.dead = false;
	System.Object.prototype._ctor.call(this);
	this.frame = 0;
	this.count = 0;
	this.first = this.last = this.delay = 0;
	this.delay = 0;
	this.dead = true;
};

AnimState.prototype._ctor$1 = function (f, l, d, L) {
	this.dead = false;
	System.Object.prototype._ctor.call(this);
	this.first = f;
	this.last = l;
	this.delay = d;
	this.loop = L;
	this.count = 0;
	this.frame = this.first;
};

AnimState.prototype.Set$0 = function (s) {
	this.Set(s.first, s.last, s.delay, s.loop);
};

AnimState.prototype.Set$1 = function (f, l, d, L) {
	this.first = f;
	this.last = l;
	this.delay = d;
	this.loop = L;
	this.frame = this.first;
	this.count = this.delay;
	this.dead = false;
};

AnimState.prototype.Update = function () {

	if (!this.dead) {
		--this.count;

		if (!(this.count > 0)) {
			this.count = this.delay;

			if (this.first < this.last) {
				++this.frame;

				if (this.frame > this.last) {

					if (!this.loop) {
						this.frame = this.last;
						this.dead = true;
						return ;
					}
					this.frame = this.first;
				}
			}

			if (this.first > this.last) {
				--this.frame;

				if (this.frame < this.last) {

					if (this.loop) {
						this.frame = this.first;
					} else {
						this.frame = this.last;
						this.dead = true;
					}
				}
			}
		}
	}
};

JSIL.OverloadedMethod(AnimState.prototype, "_ctor", [
		["_ctor$0", []], 
		["_ctor$1", [System.Int32, System.Int32, System.Int32, System.Boolean]]
	]
);
JSIL.OverloadedMethod(AnimState.prototype, "Set", [
		["Set$0", [AnimState]], 
		["Set$1", [System.Int32, System.Int32, System.Int32, System.Boolean]]
	]
);
AnimState.prototype.frame = 0;
AnimState.prototype.count = 0;
AnimState.prototype.first = 0;
AnimState.prototype.last = 0;
AnimState.prototype.delay = 0;
AnimState.prototype.loop = false;
AnimState.prototype.dead = false;
AnimState._cctor = function () {
	AnimState.playerstand = null;
	AnimState.playerwalk = null;
	AnimState.playerwalkshootingangleup = null;
	AnimState.playerwalkshootingangledown = null;
	AnimState.playerwalkshooting = null;
	AnimState.playershooting = null;
	AnimState.playershootup = null;
	AnimState.playerjump = null;
	AnimState.playerfall = null;
	AnimState.playerfallshooting = null;
	AnimState.playerfallshootingangleup = null;
	AnimState.playerfallshootingangledown = null;
	AnimState.playerfallshootingup = null;
	AnimState.playerfallshootingdown = null;
	AnimState.playercrouch = null;
	AnimState.playercrouchshooting = null;
	AnimState.playercrouchshootingangleup = null;
	AnimState.playercrouchshootingangledown = null;
	AnimState.hurt = null;
	AnimState.playerstand = JSIL.Array.New(AnimState, [new AnimState(0, 0, 0, false), new AnimState(8, 8, 0, false)]);
	AnimState.playerwalk = JSIL.Array.New(AnimState, [new AnimState(16, 23, 20, true), new AnimState(24, 31, 20, true)]);
	AnimState.playerwalkshootingangleup = JSIL.Array.New(AnimState, [new AnimState(48, 55, 20, true), new AnimState(56, 63, 20, true)]);
	AnimState.playerwalkshootingangledown = JSIL.Array.New(AnimState, [new AnimState(64, 71, 20, true), new AnimState(72, 79, 20, true)]);
	AnimState.playerwalkshooting = JSIL.Array.New(AnimState, [new AnimState(32, 39, 20, true), new AnimState(40, 47, 20, true)]);
	AnimState.playershooting = JSIL.Array.New(AnimState, [new AnimState(4, 4, 0, false), new AnimState(12, 12, 0, false)]);
	AnimState.playershootup = JSIL.Array.New(AnimState, [new AnimState(10, 10, 0, false), new AnimState(10, 10, 0, false)]);
	AnimState.playerjump = JSIL.Array.New(AnimState, [new AnimState(82, 85, 12, true), new AnimState(90, 93, 12, true)]);
	AnimState.playerfall = JSIL.Array.New(AnimState, [new AnimState(80, 80, 0, false), new AnimState(88, 88, 0, false)]);
	AnimState.playerfallshooting = JSIL.Array.New(AnimState, [new AnimState(98, 98, 0, false), new AnimState(106, 106, 0, false)]);
	AnimState.playerfallshootingangleup = JSIL.Array.New(AnimState, [new AnimState(97, 97, 0, false), new AnimState(105, 105, 0, false)]);
	AnimState.playerfallshootingangledown = JSIL.Array.New(AnimState, [new AnimState(99, 99, 0, false), new AnimState(107, 107, 0, false)]);
	AnimState.playerfallshootingup = JSIL.Array.New(AnimState, [new AnimState(96, 96, 0, false), new AnimState(104, 104, 0, false)]);
	AnimState.playerfallshootingdown = JSIL.Array.New(AnimState, [new AnimState(100, 100, 0, false), new AnimState(108, 108, 0, false)]);
	AnimState.playercrouch = JSIL.Array.New(AnimState, [new AnimState(116, 116, 0, false), new AnimState(112, 112, 0, false)]);
	AnimState.playercrouchshooting = JSIL.Array.New(AnimState, [new AnimState(118, 118, 0, false), new AnimState(114, 114, 0, false)]);
	AnimState.playercrouchshootingangleup = JSIL.Array.New(AnimState, [new AnimState(117, 117, 0, false), new AnimState(113, 113, 0, false)]);
	AnimState.playercrouchshootingangledown = JSIL.Array.New(AnimState, [new AnimState(119, 119, 0, false), new AnimState(115, 115, 0, false)]);
	AnimState.hurt = JSIL.Array.New(AnimState, [new AnimState(7, 7, 0, false), new AnimState(15, 15, 0, false)]);
};


Mannux.MannuxGame.prototype._ctor = function () {
	Microsoft.Xna.Framework.Game.prototype._ctor.call(this);
	this.graphics = new Microsoft.Xna.Framework.GraphicsDeviceManager(this);
	this.Content.RootDirectory = "Content";
};

Mannux.MannuxGame.prototype.Initialize = function () {
	Microsoft.Xna.Framework.Game.prototype.Initialize.call(this);
};

Mannux.MannuxGame.prototype.LoadContent = function () {
	this.spriteBatch = new Microsoft.Xna.Framework.Graphics.SpriteBatch(this.GraphicsDevice);
};

Mannux.MannuxGame.prototype.UnloadContent = function () {
};

Mannux.MannuxGame.prototype.Update = function (gameTime) {

	if (Microsoft.Xna.Framework.Input.GamePad.GetState(Microsoft.Xna.Framework.PlayerIndex.One).Buttons.Back === Microsoft.Xna.Framework.Input.ButtonState.Pressed) {
		Microsoft.Xna.Framework.Game.prototype.Exit.call(this);
	}
	Microsoft.Xna.Framework.Game.prototype.Update.call(this, gameTime);
};

Mannux.MannuxGame.prototype.Draw = function (gameTime) {
	this.GraphicsDevice.Clear(Microsoft.Xna.Framework.Graphics.Color.CornflowerBlue);
	Microsoft.Xna.Framework.Game.prototype.Draw.call(this, gameTime);
};

Mannux.MannuxGame.prototype.graphics = null;
Mannux.MannuxGame.prototype.spriteBatch = null;

Editor.Editor.prototype.add_OnExit = function (value) {
	this.OnExit = System.Delegate.Combine(this.OnExit, value);
};

Editor.Editor.prototype.remove_OnExit = function (value) {
	this.OnExit = System.Delegate.Remove(this.OnExit, value);
};

Editor.Editor.prototype._ctor = function (e) {
	this.form = new System.Windows.Forms.Form();
	this.tabs = new System.Windows.Forms.TabControl();
	this.Running = false;
	System.Object.prototype._ctor.call(this);
	this.engine = e;
	this.graph = e.graph;
	this.tileset = new Editor.Tileset(new System.Drawing.Bitmap("mantiles.png"), 16, 16, 19, 756);
	this.tilesetmode = new Editor.TileSetMode(this);
	this.copypastemode = new Editor.CopyPasteMode(this);
	this.obstructionmode = new Editor.ObstructionMode(this);
	this.entityeditmode = new Editor.EntityEditMode(this);
	this.tabs.Dock = System.Windows.Forms.DockStyle.Fill;
	this.form.Controls.Add(this.tabs);
	this.form.Size = new System.Drawing.Size(800, 400);
	this.statbar = new System.Windows.Forms.StatusBar();
	this.statbar.Panels.Add(new System.Windows.Forms.StatusBarPanel());
	this.statbar.Panels.Add(new System.Windows.Forms.StatusBarPanel());
	this.statbar.Panels.get_Item(0).AutoSize = System.Windows.Forms.StatusBarPanelAutoSize.Spring;
	this.statbar.Panels.get_Item(1).AutoSize = System.Windows.Forms.StatusBarPanelAutoSize.Contents;
	this.statbar.ShowPanels = true;
	this.menu = new System.Windows.Forms.MainMenu(JSIL.Array.New(System.Windows.Forms.MenuItem, [new System.Windows.Forms.MenuItem("&File", JSIL.Array.New(System.Windows.Forms.MenuItem, [new System.Windows.Forms.MenuItem("&New", JSIL.Delegate.New("System.EventHandler", this, Editor.Editor.prototype.NewMap), System.Windows.Forms.Shortcut.CtrlN), new System.Windows.Forms.MenuItem("&Open...", JSIL.Delegate.New("System.EventHandler", this, Editor.Editor.prototype.OpenMap), System.Windows.Forms.Shortcut.CtrlO), new System.Windows.Forms.MenuItem("-"), new System.Windows.Forms.MenuItem("&Save", JSIL.Delegate.New("System.EventHandler", this, Editor.Editor.prototype.SaveMap), System.Windows.Forms.Shortcut.CtrlS), new System.Windows.Forms.MenuItem("Save &As...", JSIL.Delegate.New("System.EventHandler", this, Editor.Editor.prototype.SaveMapAs), System.Windows.Forms.Shortcut.F12)])), new System.Windows.Forms.MenuItem("&Edit", JSIL.Array.New(System.Windows.Forms.MenuItem, [new System.Windows.Forms.MenuItem("&Map Properties...", JSIL.Delegate.New("System.EventHandler", this, Editor.Editor.prototype.ShowMapProperties)), new System.Windows.Forms.MenuItem("&Tileset...", JSIL.Delegate.New("System.EventHandler", this, Editor.Editor.prototype.ShowTileSet)), new System.Windows.Forms.MenuItem("Map &Entities...", JSIL.Delegate.New("System.EventHandler", this, Editor.Editor.prototype.ShowMapEntProperties)), new System.Windows.Forms.MenuItem("&Auto Selection Thing...", JSIL.Delegate.New("System.EventHandler", this, Editor.Editor.prototype.ShowAutoSelectionThing))])), new System.Windows.Forms.MenuItem("&Mode", JSIL.Array.New(System.Windows.Forms.MenuItem, [new System.Windows.Forms.MenuItem("&Tiles", JSIL.Delegate.New("System.EventHandler", this, Editor.Editor.prototype.SetTileSetMode)), new System.Windows.Forms.MenuItem("&Copy/paste", JSIL.Delegate.New("System.EventHandler", this, Editor.Editor.prototype.SetCopyPasteMode)), new System.Windows.Forms.MenuItem("&Obstructions", JSIL.Delegate.New("System.EventHandler", this, Editor.Editor.prototype.SetObstructionEditMode)), new System.Windows.Forms.MenuItem("Map &Entities", JSIL.Delegate.New("System.EventHandler", this, Editor.Editor.prototype.SetMapEntEditMode))]))]));
	this.tilesetpreview = new Editor.TileSetPreview(this.tileset);
	this.tilesetpreview.add_ChangeTile(JSIL.Delegate.New("Editor.ChangeTileHandler", this, Editor.Editor.prototype.OnTileChange));
	this.mapinfoview = new Editor.MapInfoView(this);
	this.mapentpropertiesview = new Editor.MapEntPropertiesView(this);
	this.autoselectionthing = new Editor.AutoSelectionThing(this);
	this.form.Text = "Mannux Editor";
	this.form.Menu = this.menu;
	this.form.Controls.Add(this.statbar);
	this.AddTab("Layers", this.mapinfoview);
	this.AddTab("Entities", this.mapentpropertiesview);
	this.AddTab("Tiles", this.tilesetpreview);
	this.AddTab("Selection", this.autoselectionthing);
	var i = this.engine.input.Mouse;
	i.add_MouseDown(JSIL.Delegate.New("System.Action`2[Microsoft.Xna.Framework.Point, Input.MouseButton]", this, Editor.Editor.prototype.MouseClick));
	i.add_MouseUp(JSIL.Delegate.New("System.Action`2[Microsoft.Xna.Framework.Point, Input.MouseButton]", this, Editor.Editor.prototype.MouseUp));
	i.add_Moved(JSIL.Delegate.New("System.Action`2[Microsoft.Xna.Framework.Point, Input.MouseButton]", this, Editor.Editor.prototype.MouseDown));
};

Editor.Editor.prototype.AddTab = function (text, c) {
	c.Dock = System.Windows.Forms.DockStyle.Fill;
	var tp = new System.Windows.Forms.TabPage(text);
	tp.Controls.Add(c);
	this.tabs.TabPages.Add(tp);
};

Editor.Editor.prototype.Init = function () {
	this.form.Show();
	this.state = this.tilesetmode;
};

Editor.Editor.prototype.Shutdown = function () {
	this.Running = false;
	this.engine.obs.Generate(this.engine.map.Obs);
	this.mapinfoview.Hide();
	this.mapentpropertiesview.Hide();
	this.tilesetpreview.Hide();

	if (this.OnExit !== null) {
		this.OnExit();
	}
};

Editor.Editor.prototype.Execute = function () {
	this.Running = true;
	this.Init();
};

Editor.Editor.prototype.Update = function () {

	if (this.engine.input.Keyboard.IInputDevice_Axis(1) === 0) {
		this.engine.XWin -= 2;
	}

	if (this.engine.input.Keyboard.IInputDevice_Axis(1) === 255) {
		this.engine.XWin += 2;
	}

	if (this.engine.input.Keyboard.IInputDevice_Axis(0) === 0) {
		this.engine.YWin -= 2;
	}

	if (this.engine.input.Keyboard.IInputDevice_Axis(0) === 255) {
		this.engine.YWin += 2;
	}
	this.UpdateMouse();

	if (this.engine.input.Keyboard.IInputDevice_Button(2)) {
		this.Shutdown();
	}
};

Editor.Editor.prototype.UpdateMouse = function () {
	this.engine.input.Mouse.SendEvents();
};

Editor.Editor.prototype.Render = function () {
	var i = 0;
	var enumerator = this.engine.map.Layers.GetEnumerator();

	try {

	__while0__: 
		while (enumerator.IEnumerator_MoveNext()) {
			var lay = JSIL.Cast(enumerator.IEnumerator_Current, Import.Map.Layer);

			if (lay.visible) {
				this.engine.RenderLayer(lay, (i++ !== 0));
			}
		}
	} finally {
		var disposable = JSIL.TryCast(enumerator, System.IDisposable);

		if (disposable !== null) {
			disposable.IDisposable_Dispose();
		}
	}
	this.engine.RenderEntities();
	this.state.IEditorState_RenderHUD();
};

Editor.Editor.prototype.MouseClick = function (pos, b) {
	this.state.IEditorState_MouseClick(pos, b);
};

Editor.Editor.prototype.MouseDown = function (pos, b) {
	this.state.IEditorState_MouseDown(pos, b);
};

Editor.Editor.prototype.MouseUp = function (pos, b) {
	this.state.IEditorState_MouseUp(pos, b);
};

Editor.Editor.prototype.KeyPress = function (o, e) {
	this.state.IEditorState_KeyPress(e);
};

Editor.Editor.prototype.OnClosing = function (o, e) {
	e.Cancel = true;
};

Editor.Editor.prototype.NewMap = function (o, e) {
	var dlg = new Editor.NewMapDlg();
	var result = dlg.ShowDialog();

	if (!(result === System.Windows.Forms.DialogResult.Cancel)) {
		this.engine.map = new Import.Map(dlg.MapWidth, dlg.MapHeight);
		this.engine.map.AddLayer();
		this.engine.entities.Clear();
		this.engine.entities.Add(this.engine.player);
		this.tilesetmode.curlayer = 0;
		this.mapinfoview.Update();
	}
};

Editor.Editor.prototype.OpenMap = function (o, e) {
	var dlg = new System.Windows.Forms.OpenFileDialog();
	dlg.DefaultExt = "map";
	dlg.Filter = "Mannux Maps (*.map)|*.map|All files (*.*)|*.*";
	dlg.Title = "Open Map";
	var s = System.IO.Directory.GetCurrentDirectory();
	var result = dlg.ShowDialog();

	if (result === System.Windows.Forms.DialogResult.OK) {
		this.engine.MapSwitch(dlg.FileName);
		this.mapinfoview.Update();
		System.IO.Directory.SetCurrentDirectory(s);
	}
};

Editor.Editor.prototype.SaveMap = function (o, e) {

	try {
		var fs = new System.IO.FileStream(this.engine.mapfilename, System.IO.FileMode.Create);
		Import.MannuxMap.Save(this.engine.map, fs);
		fs.Close();
	} catch ($exception) {
		var arg_30_0 = $exception;
		System.Console.WriteLine("Error saving map!");
		System.Console.WriteLine(arg_30_0.toString());
	}
};

Editor.Editor.prototype.SaveMapAs = function (o, e) {
	var dlg = new System.Windows.Forms.SaveFileDialog();
	dlg.DefaultExt = "map";
	dlg.Filter = "Mannux Maps (*.map)|*.map|All files (*.*)|*.*";
	dlg.Title = "Save Map As...";
	var s = System.IO.Directory.GetCurrentDirectory();
	var result = dlg.ShowDialog();

	if (result === System.Windows.Forms.DialogResult.OK) {
		var fs = new System.IO.FileStream(dlg.FileName, System.IO.FileMode.Create);
		Import.MannuxMap.Save(this.engine.map, fs);
		fs.Close();
		System.IO.Directory.SetCurrentDirectory(s);
		this.engine.mapfilename = dlg.FileName;
	}
};

Editor.Editor.prototype.ShowTileSet = function (o, e) {
	this.tilesetpreview.Show();
};

Editor.Editor.prototype.ShowMapProperties = function (o, e) {
	this.mapinfoview.Show();
};

Editor.Editor.prototype.ShowMapEntProperties = function (o, e) {
	this.mapentpropertiesview.Show();
};

Editor.Editor.prototype.ShowAutoSelectionThing = function (o, e) {
	this.autoselectionthing.Show();
};

Editor.Editor.prototype.SetTileSetMode = function (o, e) {
	this.state = this.tilesetmode;
};

Editor.Editor.prototype.SetCopyPasteMode = function (o, e) {
	this.state = this.copypastemode;
};

Editor.Editor.prototype.SetObstructionEditMode = function (o, e) {
	this.state = this.obstructionmode;
};

Editor.Editor.prototype.SetMapEntEditMode = function (o, e) {
	this.state = this.entityeditmode;
};

Editor.Editor.prototype.OnTileChange = function (newtile) {
	this.tilesetmode.curtile = newtile;
};

Editor.Editor.prototype.engine = null;
Editor.Editor.prototype.graph = null;
Editor.Editor.prototype.menu = null;
Editor.Editor.prototype.form = null;
Editor.Editor.prototype.tabs = null;
Editor.Editor.prototype.statbar = null;
Editor.Editor.prototype.tilesetpreview = null;
Editor.Editor.prototype.mapinfoview = null;
Editor.Editor.prototype.mapentpropertiesview = null;
Editor.Editor.prototype.autoselectionthing = null;
Editor.Editor.prototype.tileset = null;
Editor.Editor.prototype.state = null;
Editor.Editor.prototype.tilesetmode = null;
Editor.Editor.prototype.copypastemode = null;
Editor.Editor.prototype.obstructionmode = null;
Editor.Editor.prototype.entityeditmode = null;
Editor.Editor.prototype.OnExit = null;
Editor.Editor.prototype.Running = false;

Sprites.BitmapSprite.prototype._ctor = function (g, assetName, cellWidth, cellHeight, rowLength, hotspot) {
	System.Object.prototype._ctor.call(this);
	this.graph = g;
	this.tex = g.LoadImage(assetName);
	this.width = cellWidth;
	this.height = cellHeight;
	this.rowLength = rowLength;
	this.hotspot = hotspot;
};

Sprites.BitmapSprite.prototype.get_Width = function () {
	return this.width;
};

Sprites.BitmapSprite.prototype.get_Height = function () {
	return this.height;
};

Sprites.BitmapSprite.prototype.get_HotSpot = function () {
	return this.hotspot;
};

Sprites.BitmapSprite.prototype.Draw = function (x, y, frame) {
	var slice = new Microsoft.Xna.Framework.Rectangle();
	slice._ctor((1 + ((frame % this.rowLength) * (this.width + 1))), (1 + (Math.floor(frame / this.rowLength) * (this.height + 1))), this.width, this.height);
	this.graph.Blit(this.tex, new Microsoft.Xna.Framework.Vector2(x, y), slice.MemberwiseClone());
};

Object.defineProperty(Sprites.BitmapSprite.prototype, "Width", {
		get: Sprites.BitmapSprite.prototype.get_Width
	});
Object.defineProperty(Sprites.BitmapSprite.prototype, "Height", {
		get: Sprites.BitmapSprite.prototype.get_Height
	});
Object.defineProperty(Sprites.BitmapSprite.prototype, "HotSpot", {
		get: Sprites.BitmapSprite.prototype.get_HotSpot
	});
Sprites.BitmapSprite.prototype.__StructFields__ = {
	hotspot: Microsoft.Xna.Framework.Rectangle
};
Sprites.BitmapSprite.prototype.width = 0;
Sprites.BitmapSprite.prototype.height = 0;
Sprites.BitmapSprite.prototype.rowLength = 0;
Sprites.BitmapSprite.prototype.tex = null;
Sprites.BitmapSprite.prototype.graph = null;

Import.VectorIndexBuffer.prototype._ctor = function () {
	this.points = new System.Collections.Generic.List$b1/* <Vertex> */ ();
	System.Object.prototype._ctor.call(this);
	this.points = new System.Collections.Generic.List$b1/* <Vertex> */ ();
	this.lines = new System.Collections.ArrayList();
};

Import.VectorIndexBuffer.prototype.get_Points = function () {
	return this.points;
};

Import.VectorIndexBuffer.prototype.get_Lines = function () {
	return this.lines;
};

Import.VectorIndexBuffer.prototype.RemovePoint = function (idx) {
	var i = 0;

__while0__: 
	while (i < this.lines.Count) {
		var j = JSIL.Cast(this.lines.get_Item(i), System.Array.Of(System.Int32));

		if (!((j[0] !== idx) && (j[1] !== idx))) {
			this.lines.RemoveAt(i);
		} else {
			++i;
		}
	}
	var enumerator = this.lines.GetEnumerator();

	try {

	__while1__: 
		while (enumerator.IEnumerator_MoveNext()) {
			var p = JSIL.Cast(enumerator.IEnumerator_Current, System.Array.Of(System.Int32));

			if (p[0] > idx) {
				p[0] -= 1;
			}

			if (p[1] > idx) {
				p[1] -= 1;
			}
		}
	} finally {
		var disposable = JSIL.TryCast(enumerator, System.IDisposable);

		if (disposable !== null) {
			disposable.IDisposable_Dispose();
		}
	}
	this.points.RemoveAt(idx);
};

Object.defineProperty(Import.VectorIndexBuffer.prototype, "Points", {
		get: Import.VectorIndexBuffer.prototype.get_Points
	});
Object.defineProperty(Import.VectorIndexBuffer.prototype, "Lines", {
		get: Import.VectorIndexBuffer.prototype.get_Lines
	});
Import.VectorIndexBuffer.prototype.points = null;
Import.VectorIndexBuffer.prototype.lines = null;

Import.Geo.Vertex.prototype._ctor$0 = function (a, b) {
	this.x = a;
	this.y = b;
};

Import.Geo.Vertex.prototype.get_X = function () {
	return this.x;
};

Import.Geo.Vertex.prototype.set_X = function (value) {
	this.x = value;
};

Import.Geo.Vertex.prototype.get_Y = function () {
	return this.y;
};

Import.Geo.Vertex.prototype.set_Y = function (value) {
	this.y = value;
};

JSIL.OverloadedMethod(Import.Geo.Vertex.prototype, "_ctor", [
		["_ctor$0", [System.Int32, System.Int32]]
	]
);
Object.defineProperty(Import.Geo.Vertex.prototype, "X", {
		get: Import.Geo.Vertex.prototype.get_X, 
		set: Import.Geo.Vertex.prototype.set_X
	});
Object.defineProperty(Import.Geo.Vertex.prototype, "Y", {
		get: Import.Geo.Vertex.prototype.get_Y, 
		set: Import.Geo.Vertex.prototype.set_Y
	});
Import.Geo.Vertex.prototype.x = 0;
Import.Geo.Vertex.prototype.y = 0;

Controller.prototype.GetResourceFromObject = function (o) {
	var enumerator = this.resources.GetEnumerator();

	try {

	__while0__: 
		while (enumerator.IEnumerator_MoveNext()) {
			var r = JSIL.Cast(JSIL.Cast(enumerator.IEnumerator_Current, System.Collections.DictionaryEntry).Value, Controller.Resource);

			if (r.obj === o) {
				var result = r;
				return result;
			}
		}
	} finally {
		var disposable = JSIL.TryCast(enumerator, System.IDisposable);

		if (disposable !== null) {
			disposable.IDisposable_Dispose();
		}
	}
	result = null;
	return result;
};

Controller.prototype.Load = function (fname) {
	var r = JSIL.Cast(this.resources.get_Item(fname), Controller.Resource);

	if (r === null) {
		System.Console.WriteLine("Alloc {0}", fname);
		r = new Controller.Resource(this.Alloc(fname), fname);
		this.resources.set_Item(fname, r);
		var obj = r.obj;
	} else {
		r.refcount += 1;
		obj = r.obj;
	}
	return obj;
};

Controller.prototype.Free = function (o) {
	var r = this.GetResourceFromObject(o);

	if (!(r === null)) {
		System.Console.WriteLine("Decreffing {0}", r.fname);
		r.refcount -= 1;

		if (r.refcount === 0) {
			System.Console.WriteLine("Releasing {0}", r.fname);
			o.IDisposable_Dispose();
			this.resources.Remove(r.fname);
		}
	}
};

Controller.prototype.Dispose = function () {

	if (this.disposestate === 0) {
		this.disposestate = 1;
		this.DeallocAll();
		this.resources.Clear();
		this.disposestate = 2;
	}
};

Controller.prototype._ctor = function () {
	this.resources = new System.Collections.Hashtable();
	this.disposestate = 0;
	System.Object.prototype._ctor.call(this);
};

Controller.prototype.__ImplementInterface__(System.IDisposable);
Controller.prototype.resources = null;
Controller.prototype.disposestate = 0;

Controller.Resource.prototype._ctor = function (o, s) {
	System.Object.prototype._ctor.call(this);
	this.obj = o;
	this.fname = s;
	this.refcount = 1;
};

Controller.Resource.prototype.obj = null;
Controller.Resource.prototype.fname = null;
Controller.Resource.prototype.refcount = 0;

Editor.MapEntPropertiesView.prototype._ctor = function (e) {
	System.Windows.Forms.Control.prototype._ctor.call(this);
	this.editor = e;
	this.engine = e.engine;
	this.Text = "MapEnt properties";
	this.enttypes = new System.Windows.Forms.ListBox();
	this.enttypes.Location = new System.Drawing.Point(10, 10);
	this.enttypes.Items.Clear();
	var array = Import.MapEnt.enttypes;
	var j = 0;

__while0__: 
	while (j < array.length) {
		this.enttypes.Items.Add(array[j]);
		++j;
	}
	this.enttypes.Show();
	this.Controls.Add(this.enttypes);
	this.entprops = JSIL.Array.New(System.Windows.Forms.TextBox, 5);
	var y = (this.enttypes.Bottom + 5);
	var i = 0;

__while1__: 
	while (i < 5) {
		this.entprops[i] = new System.Windows.Forms.TextBox();
		this.entprops[i].Location = new System.Drawing.Point(10, y);
		this.entprops[i].Show();
		this.Controls.Add(this.entprops[i]);
		y = (this.entprops[i].Bottom + 5);
		++i;
	}
};

Editor.MapEntPropertiesView.prototype.UpdateEnt = function (e) {

	if (!(e === null)) {
		e.type = JSIL.Cast(this.enttypes.SelectedItem, System.String);
		e.data = JSIL.Array.New(System.String, 5);
		var i = 0;

	__while0__: 
		while (i < 5) {
			e.data[i] = this.entprops[i].Text;
			++i;
		}
	}
};

Editor.MapEntPropertiesView.prototype.UpdateDlg = function (e) {

	if (!(e === null)) {
		var idx = System.Array.IndexOf(Import.MapEnt.enttypes, e.type);

		if (idx === -1) {
			e.type = JSIL.Cast(this.enttypes.Items.get_Item(0), System.String);
			this.enttypes.SelectedIndex = 0;
		} else {
			this.enttypes.SelectedIndex = idx;
		}
		var i = 0;

	__while0__: 
		while (true) {

			if (!((e.data !== null) || (i >= e.data.length))) {
				this.entprops[i].Text = e.data[i];
			} else {
				this.entprops[i].Text = "";
			}

			if (++i >= 5) {
				break __while0__;
			}
		}
	}
};

Editor.MapEntPropertiesView.prototype.editor = null;
Editor.MapEntPropertiesView.prototype.engine = null;
Editor.MapEntPropertiesView.prototype.enttypes = null;
Editor.MapEntPropertiesView.prototype.entprops = null;
Editor.MapEntPropertiesView._cctor = function () {
	Object.defineProperty(Editor.MapEntPropertiesView, "numprops", {
			"value": 5}
	);
};


Vector.Increase = function (f, quantity) {
	return (f + quantity);
};

Vector.Decrease = function (f, quantity) {

	if (f > 0) {

		if ((f - quantity) <= 0) {
			var result = 0;
		} else {
			result = (f - quantity);
		}
	} else if ((f + quantity) >= 0) {
		result = 0;
	} else {
		result = (f + quantity);
	}
	return result;
};

Vector.Clamp = function (f, max) {

	if (f > 0) {

		if (f > max) {
			var result = max;
			return result;
		}
	} else if (f < -max) {
		result = -max;
		return result;
	}
	result = f;
	return result;
};

Vector.prototype._ctor = function () {
	System.Object.prototype._ctor.call(this);
};


Input.InputHandler.prototype.get_Keyboard = function () {
	return this.kd;
};

Input.InputHandler.prototype.get_Mouse = function () {
	return this.md;
};

Input.InputHandler.prototype.Poll = function () {
	this.kd.Poll();
	this.md.Poll();
};

Input.InputHandler.prototype._ctor = function () {
	this.kd = new Input.KeyboardDevice();
	this.md = new Input.MouseDevice();
	System.Object.prototype._ctor.call(this);
};

Object.defineProperty(Input.InputHandler.prototype, "Keyboard", {
		get: Input.InputHandler.prototype.get_Keyboard
	});
Object.defineProperty(Input.InputHandler.prototype, "Mouse", {
		get: Input.InputHandler.prototype.get_Mouse
	});
Input.InputHandler.prototype.kd = null;
Input.InputHandler.prototype.md = null;

Entities.Door.prototype._ctor = function (e, startx, starty, face, mapname, newx, newy) {
	this.open = false;
	Entities.Entity.prototype._ctor.call(this, e, e.DoorSprite);
	this.engine = e;
	this.width = this.sprite.HotSpot.Width;
	this.height = this.sprite.HotSpot.Height;
	this.x = startx;
	this.y = starty;
	this.direction = face;
	this.open = false;
	this.width = 16;
	this.map = mapname;
	this.Newx = newx;
	this.Newy = newy;
	this.vx = 0;
	this.vy = 0;
};

Entities.Door.prototype.Update = function () {
	Entities.Entity.prototype.HandleGravity.call(this);
	this.CollisionThings();
	this.anim.Update();
};

Entities.Door.prototype.CollisionThings = function () {
	var px = this.engine.cameraTarget.X;
	var py = this.engine.cameraTarget.Y;
	var pw = this.engine.cameraTarget.Width;
	var ph = this.engine.cameraTarget.Height;

	if (JSIL.CheckType(this.engine.DetectInYCoords(this), Entities.Player)) {

		if (!this.open) {

			if (this.direction === Entities.Dir.left) {

				if ((this.x - px) < 64) {
					this.open = true;
					this.anim.Set(0, 6, 8, false);
				}
			} else if ((px - this.x) < 64) {
				this.open = true;
				this.anim.Set(6, 0, 8, false);
			}
		}

		if (this.open) {

			if (this.direction === Entities.Dir.left) {

				if ((this.x - px) < 8) {
					this.engine.MapSwitch(this.map, this.Newx, this.Newy);
				}

				if ((this.x - px) > 64) {
					this.open = false;
					this.anim.Set(6, 0, 8, false);
				}
			} else {

				if ((px - this.x) < 24) {
					this.engine.MapSwitch(this.map, this.Newx, this.Newy);
				}

				if ((px - this.x) > 64) {
					this.open = false;
					this.anim.Set(0, 6, 8, false);
				}
			}
		}
	}
};

Entities.Door.prototype.open = false;
Entities.Door.prototype.map = null;
Entities.Door.prototype.Newx = 0;
Entities.Door.prototype.Newy = 0;

Editor.AutoSelectionThing.prototype.AddBox = function (name) {
	var i = new System.Windows.Forms.Label();
	i.Text = name;
	i.Location = this.pos.MemberwiseClone();
	this.Controls.Add(i);
	var t = new Editor.NumberEditBox();
	t.Location = new System.Drawing.Point(i.Right, i.Top);
	this.Controls.Add(t);
	this.pos.Y = (t.Bottom + 5);
	return t;
};

Editor.AutoSelectionThing.prototype._ctor = function (e) {
	this.pos = new System.Drawing.Point(0, 0);
	System.Windows.Forms.Control.prototype._ctor.call(this);
	this.editor = e;
	this.firsttile = this.AddBox("First Tile");
	this.width = this.AddBox("Width");
	this.height = this.AddBox("Height");
	this.span = this.AddBox("Span");
	this.span.Text = "19";
	var b = new System.Windows.Forms.Button();
	b.Text = "Use Current";
	b.Location = new System.Drawing.Point((this.firsttile.Right + 5), this.firsttile.Top);
	b.add_Click(JSIL.Delegate.New("System.EventHandler", this, Editor.AutoSelectionThing.prototype.UseCurrentTile));
	this.Controls.Add(b);
	b = new System.Windows.Forms.Button();
	b.Text = "Do it";
	b.Location = new System.Drawing.Point(0, (this.span.Bottom + 5));
	b.add_Click(JSIL.Delegate.New("System.EventHandler", this, Editor.AutoSelectionThing.prototype.DoIt));
	this.Controls.Add(b);
	var c = new System.Windows.Forms.Button();
	c.Text = "Close";
	c.Location = new System.Drawing.Point((b.Right + 5), b.Top);
	this.Controls.Add(c);
};

Editor.AutoSelectionThing.prototype.UseCurrentTile = function (o, e) {
	this.firsttile.Text = this.editor.tilesetmode.curtile.toString();
};

Editor.AutoSelectionThing.prototype.DoIt = function (o, e) {
	var t = System.Convert.ToInt32(this.firsttile.Text);
	var w = System.Convert.ToInt32(this.width.Text);
	var h = System.Convert.ToInt32(this.height.Text);
	var s = System.Convert.ToInt32(this.span.Text);
	var b = JSIL.MultidimensionalArray.New(System.Int32, w, h);
	var y = 0;

__while0__: 
	while (y < h) {
		var x = 0;

	__while1__: 
		while (x < w) {
			b.Set(x, y, t);
			++t;
			++x;
		}
		t += (s - w);
		++y;
	}
	this.editor.copypastemode.Selection = b;
};

Editor.AutoSelectionThing.prototype.__StructFields__ = {
	pos: System.Drawing.Point
};
Editor.AutoSelectionThing.prototype.editor = null;
Editor.AutoSelectionThing.prototype.firsttile = null;
Editor.AutoSelectionThing.prototype.width = null;
Editor.AutoSelectionThing.prototype.height = null;
Editor.AutoSelectionThing.prototype.span = null;

Sprites.ParticleSprite.prototype._ctor = function (g, d) {
	this.particles = JSIL.Array.New(Sprites.ParticleSprite.Particle, 15);
	System.Object.prototype._ctor.call(this);
	this.graph = g;
	this.direction = d;
	this.hotspot = new System.Drawing.Rectangle(0, 0, 16, 16);
	this.r = this.g = 255;
	this.a = 128;
	this.b = 0;
};

Sprites.ParticleSprite.prototype.UpdateParticles = function () {
	var i = 0;

__while0__: 
	while (i < 15) {
		var expr_15_cp_0 = this.particles;
		var expr_15_cp_1 = i;
		expr_15_cp_0[expr_15_cp_1].x = (expr_15_cp_0[expr_15_cp_1].x - 1);
		var expr_32_cp_0 = this.particles;
		var expr_32_cp_1 = i;
		expr_32_cp_0[expr_32_cp_1].y = (expr_32_cp_0[expr_32_cp_1].y + (Engine.rand.Next(0, 3) - 1));
		var expr_59_cp_0 = this.particles;
		var expr_59_cp_1 = i;
		expr_59_cp_0[expr_59_cp_1].size = (expr_59_cp_0[expr_59_cp_1].size - 0.10000000149011612);

		if (this.particles[i].size <= 0) {
			this.particles[i].x = 0;
			this.particles[i].y = 0;
			this.particles[i].size = (10 + Engine.rand.Next(5));
		}
		++i;
	}
};

Sprites.ParticleSprite.prototype.Draw = function (x, y, frame) {
	this.UpdateParticles();
	var array = this.particles;
	var i = 0;

__while0__: 
	while (i < array.length) {
		var p = array[i];
		this.graph.DrawParticle(
			(x + p.x), 
			(y + p.y), 
			p.size, 
			this.r, 
			this.g, 
			this.b, 
			this.a
		);
		++i;
	}
};

Sprites.ParticleSprite.prototype.get_Width = function () {
	return 16;
};

Sprites.ParticleSprite.prototype.get_Height = function () {
	return 16;
};

Sprites.ParticleSprite.prototype.get_NumFrames = function () {
	return 1;
};

Sprites.ParticleSprite.prototype.get_HotSpot = function () {
	return this.hotspot;
};

Sprites.ParticleSprite.prototype.Dispose = function () {
};

Object.defineProperty(Sprites.ParticleSprite.prototype, "Width", {
		get: Sprites.ParticleSprite.prototype.get_Width
	});
Object.defineProperty(Sprites.ParticleSprite.prototype, "Height", {
		get: Sprites.ParticleSprite.prototype.get_Height
	});
Object.defineProperty(Sprites.ParticleSprite.prototype, "NumFrames", {
		get: Sprites.ParticleSprite.prototype.get_NumFrames
	});
Object.defineProperty(Sprites.ParticleSprite.prototype, "HotSpot", {
		get: Sprites.ParticleSprite.prototype.get_HotSpot
	});
Sprites.ParticleSprite.prototype.__ImplementInterface__(Sprites.ISprite);
Sprites.ParticleSprite.prototype.__StructFields__ = {
	hotspot: System.Drawing.Rectangle
};
Sprites.ParticleSprite.prototype.particles = null;
Sprites.ParticleSprite.prototype.graph = null;
Sprites.ParticleSprite.prototype.direction = 0;
Sprites.ParticleSprite.prototype.r = 0;
Sprites.ParticleSprite.prototype.g = 0;
Sprites.ParticleSprite.prototype.b = 0;
Sprites.ParticleSprite.prototype.a = 0;
Sprites.ParticleSprite._cctor = function () {
	Object.defineProperty(Sprites.ParticleSprite, "maxparticles", {
			"value": 15}
	);
};


Sprites.ParticleSprite.Particle.prototype.x = 0;
Sprites.ParticleSprite.Particle.prototype.y = 0;
Sprites.ParticleSprite.Particle.prototype.size = 0;

Entities.Boom.prototype._ctor = function (e, startx, starty) {
	Entities.Entity.prototype._ctor.call(this, e, e.BoomSprite);
	this.UpdateState = JSIL.Delegate.New("System.Action", this, Entities.Boom.prototype.CheckCollision);
	this.x = startx;
	this.y = starty;
	this.anim.Set(0, 6, 4, false);
};

Entities.Boom.prototype.CheckCollision = function () {

	if (this.anim.frame === 6) {
		this.engine.DestroyEntity(this);
	}
};


Engine.prototype._ctor = function () {
	this.entities = new System.Collections.ArrayList();
	this.killlist = new System.Collections.ArrayList();
	this.addlist = new System.Collections.ArrayList();
	this.xwin = 0;
	this.ywin = 0;
	Microsoft.Xna.Framework.Game.prototype._ctor.call(this);
	this.graphics = new Microsoft.Xna.Framework.GraphicsDeviceManager(this);
	this.Content.RootDirectory = "Content";
};

Engine.prototype.Initialize = function () {
	Microsoft.Xna.Framework.Game.prototype.Initialize.call(this);
	this.IsMouseVisible = this;
};

Engine.prototype.LoadContent = function () {
	this.graph = new Cataract.XNAGraph(this.graphics.GraphicsDevice, this.Content);
	this.input = new Input.InputHandler();
	this.tileset = new Sprites.BitmapSprite(this.graph, "mantiles", 16, 16, 19, new Microsoft.Xna.Framework.Rectangle(0, 0, 16, 16));
	this.TabbySprite = new Sprites.BitmapSprite(this.graph, "tabpis", 64, 64, 8, new Microsoft.Xna.Framework.Rectangle(24, 24, 16, 40));
	this.DoorSprite = new Sprites.BitmapSprite(this.graph, "door", 16, 64, 7, new Microsoft.Xna.Framework.Rectangle(0, 0, 16, 64));
	this.RipperSprite = new Sprites.BitmapSprite(this.graph, "ripper", 16, 32, 4, new Microsoft.Xna.Framework.Rectangle(0, 0, 16, 20));
	this.BoomSprite = new Sprites.BitmapSprite(this.graph, "boom", 16, 16, 7, new Microsoft.Xna.Framework.Rectangle(0, 0, 16, 16));
	this.BulletSprite = new Sprites.BitmapSprite(this.graph, "bullet", 8, 8, 8, new Microsoft.Xna.Framework.Rectangle(0, 0, 8, 8));
	this.player = new Entities.Player(this);
	this.cameraTarget = this.player;
	this.player.X = this.player.Y = 32;
	this.map = Import.v2Map.Load("map00.map");
	this.MapSwitch("data/maps/test.map");
	this.obs = new Import.VectorObstructionMap(this.map.Obs);
	this.time = new Timer(100);
	this.editor = new Editor.Editor(this);
	this.editor.add_OnExit(JSIL.Delegate.New("System.Action", this, Engine.prototype.StopEditor));
};

Engine.prototype.Update = function (gameTime) {
	Microsoft.Xna.Framework.Game.prototype.Update.call(this, gameTime);
	this.input.Poll();

	if (this.editor.Running) {
		this.editor.Update();
	} else {

		if (this.input.Keyboard.IInputDevice_Button(2)) {
			this.editor.Execute();
		}
		this.ProcessEntities();
	}
};

Engine.prototype.StartEditor = function () {
	this.editor.Execute();
};

Engine.prototype.StopEditor = function () {
};

Engine.prototype.Draw = function (gameTime) {
	Microsoft.Xna.Framework.Game.prototype.Draw.call(this, gameTime);
	this.graph.Begin();
	this.Render();
	this.graph.End();
};

Engine.prototype.ProcessEntities = function () {
	var enumerator = this.entities.GetEnumerator();

	try {

	__while0__: 
		while (enumerator.IEnumerator_MoveNext()) {
			var e = JSIL.Cast(enumerator.IEnumerator_Current, Entities.Entity);
			e.Tick();
		}
	} finally {
		var disposable = JSIL.TryCast(enumerator, System.IDisposable);

		if (disposable !== null) {
			disposable.IDisposable_Dispose();
		}
	}
	enumerator = this.killlist.GetEnumerator();

	try {

	__while1__: 
		while (enumerator.IEnumerator_MoveNext()) {
			e = JSIL.Cast(enumerator.IEnumerator_Current, Entities.Entity);
			this.entities.Remove(e);
		}
	} finally {
		disposable = JSIL.TryCast(enumerator, System.IDisposable);

		if (disposable !== null) {
			disposable.IDisposable_Dispose();
		}
	}
	this.killlist.Clear();
	enumerator = this.addlist.GetEnumerator();

	try {

	__while2__: 
		while (enumerator.IEnumerator_MoveNext()) {
			e = JSIL.Cast(enumerator.IEnumerator_Current, Entities.Entity);
			this.entities.Add(e);
		}
	} finally {
		disposable = JSIL.TryCast(enumerator, System.IDisposable);

		if (disposable !== null) {
			disposable.IDisposable_Dispose();
		}
	}
	this.addlist.Clear();
};

Engine.prototype.IsObs = function (x1, y1, x2, y2) {
	return this.obs.Test(x1, y1, x2, y2);
};

Engine.prototype.get_XWin = function () {
	return this.xwin;
};

Engine.prototype.set_XWin = function (value) {
	this.xwin = value;

	if (this.xwin > ((this.map.Width * this.tileset.Width) - this.graph.XRes)) {
		this.xwin = ((this.map.Width * this.tileset.Width) - this.graph.XRes);
	}

	if (this.xwin < 0) {
		this.xwin = 0;
	}
};

Engine.prototype.get_YWin = function () {
	return this.ywin;
};

Engine.prototype.set_YWin = function (value) {
	this.ywin = value;

	if (this.ywin > ((this.map.Height * this.tileset.Height) - this.graph.YRes)) {
		this.ywin = ((this.map.Height * this.tileset.Height) - this.graph.YRes);
	}

	if (this.ywin < 0) {
		this.ywin = 0;
	}
};

Engine.prototype.RenderLayer = function (lay, transparent) {
	var xw = (lay.ParallaxX * this.xwin);
	var yw = (lay.ParallaxY * this.ywin);
	var xs = Math.floor(xw / this.tileset.Width);
	var ys = Math.floor(yw / this.tileset.Height);
	var xofs = -(xw % this.tileset.Width);
	var yofs = -(yw % this.tileset.Height);
	var xl = (Math.floor(this.graph.XRes / this.tileset.Width) + 1);
	var yl = (Math.floor(this.graph.YRes / this.tileset.Height) + 2);

	if ((xs + xl) > lay.Width) {
		xl = (lay.Width - xs);
	}

	if ((ys + yl) > lay.Height) {
		yl = (lay.Height - ys);
	}
	var curx = xofs;
	var cury = yofs;
	var y = 0;

__while0__: 
	while (y < yl) {
		var x = 0;

	__while1__: 
		while (x < xl) {
			var t = lay.get_Item((x + xs), (y + ys));

			if (!((t !== 0) && transparent)) {
				this.tileset.Draw(curx, cury, t);
			}
			curx += this.tileset.Width;
			++x;
		}
		cury += this.tileset.Height;
		curx = xofs;
		++y;
	}
};

Engine.prototype.RenderEntity = function (e) {
	var x = (e.X - e.sprite.HotSpot.X - this.xwin);
	var y = (e.Y - e.sprite.HotSpot.Y - this.ywin);

	if (e.Visible) {
		e.sprite.Draw(x, y, e.anim.frame);
	}
};

Engine.prototype.RenderEntities = function () {
	var enumerator = this.entities.GetEnumerator();

	try {

	__while0__: 
		while (enumerator.IEnumerator_MoveNext()) {
			this.RenderEntity(JSIL.Cast(enumerator.IEnumerator_Current, Entities.Entity));
		}
	} finally {
		var disposable = JSIL.TryCast(enumerator, System.IDisposable);

		if (disposable !== null) {
			disposable.IDisposable_Dispose();
		}
	}
};

Engine.prototype.Render = function () {
	this.graph.Clear();

	if (this.cameraTarget !== null) {
		this.XWin = (this.cameraTarget.X - Math.floor(this.graph.XRes / 2));
		this.YWin = (this.cameraTarget.Y - Math.floor(this.graph.YRes / 2));
	}
	var i = 0;
	var enumerator = this.map.Layers.GetEnumerator();

	try {

	__while0__: 
		while (enumerator.IEnumerator_MoveNext()) {
			var j = JSIL.Cast(enumerator.IEnumerator_Current, Import.Map.Layer);

			if (j.visible) {
				this.RenderLayer(j, (i !== 0));
				++i;
			}
		}
	} finally {
		var disposable = JSIL.TryCast(enumerator, System.IDisposable);

		if (disposable !== null) {
			disposable.IDisposable_Dispose();
		}
	}
	this.RenderEntities();
};

Engine.prototype.MapSwitch$0 = function (mapname) {

	try {
		var fs = new System.IO.FileStream(mapname, System.IO.FileMode.Open);
		var newmap = Import.MannuxMap.Load(fs);
		fs.Close();
	} catch ($exception) {
		var arg_1B_0 = $exception;
		System.Console.Write(arg_1B_0.toString());
		return ;
	}
	this.map = newmap;
	this.mapfilename = mapname;
	this.entities.Clear();
	this.entities.Add(this.player);
	var enumerator = this.map.Entities.GetEnumerator();

	try {

	__while0__: 
		while (enumerator.IEnumerator_MoveNext()) {
			this.SpawnEntity(JSIL.Cast(enumerator.IEnumerator_Current, Import.MapEnt));
		}
	} finally {
		var disposable = JSIL.TryCast(enumerator, System.IDisposable);

		if (disposable !== null) {
			disposable.IDisposable_Dispose();
		}
	}
};

Engine.prototype.MapSwitch$1 = function (mapname, x, y) {
	this.MapSwitch(mapname);
	this.player.X = x;
	this.player.Y = y;
};

Engine.prototype.SpawnEntity$0 = function (e) {

	var __label0__ = "__entry0__";
__step0__: 
	while (true) {

		switch (__label0__) {

			case "__entry0__":
				var type = e.type;

				if (type === null) {

					if (!System.String.op_Equality(type, "player")) {

						if (!System.String.op_Equality(type, "door")) {

							if (!System.String.op_Equality(type, "ripper")) {

								if (!System.String.op_Equality(type, "hopper")) {
									__label0__ = "IL_E7";
									continue __step0__;
								}
								this.SpawnEntity(new Entities.Enemies.Hopper(this, e.x, e.y));
							} else {
								this.SpawnEntity(new Entities.Enemies.Ripper(this, e.x, e.y));
							}
						} else {
							this.SpawnEntity(new Entities.Door(this, e.x, e.y, Entities.Dir.left, e.data[0], System.Convert.ToInt32(e.data[1]), System.Convert.ToInt32(e.data[2])));
						}
					} else {
						this.player.X = e.x;
						this.player.Y = e.y;
					}
					return ;
				}
				__label0__ = "IL_E7";
				continue __step0__;
				break;

			case "IL_E7":
				throw new System.Exception(System.String.Format("Engine::MapSwitch Unknown entity type {0}", e.type));
				break __step0__;
		}
	}
};

Engine.prototype.DetectCollision = function (ent) {
	var enumerator = this.entities.GetEnumerator();

	try {

	__while0__: 
		while (enumerator.IEnumerator_MoveNext()) {
			var ent2 = JSIL.Cast(enumerator.IEnumerator_Current, Entities.Entity);

			if (!(ent === ent2)) {

				if (!(((ent.X + ent.Width) <= ent2.X) || (ent.X >= (ent2.X + ent2.Width)))) {

					if (!(((ent.Y + ent.Height) <= ent2.Y) || (ent.Y >= (ent2.Y + ent2.Height)))) {
						var result = ent2;
						return result;
					}
				}
			}
		}
	} finally {
		var disposable = JSIL.TryCast(enumerator, System.IDisposable);

		if (disposable !== null) {
			disposable.IDisposable_Dispose();
		}
	}
	result = null;
	return result;
};

Engine.prototype.DetectInYCoords = function (ent) {
	var enumerator = this.entities.GetEnumerator();

	try {

	__while0__: 
		while (enumerator.IEnumerator_MoveNext()) {
			var ent2 = JSIL.Cast(enumerator.IEnumerator_Current, Entities.Entity);

			if (!(ent === ent2)) {

				if (!(((ent.Y + ent.Height) <= ent2.Y) || (ent.Y >= (ent2.Y + ent2.Height)))) {
					var result = ent2;
					return result;
				}
			}
		}
	} finally {
		var disposable = JSIL.TryCast(enumerator, System.IDisposable);

		if (disposable !== null) {
			disposable.IDisposable_Dispose();
		}
	}
	result = null;
	return result;
};

Engine.prototype.DetectInXCoords = function (ent) {
	var enumerator = this.entities.GetEnumerator();

	try {

	__while0__: 
		while (enumerator.IEnumerator_MoveNext()) {
			var ent2 = JSIL.Cast(enumerator.IEnumerator_Current, Entities.Entity);

			if (!(ent === ent2)) {

				if (!(((ent.X + ent.Width) <= ent2.X) || (ent.X >= (ent2.X + ent2.Width)))) {
					var result = ent2;
					return result;
				}
			}
		}
	} finally {
		var disposable = JSIL.TryCast(enumerator, System.IDisposable);

		if (disposable !== null) {
			disposable.IDisposable_Dispose();
		}
	}
	result = null;
	return result;
};

Engine.prototype.SpawnEntity$1 = function (e) {
	this.addlist.Add(e);
};

Engine.prototype.DestroyEntity = function (e) {
	this.killlist.Add(e);
};

JSIL.OverloadedMethod(Engine.prototype, "MapSwitch", [
		["MapSwitch$0", [System.String]], 
		["MapSwitch$1", [System.String, System.Int32, System.Int32]]
	]
);
JSIL.OverloadedMethod(Engine.prototype, "SpawnEntity", [
		["SpawnEntity$0", [Import.MapEnt]], 
		["SpawnEntity$1", [Entities.Entity]]
	]
);
Object.defineProperty(Engine.prototype, "XWin", {
		get: Engine.prototype.get_XWin, 
		set: Engine.prototype.set_XWin
	});
Object.defineProperty(Engine.prototype, "YWin", {
		get: Engine.prototype.get_YWin, 
		set: Engine.prototype.set_YWin
	});
Engine.prototype.graph = null;
Engine.prototype.graphics = null;
Engine.prototype.input = null;
Engine.prototype.map = null;
Engine.prototype.tileset = null;
Engine.prototype.cameraTarget = null;
Engine.prototype.player = null;
Engine.prototype.time = null;
Engine.prototype.editor = null;
Engine.prototype.obs = null;
Engine.prototype.entities = null;
Engine.prototype.killlist = null;
Engine.prototype.addlist = null;
Engine.prototype.xwin = 0;
Engine.prototype.ywin = 0;
Engine.prototype.mapfilename = null;
Engine.prototype.TabbySprite = null;
Engine.prototype.RipperSprite = null;
Engine.prototype.DoorSprite = null;
Engine.prototype.BoomSprite = null;
Engine.prototype.BulletSprite = null;
Engine._cctor = function () {
	Engine.rand = null;
	Engine.rand = new System.Random();
};


Editor.ObstructionMode.prototype._ctor = function (e) {
	this.leftdown = false;
	this.rightdown = false;
	this.snap = true;
	this.MOVE_KEYS = JSIL.Array.New(System.Windows.Forms.Keys, [System.Windows.Forms.Keys.None | System.Windows.Forms.Keys.RButton | System.Windows.Forms.Keys.Space | System.Windows.Forms.Keys.Next | System.Windows.Forms.Keys.PageDown | System.Windows.Forms.Keys.B | System.Windows.Forms.Keys.NumPad0 | System.Windows.Forms.Keys.NumPad2, System.Windows.Forms.Keys.None | System.Windows.Forms.Keys.MButton | System.Windows.Forms.Keys.Space | System.Windows.Forms.Keys.Home | System.Windows.Forms.Keys.D | System.Windows.Forms.Keys.NumPad0 | System.Windows.Forms.Keys.NumPad4, System.Windows.Forms.Keys.None | System.Windows.Forms.Keys.RButton | System.Windows.Forms.Keys.MButton | System.Windows.Forms.Keys.XButton2 | System.Windows.Forms.Keys.Space | System.Windows.Forms.Keys.Next | System.Windows.Forms.Keys.PageDown | System.Windows.Forms.Keys.Home | System.Windows.Forms.Keys.Up | System.Windows.Forms.Keys.B | System.Windows.Forms.Keys.D | System.Windows.Forms.Keys.F | System.Windows.Forms.Keys.NumPad0 | System.Windows.Forms.Keys.NumPad2 | System.Windows.Forms.Keys.NumPad4 | System.Windows.Forms.Keys.NumPad6, System.Windows.Forms.Keys.None | System.Windows.Forms.Keys.Back | System.Windows.Forms.Keys.Space | System.Windows.Forms.Keys.Down | System.Windows.Forms.Keys.H | System.Windows.Forms.Keys.NumPad0 | System.Windows.Forms.Keys.NumPad8]);
	System.Object.prototype._ctor.call(this);
	this.editor = e;
	this.engine = e.engine;
};

Editor.ObstructionMode.prototype.dist = function (/* ref */ p, x, y) {
	var dx = (JSIL.Cast(p.X, System.Single) - x);
	var dy = (JSIL.Cast(p.Y, System.Single) - y);
	return JSIL.Cast(System.Math.Sqrt(((dx * dx) + (dy * dy))), System.Single);
};

Editor.ObstructionMode.prototype.LeftClick = function (x, y) {

	if (!this.leftdown) {
		this.leftdown = true;
		var idx = 0;
		var best = 99999;
		var i = 0;

	__while0__: 
		while (i < this.engine.map.Obs.Points.Count) {
			var d = this.dist(JSIL.UnmaterializedReference(), x, y);

			if (!((d >= best) || (i === this.curpoint))) {
				idx = i;
				best = d;
			}
			++i;
		}

		if (!(idx === this.curpoint)) {

			if ((System.Windows.Forms.Control.ModifierKeys & System.Windows.Forms.Keys.None | System.Windows.Forms.Keys.Shift) !== System.Windows.Forms.Keys.None) {
				var enumerator = this.engine.map.Obs.Lines.GetEnumerator();

				try {

				__while1__: 
					while (enumerator.IEnumerator_MoveNext()) {
						var j = JSIL.Cast(enumerator.IEnumerator_Current, System.Array.Of(System.Int32));

						if (!(!((j[0] === idx) && 
									(j[1] === this.curpoint)) && ((j[1] !== idx) || 
									(j[0] !== this.curpoint)))) {
							return ;
						}
					}
				} finally {
					var disposable = JSIL.TryCast(enumerator, System.IDisposable);

					if (disposable !== null) {
						disposable.IDisposable_Dispose();
					}
				}
				this.engine.map.Obs.Lines.Add(JSIL.Array.New(System.Int32, [idx, this.curpoint]));
			}
			this.curpoint = idx;
		}
	}
};

Editor.ObstructionMode.prototype.RightClick = function (x, y, e) {

	if (!this.rightdown) {
		this.rightdown = true;

		if (!this.snap) {
			this.engine.map.Obs.Points.Add(new Import.Geo.Vertex(x, y));
		} else {

			if ((x % 16) <= 8) {

			__while1__: 
				while ((x % 16) !== 0) {
					--x;
				}
			} else {

			__while0__: 
				while ((x % 16) !== 0) {
					++x;
				}
			}

			if ((y % 16) <= 8) {

			__while3__: 
				while ((y % 16) !== 0) {
					--y;
				}
			} else {

			__while2__: 
				while ((y % 16) !== 0) {
					++y;
				}
			}
			this.engine.map.Obs.Points.Add(new Import.Geo.Vertex(x, y));
		}
	}
};

Editor.ObstructionMode.prototype.MouseDown = function (e, b) {
};

Editor.ObstructionMode.prototype.MouseUp = function (e, b) {
	this.leftdown = false;
};

Editor.ObstructionMode.prototype.MouseClick = function (e, b) {
	var x = (e.X + this.engine.XWin);
	this.LeftClick(x, (e.Y + this.engine.YWin));
};

Editor.ObstructionMode.prototype.KeyPress = function (e) {

	if (-1 !== System.Array.IndexOf(this.MOVE_KEYS, e.KeyCode)) {
		var distance = 1;

		if ((System.Windows.Forms.Control.ModifierKeys & System.Windows.Forms.Keys.None | System.Windows.Forms.Keys.Shift) !== System.Windows.Forms.Keys.None) {
			distance = 10;
		}
		var pos = this.engine.map.Obs.Points.get_Item(this.curpoint);

		switch (e.KeyCode) {
			case 98: 
				pos.Y = (pos.Y + distance);
				break;
			case 100: 
				pos.X = (pos.X - distance);
				break;
			case 102: 
				pos.X = (pos.X + distance);
				break;
			case 104: 
				pos.Y = (pos.Y - distance);
				break;
		}
		this.engine.map.Obs.Points.set_Item(this.curpoint, pos.MemberwiseClone());
	} else if (e.KeyCode === System.Windows.Forms.Keys.None | System.Windows.Forms.Keys.RButton | System.Windows.Forms.Keys.MButton | System.Windows.Forms.Keys.XButton2 | System.Windows.Forms.Keys.Back | System.Windows.Forms.Keys.LineFeed | System.Windows.Forms.Keys.Clear | System.Windows.Forms.Keys.Space | System.Windows.Forms.Keys.Next | System.Windows.Forms.Keys.PageDown | System.Windows.Forms.Keys.Home | System.Windows.Forms.Keys.Up | System.Windows.Forms.Keys.Down | System.Windows.Forms.Keys.Print | System.Windows.Forms.Keys.Snapshot | System.Windows.Forms.Keys.PrintScreen | System.Windows.Forms.Keys.Delete) {
		this.engine.map.Obs.RemovePoint(this.curpoint);
		this.curpoint = 0;
	}
};

Editor.ObstructionMode.prototype.MouseWheel = function (p, delta) {
};

Editor.ObstructionMode.prototype.RenderHUD = function () {
};

Editor.ObstructionMode.prototype.__ImplementInterface__(Editor.IEditorState);
Editor.ObstructionMode.prototype.editor = null;
Editor.ObstructionMode.prototype.engine = null;
Editor.ObstructionMode.prototype.curpoint = 0;
Editor.ObstructionMode.prototype.leftdown = false;
Editor.ObstructionMode.prototype.rightdown = false;
Editor.ObstructionMode.prototype.snap = false;
Editor.ObstructionMode.prototype.MOVE_KEYS = 0;

Import.VectorObstructionMap.prototype._ctor = function (b) {
	System.Object.prototype._ctor.call(this);
	this.Generate(b);
};

Import.VectorObstructionMap.prototype.Generate = function (b) {
	this.lines = JSIL.Array.New(Import.Geo.Line, b.Lines.Count);
	var pos = 0;
	var enumerator = b.Lines.GetEnumerator();

	try {

	__while0__: 
		while (enumerator.IEnumerator_MoveNext()) {
			var i = JSIL.Cast(enumerator.IEnumerator_Current, System.Array.Of(System.Int32));
			this.lines[pos++] = new Import.Geo.Line(b.Points.get_Item(i[0]), b.Points.get_Item(i[1]));
		}
	} finally {
		var disposable = JSIL.TryCast(enumerator, System.IDisposable);

		if (disposable !== null) {
			disposable.IDisposable_Dispose();
		}
	}
};

Import.VectorObstructionMap.prototype.Test = function (x, y, x2, y2) {
	var r = new System.Drawing.Rectangle();
	r._ctor(x, y, (x2 - x), (y2 - y));
	var p = new Import.Geo.Vertex(0, 0);
	var array = this.lines;
	var j = 0;

__while0__: 
	while (j < array.length) {
		var i = array[j];

		if (i.Touches(r.MemberwiseClone(), /* ref */ p)) {
			var result = i;
			return result;
		}
		++j;
	}
	result = null;
	return result;
};

Import.VectorObstructionMap.prototype.lines = null;

Import.Map.prototype._ctor$0 = function (x, y) {
	System.Object.prototype._ctor.call(this);
	this.width = x;
	this.height = y;
	this.layers = new System.Collections.ArrayList();
	this.entities = new System.Collections.ArrayList();
	this.obs = new Import.VectorIndexBuffer();
};

Import.Map.prototype._ctor$1 = function () {
	this._ctor(100, 100);
};

Import.Map.prototype.AddLayer$0 = function () {
	return this.AddLayer(this.NumLayers);
};

Import.Map.prototype.AddLayer$1 = function (idx) {
	var i = new Import.Map.Layer(this.width, this.height, System.String.Concat("New Layer ", this.NumLayers));
	this.layers.Insert(idx, i);
	return i;
};

Import.Map.prototype.AddLayer$2 = function (data) {
	var i = new Import.Map.Layer(this.width, this.height, System.String.Concat("New Layer ", this.NumLayers));
	var j = 0;
	var y = 0;

__while0__: 
	while (y < this.height) {
		var x = 0;

	__while1__: 
		while (x < this.width) {
			i.set_Item(x, y, data[j++]);
			++x;
		}
		++y;
	}
	this.layers.Add(i);
	return i;
};

Import.Map.prototype.RemoveLayer = function (idx) {

	if (!((idx >= 0) && (idx < this.layers.Count))) {
		var result = null;
	} else {
		var i = JSIL.Cast(this.layers.get_Item(idx), Import.Map.Layer);
		this.layers.RemoveAt(idx);
		result = i;
	}
	return result;
};

Import.Map.prototype.SwapLayers = function (a, b) {

	if ((a >= 0) && (a < this.NumLayers)) {

		if ((b >= 0) && (b < this.NumLayers)) {
			var o = this.layers.get_Item(a);
			this.layers.set_Item(a, this.layers.get_Item(b));
			this.layers.set_Item(b, o);
		}
	}
};

Import.Map.prototype.Resize = function (x, y) {
	var enumerator = this.layers.GetEnumerator();

	try {

	__while0__: 
		while (enumerator.IEnumerator_MoveNext()) {
			JSIL.Cast(enumerator.IEnumerator_Current, Import.Map.Layer).Resize(x, y);
		}
	} finally {
		var disposable = JSIL.TryCast(enumerator, System.IDisposable);

		if (disposable !== null) {
			disposable.IDisposable_Dispose();
		}
	}
	this.width = x;
	this.height = y;
};

Import.Map.prototype.get_Item = function (idx) {

	if (!((idx >= 0) && (idx < this.layers.Count))) {
		var result = null;
	} else {
		result = JSIL.Cast(this.layers.get_Item(idx), Import.Map.Layer);
	}
	return result;
};

Import.Map.prototype.get_Layers = function () {
	return this.layers;
};

Import.Map.prototype.get_NumLayers = function () {
	return this.layers.Count;
};

Import.Map.prototype.get_Width = function () {
	return this.width;
};

Import.Map.prototype.get_Height = function () {
	return this.height;
};

Import.Map.prototype.get_Obs = function () {
	return this.obs;
};

Import.Map.prototype.get_Entities = function () {
	return this.entities;
};

JSIL.OverloadedMethod(Import.Map.prototype, "_ctor", [
		["_ctor$0", [System.Int32, System.Int32]], 
		["_ctor$1", []]
	]
);
JSIL.OverloadedMethod(Import.Map.prototype, "AddLayer", [
		["AddLayer$0", []], 
		["AddLayer$1", [System.Int32]], 
		["AddLayer$2", [System.Array.Of(System.Int32)]]
	]
);
Object.defineProperty(Import.Map.prototype, "Item", {
		get: Import.Map.prototype.get_Item
	});
Object.defineProperty(Import.Map.prototype, "Layers", {
		get: Import.Map.prototype.get_Layers
	});
Object.defineProperty(Import.Map.prototype, "NumLayers", {
		get: Import.Map.prototype.get_NumLayers
	});
Object.defineProperty(Import.Map.prototype, "Width", {
		get: Import.Map.prototype.get_Width
	});
Object.defineProperty(Import.Map.prototype, "Height", {
		get: Import.Map.prototype.get_Height
	});
Object.defineProperty(Import.Map.prototype, "Obs", {
		get: Import.Map.prototype.get_Obs
	});
Object.defineProperty(Import.Map.prototype, "Entities", {
		get: Import.Map.prototype.get_Entities
	});
Import.Map.prototype.layers = null;
Import.Map.prototype.entities = null;
Import.Map.prototype.obs = null;
Import.Map.prototype.width = 0;
Import.Map.prototype.height = 0;
Import.Map.prototype.vspname = null;
Import.Map.prototype.musicname = null;
Import.Map.prototype.renderstring = null;

Import.Map.Layer.prototype._ctor = function (width, height, n) {
	System.Object.prototype._ctor.call(this);
	this.name = n;
	this.tiles = JSIL.MultidimensionalArray.New(System.Int32, width, height);
	this.parx = this.pary = 1;
	this.visible = true;
};

Import.Map.Layer.prototype.get_Width = function () {
	return this.tiles.GetUpperBound(0);
};

Import.Map.Layer.prototype.get_Height = function () {
	return this.tiles.GetUpperBound(1);
};

Import.Map.Layer.prototype.get_ParallaxX = function () {
	return this.parx;
};

Import.Map.Layer.prototype.get_ParallaxY = function () {
	return this.pary;
};

Import.Map.Layer.prototype.get_Name = function () {
	return this.name;
};

Import.Map.Layer.prototype.set_Name = function (value) {
	this.name = value;
};

Import.Map.Layer.prototype.get_Item = function (x, y) {

	if (!((x < this.tiles.GetLength(0)) && (x >= 0))) {
		var result = 0;
	} else if (!((y < this.tiles.GetLength(1)) && (y >= 0))) {
		result = 0;
	} else {
		result = this.tiles.Get(x, y);
	}
	return result;
};

Import.Map.Layer.prototype.set_Item = function (x, y, value) {

	if ((x < this.tiles.GetLength(0)) && (x >= 0)) {

		if ((y < this.tiles.GetLength(1)) && (y >= 0)) {
			this.tiles.Set(x, y, value);
		}
	}
};

Import.Map.Layer.prototype.Resize = function (newx, newy) {
	var newdata = JSIL.MultidimensionalArray.New(System.Int32, newx, newy);
	var sx = newx;
	var sy = newy;

	if (sx > this.Width) {
		sx = this.Width;
	}

	if (sy > this.Height) {
		sy = this.Height;
	}
	var y = 0;

__while0__: 
	while (y < sy) {
		var x = 0;

	__while1__: 
		while (x < sx) {
			newdata.Set(x, y, this.tiles.Get(x, y));
			++x;
		}
		++y;
	}
	this.tiles = newdata;
};

Object.defineProperty(Import.Map.Layer.prototype, "Width", {
		get: Import.Map.Layer.prototype.get_Width
	});
Object.defineProperty(Import.Map.Layer.prototype, "Height", {
		get: Import.Map.Layer.prototype.get_Height
	});
Object.defineProperty(Import.Map.Layer.prototype, "ParallaxX", {
		get: Import.Map.Layer.prototype.get_ParallaxX
	});
Object.defineProperty(Import.Map.Layer.prototype, "ParallaxY", {
		get: Import.Map.Layer.prototype.get_ParallaxY
	});
Object.defineProperty(Import.Map.Layer.prototype, "Name", {
		get: Import.Map.Layer.prototype.get_Name, 
		set: Import.Map.Layer.prototype.set_Name
	});
Object.defineProperty(Import.Map.Layer.prototype, "Item", {
		get: Import.Map.Layer.prototype.get_Item, 
		set: Import.Map.Layer.prototype.set_Item
	});
Import.Map.Layer.prototype.tiles = null;
Import.Map.Layer.prototype.name = null;
Import.Map.Layer.prototype.parx = 0;
Import.Map.Layer.prototype.pary = 0;
Import.Map.Layer.prototype.visible = false;

Import.Geo.Line.prototype._ctor$0 = function (A, B) {
	System.Object.prototype._ctor.call(this);
	this.a = A;
	this.b = B;

	if (this.a.X === this.b.X) {
		this.slope = 9999999827968;
	} else {
		this.slope = (((1 * JSIL.Cast(this.a.Y, System.Single)) - JSIL.Cast(this.b.Y, System.Single)) / (this.a.X - this.b.X));
	}
	this.yint = (JSIL.Cast(this.a.Y, System.Single) - (this.slope * JSIL.Cast(this.a.X, System.Single)));
};

Import.Geo.Line.prototype._ctor$1 = function (x1, y1, x2, y2) {
	this._ctor(new Import.Geo.Vertex(x1, y1), new Import.Geo.Vertex(x2, y2));
};

Import.Geo.Line.prototype.get_Slope = function () {
	return this.slope;
};

Import.Geo.Line.prototype.get_YIntercept = function () {
	return this.yint;
};

Import.Geo.Line.prototype.atX = function (x) {
	return ((x * this.Slope) + this.YIntercept);
};

Import.Geo.Line.prototype.atY = function (y) {
	return ((y - this.YIntercept) / this.Slope);
};

Import.Geo.Line.prototype.Intersects = function (l, /* ref */ intercept) {
	var this_m = this.Slope;
	var l_m = l.Slope;
	var this_b = this.YIntercept;
	var l_b = l.YIntercept;

	if (!((this.a.X !== this.b.X) || (l.a.X !== l.b.X))) {
		var result = (this.a.X === l.a.X);
	} else if (l.a.X === l.b.X) {
		intercept.X = l.a.X;
		intercept.Y = JSIL.Cast(this.atX(JSIL.Cast(l.a.X, System.Single)), System.Int32);
		result = true;
	} else if (this.a.X === this.b.X) {
		result = l.Intersects(this, /* ref */ intercept);
	} else if (!((l_m !== this_m) || (this.a.X === l.a.X))) {
		result = false;
	} else {
		intercept.X = Math.floor((l_b - this_b) / (this_m - l_m));
		intercept.Y = ((this_m * JSIL.Cast(intercept.X, System.Single)) + this_b);
		result = true;
	}
	return result;
};

Import.Geo.Line.prototype.Touches$0 = function (l, /* ref */ intercept) {
	return (((this.a.X >= l.a.X) || 
			(this.a.X >= l.b.X) || 
			(this.b.X >= l.a.X) || 
			(this.b.X >= l.b.X)) && 
		((this.a.X <= l.a.X) || 
			(this.a.X <= l.b.X) || 
			(this.b.X <= l.a.X) || 
			(this.b.X <= l.b.X)) && 
		((this.a.Y >= l.a.Y) || 
			(this.a.Y >= l.b.Y) || 
			(this.b.Y >= l.a.Y) || 
			(this.b.Y >= l.b.Y)) && 
		((this.a.Y <= l.a.Y) || 
			(this.a.Y <= l.b.Y) || 
			(this.b.Y <= l.a.Y) || 
			(this.b.Y <= l.b.Y)) && 
		this.Intersects(l, /* ref */ intercept) && 
		!((intercept.X < this.a.X) && 
			(intercept.X < this.b.X)) && 
		((intercept.X <= this.a.X) || 
			(intercept.X <= this.b.X)) && 
		!((intercept.Y < this.a.Y) && 
			(intercept.Y < this.b.Y)) && 
		((intercept.Y <= this.a.Y) || 
			(intercept.Y <= this.b.Y)) && 
		!((intercept.X < l.a.X) && 
			(intercept.X < l.b.X)) && 
		((intercept.X <= l.a.X) || 
			(intercept.X <= l.b.X)) && 
		!((intercept.Y < l.a.Y) && 
			(intercept.Y < l.b.Y)) && ((intercept.Y <= l.a.Y) || 
			(intercept.Y <= l.b.Y)));
};

Import.Geo.Line.prototype.Touches$1 = function (r, /* ref */ intercept) {

	if (!((this.a.X >= r.Left) || (this.b.X >= r.Left))) {
		var result = false;
	} else if (!((this.a.X <= r.Right) || (this.b.X <= r.Right))) {
		result = false;
	} else if (!((this.a.Y >= r.Top) || (this.b.Y >= r.Top))) {
		result = false;
	} else if (!((this.a.Y <= r.Bottom) || (this.b.Y <= r.Bottom))) {
		result = false;
	} else {
		var x = JSIL.Cast(this.atY(JSIL.Cast(r.Top, System.Single)), System.Int32);

		if (!((x < r.Left) || (x > r.Right))) {
			intercept.value = new Import.Geo.Vertex(x, r.Top);
			result = true;
		} else {
			x = JSIL.Cast(this.atY(JSIL.Cast(r.Bottom, System.Single)), System.Int32);

			if (!((x < r.Left) || (x > r.Right))) {
				intercept.value = new Import.Geo.Vertex(x, r.Bottom);
				result = true;
			} else {
				var y = JSIL.Cast(this.atX(JSIL.Cast(r.Left, System.Single)), System.Int32);

				if (!((y < r.Top) || (y > r.Bottom))) {
					intercept.value = new Import.Geo.Vertex(r.Left, y);
					result = true;
				} else {
					y = JSIL.Cast(this.atX(JSIL.Cast(r.Right, System.Single)), System.Int32);

					if (!((y < r.Top) || (y > r.Bottom))) {
						intercept.value = new Import.Geo.Vertex(r.Right, y);
						result = true;
					} else {
						result = false;
					}
				}
			}
		}
	}
	return result;
};

Import.Geo.Line.prototype.get_A = function () {
	return this.a;
};

Import.Geo.Line.prototype.get_B = function () {
	return this.b;
};

Import.Geo.Line.prototype.toString = function () {
	return System.String.Format("({0},{1})-({2},{3})", [this.a.X, this.a.Y, this.b.X, this.b.Y]);
};

JSIL.OverloadedMethod(Import.Geo.Line.prototype, "_ctor", [
		["_ctor$0", [Import.Geo.Vertex, Import.Geo.Vertex]], 
		["_ctor$1", [System.Int32, System.Int32, System.Int32, System.Int32]]
	]
);
JSIL.OverloadedMethod(Import.Geo.Line.prototype, "Touches", [
		["Touches$0", [Import.Geo.Line, JSIL.Reference.Of(Import.Geo.Vertex)]], 
		["Touches$1", [System.Drawing.Rectangle, JSIL.Reference.Of(Import.Geo.Vertex)]]
	]
);
Object.defineProperty(Import.Geo.Line.prototype, "Slope", {
		get: Import.Geo.Line.prototype.get_Slope
	});
Object.defineProperty(Import.Geo.Line.prototype, "YIntercept", {
		get: Import.Geo.Line.prototype.get_YIntercept
	});
Object.defineProperty(Import.Geo.Line.prototype, "A", {
		get: Import.Geo.Line.prototype.get_A
	});
Object.defineProperty(Import.Geo.Line.prototype, "B", {
		get: Import.Geo.Line.prototype.get_B
	});
Import.Geo.Line.prototype.__StructFields__ = {
	a: Import.Geo.Vertex, 
	b: Import.Geo.Vertex
};
Import.Geo.Line.prototype.slope = 0;
Import.Geo.Line.prototype.yint = 0;

RLE.prototype._ctor = function () {
	System.Object.prototype._ctor.call(this);
};

RLE.Read$0 = function (dest, count, src) {
	var sidx = 0;
	var didx = 0;
	var i = 0;

__while0__: 
	while (true) {
		var w = src[sidx++];

		if (w === 255) {
			var run = src[sidx++];
			w = src[sidx++];
			var j = 0;

		__while1__: 
			while (j < run) {
				dest[didx++] = w;
				++j;
			}
			i += run;
		} else {
			dest[didx++] = w;
			++i;
		}

		if (i >= count) {
			break __while0__;
		}
	}
};

RLE.Read$1 = function (dest, count, src) {
	var sidx = 0;
	var didx = 0;
	var i = 0;

__while0__: 
	while (true) {
		var w = src[sidx++];

		if ((w & 65280) === 65280) {
			var run = (w & 255);
			w = src[sidx++];
			var j = 0;

		__while1__: 
			while (j < run) {
				dest[didx++] = w;
				++j;
			}
			i += run;
		} else {
			dest[didx++] = w;
			++i;
		}

		if (i >= count) {
			break __while0__;
		}
	}
};

JSIL.OverloadedMethod(RLE, "Read", [
		["Read$0", [System.Array.Of(System.Byte), System.Int32, System.Array.Of(System.Byte)]], 
		["Read$1", [System.Array.Of(System.Int32), System.Int32, System.Array.Of(System.UInt16)]]
	]
);

Import.v2Map.Load = function (fname) {
	var f = new System.IO.FileStream(fname, System.IO.FileMode.Open);
	var stream = new System.IO.BinaryReader(f, System.Text.Encoding.ASCII);
	var map = new Import.Map();
	var buffer = stream.ReadChars(6);
	buffer = stream.ReadChars(4);
	buffer = stream.ReadChars(60);
	map.vspname = new System.String(buffer, 0, System.Array.IndexOf(buffer, 0));
	buffer = stream.ReadChars(60);
	map.musicname = new System.String(buffer, 0, System.Array.IndexOf(buffer, 0));
	buffer = stream.ReadChars(20);
	map.renderstring = new System.String(buffer, 0, System.Array.IndexOf(buffer, 0));
	buffer = stream.ReadChars(55);
	var numlayers = stream.ReadByte();
	var width = 0;
	var height = 0;
	var parx = JSIL.Array.New(System.Single, numlayers);
	var pary = JSIL.Array.New(System.Single, numlayers);
	var trans = JSIL.Array.New(System.Boolean, numlayers);
	var i = 0;

__while0__: 
	while (i < numlayers) {
		var mx = stream.ReadSByte();
		var dx = stream.ReadSByte();
		var my = stream.ReadSByte();
		var dy = stream.ReadSByte();
		parx[i] = ((1 * mx) / dx);
		pary[i] = ((1 * my) / dy);
		width = stream.ReadUInt16();
		height = stream.ReadUInt16();
		var hline = stream.ReadByte();
		trans[i] = (stream.ReadByte() !== 0);
		stream.ReadInt16();
		++i;
	}
	map.Resize(width, height);
	i = 0;

__while1__: 
	while (i < numlayers) {
		var buffersize = stream.ReadUInt32();
		var cbuffer = JSIL.Array.New(System.UInt16, buffersize);
		var layerdata = JSIL.Array.New(System.Int32, (width * height));
		var j = 0;

	__while2__: 
		while (j < Math.floor(buffersize / 2)) {
			cbuffer[j] = stream.ReadUInt16();
			++j;
		}
		RLE.Read(layerdata, (width * height), cbuffer);
		var k = map.AddLayer(layerdata);
		k.parx = parx[i];
		k.pary = pary[i];
		++i;
	}
	stream.Close();
	f.Close();
	return map;
};

Import.v2Map.prototype._ctor = function () {
	System.Object.prototype._ctor.call(this);
};


Mannux.Import.AutoArray$b1.prototype._ctor = function () {
	this.data = null;
	this.length = 0;
	System.Object.prototype._ctor.call(this);
	this.Reserve(8);
};

Mannux.Import.AutoArray$b1.prototype.get_Capacity = function () {
	return this.data ? this.data.length : 0;
};

Mannux.Import.AutoArray$b1.prototype.Realloc = function (count) {
	System.Diagnostics.Debug.Assert((count >= this.length));
	var newData = JSIL.Array.New(T, count);
	var i = 0;

__while0__: 
	while (i < this.length) {
		newData[i] = this.data[i];
		++i;
	}
	this.data = newData;
};

Mannux.Import.AutoArray$b1.prototype.Reserve = function (count) {

	if (count > this.Capacity) {
		this.Realloc(count);
	}
};

Mannux.Import.AutoArray$b1.prototype.Add = function (t) {

	if (this.length === this.Capacity) {
		this.Reserve((this.length * 2));
	}
	this.data[this.length++] = t;
};

Mannux.Import.AutoArray$b1.prototype.RemoveAt = function (index) {
	System.Diagnostics.Debug.Assert(((0 <= index) && (index < this.length)));
	var i = index;

__while0__: 
	while (i < this.length) {
		this.data[i] = this.data[(i + 1)];
		++i;
	}
};

Mannux.Import.AutoArray$b1.prototype.get_Length = function () {
	return this.length;
};

Mannux.Import.AutoArray$b1.prototype.get_Item = function (index) {
	return this.data[index];
};

Mannux.Import.AutoArray$b1.prototype.set_Item = function (index, value) {
	this.data[index] = value;
};

Mannux.Import.AutoArray$b1.prototype.ToArray = function () {
	var result = JSIL.Array.New(T, this.length);
	System.Array.Copy(this.data, result, this.length);
	return result;
};

Mannux.Import.AutoArray$b1.prototype.get_Array = function () {
	return this.data;
};

Object.defineProperty(Mannux.Import.AutoArray$b1.prototype, "Capacity", {
		get: Mannux.Import.AutoArray$b1.prototype.get_Capacity
	});
Object.defineProperty(Mannux.Import.AutoArray$b1.prototype, "Length", {
		get: Mannux.Import.AutoArray$b1.prototype.get_Length
	});
Object.defineProperty(Mannux.Import.AutoArray$b1.prototype, "Item", {
		get: Mannux.Import.AutoArray$b1.prototype.get_Item, 
		set: Mannux.Import.AutoArray$b1.prototype.set_Item
	});
Object.defineProperty(Mannux.Import.AutoArray$b1.prototype, "Array", {
		get: Mannux.Import.AutoArray$b1.prototype.get_Array
	});
Mannux.Import.AutoArray$b1.prototype.data = null;
Mannux.Import.AutoArray$b1.prototype.length = 0;

Editor.NewMapDlg.prototype._ctor = function () {
	System.Windows.Forms.Form.prototype._ctor.call(this);
	this.FormBorderStyle = this;
	var t = new System.Windows.Forms.Label();
	t.Text = "Width";
	t.Location = new System.Drawing.Point(10, 10);
	t.Show();
	var u = new System.Windows.Forms.Label();
	u.Text = "Height";
	u.Location = new System.Drawing.Point(t.Left, (t.Bottom + 5));
	u.Show();
	this.widthbox = new System.Windows.Forms.TextBox();
	this.widthbox.Location = new System.Drawing.Point((t.Right + 5), 10);
	this.widthbox.Show();
	this.heightbox = new System.Windows.Forms.TextBox();
	this.heightbox.Location = new System.Drawing.Point(this.widthbox.Left, (this.widthbox.Bottom + 5));
	this.heightbox.Show();
	var okbutton = new System.Windows.Forms.Button();
	okbutton.Text = "OK";
	okbutton.DialogResult = System.Windows.Forms.DialogResult.OK;
	okbutton.Location = new System.Drawing.Point(t.Left, (u.Bottom + 5));
	okbutton.Show();
	var cancelbutton = new System.Windows.Forms.Button();
	cancelbutton.Text = "Cancel";
	cancelbutton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
	cancelbutton.Location = new System.Drawing.Point((okbutton.Right + 5), okbutton.Top);
	cancelbutton.Show();
	this.Controls.Add(t);
	this.Controls.Add(u);
	this.Controls.Add(this.widthbox);
	this.Controls.Add(this.heightbox);
	this.Controls.Add(okbutton);
	this.Controls.Add(cancelbutton);
	this.AcceptButton = this;
	this.CancelButton = this;
	this.Width = this;
	this.Height = this;
};

Editor.NewMapDlg.prototype.get_MapWidth = function () {
	return System.Convert.ToInt32(this.widthbox.Text);
};

Editor.NewMapDlg.prototype.get_MapHeight = function () {
	return System.Convert.ToInt32(this.heightbox.Text);
};

Object.defineProperty(Editor.NewMapDlg.prototype, "MapWidth", {
		get: Editor.NewMapDlg.prototype.get_MapWidth
	});
Object.defineProperty(Editor.NewMapDlg.prototype, "MapHeight", {
		get: Editor.NewMapDlg.prototype.get_MapHeight
	});
Editor.NewMapDlg.prototype.widthbox = null;
Editor.NewMapDlg.prototype.heightbox = null;

Mannux.Program.Main = function (args) {
	var game = new Engine();

	try {
		game.Run();
	} finally {

		if (game !== null) {
			game.IDisposable_Dispose();
		}
	}
};


Import.MapEnt.prototype._ctor = function () {
	System.Object.prototype._ctor.call(this);
};

Import.MapEnt.prototype.x = 0;
Import.MapEnt.prototype.y = 0;
Import.MapEnt.prototype.type = null;
Import.MapEnt.prototype.data = null;
Import.MapEnt._cctor = function () {
	Import.MapEnt.enttypes = null;
	Import.MapEnt.enttypes = JSIL.Array.New(System.String, ["player", "door", "ripper", "hopper"]);
};


Input.MouseDevice.prototype.add_MouseDown = function (value) {
	this.MouseDown = System.Delegate.Combine(this.MouseDown, value);
};

Input.MouseDevice.prototype.remove_MouseDown = function (value) {
	this.MouseDown = System.Delegate.Remove(this.MouseDown, value);
};

Input.MouseDevice.prototype.add_MouseUp = function (value) {
	this.MouseUp = System.Delegate.Combine(this.MouseUp, value);
};

Input.MouseDevice.prototype.remove_MouseUp = function (value) {
	this.MouseUp = System.Delegate.Remove(this.MouseUp, value);
};

Input.MouseDevice.prototype.add_MouseWheel = function (value) {
	this.MouseWheel = System.Delegate.Combine(this.MouseWheel, value);
};

Input.MouseDevice.prototype.remove_MouseWheel = function (value) {
	this.MouseWheel = System.Delegate.Remove(this.MouseWheel, value);
};

Input.MouseDevice.prototype.add_Moved = function (value) {
	this.Moved = System.Delegate.Combine(this.Moved, value);
};

Input.MouseDevice.prototype.remove_Moved = function (value) {
	this.Moved = System.Delegate.Remove(this.Moved, value);
};

Input.MouseDevice.prototype.Poll = function () {
	this.oldms = this.ms.MemberwiseClone();
	this.ms = Microsoft.Xna.Framework.Input.Mouse.GetState();
};

Input.MouseDevice.prototype.LeftClicked = function () {
	return ((this.ms.LeftButton === Microsoft.Xna.Framework.Input.ButtonState.Pressed) && (this.ms.LeftButton !== this.oldms.LeftButton));
};

Input.MouseDevice.prototype.RightClicked = function () {
	return ((this.ms.RightButton === Microsoft.Xna.Framework.Input.ButtonState.Pressed) && (this.ms.RightButton !== this.oldms.RightButton));
};

Input.MouseDevice.prototype.SendEvents = function () {

	if (this.ms.LeftButton !== this.oldms.LeftButton) {

		if (this.ms.LeftButton === Microsoft.Xna.Framework.Input.ButtonState.Pressed) {

			if (this.MouseDown !== null) {
				this.MouseDown(new Microsoft.Xna.Framework.Point(this.ms.X, this.ms.Y), Input.MouseButton.Left);
			}
		} else if (this.MouseUp !== null) {
			this.MouseUp(new Microsoft.Xna.Framework.Point(this.ms.X, this.ms.Y), Input.MouseButton.Left);
		}
	}

	if (this.ms.RightButton !== this.oldms.RightButton) {

		if (this.ms.RightButton === Microsoft.Xna.Framework.Input.ButtonState.Pressed) {

			if (this.MouseDown !== null) {
				this.MouseDown(new Microsoft.Xna.Framework.Point(this.ms.X, this.ms.Y), Input.MouseButton.Right);
			}
		} else if (this.MouseUp !== null) {
			this.MouseUp(new Microsoft.Xna.Framework.Point(this.ms.X, this.ms.Y), Input.MouseButton.Right);
		}
	}
	var wheelDelta = (this.oldms.ScrollWheelValue - this.ms.ScrollWheelValue);

	if (wheelDelta !== 0) {

		if (this.MouseWheel !== null) {
			this.MouseWheel(new Microsoft.Xna.Framework.Point(this.ms.X, this.ms.Y), wheelDelta);
		}
	} else if (!((this.oldms.X === this.ms.X) && (this.oldms.Y === this.ms.Y))) {

		if (this.Moved !== null) {
			var mouseMask = Input.MouseButton.None;

			if (this.ms.LeftButton === Microsoft.Xna.Framework.Input.ButtonState.Pressed) {
				mouseMask = (mouseMask | Input.MouseButton.Left);
			}

			if (this.ms.RightButton === Microsoft.Xna.Framework.Input.ButtonState.Pressed) {
				mouseMask = (mouseMask | Input.MouseButton.Right);
			}
			this.Moved(new Microsoft.Xna.Framework.Point(this.ms.X, this.ms.Y), mouseMask);
		}
	}
};

Input.MouseDevice.prototype._ctor = function () {
	System.Object.prototype._ctor.call(this);
};

Input.MouseDevice.prototype.__StructFields__ = {
	oldms: Microsoft.Xna.Framework.Input.MouseState, 
	ms: Microsoft.Xna.Framework.Input.MouseState
};
Input.MouseDevice.prototype.MouseDown = null;
Input.MouseDevice.prototype.MouseUp = null;
Input.MouseDevice.prototype.MouseWheel = null;
Input.MouseDevice.prototype.Moved = null;

Entities.Player.prototype._ctor = function (e) {
	this.hp = 100;
	this.hurtcount = 0;
	Entities.Entity.prototype._ctor.call(this, e, e.TabbySprite);
	this.input = this.engine.input.Keyboard;
	this.width = 12;
	System.Console.WriteLine("Player {0}x{1}", this.width, this.height);
	this.UpdateState = JSIL.Delegate.New("System.Action", this, Entities.Player.prototype.Walk);
};

Entities.Player.prototype.Update = function () {
	Entities.Entity.prototype.Update.call(this);
	this.CollisionThings();
	this.anim.Update();

	if (this.firedelay > 0) {
		--this.firedelay;
	}
	this.vx = Vector.Clamp(this.vx, 2.5);
	this.vy = Vector.Clamp(this.vy, 250);
};

Entities.Player.prototype.CollisionThings = function () {

	if (!(!this.touchingleftwall || (this.vx >= 0))) {
		this.vx = 0;
	}

	if (!(!this.touchingrightwall || (this.vx <= 0))) {
		this.vx = 0;
	}
	var temp = this.engine.DetectCollision(this);

	if (JSIL.CheckType(temp, Entities.Enemies.Enemy)) {

		if (this.hurtcount === 0) {
		}
	}
};

Entities.Player.prototype.SetStandState = function () {
	this.UpdateState = JSIL.Delegate.New("System.Action", this, Entities.Player.prototype.Stand);
	this.anim.Set(AnimState.playerstand[this.direction]);
};

Entities.Player.prototype.SetShootUpState = function () {
	this.UpdateState = JSIL.Delegate.New("System.Action", this, Entities.Player.prototype.Stand);
	this.anim.Set(AnimState.playershootup[this.direction]);
};

Entities.Player.prototype.SetWalkState = function () {
	this.UpdateState = JSIL.Delegate.New("System.Action", this, Entities.Player.prototype.Walk);
	this.anim.Set(AnimState.playerwalk[this.direction]);
};

Entities.Player.prototype.SetWalkFireState = function () {
	this.UpdateState = JSIL.Delegate.New("System.Action", this, Entities.Player.prototype.Walk);
	this.anim.Set(AnimState.playerwalkshooting[this.direction]);
};

Entities.Player.prototype.SetFireState = function () {
	this.UpdateState = JSIL.Delegate.New("System.Action", this, Entities.Player.prototype.Stand);
	this.anim.Set(AnimState.playershooting[this.direction]);
};

Entities.Player.prototype.SetWalkFireAngleUpState = function () {
	this.UpdateState = JSIL.Delegate.New("System.Action", this, Entities.Player.prototype.Walk);
	this.anim.Set(AnimState.playerwalkshootingangleup[this.direction]);
};

Entities.Player.prototype.SetWalkFireAngleDownState = function () {
	this.UpdateState = JSIL.Delegate.New("System.Action", this, Entities.Player.prototype.Walk);
	this.anim.Set(AnimState.playerwalkshootingangledown[this.direction]);
};

Entities.Player.prototype.SetJumpState = function () {
	this.jumpcount = 30;
	this.UpdateState = JSIL.Delegate.New("System.Action", this, Entities.Player.prototype.Jump);
	this.anim.Set(AnimState.playerjump[this.direction]);
};

Entities.Player.prototype.SetFallState = function () {
	this.UpdateState = JSIL.Delegate.New("System.Action", this, Entities.Player.prototype.Fall);
	this.anim.Set(AnimState.playerfall[this.direction]);
};

Entities.Player.prototype.SetCrouchState = function () {
	this.UpdateState = JSIL.Delegate.New("System.Action", this, Entities.Player.prototype.Crouch);
	this.anim.Set(AnimState.playercrouch[this.direction]);
};

Entities.Player.prototype.SetCrouchFireState = function () {
	this.UpdateState = JSIL.Delegate.New("System.Action", this, Entities.Player.prototype.Crouch);
	this.anim.Set(AnimState.playercrouchshooting[this.direction]);
};

Entities.Player.prototype.SetFallFireState = function (d) {
	this.UpdateState = JSIL.Delegate.New("System.Action", this, Entities.Player.prototype.Fall);
	this.Fire(d);

	switch (d) {
		case 0: 
			this.anim.Set(AnimState.playerfallshooting[this.direction]);
			break;
		case 1: 
			this.anim.Set(AnimState.playerfallshooting[this.direction]);
			break;
		case 2: 
			this.anim.Set(AnimState.playerfallshootingup[this.direction]);
			break;
		case 3: 
			this.anim.Set(AnimState.playerfallshootingdown[this.direction]);
			break;
		case 4: 
			this.anim.Set(AnimState.playerfallshootingangleup[this.direction]);
			break;
		case 5: 
			this.anim.Set(AnimState.playerfallshootingangleup[this.direction]);
			break;
		case 6: 
			this.anim.Set(AnimState.playerfallshootingangledown[this.direction]);
			break;
		case 7: 
			this.anim.Set(AnimState.playerfallshootingangledown[this.direction]);
			break;
	}
};

Entities.Player.prototype.SetHurtState = function () {
	this.UpdateState = JSIL.Delegate.New("System.Action", this, Entities.Player.prototype.Hurt);
	this.anim.Set(AnimState.hurt[this.direction]);
};

Entities.Player.prototype.Crouch = function () {

	if (this.input.IInputDevice_Axis(0) === 0) {
		this.SetStandState();
	}

	if (this.input.IInputDevice_Button(1)) {
		this.Fire(this.direction);
		this.SetCrouchFireState();
	}
};

Entities.Player.prototype.Stand = function () {

	if (this.input.IInputDevice_Button(1)) {

		if (this.input.IInputDevice_Axis(0) === 0) {
			this.Fire(Entities.Dir.up);
			this.SetShootUpState();
		} else {
			this.Fire(this.direction);
			this.SetFireState();
		}
	}

	if (this.firedelay === 0) {
		this.SetStandState();
	}

	if (this.input.IInputDevice_Axis(0) === 255) {
		this.SetCrouchState();
	}

	if (this.input.IInputDevice_Axis(1) === 0) {
		this.SetWalkState();
	} else if (this.input.IInputDevice_Axis(1) === 255) {
		this.SetWalkState();
	} else if (this.input.IInputDevice_Button(0)) {
		this.SetJumpState();
	} else if (!this.touchingground) {
		this.SetFallState();
	} else {
		this.y = (this.floor.atX((this.x + Math.floor(this.width / 2))) - this.height);
	}
};

Entities.Player.prototype.Walk = function () {
	this.vx = Vector.Decrease(this.vx, 0.25999999046325684);
	this.vy = 0;

	if (this.input.IInputDevice_Axis(0) === 0) {
		this.SetWalkFireAngleUpState();

		if (this.input.IInputDevice_Button(1)) {

			if (this.direction === Entities.Dir.left) {
				this.Fire(Entities.Dir.up_left);
			} else {
				this.Fire(Entities.Dir.up_right);
			}
		}
	}

	if (this.input.IInputDevice_Axis(0) === 255) {
		this.SetWalkFireAngleDownState();

		if (this.input.IInputDevice_Button(1)) {

			if (this.direction === Entities.Dir.left) {
				this.Fire(Entities.Dir.down_left);
			} else {
				this.Fire(Entities.Dir.down_right);
			}
		}
	}

	if (!(!this.input.IInputDevice_Button(1) || 
			(this.input.IInputDevice_Axis(0) === 255) || (this.input.IInputDevice_Axis(0) === 0))) {
		this.Fire(this.direction);
		this.SetWalkFireState();
	}

	if (this.input.IInputDevice_Axis(1) === 0) {
		this.vx -= 0.52999997138977051;

		if (this.direction !== Entities.Dir.left) {
			this.direction = Entities.Dir.left;
			this.anim.Set(AnimState.playerwalk[0]);
		}
	}

	if (this.input.IInputDevice_Axis(1) === 255) {
		this.vx += 0.52999997138977051;

		if (this.direction !== Entities.Dir.right) {
			this.direction = Entities.Dir.right;
			this.anim.Set(AnimState.playerwalk[1]);
		}
	}

	if (this.vx === 0) {
		this.SetStandState();
	} else {

		if (!(this.touchingceiling || !this.input.IInputDevice_Button(0))) {
			this.SetJumpState();
		}

		if (!this.touchingground) {
			this.SetFallState();
		}
	}
};

Entities.Player.prototype.Jump = function () {

	if (!(this.input.IInputDevice_Button(0) && (this.jumpcount !== 0))) {
		this.jumpcount = 0;
		this.vy = -1.6649999618530273;
		this.SetFallState();
	} else {

		if (this.input.IInputDevice_Button(1)) {

			if (this.input.IInputDevice_Axis(0) === 0) {

				if (this.input.IInputDevice_Axis(1) === 0) {
					this.SetFallFireState(Entities.Dir.up_left);
				}

				if (this.input.IInputDevice_Axis(1) === 255) {
					this.SetFallFireState(Entities.Dir.up_right);
				} else {
					this.SetFallFireState(Entities.Dir.up);
				}
			}

			if (this.input.IInputDevice_Axis(0) === 255) {

				if (this.input.IInputDevice_Axis(1) === 0) {
					this.SetFallFireState(Entities.Dir.down_left);
				}

				if (this.input.IInputDevice_Axis(1) === 255) {
					this.SetFallFireState(Entities.Dir.down_right);
				} else {
					this.SetFallFireState(Entities.Dir.down);
				}
			}

			if (!((this.input.IInputDevice_Axis(0) === 255) || (this.input.IInputDevice_Axis(0) === 0))) {
				this.SetFallFireState(this.direction);
			}
		}

		if (this.touchingceiling) {
			this.vy = 0;
			this.SetFallState();
		}
		this.vy = -3.3299999237060547;
		--this.jumpcount;
		this.vx = Vector.Decrease(this.vx, 0.016599999740719795);

		if (this.input.IInputDevice_Axis(1) === 0) {
			this.vx -= 0.20000000298023224;
		}

		if (this.input.IInputDevice_Axis(1) === 255) {
			this.vx += 0.20000000298023224;
		}
	}
};

Entities.Player.prototype.Fall = function () {

	if (this.input.IInputDevice_Button(1)) {

		if (this.input.IInputDevice_Axis(0) === 0) {

			if (this.input.IInputDevice_Axis(1) === 0) {
				this.SetFallFireState(Entities.Dir.up_left);
			}

			if (this.input.IInputDevice_Axis(1) === 255) {
				this.SetFallFireState(Entities.Dir.up_right);
			}

			if (!((this.input.IInputDevice_Axis(1) === 255) || (this.input.IInputDevice_Axis(1) === 0))) {
				this.SetFallFireState(Entities.Dir.up);
			}
		}

		if (this.input.IInputDevice_Axis(0) === 255) {

			if (this.input.IInputDevice_Axis(1) === 0) {
				this.SetFallFireState(Entities.Dir.down_left);
			}

			if (this.input.IInputDevice_Axis(1) === 255) {
				this.SetFallFireState(Entities.Dir.down_right);
			}

			if (!((this.input.IInputDevice_Axis(1) === 255) || (this.input.IInputDevice_Axis(1) === 0))) {
				this.SetFallFireState(Entities.Dir.down);
			}
		}

		if (!((this.input.IInputDevice_Axis(0) === 255) || (this.input.IInputDevice_Axis(0) === 0))) {
			this.SetFallFireState(this.direction);
		}
	}

	if (!(!this.touchingceiling || (this.vy >= 0))) {
		this.vy = 0;
	}

	if (this.touchingground) {

		if (this.vx !== 0) {
			this.SetWalkState();
		} else {
			this.SetStandState();
		}
		this.vy = 0;
		this.y = (this.floor.atX((this.x + Math.floor(this.width / 2))) - this.height);
	}
	this.vx = Vector.Decrease(this.vx, 0.016599999740719795);

	if (this.input.IInputDevice_Axis(1) === 0) {
		this.vx -= 0.20000000298023224;

		if (this.direction !== Entities.Dir.left) {
			this.direction = Entities.Dir.left;
			this.anim.Set(AnimState.playerfall[0]);
		}
	}

	if (this.input.IInputDevice_Axis(1) === 255) {
		this.vx += 0.20000000298023224;

		if (this.direction !== Entities.Dir.right) {
			this.direction = Entities.Dir.right;
			this.anim.Set(AnimState.playerfall[1]);
		}
	}
	Entities.Entity.prototype.HandleGravity.call(this);
};

Entities.Player.prototype.Hurt = function () {

	if (this.hurtcount > 0) {
		--this.hurtcount;
		this.visible = ((this.hurtcount % 8) < 4);
	} else {
		this.visible = true;
		this.SetStandState();
	}
	Entities.Entity.prototype.HandleGravity.call(this);
};

Entities.Player.prototype.Fire = function (d) {
	var bx = this.x;

	if (this.direction === Entities.Dir.right) {
		bx += this.width;
	}

	if (this.direction === Entities.Dir.left) {
		bx -= 8;
	}

	if (this.firedelay === 0) {
		this.engine.SpawnEntity(new Entities.Bullet(this.engine, bx, (this.y + 14), d));
		this.firedelay = 12;
	}
};

Entities.Player.prototype.Die = function () {
	this.hp = 0;
};

Entities.Player.prototype.get_HP = function () {
	return this.hp;
};

Entities.Player.prototype.set_HP = function (value) {

	if (value <= 0) {
		this.Die();
	} else {
		this.hp = value;
	}
};

Object.defineProperty(Entities.Player.prototype, "HP", {
		get: Entities.Player.prototype.get_HP, 
		set: Entities.Player.prototype.set_HP
	});
Entities.Player.prototype.input = null;
Entities.Player.prototype.jumpcount = 0;
Entities.Player.prototype.hp = 0;
Entities.Player.prototype.hurtcount = 0;
Entities.Player.prototype.firedelay = 0;
Entities.Player._cctor = function () {
	Object.defineProperty(Entities.Player, "key_C", {
			"value": 0}
	);
	Object.defineProperty(Entities.Player, "key_SPACE", {
			"value": 1}
	);
	Object.defineProperty(Entities.Player, "groundfriction", {
			"value": 0.25999999046325684}
	);
	Object.defineProperty(Entities.Player, "airfriction", {
			"value": 0.016599999740719795}
	);
	Object.defineProperty(Entities.Player, "groundaccel", {
			"value": 0.52999997138977051}
	);
	Object.defineProperty(Entities.Player, "airaccel", {
			"value": 0.20000000298023224}
	);
	Object.defineProperty(Entities.Player, "maxxvelocity", {
			"value": 2.5}
	);
	Object.defineProperty(Entities.Player, "maxyvelocity", {
			"value": 250}
	);
	Object.defineProperty(Entities.Player, "jumpvelocity", {
			"value": -3.3299999237060547}
	);
	Object.defineProperty(Entities.Player, "fire_delay", {
			"value": 12}
	);
	Object.defineProperty(Entities.Player, "jumpheight", {
			"value": 30}
	);
};


Entities.Enemies.Ripper.prototype._ctor = function (e, startx, starty) {
	Entities.Enemies.Enemy.prototype._ctor.call(this, e, e.RipperSprite);
	this.x = startx;
	this.y = starty;
	this.hp = 10;
	this.damage = 8;
	this.vx = 1;
	this.direction = Entities.Dir.right;
	this.anim.Set(2, 3, 5, true);
	this.UpdateState = JSIL.Delegate.New("System.Action", this, Entities.Enemies.Ripper.prototype.DoTick);
};

Entities.Enemies.Ripper.prototype.DoTick = function () {

	if (!(!this.touchingleftwall || (this.vx >= 0))) {
		this.vx = 1;
		this.direction = Entities.Dir.right;
		this.anim.Set(2, 3, 5, true);
	}

	if (!(!this.touchingrightwall || (this.vx <= 0))) {
		this.vx = -1;
		this.direction = Entities.Dir.left;
		this.anim.Set(0, 1, 5, true);
	}
};

Entities.Enemies.Ripper._cctor = function () {
	Object.defineProperty(Entities.Enemies.Ripper, "speed", {
			"value": 1}
	);
};


Editor.NumberEditBox.prototype.get_CreateParams = function () {
	var c = System.Windows.Forms.TextBox.prototype.get_CreateParams.call(this);
	c.ClassStyle |= 8192;
	return c;
};

Editor.NumberEditBox.prototype._ctor = function () {
	System.Windows.Forms.TextBox.prototype._ctor.call(this);
};

Object.defineProperty(Editor.NumberEditBox.prototype, "CreateParams", {
		get: Editor.NumberEditBox.prototype.get_CreateParams
	});

Editor.Util.prototype._ctor = function () {
	System.Object.prototype._ctor.call(this);
};


Editor.CopyPasteMode.prototype._ctor = function (e) {
	this.state = Editor.CopyPasteMode.EditState.DoingNothing;
	System.Object.prototype._ctor.call(this);
	this.editor = e;
	this.engine = e.engine;
};

Editor.CopyPasteMode.prototype.MouseDown = function (e, b) {
	var x = new JSIL.Variable(e.X);
	var y = new JSIL.Variable(e.Y);
	this.ScreenToTile(/* ref */ x, /* ref */ y);

	switch (this.state) {
		case 1: 

			if (x.value >= this.p1.X) {
				++x.value;
			}

			if (y.value >= this.p1.Y) {
				++y.value;
			}
			this.p2.X = x.value;
			this.p2.Y = y.value;
			break;
		case 2: 
			this.p1.X = x.value;
			this.p1.Y = y.value;
			this.p2.X = (x.value + this.curselection.GetLength(0));
			this.p2.Y = (y.value + this.curselection.GetLength(1));
			break;
	}
};

Editor.CopyPasteMode.prototype.MouseUp = function (e, b) {

	switch (this.state) {
		case 1: 
			var x = this.p1.X;
			var y = this.p1.Y;
			var x2 = this.p2.X;
			var y2 = this.p2.Y;

			if (x > x2) {
				var i = x;
				x = x2;
				x2 = i;
			}

			if (y > y2) {
				i = y;
				y = y2;
				y2 = i;
			}
			this.curselection = this.Copy(x, y, x2, y2);
			this.state = Editor.CopyPasteMode.EditState.DoingNothing;
			break;
		case 2: 
			var x3 = new JSIL.Variable(e.X);
			var y3 = new JSIL.Variable(e.Y);
			this.ScreenToTile(/* ref */ x3, /* ref */ y3);
			this.Paste(x3.value, y3.value, this.curselection);
			this.state = Editor.CopyPasteMode.EditState.DoingNothing;
			break;
	}
};

Editor.CopyPasteMode.prototype.ScreenToTile = function (/* ref */ x, /* ref */ y) {
	x.value = Math.floor((x.value + this.engine.XWin) / this.engine.tileset.Width);
	y.value = Math.floor((y.value + this.engine.YWin) / this.engine.tileset.Height);
};

Editor.CopyPasteMode.prototype.MouseClick = function (e, b) {

	if ((System.Windows.Forms.Control.ModifierKeys & System.Windows.Forms.Keys.None | System.Windows.Forms.Keys.Shift) !== System.Windows.Forms.Keys.None) {
		var x = new JSIL.Variable(e.X);
		var y = new JSIL.Variable(e.Y);
		this.ScreenToTile(/* ref */ x, /* ref */ y);
		this.p1.X = x.value;
		this.p1.Y = y.value;

		if (this.state === Editor.CopyPasteMode.EditState.Pasting) {
			this.state = Editor.CopyPasteMode.EditState.DoingNothing;
		} else {
			this.state = Editor.CopyPasteMode.EditState.Copying;
			this.p2.X = x.value;
			this.p2.Y = y.value;
		}
	}
};

Editor.CopyPasteMode.prototype.KeyPress = function (e) {
};

Editor.CopyPasteMode.prototype.MouseWheel = function (p, delta) {
};

Editor.CopyPasteMode.prototype.RenderHUD = function () {
};

Editor.CopyPasteMode.prototype.Copy = function (x1, y1, x2, y2) {
	var a = JSIL.MultidimensionalArray.New(System.Int32, (x2 - x1), (y2 - y1));
	var y3 = 0;

__while0__: 
	while (y3 < (y2 - y1)) {
		var x3 = 0;

	__while1__: 
		while (x3 < (x2 - x1)) {
			a.Set(x3, y3, this.engine.map.get_Item(this.editor.tilesetmode.curlayer).get_Item((x3 + x1), (y3 + y1)));
			++x3;
		}
		++y3;
	}
	return a;
};

Editor.CopyPasteMode.prototype.Paste = function (sx, sy, src) {
	var y = 0;

__while0__: 
	while (y < src.GetLength(1)) {
		var x = 0;

	__while1__: 
		while (x < src.GetLength(0)) {

			if ((sx + x) >= this.engine.map.Width) {
				break __while1__;
			}

			if ((sy + y) >= this.engine.map.Height) {
				return ;
			}
			this.engine.map.get_Item(this.editor.tilesetmode.curlayer).set_Item((sx + x), (sy + y), src.Get(x, y));
			++x;
		}
		++y;
	}
};

Editor.CopyPasteMode.prototype.get_Selection = function () {
	return this.curselection;
};

Editor.CopyPasteMode.prototype.set_Selection = function (value) {
	this.curselection = value;
};

Object.defineProperty(Editor.CopyPasteMode.prototype, "Selection", {
		get: Editor.CopyPasteMode.prototype.get_Selection, 
		set: Editor.CopyPasteMode.prototype.set_Selection
	});
Editor.CopyPasteMode.prototype.__ImplementInterface__(Editor.IEditorState);
Editor.CopyPasteMode.prototype.__StructFields__ = {
	p1: Import.Geo.Vertex, 
	p2: Import.Geo.Vertex
};
Editor.CopyPasteMode.prototype.editor = null;
Editor.CopyPasteMode.prototype.engine = null;
Editor.CopyPasteMode.prototype.curselection = null;
Editor.CopyPasteMode.prototype.state = 0;

Input.KeyboardDevice.prototype.Poll = function () {
	this.oldks = this.ks.MemberwiseClone();
	this.ks = Microsoft.Xna.Framework.Input.Keyboard.GetState();

	if (this.ks.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.Left)) {
		this.xAxis = 0;
	} else if (this.ks.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.Right)) {
		this.xAxis = 255;
	} else {
		this.xAxis = 127;
	}

	if (this.ks.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.Up)) {
		this.yAxis = 0;
	} else if (this.ks.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.Down)) {
		this.yAxis = 255;
	} else {
		this.yAxis = 127;
	}
};

Input.KeyboardDevice.prototype.Axis = function (N) {

	switch (N) {
		case 0: 
			var result = this.yAxis;
			break;
		case 1: 
			result = this.xAxis;
			break;
		default: 
			throw new System.NotImplementedException();
	}
	return result;
};

Input.KeyboardDevice.prototype.IsPressed = function (k) {
	return (this.oldks.IsKeyUp(k) && this.ks.IsKeyDown(k));
};

Input.KeyboardDevice.prototype.Button = function (b) {

	switch (b) {
		case 0: 
			var result = this.ks.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.Space);
			break;
		case 1: 
			result = this.IsPressed(Microsoft.Xna.Framework.Input.Keys.LeftControl);
			break;
		case 2: 
			result = this.IsPressed(Microsoft.Xna.Framework.Input.Keys.Escape);
			break;
		default: 
			result = false;
			break;
	}
	return result;
};

Input.KeyboardDevice.prototype._ctor = function () {
	System.Object.prototype._ctor.call(this);
};

Input.KeyboardDevice.prototype.__ImplementInterface__(Input.IInputDevice);
Input.KeyboardDevice.prototype.__StructFields__ = {
	oldks: Microsoft.Xna.Framework.Input.KeyboardState, 
	ks: Microsoft.Xna.Framework.Input.KeyboardState
};
Input.KeyboardDevice.prototype.xAxis = 0;
Input.KeyboardDevice.prototype.yAxis = 0;

Entities.Bullet.prototype._ctor = function (e, startx, starty, d) {
	Entities.Entity.prototype._ctor.call(this, e, e.BulletSprite);
	this.UpdateState = JSIL.Delegate.New("System.Action", this, Entities.Bullet.prototype.CheckCollision);
	this.x = startx;
	this.y = starty;
	this.direction = d;

	switch (d) {
		case 0: 
			this.vx = -6.5999999046325684;
			this.anim.Set(1, 1, 0, false);
			break;
		case 1: 
			this.vx = 6.5999999046325684;
			this.anim.Set(0, 0, 0, false);
			break;
		case 2: 
			this.vy = -6.5999999046325684;
			this.anim.Set(3, 3, 0, false);
			break;
		case 3: 
			this.vy = 6.5999999046325684;
			this.anim.Set(2, 2, 0, false);
			break;
		case 4: 
			this.vx = -6.5999999046325684;
			this.vy = -6.5999999046325684;
			this.anim.Set(6, 6, 0, false);
			break;
		case 5: 
			this.vy = -6.5999999046325684;
			this.vx = 6.5999999046325684;
			this.anim.Set(7, 7, 0, false);
			break;
		case 6: 
			this.vy = 6.5999999046325684;
			this.vx = -6.5999999046325684;
			this.anim.Set(5, 5, 0, false);
			break;
		case 7: 
			this.vx = 6.5999999046325684;
			this.vy = 6.5999999046325684;
			this.anim.Set(4, 4, 0, false);
			break;
	}
};

Entities.Bullet.prototype.CheckCollision = function () {

	if (!(!(this.touchingleftwall || 
				this.touchingrightwall || 
				this.touchingground) && !this.touchingceiling)) {

		if (this.direction === Entities.Dir.left) {
			var nx = (this.x - 8);
		} else {
			nx = this.x;
		}
		this.engine.SpawnEntity(new Entities.Boom(this.engine, nx, (this.y - 4)));
		this.engine.DestroyEntity(this);
	} else {
		var temp = this.engine.DetectCollision(this);

		if (JSIL.CheckType(temp, Entities.Enemies.Enemy)) {
			var ent = JSIL.Cast(temp, Entities.Enemies.Enemy);
			ent.HP -= 5;
			System.Console.WriteLine(System.String.Concat("HP: ", ent.HP));

			if (this.direction === Entities.Dir.left) {
				nx = (this.x - 8);
			} else {
				nx = this.x;
			}
			this.engine.SpawnEntity(new Entities.Boom(this.engine, this.x, (this.y - 4)));
			this.engine.DestroyEntity(this);
		}
	}
};

Entities.Bullet._cctor = function () {
	Object.defineProperty(Entities.Bullet, "velocity", {
			"value": 6.5999999046325684}
	);
};


JSIL.InitializeType(Entities.Entity);
JSIL.InitializeType(Entities.Enemies.Enemy);
JSIL.InitializeType(Entities.Enemies.Hopper);
JSIL.InitializeType(Editor.TileSetMode);
JSIL.InitializeType(Editor.Tileset);
JSIL.InitializeType(Timer);
JSIL.InitializeType(Input.X360Gamepad);
JSIL.InitializeType(Cataract.XNAGraph);
JSIL.InitializeType(Editor.ChangeTileHandler);
JSIL.InitializeType(Editor.TileSetPreview.DoubleBufferedPanel);
JSIL.InitializeType(Editor.TileSetPreview);
JSIL.InitializeType(Editor.MapInfoView);
JSIL.InitializeType(Editor.EntityEditMode);
JSIL.InitializeType(Entities.Enemies.Skree);
JSIL.InitializeType(Import.MannuxMap);
JSIL.InitializeType(AnimState);
JSIL.InitializeType(Mannux.MannuxGame);
JSIL.InitializeType(Editor.Editor);
JSIL.InitializeType(Sprites.BitmapSprite);
JSIL.InitializeType(Import.VectorIndexBuffer);
JSIL.InitializeType(Import.Geo.Vertex);
JSIL.InitializeType(Controller.Resource);
JSIL.InitializeType(Controller);
JSIL.InitializeType(Editor.MapEntPropertiesView);
JSIL.InitializeType(Vector);
JSIL.InitializeType(Input.InputHandler);
JSIL.InitializeType(Entities.Door);
JSIL.InitializeType(Editor.AutoSelectionThing);
JSIL.InitializeType(Sprites.ParticleSprite.Particle);
JSIL.InitializeType(Sprites.ParticleSprite);
JSIL.InitializeType(Entities.Boom);
JSIL.InitializeType(Engine);
JSIL.InitializeType(Editor.ObstructionMode);
JSIL.InitializeType(Import.VectorObstructionMap);
JSIL.InitializeType(Import.Map.Layer);
JSIL.InitializeType(Import.Map);
JSIL.InitializeType(Import.Geo.Line);
JSIL.InitializeType(RLE);
JSIL.InitializeType(Import.v2Map);
JSIL.InitializeType(Mannux.Import.AutoArray$b1);
JSIL.InitializeType(Editor.NewMapDlg);
JSIL.InitializeType(Mannux.Program);
JSIL.InitializeType(Import.MapEnt);
JSIL.InitializeType(Input.MouseDevice);
JSIL.InitializeType(Entities.Player);
JSIL.InitializeType(Entities.Enemies.Ripper);
JSIL.InitializeType(Editor.NumberEditBox);
JSIL.InitializeType(Editor.Util);
JSIL.InitializeType(Editor.CopyPasteMode);
JSIL.InitializeType(Input.KeyboardDevice);
JSIL.InitializeType(Entities.Bullet);
