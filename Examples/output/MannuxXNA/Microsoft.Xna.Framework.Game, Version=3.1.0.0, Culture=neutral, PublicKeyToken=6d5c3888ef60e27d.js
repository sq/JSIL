JSIL.DeclareNamespace(this, "Microsoft");
JSIL.DeclareNamespace(Microsoft, "Xna");
JSIL.DeclareNamespace(Microsoft.Xna, "Framework");
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

JSIL.MakeClass(System.EventArgs, Microsoft.Xna.Framework, "PreparingDeviceSettingsEventArgs", "Microsoft.Xna.Framework.PreparingDeviceSettingsEventArgs");

JSIL.MakeClass(System.Object, Microsoft.Xna.Framework, "Game", "Microsoft.Xna.Framework.Game");

JSIL.MakeClass(System.Object, Microsoft.Xna.Framework, "GameClock", "Microsoft.Xna.Framework.GameClock");

JSIL.MakeClass(System.EventArgs, Microsoft.Xna.Framework, "GameComponentCollectionEventArgs", "Microsoft.Xna.Framework.GameComponentCollectionEventArgs");

JSIL.MakeClass(System.Collections.ObjectModel.Collection$b1.Of(Microsoft.Xna.Framework.IGameComponent), Microsoft.Xna.Framework, "GameComponentCollection", "Microsoft.Xna.Framework.GameComponentCollection");

JSIL.MakeClass(System.Object, Microsoft.Xna.Framework, "GameHost", "Microsoft.Xna.Framework.GameHost");

JSIL.DeclareNamespace(Microsoft.Xna.Framework, "GamerServices");
JSIL.MakeClass(Microsoft.Xna.Framework.GameComponent, Microsoft.Xna.Framework.GamerServices, "GamerServicesComponent", "Microsoft.Xna.Framework.GamerServices.GamerServicesComponent");

JSIL.MakeClass(System.Object, Microsoft.Xna.Framework, "GameServiceContainer", "Microsoft.Xna.Framework.GameServiceContainer");

JSIL.MakeClass(System.Object, Microsoft.Xna.Framework, "GameTime", "Microsoft.Xna.Framework.GameTime");

JSIL.MakeClass(System.Object, Microsoft.Xna.Framework, "GameWindow", "Microsoft.Xna.Framework.GameWindow");

JSIL.MakeClass(System.Object, Microsoft.Xna.Framework, "GraphicsDeviceInformation", "Microsoft.Xna.Framework.GraphicsDeviceInformation");

JSIL.MakeClass(System.Object, Microsoft.Xna.Framework, "GraphicsDeviceInformationComparer", "Microsoft.Xna.Framework.GraphicsDeviceInformationComparer");

JSIL.MakeInterface(
	Microsoft.Xna.Framework, "IGraphicsDeviceManager", "Microsoft.Xna.Framework.IGraphicsDeviceManager", {
		"CreateDevice": Function, 
		"BeginDraw": Function, 
		"EndDraw": Function
	});

JSIL.MakeClass(System.Object, Microsoft.Xna.Framework, "GraphicsDeviceManager", "Microsoft.Xna.Framework.GraphicsDeviceManager");

JSIL.MakeClass(System.ApplicationException, Microsoft.Xna.Framework, "NoSuitableGraphicsDeviceException", "Microsoft.Xna.Framework.NoSuitableGraphicsDeviceException");

JSIL.MakeClass(System.Object, Microsoft.Xna.Framework, "DrawOrderComparer", "Microsoft.Xna.Framework.DrawOrderComparer");

JSIL.MakeClass(System.Object, Microsoft.Xna.Framework, "UpdateOrderComparer", "Microsoft.Xna.Framework.UpdateOrderComparer");

JSIL.MakeClass(System.Object, Microsoft.Xna.Framework, "NativeMethods", "Microsoft.Xna.Framework.NativeMethods");
JSIL.MakeEnum(
	Microsoft.Xna.Framework.NativeMethods, "WindowMessage", "Microsoft.Xna.Framework.NativeMethods/WindowMessage", {
		Destroy: 2, 
		Close: 16, 
		Quit: 18, 
		Paint: 15, 
		SetCursor: 32, 
		ActivateApplication: 28, 
		EnterMenuLoop: 529, 
		ExitMenuLoop: 530, 
		NonClientHitTest: 132, 
		PowerBroadcast: 536, 
		SystemCommand: 274, 
		GetMinMax: 36, 
		KeyDown: 256, 
		KeyUp: 257, 
		Character: 258, 
		SystemKeyDown: 260, 
		SystemKeyUp: 261, 
		SystemCharacter: 262, 
		MouseMove: 512, 
		LeftButtonDown: 513, 
		LeftButtonUp: 514, 
		LeftButtonDoubleClick: 515, 
		RightButtonDown: 516, 
		RightButtonUp: 517, 
		RightButtonDoubleClick: 518, 
		MiddleButtonDown: 519, 
		MiddleButtonUp: 520, 
		MiddleButtonDoubleClick: 521, 
		MouseWheel: 522, 
		XButtonDown: 523, 
		XButtonUp: 524, 
		XButtonDoubleClick: 525, 
		MouseFirst: 513, 
		MouseLast: 525, 
		EnterSizeMove: 561, 
		ExitSizeMove: 562, 
		Size: 5
	}, false
);

JSIL.MakeEnum(
	Microsoft.Xna.Framework.NativeMethods, "MouseButtons", "Microsoft.Xna.Framework.NativeMethods/MouseButtons", {
		Left: 1, 
		Right: 2, 
		Middle: 16, 
		Side1: 32, 
		Side2: 64
	}, false
);

JSIL.MakeStruct(Microsoft.Xna.Framework.NativeMethods, "Message", "Microsoft.Xna.Framework.NativeMethods/Message");

JSIL.MakeStruct(Microsoft.Xna.Framework.NativeMethods, "MinMaxInformation", "Microsoft.Xna.Framework.NativeMethods/MinMaxInformation");

JSIL.MakeStruct(Microsoft.Xna.Framework.NativeMethods, "MonitorInformation", "Microsoft.Xna.Framework.NativeMethods/MonitorInformation");

JSIL.MakeStruct(Microsoft.Xna.Framework.NativeMethods, "RECT", "Microsoft.Xna.Framework.NativeMethods/RECT");

JSIL.MakeStruct(Microsoft.Xna.Framework.NativeMethods, "POINT", "Microsoft.Xna.Framework.NativeMethods/POINT");


JSIL.MakeClass(System.Object, Microsoft.Xna.Framework, "Resources", "Microsoft.Xna.Framework.Resources");

JSIL.MakeClass(System.Windows.Forms.Form, Microsoft.Xna.Framework, "WindowsGameForm", "Microsoft.Xna.Framework.WindowsGameForm");

JSIL.MakeClass(Microsoft.Xna.Framework.GameHost, Microsoft.Xna.Framework, "WindowsGameHost", "Microsoft.Xna.Framework.WindowsGameHost");

JSIL.MakeClass(Microsoft.Xna.Framework.GameWindow, Microsoft.Xna.Framework, "WindowsGameWindow", "Microsoft.Xna.Framework.WindowsGameWindow");

Microsoft.Xna.Framework.GameComponent.prototype.enabled = new System.Boolean();
Microsoft.Xna.Framework.GameComponent.prototype.updateOrder = 0;
Microsoft.Xna.Framework.GameComponent.prototype.game = null;
Microsoft.Xna.Framework.GameComponent.prototype.EnabledChanged = null;
Microsoft.Xna.Framework.GameComponent.prototype.UpdateOrderChanged = null;
Microsoft.Xna.Framework.GameComponent.prototype.Disposed = null;
Microsoft.Xna.Framework.GameComponent.prototype.get_Enabled = function () {
	return this.enabled;
};

Microsoft.Xna.Framework.GameComponent.prototype.set_Enabled = function (value) {

	if (this.enabled !== value) {
		this.enabled = value;
		this.OnEnabledChanged(this, System.EventArgs.Empty);
	}
};

Microsoft.Xna.Framework.GameComponent.prototype.get_UpdateOrder = function () {
	return this.updateOrder;
};

Microsoft.Xna.Framework.GameComponent.prototype.set_UpdateOrder = function (value) {

	if (this.updateOrder !== value) {
		this.updateOrder = value;
		this.OnUpdateOrderChanged(this, System.EventArgs.Empty);
	}
};

Microsoft.Xna.Framework.GameComponent.prototype.get_Game = function () {
	return this.game;
};

Microsoft.Xna.Framework.GameComponent.prototype.add_EnabledChanged = function (value) {
	this.EnabledChanged = System.Delegate.Combine(this.EnabledChanged, value);
};

Microsoft.Xna.Framework.GameComponent.prototype.remove_EnabledChanged = function (value) {
	this.EnabledChanged = System.Delegate.Remove(this.EnabledChanged, value);
};

Microsoft.Xna.Framework.GameComponent.prototype.add_UpdateOrderChanged = function (value) {
	this.UpdateOrderChanged = System.Delegate.Combine(this.UpdateOrderChanged, value);
};

Microsoft.Xna.Framework.GameComponent.prototype.remove_UpdateOrderChanged = function (value) {
	this.UpdateOrderChanged = System.Delegate.Remove(this.UpdateOrderChanged, value);
};

Microsoft.Xna.Framework.GameComponent.prototype.add_Disposed = function (value) {
	this.Disposed = System.Delegate.Combine(this.Disposed, value);
};

Microsoft.Xna.Framework.GameComponent.prototype.remove_Disposed = function (value) {
	this.Disposed = System.Delegate.Remove(this.Disposed, value);
};

Microsoft.Xna.Framework.GameComponent.prototype._ctor = function (game) {
	this.enabled = true;
	System.Object.prototype._ctor.call(this);
	this.game = game;
};

Microsoft.Xna.Framework.GameComponent.prototype.Initialize = function () {
};

Microsoft.Xna.Framework.GameComponent.prototype.Update = function (gameTime) {
};

Microsoft.Xna.Framework.GameComponent.prototype.Dispose$0 = function () {
	this.Dispose(true);
	System.GC.SuppressFinalize(this);
};

Microsoft.Xna.Framework.GameComponent.prototype.Finalize = function () {

	try {
		this.Dispose(false);
	} finally {
		System.Object.prototype.Finalize.call(this);
	}
};

Microsoft.Xna.Framework.GameComponent.prototype.Dispose$1 = function (disposing) {

	if (disposing) {
		System.Threading.Monitor.Enter(this);

		try {

			if (this.Game === null) {
				this.Game.Components.Remove(this);
			}

			if (this.Disposed === null) {
				this.Disposed(this, System.EventArgs.Empty);
			}
		} finally {
			System.Threading.Monitor.Exit(this);
		}
	}
};

Microsoft.Xna.Framework.GameComponent.prototype.OnUpdateOrderChanged = function (sender, args) {

	if (this.UpdateOrderChanged === null) {
		this.UpdateOrderChanged(this, args);
	}
};

Microsoft.Xna.Framework.GameComponent.prototype.OnEnabledChanged = function (sender, args) {

	if (this.EnabledChanged === null) {
		this.EnabledChanged(this, args);
	}
};

JSIL.OverloadedMethod(Microsoft.Xna.Framework.GameComponent.prototype, "Dispose", [
		["Dispose$0", []], 
		["Dispose$1", [System.Boolean]]
	]
);
Object.defineProperty(Microsoft.Xna.Framework.GameComponent.prototype, "Enabled", {
		get: Microsoft.Xna.Framework.GameComponent.prototype.get_Enabled, 
		set: Microsoft.Xna.Framework.GameComponent.prototype.set_Enabled
	});
Object.defineProperty(Microsoft.Xna.Framework.GameComponent.prototype, "UpdateOrder", {
		get: Microsoft.Xna.Framework.GameComponent.prototype.get_UpdateOrder, 
		set: Microsoft.Xna.Framework.GameComponent.prototype.set_UpdateOrder
	});
Object.defineProperty(Microsoft.Xna.Framework.GameComponent.prototype, "Game", {
		get: Microsoft.Xna.Framework.GameComponent.prototype.get_Game
	});
Microsoft.Xna.Framework.GameComponent.prototype.__ImplementInterface__(Microsoft.Xna.Framework.IGameComponent);
Microsoft.Xna.Framework.GameComponent.prototype.__ImplementInterface__(Microsoft.Xna.Framework.IUpdateable);
Microsoft.Xna.Framework.GameComponent.prototype.__ImplementInterface__(System.IDisposable);

Object.seal(Microsoft.Xna.Framework.GameComponent.prototype);
Object.seal(Microsoft.Xna.Framework.GameComponent);
Microsoft.Xna.Framework.DrawableGameComponent.prototype.initialized = new System.Boolean();
Microsoft.Xna.Framework.DrawableGameComponent.prototype.visible = new System.Boolean();
Microsoft.Xna.Framework.DrawableGameComponent.prototype.drawOrder = 0;
Microsoft.Xna.Framework.DrawableGameComponent.prototype.deviceService = null;
Microsoft.Xna.Framework.DrawableGameComponent.prototype.VisibleChanged = null;
Microsoft.Xna.Framework.DrawableGameComponent.prototype.DrawOrderChanged = null;
Microsoft.Xna.Framework.DrawableGameComponent.prototype.get_Visible = function () {
	return this.visible;
};

Microsoft.Xna.Framework.DrawableGameComponent.prototype.set_Visible = function (value) {

	if (this.visible !== value) {
		this.visible = value;
		this.OnVisibleChanged(this, System.EventArgs.Empty);
	}
};

Microsoft.Xna.Framework.DrawableGameComponent.prototype.get_DrawOrder = function () {
	return this.drawOrder;
};

Microsoft.Xna.Framework.DrawableGameComponent.prototype.set_DrawOrder = function (value) {

	if (this.drawOrder !== value) {
		this.drawOrder = value;
		this.OnDrawOrderChanged(this, System.EventArgs.Empty);
	}
};

Microsoft.Xna.Framework.DrawableGameComponent.prototype.get_GraphicsDevice = function () {

	if (this.deviceService !== null) {
		throw new System.InvalidOperationException(Microsoft.Xna.Framework.Resources.PropertyCannotBeCalledBeforeInitialize);
	}
	return this.deviceService.IGraphicsDeviceService_GraphicsDevice;
};

Microsoft.Xna.Framework.DrawableGameComponent.prototype.add_VisibleChanged = function (value) {
	this.VisibleChanged = System.Delegate.Combine(this.VisibleChanged, value);
};

Microsoft.Xna.Framework.DrawableGameComponent.prototype.remove_VisibleChanged = function (value) {
	this.VisibleChanged = System.Delegate.Remove(this.VisibleChanged, value);
};

Microsoft.Xna.Framework.DrawableGameComponent.prototype.add_DrawOrderChanged = function (value) {
	this.DrawOrderChanged = System.Delegate.Combine(this.DrawOrderChanged, value);
};

Microsoft.Xna.Framework.DrawableGameComponent.prototype.remove_DrawOrderChanged = function (value) {
	this.DrawOrderChanged = System.Delegate.Remove(this.DrawOrderChanged, value);
};

Microsoft.Xna.Framework.DrawableGameComponent.prototype._ctor = function (game) {
	this.visible = true;
	Microsoft.Xna.Framework.GameComponent.prototype._ctor.call(this, game);
};

Microsoft.Xna.Framework.DrawableGameComponent.prototype.Initialize = function () {
	Microsoft.Xna.Framework.GameComponent.prototype.Initialize.call(this);

	if (!this.initialized) {
		this.deviceService = JSIL.TryCast(this.Game.Services.GetService(Microsoft.Xna.Framework.Graphics.IGraphicsDeviceService), Microsoft.Xna.Framework.Graphics.IGraphicsDeviceService);

		if (this.deviceService !== null) {
			throw new System.InvalidOperationException(Microsoft.Xna.Framework.Resources.MissingGraphicsDeviceService);
		}
		this.deviceService.IGraphicsDeviceService_add_DeviceCreated(JSIL.Delegate.New("System.EventHandler", this, Microsoft.Xna.Framework.DrawableGameComponent.prototype.DeviceCreated));
		this.deviceService.IGraphicsDeviceService_add_DeviceResetting(JSIL.Delegate.New("System.EventHandler", this, Microsoft.Xna.Framework.DrawableGameComponent.prototype.DeviceResetting));
		this.deviceService.IGraphicsDeviceService_add_DeviceReset(JSIL.Delegate.New("System.EventHandler", this, Microsoft.Xna.Framework.DrawableGameComponent.prototype.DeviceReset));
		this.deviceService.IGraphicsDeviceService_add_DeviceDisposing(JSIL.Delegate.New("System.EventHandler", this, Microsoft.Xna.Framework.DrawableGameComponent.prototype.DeviceDisposing));

		if (this.deviceService.IGraphicsDeviceService_GraphicsDevice === null) {
			this.LoadGraphicsContent(true);
			this.LoadContent();
		}
	}
	this.initialized = true;
};

Microsoft.Xna.Framework.DrawableGameComponent.prototype.Dispose = function (disposing) {

	if (disposing) {
		this.UnloadGraphicsContent(true);
		this.UnloadContent();

		if (this.deviceService === null) {
			this.deviceService.IGraphicsDeviceService_remove_DeviceCreated(JSIL.Delegate.New("System.EventHandler", this, Microsoft.Xna.Framework.DrawableGameComponent.prototype.DeviceCreated));
			this.deviceService.IGraphicsDeviceService_remove_DeviceResetting(JSIL.Delegate.New("System.EventHandler", this, Microsoft.Xna.Framework.DrawableGameComponent.prototype.DeviceResetting));
			this.deviceService.IGraphicsDeviceService_remove_DeviceReset(JSIL.Delegate.New("System.EventHandler", this, Microsoft.Xna.Framework.DrawableGameComponent.prototype.DeviceReset));
			this.deviceService.IGraphicsDeviceService_remove_DeviceDisposing(JSIL.Delegate.New("System.EventHandler", this, Microsoft.Xna.Framework.DrawableGameComponent.prototype.DeviceDisposing));
		}
	}
	Microsoft.Xna.Framework.GameComponent.prototype.Dispose.call(this, disposing);
};

Microsoft.Xna.Framework.DrawableGameComponent.prototype.DeviceResetting = function (sender, e) {
	this.UnloadGraphicsContent(false);
};

Microsoft.Xna.Framework.DrawableGameComponent.prototype.DeviceReset = function (sender, e) {
	this.LoadGraphicsContent(false);
};

Microsoft.Xna.Framework.DrawableGameComponent.prototype.DeviceCreated = function (sender, e) {
	this.LoadGraphicsContent(true);
	this.LoadContent();
};

Microsoft.Xna.Framework.DrawableGameComponent.prototype.DeviceDisposing = function (sender, e) {
	this.UnloadGraphicsContent(true);
	this.UnloadContent();
};

Microsoft.Xna.Framework.DrawableGameComponent.prototype.Draw = function (gameTime) {
};

Microsoft.Xna.Framework.DrawableGameComponent.prototype.LoadGraphicsContent = function (loadAllContent) {
};

Microsoft.Xna.Framework.DrawableGameComponent.prototype.UnloadGraphicsContent = function (unloadAllContent) {
};

Microsoft.Xna.Framework.DrawableGameComponent.prototype.LoadContent = function () {
};

Microsoft.Xna.Framework.DrawableGameComponent.prototype.UnloadContent = function () {
};

Microsoft.Xna.Framework.DrawableGameComponent.prototype.OnDrawOrderChanged = function (sender, args) {

	if (this.DrawOrderChanged === null) {
		this.DrawOrderChanged(this, args);
	}
};

Microsoft.Xna.Framework.DrawableGameComponent.prototype.OnVisibleChanged = function (sender, args) {

	if (this.VisibleChanged === null) {
		this.VisibleChanged(this, args);
	}
};

Object.defineProperty(Microsoft.Xna.Framework.DrawableGameComponent.prototype, "Visible", {
		get: Microsoft.Xna.Framework.DrawableGameComponent.prototype.get_Visible, 
		set: Microsoft.Xna.Framework.DrawableGameComponent.prototype.set_Visible
	});
Object.defineProperty(Microsoft.Xna.Framework.DrawableGameComponent.prototype, "DrawOrder", {
		get: Microsoft.Xna.Framework.DrawableGameComponent.prototype.get_DrawOrder, 
		set: Microsoft.Xna.Framework.DrawableGameComponent.prototype.set_DrawOrder
	});
Object.defineProperty(Microsoft.Xna.Framework.DrawableGameComponent.prototype, "GraphicsDevice", {
		get: Microsoft.Xna.Framework.DrawableGameComponent.prototype.get_GraphicsDevice
	});
Microsoft.Xna.Framework.DrawableGameComponent.prototype.__ImplementInterface__(Microsoft.Xna.Framework.IDrawable);

Object.seal(Microsoft.Xna.Framework.DrawableGameComponent.prototype);
Object.seal(Microsoft.Xna.Framework.DrawableGameComponent);
Microsoft.Xna.Framework.PreparingDeviceSettingsEventArgs.prototype.graphicsDeviceInformation = null;
Microsoft.Xna.Framework.PreparingDeviceSettingsEventArgs.prototype.get_GraphicsDeviceInformation = function () {
	return this.graphicsDeviceInformation;
};

Microsoft.Xna.Framework.PreparingDeviceSettingsEventArgs.prototype._ctor = function (graphicsDeviceInformation) {
	System.EventArgs.prototype._ctor.call(this);
	this.graphicsDeviceInformation = graphicsDeviceInformation;
};

Object.defineProperty(Microsoft.Xna.Framework.PreparingDeviceSettingsEventArgs.prototype, "GraphicsDeviceInformation", {
		get: Microsoft.Xna.Framework.PreparingDeviceSettingsEventArgs.prototype.get_GraphicsDeviceInformation
	});

Object.seal(Microsoft.Xna.Framework.PreparingDeviceSettingsEventArgs.prototype);
Object.seal(Microsoft.Xna.Framework.PreparingDeviceSettingsEventArgs);
Microsoft.Xna.Framework.Game.prototype.graphicsDeviceManager = null;
Microsoft.Xna.Framework.Game.prototype.graphicsDeviceService = null;
Microsoft.Xna.Framework.Game.prototype.host = null;
Microsoft.Xna.Framework.Game.prototype.isActive = new System.Boolean();
Microsoft.Xna.Framework.Game.prototype.exitRequested = new System.Boolean();
Microsoft.Xna.Framework.Game.prototype.isMouseVisible = new System.Boolean();
Microsoft.Xna.Framework.Game.prototype.inRun = new System.Boolean();
Microsoft.Xna.Framework.Game.prototype.gameTime = null;
Microsoft.Xna.Framework.Game.prototype.clock = null;
Microsoft.Xna.Framework.Game.prototype.isFixedTimeStep = new System.Boolean();
Microsoft.Xna.Framework.Game.prototype.drawRunningSlowly = new System.Boolean();
Microsoft.Xna.Framework.Game.prototype.updatesSinceRunningSlowly1 = 0;
Microsoft.Xna.Framework.Game.prototype.updatesSinceRunningSlowly2 = 0;
Microsoft.Xna.Framework.Game.prototype.doneFirstUpdate = new System.Boolean();
Microsoft.Xna.Framework.Game.prototype.doneFirstDraw = new System.Boolean();
Microsoft.Xna.Framework.Game.prototype.forceElapsedTimeToZero = new System.Boolean();
Microsoft.Xna.Framework.Game.prototype.suppressDraw = new System.Boolean();
Microsoft.Xna.Framework.Game.prototype.gameComponents = null;
Microsoft.Xna.Framework.Game.prototype.updateableComponents = null;
Microsoft.Xna.Framework.Game.prototype.currentlyUpdatingComponents = null;
Microsoft.Xna.Framework.Game.prototype.drawableComponents = null;
Microsoft.Xna.Framework.Game.prototype.currentlyDrawingComponents = null;
Microsoft.Xna.Framework.Game.prototype.notYetInitialized = null;
Microsoft.Xna.Framework.Game.prototype.gameServices = null;
Microsoft.Xna.Framework.Game.prototype.content = null;
Microsoft.Xna.Framework.Game.prototype.Activated = null;
Microsoft.Xna.Framework.Game.prototype.Deactivated = null;
Microsoft.Xna.Framework.Game.prototype.Exiting = null;
Microsoft.Xna.Framework.Game.prototype.Disposed = null;
Microsoft.Xna.Framework.Game.prototype.__StructFields__ = {
	maximumElapsedTime: System.TimeSpan, 
	inactiveSleepTime: System.TimeSpan, 
	lastFrameElapsedRealTime: System.TimeSpan, 
	totalGameTime: System.TimeSpan, 
	targetElapsedTime: System.TimeSpan, 
	accumulatedElapsedGameTime: System.TimeSpan, 
	lastFrameElapsedGameTime: System.TimeSpan
};
Microsoft.Xna.Framework.Game.prototype.get_Components = function () {
	return this.gameComponents;
};

Microsoft.Xna.Framework.Game.prototype.get_Services = function () {
	return this.gameServices;
};

Microsoft.Xna.Framework.Game.prototype.get_InactiveSleepTime = function () {
	return this.inactiveSleepTime;
};

Microsoft.Xna.Framework.Game.prototype.set_InactiveSleepTime = function (value) {

	if (System.TimeSpan.op_LessThan(value.MemberwiseClone(), System.TimeSpan.Zero.MemberwiseClone())) {
		throw new System.ArgumentOutOfRangeException("value", Microsoft.Xna.Framework.Resources.InactiveSleepTimeCannotBeZero);
	}
	this.inactiveSleepTime = value.MemberwiseClone();
};

Microsoft.Xna.Framework.Game.prototype.get_IsMouseVisible = function () {
	return this.isMouseVisible;
};

Microsoft.Xna.Framework.Game.prototype.set_IsMouseVisible = function (value) {
	this.isMouseVisible = value;

	if (this.get_Window() === null) {
		this.get_Window().IsMouseVisible = value;
	}
};

Microsoft.Xna.Framework.Game.prototype.get_TargetElapsedTime = function () {
	return this.targetElapsedTime;
};

Microsoft.Xna.Framework.Game.prototype.set_TargetElapsedTime = function (value) {

	if (System.TimeSpan.op_LessThanOrEqual(value.MemberwiseClone(), System.TimeSpan.Zero.MemberwiseClone())) {
		throw new System.ArgumentOutOfRangeException("value", Microsoft.Xna.Framework.Resources.TargetElaspedCannotBeZero);
	}
	this.targetElapsedTime = value.MemberwiseClone();
};

Microsoft.Xna.Framework.Game.prototype.get_IsFixedTimeStep = function () {
	return this.isFixedTimeStep;
};

Microsoft.Xna.Framework.Game.prototype.set_IsFixedTimeStep = function (value) {
	this.isFixedTimeStep = value;
};

Microsoft.Xna.Framework.Game.prototype.get_Window = function () {

	if (this.host === null) {
		return this.host.Window;
	}
	return null;
};

Microsoft.Xna.Framework.Game.prototype.get_IsActive = function () {
	var flag = false;

	if (Microsoft.Xna.Framework.GamerServices.GamerServicesDispatcher.IsInitialized) {
		flag = Microsoft.Xna.Framework.GamerServices.Guide.IsVisible;
	}
	return (this.isActive && !flag);
};

Microsoft.Xna.Framework.Game.prototype.get_GraphicsDevice = function () {
	var graphicsDeviceService = this.graphicsDeviceService;

	if (graphicsDeviceService !== null) {
		graphicsDeviceService = JSIL.TryCast(this.get_Services().GetService(Microsoft.Xna.Framework.Graphics.IGraphicsDeviceService), Microsoft.Xna.Framework.Graphics.IGraphicsDeviceService);

		if (graphicsDeviceService !== null) {
			throw new System.InvalidOperationException(Microsoft.Xna.Framework.Resources.NoGraphicsDeviceService);
		}
	}
	return graphicsDeviceService.IGraphicsDeviceService_GraphicsDevice;
};

Microsoft.Xna.Framework.Game.prototype.get_Content = function () {
	return this.content;
};

Microsoft.Xna.Framework.Game.prototype.set_Content = function (value) {

	if (value !== null) {
		throw new System.ArgumentNullException();
	}
	this.content = value;
};

Microsoft.Xna.Framework.Game.prototype.get_IsActiveIgnoringGuide = function () {
	return this.isActive;
};

Microsoft.Xna.Framework.Game.prototype.add_Activated = function (value) {
	this.Activated = System.Delegate.Combine(this.Activated, value);
};

Microsoft.Xna.Framework.Game.prototype.remove_Activated = function (value) {
	this.Activated = System.Delegate.Remove(this.Activated, value);
};

Microsoft.Xna.Framework.Game.prototype.add_Deactivated = function (value) {
	this.Deactivated = System.Delegate.Combine(this.Deactivated, value);
};

Microsoft.Xna.Framework.Game.prototype.remove_Deactivated = function (value) {
	this.Deactivated = System.Delegate.Remove(this.Deactivated, value);
};

Microsoft.Xna.Framework.Game.prototype.add_Exiting = function (value) {
	this.Exiting = System.Delegate.Combine(this.Exiting, value);
};

Microsoft.Xna.Framework.Game.prototype.remove_Exiting = function (value) {
	this.Exiting = System.Delegate.Remove(this.Exiting, value);
};

Microsoft.Xna.Framework.Game.prototype.add_Disposed = function (value) {
	this.Disposed = System.Delegate.Combine(this.Disposed, value);
};

Microsoft.Xna.Framework.Game.prototype.remove_Disposed = function (value) {
	this.Disposed = System.Delegate.Remove(this.Disposed, value);
};

Microsoft.Xna.Framework.Game.prototype._ctor = function () {
	this.maximumElapsedTime = System.TimeSpan.FromMilliseconds(500);
	this.gameTime = new Microsoft.Xna.Framework.GameTime();
	this.isFixedTimeStep = true;
	this.updatesSinceRunningSlowly1 = 2147483647;
	this.updatesSinceRunningSlowly2 = 2147483647;
	this.updateableComponents = new (System.Collections.Generic.List$b1.Of(Microsoft.Xna.Framework.IUpdateable)) ();
	this.currentlyUpdatingComponents = new (System.Collections.Generic.List$b1.Of(Microsoft.Xna.Framework.IUpdateable)) ();
	this.drawableComponents = new (System.Collections.Generic.List$b1.Of(Microsoft.Xna.Framework.IDrawable)) ();
	this.currentlyDrawingComponents = new (System.Collections.Generic.List$b1.Of(Microsoft.Xna.Framework.IDrawable)) ();
	this.notYetInitialized = new (System.Collections.Generic.List$b1.Of(Microsoft.Xna.Framework.IGameComponent)) ();
	this.gameServices = new Microsoft.Xna.Framework.GameServiceContainer();
	System.Object.prototype._ctor.call(this);
	this.EnsureHost();
	this.gameComponents = new Microsoft.Xna.Framework.GameComponentCollection();
	this.gameComponents.add_ComponentAdded(JSIL.Delegate.New("System.EventHandler`1[Microsoft.Xna.Framework.GameComponentCollectionEventArgs]", this, Microsoft.Xna.Framework.Game.prototype.GameComponentAdded));
	this.gameComponents.add_ComponentRemoved(JSIL.Delegate.New("System.EventHandler`1[Microsoft.Xna.Framework.GameComponentCollectionEventArgs]", this, Microsoft.Xna.Framework.Game.prototype.GameComponentRemoved));
	this.content = new Microsoft.Xna.Framework.Content.ContentManager(this.gameServices);
	this.host.Window.add_Paint(JSIL.Delegate.New("System.EventHandler", this, Microsoft.Xna.Framework.Game.prototype.Paint));
	this.clock = new Microsoft.Xna.Framework.GameClock();
	this.totalGameTime = System.TimeSpan.Zero.MemberwiseClone();
	this.accumulatedElapsedGameTime = System.TimeSpan.Zero.MemberwiseClone();
	this.lastFrameElapsedGameTime = System.TimeSpan.Zero.MemberwiseClone();
	this.targetElapsedTime = System.TimeSpan.FromTicks(166667);
	this.inactiveSleepTime = System.TimeSpan.FromMilliseconds(20);
};

Microsoft.Xna.Framework.Game.prototype.Run = function () {

	try {

		try {
			this.graphicsDeviceManager = JSIL.TryCast(this.get_Services().GetService(Microsoft.Xna.Framework.IGraphicsDeviceManager), Microsoft.Xna.Framework.IGraphicsDeviceManager);

			if (this.graphicsDeviceManager === null) {
				this.graphicsDeviceManager.IGraphicsDeviceManager_CreateDevice();
			}
			this.Initialize();
			this.inRun = true;
			this.BeginRun();
			this.gameTime.ElapsedGameTime = System.TimeSpan.Zero.MemberwiseClone();
			this.gameTime.ElapsedRealTime = System.TimeSpan.Zero.MemberwiseClone();
			this.gameTime.TotalGameTime = this.totalGameTime.MemberwiseClone();
			this.gameTime.TotalRealTime = this.clock.CurrentTime;
			this.gameTime.IsRunningSlowly = false;
			this.Update(this.gameTime);
			this.doneFirstUpdate = true;

			if (this.host === null) {
				this.host.Run();
			}
			this.EndRun();
		} catch ($exception) {

			if (JSIL.CheckType($exception, Microsoft.Xna.Framework.NoSuitableGraphicsDeviceException)) {
				var arg_C7_0 = $exception;
				var exception = arg_C7_0;

				if (!this.ShowMissingRequirementMessage(exception)) {
					throw $exception;
				}
			} else if (JSIL.CheckType($exception, Microsoft.Xna.Framework.Audio.NoAudioHardwareException)) {
				var arg_D5_0 = $exception;
				var exception2 = arg_D5_0;

				if (!this.ShowMissingRequirementMessage(exception2)) {
					throw $exception;
				}
			} else {
				throw $exception;
			}
		}
	} finally {
		this.inRun = false;
	}
};

Microsoft.Xna.Framework.Game.prototype.Tick = function () {

	if (this.get_ShouldExit()) {
		return ;
	}

	if (!this.isActive) {
		System.Threading.Thread.Sleep(JSIL.Cast(this.inactiveSleepTime.TotalMilliseconds, System.Int32));
	}
	this.clock.Step();
	var flag = true;
	this.gameTime.TotalRealTime = this.clock.CurrentTime;
	this.gameTime.ElapsedRealTime = this.clock.ElapsedTime;
	this.lastFrameElapsedRealTime = System.TimeSpan.op_Addition(this.lastFrameElapsedRealTime.MemberwiseClone(), this.clock.ElapsedTime);
	var timeSpan = this.clock.ElapsedAdjustedTime;

	if (System.TimeSpan.op_LessThan(timeSpan.MemberwiseClone(), System.TimeSpan.Zero.MemberwiseClone())) {
		timeSpan = System.TimeSpan.Zero.MemberwiseClone();
	}

	if (this.forceElapsedTimeToZero) {
		this.gameTime.ElapsedRealTime = this.lastFrameElapsedRealTime = timeSpan = System.TimeSpan.Zero.MemberwiseClone().MemberwiseClone().MemberwiseClone();
		this.forceElapsedTimeToZero = false;
	}

	if (System.TimeSpan.op_GreaterThan(timeSpan.MemberwiseClone(), this.maximumElapsedTime.MemberwiseClone())) {
		timeSpan = this.maximumElapsedTime.MemberwiseClone();
	}

	if (this.isFixedTimeStep) {

		if (System.Math.Abs((timeSpan.Ticks - this.targetElapsedTime.Ticks)) < (this.targetElapsedTime.Ticks >> 6)) {
			timeSpan = this.targetElapsedTime.MemberwiseClone();
		}
		this.accumulatedElapsedGameTime = System.TimeSpan.op_Addition(this.accumulatedElapsedGameTime.MemberwiseClone(), timeSpan.MemberwiseClone());
		var num = Math.floor(this.accumulatedElapsedGameTime.Ticks / this.targetElapsedTime.Ticks);
		this.accumulatedElapsedGameTime = System.TimeSpan.FromTicks((this.accumulatedElapsedGameTime.Ticks % this.targetElapsedTime.Ticks));
		this.lastFrameElapsedGameTime = System.TimeSpan.Zero.MemberwiseClone();

		if (num === 0) {
			return ;
		}
		var timeSpan2 = this.targetElapsedTime.MemberwiseClone();

		if (num > 1) {
			this.updatesSinceRunningSlowly2 = this.updatesSinceRunningSlowly1;
			this.updatesSinceRunningSlowly1 = 0;
		} else {

			if (this.updatesSinceRunningSlowly1 < 2147483647) {
				++this.updatesSinceRunningSlowly1;
			}

			if (this.updatesSinceRunningSlowly2 < 2147483647) {
				++this.updatesSinceRunningSlowly2;
			}
		}
		this.drawRunningSlowly = (this.updatesSinceRunningSlowly2 < 20);

	__while0__: 
		while (num > 0) {

			if (this.get_ShouldExit()) {
				break __while0__;
			}
			--num;

			try {
				this.gameTime.ElapsedGameTime = timeSpan2.MemberwiseClone();
				this.gameTime.TotalGameTime = this.totalGameTime.MemberwiseClone();
				this.gameTime.IsRunningSlowly = this.drawRunningSlowly;
				this.Update(this.gameTime);
				flag = (flag & this.suppressDraw);
				this.suppressDraw = false;
			} finally {
				this.lastFrameElapsedGameTime = System.TimeSpan.op_Addition(this.lastFrameElapsedGameTime.MemberwiseClone(), timeSpan2.MemberwiseClone());
				this.totalGameTime = System.TimeSpan.op_Addition(this.totalGameTime.MemberwiseClone(), timeSpan2.MemberwiseClone());
			}
		}
	} else {
		var t = timeSpan.MemberwiseClone();
		this.drawRunningSlowly = false;
		this.updatesSinceRunningSlowly1 = 2147483647;
		this.updatesSinceRunningSlowly2 = 2147483647;

		if (!this.get_ShouldExit()) {

			try {
				this.gameTime.ElapsedGameTime = this.lastFrameElapsedGameTime = t.MemberwiseClone().MemberwiseClone();
				this.gameTime.TotalGameTime = this.totalGameTime.MemberwiseClone();
				this.gameTime.IsRunningSlowly = false;
				this.Update(this.gameTime);
				flag = (flag & this.suppressDraw);
				this.suppressDraw = false;
			} finally {
				this.totalGameTime = System.TimeSpan.op_Addition(this.totalGameTime.MemberwiseClone(), t.MemberwiseClone());
			}
		}
	}

	if (!flag) {
		this.DrawFrame();
	}
};

Microsoft.Xna.Framework.Game.prototype.SuppressDraw = function () {
	this.suppressDraw = true;
};

Microsoft.Xna.Framework.Game.prototype.Exit = function () {
	this.exitRequested = true;
	this.host.Exit();
};

Microsoft.Xna.Framework.Game.prototype.BeginRun = function () {
};

Microsoft.Xna.Framework.Game.prototype.EndRun = function () {
};

Microsoft.Xna.Framework.Game.prototype.Update = function (gameTime) {
	var i = 0;

__while0__: 
	while (i < this.updateableComponents.Count) {
		this.currentlyUpdatingComponents.Add(this.updateableComponents.get_Item(i));
		++i;
	}
	var j = 0;

__while1__: 
	while (j < this.currentlyUpdatingComponents.Count) {
		var updateable = this.currentlyUpdatingComponents.get_Item(j);

		if (updateable.IUpdateable_Enabled) {
			updateable.IUpdateable_Update(gameTime);
		}
		++j;
	}
	this.currentlyUpdatingComponents.Clear();
	Microsoft.Xna.Framework.FrameworkDispatcher.Update();
	this.doneFirstUpdate = true;
};

Microsoft.Xna.Framework.Game.prototype.BeginDraw = function () {
	return ((this.graphicsDeviceManager !== null) || this.graphicsDeviceManager.IGraphicsDeviceManager_BeginDraw());
};

Microsoft.Xna.Framework.Game.prototype.Draw = function (gameTime) {
	var i = 0;

__while0__: 
	while (i < this.drawableComponents.Count) {
		this.currentlyDrawingComponents.Add(this.drawableComponents.get_Item(i));
		++i;
	}
	var j = 0;

__while1__: 
	while (j < this.currentlyDrawingComponents.Count) {
		var drawable = this.currentlyDrawingComponents.get_Item(j);

		if (drawable.IDrawable_Visible) {
			drawable.IDrawable_Draw(gameTime);
		}
		++j;
	}
	this.currentlyDrawingComponents.Clear();
};

Microsoft.Xna.Framework.Game.prototype.EndDraw = function () {

	if (this.graphicsDeviceManager === null) {
		this.graphicsDeviceManager.IGraphicsDeviceManager_EndDraw();
	}
};

Microsoft.Xna.Framework.Game.prototype.Paint = function (sender, e) {

	if (!this.doneFirstDraw) {
		return ;
	}
	this.DrawFrame();
};

Microsoft.Xna.Framework.Game.prototype.Initialize = function () {
	this.HookDeviceEvents();

__while0__: 
	while (this.notYetInitialized.Count) {
		this.notYetInitialized.get_Item(0).IGameComponent_Initialize();
		this.notYetInitialized.RemoveAt(0);
	}

	if (!((this.graphicsDeviceService !== null) || (this.graphicsDeviceService.IGraphicsDeviceService_GraphicsDevice !== null))) {
		this.LoadGraphicsContent(true);
		this.LoadContent();
	}
};

Microsoft.Xna.Framework.Game.prototype.ResetElapsedTime = function () {
	this.forceElapsedTimeToZero = true;
	this.drawRunningSlowly = false;
	this.updatesSinceRunningSlowly1 = 2147483647;
	this.updatesSinceRunningSlowly2 = 2147483647;
};

Microsoft.Xna.Framework.Game.prototype.DrawFrame = function () {

	try {

		if (!this.get_ShouldExit()) {

			if (this.doneFirstUpdate) {

				if (!this.get_Window().IsMinimized) {

					if (this.BeginDraw()) {
						this.gameTime.TotalRealTime = this.clock.CurrentTime;
						this.gameTime.ElapsedRealTime = this.lastFrameElapsedRealTime.MemberwiseClone();
						this.gameTime.TotalGameTime = this.totalGameTime.MemberwiseClone();
						this.gameTime.ElapsedGameTime = this.lastFrameElapsedGameTime.MemberwiseClone();
						this.gameTime.IsRunningSlowly = this.drawRunningSlowly;
						this.Draw(this.gameTime);
						this.EndDraw();
						this.doneFirstDraw = true;
					}
				}
			}
		}
	} finally {
		this.lastFrameElapsedRealTime = System.TimeSpan.Zero.MemberwiseClone();
		this.lastFrameElapsedGameTime = System.TimeSpan.Zero.MemberwiseClone();
	}
};

Microsoft.Xna.Framework.Game.prototype.GameComponentRemoved = function (sender, e) {

	if (!this.inRun) {
		this.notYetInitialized.Remove(e.GameComponent);
	}
	var updateable = JSIL.TryCast(e.GameComponent, Microsoft.Xna.Framework.IUpdateable);

	if (updateable === null) {
		this.updateableComponents.Remove(updateable);
		updateable.IUpdateable_remove_UpdateOrderChanged(JSIL.Delegate.New("System.EventHandler", this, Microsoft.Xna.Framework.Game.prototype.UpdateableUpdateOrderChanged));
	}
	var drawable = JSIL.TryCast(e.GameComponent, Microsoft.Xna.Framework.IDrawable);

	if (drawable === null) {
		this.drawableComponents.Remove(drawable);
		drawable.IDrawable_remove_DrawOrderChanged(JSIL.Delegate.New("System.EventHandler", this, Microsoft.Xna.Framework.Game.prototype.DrawableDrawOrderChanged));
	}
};

Microsoft.Xna.Framework.Game.prototype.GameComponentAdded = function (sender, e) {

	if (this.inRun) {
		e.GameComponent.IGameComponent_Initialize();
	} else {
		this.notYetInitialized.Add(e.GameComponent);
	}
	var updateable = JSIL.TryCast(e.GameComponent, Microsoft.Xna.Framework.IUpdateable);

	if (updateable === null) {
		var num = this.updateableComponents.BinarySearch(updateable, Microsoft.Xna.Framework.UpdateOrderComparer.Default);

		if (num < 0) {
			num = ~num;

		__while0__: 
			while ((num < this.updateableComponents.Count) && (this.updateableComponents.get_Item(num).IUpdateable_UpdateOrder === updateable.IUpdateable_UpdateOrder)) {
				++num;
			}
			this.updateableComponents.Insert(num, updateable);
			updateable.IUpdateable_add_UpdateOrderChanged(JSIL.Delegate.New("System.EventHandler", this, Microsoft.Xna.Framework.Game.prototype.UpdateableUpdateOrderChanged));
		}
	}
	var drawable = JSIL.TryCast(e.GameComponent, Microsoft.Xna.Framework.IDrawable);

	if (drawable === null) {
		var num2 = this.drawableComponents.BinarySearch(drawable, Microsoft.Xna.Framework.DrawOrderComparer.Default);

		if (num2 < 0) {
			num2 = ~num2;

		__while1__: 
			while ((num2 < this.drawableComponents.Count) && (this.drawableComponents.get_Item(num2).IDrawable_DrawOrder === drawable.IDrawable_DrawOrder)) {
				++num2;
			}
			this.drawableComponents.Insert(num2, drawable);
			drawable.IDrawable_add_DrawOrderChanged(JSIL.Delegate.New("System.EventHandler", this, Microsoft.Xna.Framework.Game.prototype.DrawableDrawOrderChanged));
		}
	}
};

Microsoft.Xna.Framework.Game.prototype.DrawableDrawOrderChanged = function (sender, e) {
	var drawable = JSIL.TryCast(sender, Microsoft.Xna.Framework.IDrawable);
	this.drawableComponents.Remove(drawable);
	var num = this.drawableComponents.BinarySearch(drawable, Microsoft.Xna.Framework.DrawOrderComparer.Default);

	if (num < 0) {
		num = ~num;

	__while0__: 
		while ((num < this.drawableComponents.Count) && (this.drawableComponents.get_Item(num).IDrawable_DrawOrder === drawable.IDrawable_DrawOrder)) {
			++num;
		}
		this.drawableComponents.Insert(num, drawable);
	}
};

Microsoft.Xna.Framework.Game.prototype.UpdateableUpdateOrderChanged = function (sender, e) {
	var updateable = JSIL.TryCast(sender, Microsoft.Xna.Framework.IUpdateable);
	this.updateableComponents.Remove(updateable);
	var num = this.updateableComponents.BinarySearch(updateable, Microsoft.Xna.Framework.UpdateOrderComparer.Default);

	if (num < 0) {
		num = ~num;

	__while0__: 
		while ((num < this.updateableComponents.Count) && (this.updateableComponents.get_Item(num).IUpdateable_UpdateOrder === updateable.IUpdateable_UpdateOrder)) {
			++num;
		}
		this.updateableComponents.Insert(num, updateable);
	}
};

Microsoft.Xna.Framework.Game.prototype.OnActivated = function (sender, args) {

	if (this.Activated === null) {
		this.Activated(this, args);
	}
};

Microsoft.Xna.Framework.Game.prototype.OnDeactivated = function (sender, args) {

	if (this.Deactivated === null) {
		this.Deactivated(this, args);
	}
};

Microsoft.Xna.Framework.Game.prototype.OnExiting = function (sender, args) {

	if (this.Exiting === null) {
		this.Exiting(null, args);
	}
};

Microsoft.Xna.Framework.Game.prototype.EnsureHost = function () {

	if (this.host === null) {
		return ;
	}
	this.host = new Microsoft.Xna.Framework.WindowsGameHost(this);
	this.host.add_Activated(JSIL.Delegate.New("System.EventHandler", this, Microsoft.Xna.Framework.Game.prototype.HostActivated));
	this.host.add_Deactivated(JSIL.Delegate.New("System.EventHandler", this, Microsoft.Xna.Framework.Game.prototype.HostDeactivated));
	this.host.add_Suspend(JSIL.Delegate.New("System.EventHandler", this, Microsoft.Xna.Framework.Game.prototype.HostSuspend));
	this.host.add_Resume(JSIL.Delegate.New("System.EventHandler", this, Microsoft.Xna.Framework.Game.prototype.HostResume));
	this.host.add_Idle(JSIL.Delegate.New("System.EventHandler", this, Microsoft.Xna.Framework.Game.prototype.HostIdle));
	this.host.add_Exiting(JSIL.Delegate.New("System.EventHandler", this, Microsoft.Xna.Framework.Game.prototype.HostExiting));
};

Microsoft.Xna.Framework.Game.prototype.HostSuspend = function (sender, e) {
	this.clock.Suspend();
};

Microsoft.Xna.Framework.Game.prototype.HostResume = function (sender, e) {
	this.clock.Resume();
};

Microsoft.Xna.Framework.Game.prototype.HostExiting = function (sender, e) {
	this.OnExiting(this, System.EventArgs.Empty);
};

Microsoft.Xna.Framework.Game.prototype.HostIdle = function (sender, e) {
	this.Tick();
};

Microsoft.Xna.Framework.Game.prototype.HostDeactivated = function (sender, e) {

	if (!this.isActive) {
		return ;
	}
	this.isActive = false;
	this.OnDeactivated(this, System.EventArgs.Empty);
};

Microsoft.Xna.Framework.Game.prototype.HostActivated = function (sender, e) {

	if (this.isActive) {
		return ;
	}
	this.isActive = true;
	this.OnActivated(this, System.EventArgs.Empty);
};

Microsoft.Xna.Framework.Game.prototype.HookDeviceEvents = function () {
	this.graphicsDeviceService = JSIL.TryCast(this.get_Services().GetService(Microsoft.Xna.Framework.Graphics.IGraphicsDeviceService), Microsoft.Xna.Framework.Graphics.IGraphicsDeviceService);

	if (this.graphicsDeviceService === null) {
		this.graphicsDeviceService.IGraphicsDeviceService_add_DeviceCreated(JSIL.Delegate.New("System.EventHandler", this, Microsoft.Xna.Framework.Game.prototype.DeviceCreated));
		this.graphicsDeviceService.IGraphicsDeviceService_add_DeviceResetting(JSIL.Delegate.New("System.EventHandler", this, Microsoft.Xna.Framework.Game.prototype.DeviceResetting));
		this.graphicsDeviceService.IGraphicsDeviceService_add_DeviceReset(JSIL.Delegate.New("System.EventHandler", this, Microsoft.Xna.Framework.Game.prototype.DeviceReset));
		this.graphicsDeviceService.IGraphicsDeviceService_add_DeviceDisposing(JSIL.Delegate.New("System.EventHandler", this, Microsoft.Xna.Framework.Game.prototype.DeviceDisposing));
	}
};

Microsoft.Xna.Framework.Game.prototype.UnhookDeviceEvents = function () {

	if (this.graphicsDeviceService === null) {
		this.graphicsDeviceService.IGraphicsDeviceService_remove_DeviceCreated(JSIL.Delegate.New("System.EventHandler", this, Microsoft.Xna.Framework.Game.prototype.DeviceCreated));
		this.graphicsDeviceService.IGraphicsDeviceService_remove_DeviceResetting(JSIL.Delegate.New("System.EventHandler", this, Microsoft.Xna.Framework.Game.prototype.DeviceResetting));
		this.graphicsDeviceService.IGraphicsDeviceService_remove_DeviceReset(JSIL.Delegate.New("System.EventHandler", this, Microsoft.Xna.Framework.Game.prototype.DeviceReset));
		this.graphicsDeviceService.IGraphicsDeviceService_remove_DeviceDisposing(JSIL.Delegate.New("System.EventHandler", this, Microsoft.Xna.Framework.Game.prototype.DeviceDisposing));
	}
};

Microsoft.Xna.Framework.Game.prototype.DeviceResetting = function (sender, e) {
	this.UnloadGraphicsContent(false);
};

Microsoft.Xna.Framework.Game.prototype.DeviceReset = function (sender, e) {
	this.LoadGraphicsContent(false);
};

Microsoft.Xna.Framework.Game.prototype.DeviceCreated = function (sender, e) {
	this.LoadGraphicsContent(true);
	this.LoadContent();
};

Microsoft.Xna.Framework.Game.prototype.DeviceDisposing = function (sender, e) {
	this.content.Unload();
	this.UnloadGraphicsContent(true);
	this.UnloadContent();
};

Microsoft.Xna.Framework.Game.prototype.LoadGraphicsContent = function (loadAllContent) {
};

Microsoft.Xna.Framework.Game.prototype.UnloadGraphicsContent = function (unloadAllContent) {
};

Microsoft.Xna.Framework.Game.prototype.LoadContent = function () {
};

Microsoft.Xna.Framework.Game.prototype.UnloadContent = function () {
};

Microsoft.Xna.Framework.Game.prototype.get_ShouldExit = function () {
	return this.exitRequested;
};

Microsoft.Xna.Framework.Game.prototype.Dispose$0 = function () {
	this.Dispose(true);
	System.GC.SuppressFinalize(this);
};

Microsoft.Xna.Framework.Game.prototype.Finalize = function () {

	try {
		this.Dispose(false);
	} finally {
		System.Object.prototype.Finalize.call(this);
	}
};

Microsoft.Xna.Framework.Game.prototype.Dispose$1 = function (disposing) {

	if (disposing) {
		System.Threading.Monitor.Enter(this);

		try {
			var array = JSIL.Array.New(Microsoft.Xna.Framework.IGameComponent, this.gameComponents.Count);
			this.gameComponents.CopyTo(array, 0);
			var i = 0;

		__while0__: 
			while (i < array.length) {
				var disposable = JSIL.TryCast(array[i], System.IDisposable);

				if (disposable === null) {
					disposable.IDisposable_Dispose();
				}
				++i;
			}
			var disposable2 = JSIL.TryCast(this.graphicsDeviceManager, System.IDisposable);

			if (disposable2 === null) {
				disposable2.IDisposable_Dispose();
			}
			this.UnhookDeviceEvents();

			if (this.Disposed === null) {
				this.Disposed(this, System.EventArgs.Empty);
			}
		} finally {
			System.Threading.Monitor.Exit(this);
		}
	}
};

Microsoft.Xna.Framework.Game.prototype.ShowMissingRequirementMessage = function (exception) {
	return ((this.host === null) && this.host.ShowMissingRequirementMessage(exception));
};

JSIL.OverloadedMethod(Microsoft.Xna.Framework.Game.prototype, "Dispose", [
		["Dispose$0", []], 
		["Dispose$1", [System.Boolean]]
	]
);
Object.defineProperty(Microsoft.Xna.Framework.Game.prototype, "Components", {
		get: Microsoft.Xna.Framework.Game.prototype.get_Components
	});
Object.defineProperty(Microsoft.Xna.Framework.Game.prototype, "Services", {
		get: Microsoft.Xna.Framework.Game.prototype.get_Services
	});
Object.defineProperty(Microsoft.Xna.Framework.Game.prototype, "InactiveSleepTime", {
		get: Microsoft.Xna.Framework.Game.prototype.get_InactiveSleepTime, 
		set: Microsoft.Xna.Framework.Game.prototype.set_InactiveSleepTime
	});
Object.defineProperty(Microsoft.Xna.Framework.Game.prototype, "IsMouseVisible", {
		get: Microsoft.Xna.Framework.Game.prototype.get_IsMouseVisible, 
		set: Microsoft.Xna.Framework.Game.prototype.set_IsMouseVisible
	});
Object.defineProperty(Microsoft.Xna.Framework.Game.prototype, "TargetElapsedTime", {
		get: Microsoft.Xna.Framework.Game.prototype.get_TargetElapsedTime, 
		set: Microsoft.Xna.Framework.Game.prototype.set_TargetElapsedTime
	});
Object.defineProperty(Microsoft.Xna.Framework.Game.prototype, "IsFixedTimeStep", {
		get: Microsoft.Xna.Framework.Game.prototype.get_IsFixedTimeStep, 
		set: Microsoft.Xna.Framework.Game.prototype.set_IsFixedTimeStep
	});
Object.defineProperty(Microsoft.Xna.Framework.Game.prototype, "Window", {
		get: Microsoft.Xna.Framework.Game.prototype.get_Window
	});
Object.defineProperty(Microsoft.Xna.Framework.Game.prototype, "IsActive", {
		get: Microsoft.Xna.Framework.Game.prototype.get_IsActive
	});
Object.defineProperty(Microsoft.Xna.Framework.Game.prototype, "GraphicsDevice", {
		get: Microsoft.Xna.Framework.Game.prototype.get_GraphicsDevice
	});
Object.defineProperty(Microsoft.Xna.Framework.Game.prototype, "Content", {
		get: Microsoft.Xna.Framework.Game.prototype.get_Content, 
		set: Microsoft.Xna.Framework.Game.prototype.set_Content
	});
Object.defineProperty(Microsoft.Xna.Framework.Game.prototype, "IsActiveIgnoringGuide", {
		get: Microsoft.Xna.Framework.Game.prototype.get_IsActiveIgnoringGuide
	});
Object.defineProperty(Microsoft.Xna.Framework.Game.prototype, "ShouldExit", {
		get: Microsoft.Xna.Framework.Game.prototype.get_ShouldExit
	});
Microsoft.Xna.Framework.Game.prototype.__ImplementInterface__(System.IDisposable);

Object.seal(Microsoft.Xna.Framework.Game.prototype);
Object.seal(Microsoft.Xna.Framework.Game);
Microsoft.Xna.Framework.GameClock.prototype.baseRealTime = 0;
Microsoft.Xna.Framework.GameClock.prototype.lastRealTime = 0;
Microsoft.Xna.Framework.GameClock.prototype.lastRealTimeValid = new System.Boolean();
Microsoft.Xna.Framework.GameClock.prototype.suspendCount = 0;
Microsoft.Xna.Framework.GameClock.prototype.suspendStartTime = 0;
Microsoft.Xna.Framework.GameClock.prototype.timeLostToSuspension = 0;
Microsoft.Xna.Framework.GameClock.prototype.__StructFields__ = {
	currentTimeOffset: System.TimeSpan, 
	currentTimeBase: System.TimeSpan, 
	elapsedTime: System.TimeSpan, 
	elapsedAdjustedTime: System.TimeSpan
};
Microsoft.Xna.Framework.GameClock.prototype.get_CurrentTime = function () {
	return System.TimeSpan.op_Addition(this.currentTimeBase.MemberwiseClone(), this.currentTimeOffset.MemberwiseClone());
};

Microsoft.Xna.Framework.GameClock.prototype.get_ElapsedTime = function () {
	return this.elapsedTime;
};

Microsoft.Xna.Framework.GameClock.prototype.get_ElapsedAdjustedTime = function () {
	return this.elapsedAdjustedTime;
};

Microsoft.Xna.Framework.GameClock.prototype._ctor = function () {
	System.Object.prototype._ctor.call(this);
	this.Reset();
};

Microsoft.Xna.Framework.GameClock.prototype.Reset = function () {
	this.currentTimeBase = System.TimeSpan.Zero.MemberwiseClone();
	this.currentTimeOffset = System.TimeSpan.Zero.MemberwiseClone();
	this.baseRealTime = Microsoft.Xna.Framework.GameClock.Counter;
	this.lastRealTimeValid = false;
};

Microsoft.Xna.Framework.GameClock.prototype.Step = function () {
	var counter = Microsoft.Xna.Framework.GameClock.Counter;

	if (!this.lastRealTimeValid) {
		this.lastRealTime = counter;
		this.lastRealTimeValid = true;
	}

	try {
		this.currentTimeOffset = Microsoft.Xna.Framework.GameClock.CounterToTimeSpan((counter - this.baseRealTime));
	} catch ($exception) {

		if (JSIL.CheckType($exception, System.OverflowException)) {
			this.currentTimeBase = System.TimeSpan.op_Addition(this.currentTimeBase.MemberwiseClone(), this.currentTimeOffset.MemberwiseClone());
			this.baseRealTime = this.lastRealTime;

			try {
				this.currentTimeOffset = Microsoft.Xna.Framework.GameClock.CounterToTimeSpan((counter - this.baseRealTime));
			} catch ($exception) {

				if (JSIL.CheckType($exception, System.OverflowException)) {
					this.baseRealTime = counter;
					this.currentTimeOffset = System.TimeSpan.Zero.MemberwiseClone();
				} else {
					throw $exception;
				}
			}
		} else {
			throw $exception;
		}
	}

	try {
		this.elapsedTime = Microsoft.Xna.Framework.GameClock.CounterToTimeSpan((counter - this.lastRealTime));
	} catch ($exception) {

		if (JSIL.CheckType($exception, System.OverflowException)) {
			this.elapsedTime = System.TimeSpan.Zero.MemberwiseClone();
		} else {
			throw $exception;
		}
	}

	try {
		this.elapsedAdjustedTime = Microsoft.Xna.Framework.GameClock.CounterToTimeSpan((counter - (this.lastRealTime + this.timeLostToSuspension)));
		this.timeLostToSuspension = 0;
	} catch ($exception) {

		if (JSIL.CheckType($exception, System.OverflowException)) {
			this.elapsedAdjustedTime = System.TimeSpan.Zero.MemberwiseClone();
		} else {
			throw $exception;
		}
	}
	this.lastRealTime = counter;
};

Microsoft.Xna.Framework.GameClock.prototype.Suspend = function () {
	++this.suspendCount;

	if (this.suspendCount === 1) {
		this.suspendStartTime = Microsoft.Xna.Framework.GameClock.Counter;
	}
};

Microsoft.Xna.Framework.GameClock.prototype.Resume = function () {
	--this.suspendCount;

	if (this.suspendCount > 0) {
		return ;
	}
	this.timeLostToSuspension += (Microsoft.Xna.Framework.GameClock.Counter - this.suspendStartTime);
	this.suspendStartTime = 0;
};

Microsoft.Xna.Framework.GameClock.get_Counter = function () {
	return System.Diagnostics.Stopwatch.GetTimestamp();
};

Microsoft.Xna.Framework.GameClock.get_Frequency = function () {
	return System.Diagnostics.Stopwatch.Frequency;
};

Microsoft.Xna.Framework.GameClock.CounterToTimeSpan = function (delta) {
	return System.TimeSpan.FromTicks(Math.floor((delta * 10000000) / Microsoft.Xna.Framework.GameClock.Frequency));
};

Object.defineProperty(Microsoft.Xna.Framework.GameClock.prototype, "CurrentTime", {
		get: Microsoft.Xna.Framework.GameClock.prototype.get_CurrentTime
	});
Object.defineProperty(Microsoft.Xna.Framework.GameClock.prototype, "ElapsedTime", {
		get: Microsoft.Xna.Framework.GameClock.prototype.get_ElapsedTime
	});
Object.defineProperty(Microsoft.Xna.Framework.GameClock.prototype, "ElapsedAdjustedTime", {
		get: Microsoft.Xna.Framework.GameClock.prototype.get_ElapsedAdjustedTime
	});
Object.defineProperty(Microsoft.Xna.Framework.GameClock, "Counter", {
		get: Microsoft.Xna.Framework.GameClock.get_Counter
	});
Object.defineProperty(Microsoft.Xna.Framework.GameClock, "Frequency", {
		get: Microsoft.Xna.Framework.GameClock.get_Frequency
	});

Object.seal(Microsoft.Xna.Framework.GameClock.prototype);
Object.seal(Microsoft.Xna.Framework.GameClock);
Microsoft.Xna.Framework.GameComponentCollectionEventArgs.prototype.gameComponent = null;
Microsoft.Xna.Framework.GameComponentCollectionEventArgs.prototype.get_GameComponent = function () {
	return this.gameComponent;
};

Microsoft.Xna.Framework.GameComponentCollectionEventArgs.prototype._ctor = function (gameComponent) {
	System.EventArgs.prototype._ctor.call(this);
	this.gameComponent = gameComponent;
};

Object.defineProperty(Microsoft.Xna.Framework.GameComponentCollectionEventArgs.prototype, "GameComponent", {
		get: Microsoft.Xna.Framework.GameComponentCollectionEventArgs.prototype.get_GameComponent
	});

Object.seal(Microsoft.Xna.Framework.GameComponentCollectionEventArgs.prototype);
Object.seal(Microsoft.Xna.Framework.GameComponentCollectionEventArgs);
Microsoft.Xna.Framework.GameComponentCollection.prototype.ComponentAdded = null;
Microsoft.Xna.Framework.GameComponentCollection.prototype.ComponentRemoved = null;
Microsoft.Xna.Framework.GameComponentCollection.prototype.add_ComponentAdded = function (value) {
	this.ComponentAdded = System.Delegate.Combine(this.ComponentAdded, value);
};

Microsoft.Xna.Framework.GameComponentCollection.prototype.remove_ComponentAdded = function (value) {
	this.ComponentAdded = System.Delegate.Remove(this.ComponentAdded, value);
};

Microsoft.Xna.Framework.GameComponentCollection.prototype.add_ComponentRemoved = function (value) {
	this.ComponentRemoved = System.Delegate.Combine(this.ComponentRemoved, value);
};

Microsoft.Xna.Framework.GameComponentCollection.prototype.remove_ComponentRemoved = function (value) {
	this.ComponentRemoved = System.Delegate.Remove(this.ComponentRemoved, value);
};

Microsoft.Xna.Framework.GameComponentCollection.prototype._ctor = function () {
	System.Collections.ObjectModel.Collection$b1.Of(Microsoft.Xna.Framework.IGameComponent).prototype._ctor.call(this);
};

Microsoft.Xna.Framework.GameComponentCollection.prototype.InsertItem = function (index, item) {

	if (System.Collections.ObjectModel.Collection$b1.Of(Microsoft.Xna.Framework.IGameComponent).prototype.IndexOf.call(this, item) !== -1) {
		throw new System.ArgumentException(Microsoft.Xna.Framework.Resources.CannotAddSameComponentMultipleTimes);
	}
	System.Collections.ObjectModel.Collection$b1.Of(Microsoft.Xna.Framework.IGameComponent).prototype.InsertItem.call(this, index, item);

	if (item === null) {
		this.OnComponentAdded(new Microsoft.Xna.Framework.GameComponentCollectionEventArgs(item));
	}
};

Microsoft.Xna.Framework.GameComponentCollection.prototype.RemoveItem = function (index) {
	var gameComponent = System.Collections.ObjectModel.Collection$b1.Of(Microsoft.Xna.Framework.IGameComponent).prototype.get_Item.call(this, index);
	System.Collections.ObjectModel.Collection$b1.Of(Microsoft.Xna.Framework.IGameComponent).prototype.RemoveItem.call(this, index);

	if (gameComponent === null) {
		this.OnComponentRemoved(new Microsoft.Xna.Framework.GameComponentCollectionEventArgs(gameComponent));
	}
};

Microsoft.Xna.Framework.GameComponentCollection.prototype.SetItem = function (index, item) {
	throw new System.NotSupportedException(Microsoft.Xna.Framework.Resources.CannotSetItemsIntoGameComponentCollection);
};

Microsoft.Xna.Framework.GameComponentCollection.prototype.ClearItems = function () {
	var i = 0;

__while0__: 
	while (i < this.Count) {
		this.OnComponentRemoved(new Microsoft.Xna.Framework.GameComponentCollectionEventArgs(System.Collections.ObjectModel.Collection$b1.Of(Microsoft.Xna.Framework.IGameComponent).prototype.get_Item.call(this, i)));
		++i;
	}
	System.Collections.ObjectModel.Collection$b1.Of(Microsoft.Xna.Framework.IGameComponent).prototype.ClearItems.call(this);
};

Microsoft.Xna.Framework.GameComponentCollection.prototype.OnComponentAdded = function (eventArgs) {

	if (this.ComponentAdded === null) {
		this.ComponentAdded(this, eventArgs);
	}
};

Microsoft.Xna.Framework.GameComponentCollection.prototype.OnComponentRemoved = function (eventArgs) {

	if (this.ComponentRemoved === null) {
		this.ComponentRemoved(this, eventArgs);
	}
};


Object.seal(Microsoft.Xna.Framework.GameComponentCollection.prototype);
Object.seal(Microsoft.Xna.Framework.GameComponentCollection);
Microsoft.Xna.Framework.GameHost.prototype.Suspend = null;
Microsoft.Xna.Framework.GameHost.prototype.Resume = null;
Microsoft.Xna.Framework.GameHost.prototype.Activated = null;
Microsoft.Xna.Framework.GameHost.prototype.Deactivated = null;
Microsoft.Xna.Framework.GameHost.prototype.Idle = null;
Microsoft.Xna.Framework.GameHost.prototype.Exiting = null;
Microsoft.Xna.Framework.GameHost.prototype.add_Suspend = function (value) {
	this.Suspend = System.Delegate.Combine(this.Suspend, value);
};

Microsoft.Xna.Framework.GameHost.prototype.remove_Suspend = function (value) {
	this.Suspend = System.Delegate.Remove(this.Suspend, value);
};

Microsoft.Xna.Framework.GameHost.prototype.add_Resume = function (value) {
	this.Resume = System.Delegate.Combine(this.Resume, value);
};

Microsoft.Xna.Framework.GameHost.prototype.remove_Resume = function (value) {
	this.Resume = System.Delegate.Remove(this.Resume, value);
};

Microsoft.Xna.Framework.GameHost.prototype.add_Activated = function (value) {
	this.Activated = System.Delegate.Combine(this.Activated, value);
};

Microsoft.Xna.Framework.GameHost.prototype.remove_Activated = function (value) {
	this.Activated = System.Delegate.Remove(this.Activated, value);
};

Microsoft.Xna.Framework.GameHost.prototype.add_Deactivated = function (value) {
	this.Deactivated = System.Delegate.Combine(this.Deactivated, value);
};

Microsoft.Xna.Framework.GameHost.prototype.remove_Deactivated = function (value) {
	this.Deactivated = System.Delegate.Remove(this.Deactivated, value);
};

Microsoft.Xna.Framework.GameHost.prototype.add_Idle = function (value) {
	this.Idle = System.Delegate.Combine(this.Idle, value);
};

Microsoft.Xna.Framework.GameHost.prototype.remove_Idle = function (value) {
	this.Idle = System.Delegate.Remove(this.Idle, value);
};

Microsoft.Xna.Framework.GameHost.prototype.add_Exiting = function (value) {
	this.Exiting = System.Delegate.Combine(this.Exiting, value);
};

Microsoft.Xna.Framework.GameHost.prototype.remove_Exiting = function (value) {
	this.Exiting = System.Delegate.Remove(this.Exiting, value);
};

Microsoft.Xna.Framework.GameHost.prototype.OnSuspend = function () {

	if (this.Suspend === null) {
		this.Suspend(this, System.EventArgs.Empty);
	}
};

Microsoft.Xna.Framework.GameHost.prototype.OnResume = function () {

	if (this.Resume === null) {
		this.Resume(this, System.EventArgs.Empty);
	}
};

Microsoft.Xna.Framework.GameHost.prototype.OnActivated = function () {

	if (this.Activated === null) {
		this.Activated(this, System.EventArgs.Empty);
	}
};

Microsoft.Xna.Framework.GameHost.prototype.OnDeactivated = function () {

	if (this.Deactivated === null) {
		this.Deactivated(this, System.EventArgs.Empty);
	}
};

Microsoft.Xna.Framework.GameHost.prototype.OnIdle = function () {

	if (this.Idle === null) {
		this.Idle(this, System.EventArgs.Empty);
	}
};

Microsoft.Xna.Framework.GameHost.prototype.OnExiting = function () {

	if (this.Exiting === null) {
		this.Exiting(this, System.EventArgs.Empty);
	}
};

Microsoft.Xna.Framework.GameHost.prototype.ShowMissingRequirementMessage = function (exception) {
	return false;
};

Microsoft.Xna.Framework.GameHost.prototype._ctor = function () {
	System.Object.prototype._ctor.call(this);
};

Object.defineProperty(Microsoft.Xna.Framework.GameHost.prototype, "Window", {
		get: Microsoft.Xna.Framework.GameHost.prototype.get_Window
	});

Object.seal(Microsoft.Xna.Framework.GameHost.prototype);
Object.seal(Microsoft.Xna.Framework.GameHost);
Microsoft.Xna.Framework.GamerServices.GamerServicesComponent.prototype._ctor = function (game) {
	Microsoft.Xna.Framework.GameComponent.prototype._ctor.call(this, game);
};

Microsoft.Xna.Framework.GamerServices.GamerServicesComponent.prototype.Initialize = function () {
	Microsoft.Xna.Framework.GamerServices.GamerServicesDispatcher.WindowHandle = this.Game.Window.Handle;
	Microsoft.Xna.Framework.GamerServices.GamerServicesDispatcher.add_InstallingTitleUpdate(JSIL.Delegate.New("System.EventHandler`1[System.EventArgs]", this, Microsoft.Xna.Framework.GamerServices.GamerServicesComponent.prototype.GamerServicesDispatcher_InstallingTitleUpdate));
	Microsoft.Xna.Framework.GamerServices.GamerServicesDispatcher.Initialize(this.Game.Services);
	Microsoft.Xna.Framework.GameComponent.prototype.Initialize.call(this);
};

Microsoft.Xna.Framework.GamerServices.GamerServicesComponent.prototype.Update = function (gameTime) {
	Microsoft.Xna.Framework.GamerServices.GamerServicesDispatcher.Update();
	Microsoft.Xna.Framework.GameComponent.prototype.Update.call(this, gameTime);
};

Microsoft.Xna.Framework.GamerServices.GamerServicesComponent.prototype.GamerServicesDispatcher_InstallingTitleUpdate = function (sender, e) {
	this.Game.Exit();
};


Object.seal(Microsoft.Xna.Framework.GamerServices.GamerServicesComponent.prototype);
Object.seal(Microsoft.Xna.Framework.GamerServices.GamerServicesComponent);
Microsoft.Xna.Framework.GameServiceContainer.prototype.services = null;
Microsoft.Xna.Framework.GameServiceContainer.prototype._ctor = function () {
	this.services = new (System.Collections.Generic.Dictionary$b2.Of(System.Type, System.Object)) ();
	System.Object.prototype._ctor.call(this);
};

Microsoft.Xna.Framework.GameServiceContainer.prototype.AddService = function (type, provider) {

	if (type !== null) {
		throw new System.ArgumentNullException("type", Microsoft.Xna.Framework.Resources.ServiceTypeCannotBeNull);
	}

	if (provider !== null) {
		throw new System.ArgumentNullException("provider", Microsoft.Xna.Framework.Resources.ServiceProviderCannotBeNull);
	}

	if (this.services.ContainsKey(type)) {
		throw new System.ArgumentException(Microsoft.Xna.Framework.Resources.ServiceAlreadyPresent, "type");
	}

	if (!type.IsAssignableFrom(provider.GetType())) {
		throw new System.ArgumentException(System.String.Format(System.Globalization.CultureInfo.CurrentUICulture, Microsoft.Xna.Framework.Resources.ServiceMustBeAssignable, [provider.GetType().FullName, type.GetType().FullName]));
	}
	this.services.Add(type, provider);
};

Microsoft.Xna.Framework.GameServiceContainer.prototype.RemoveService = function (type) {

	if (type !== null) {
		throw new System.ArgumentNullException("type", Microsoft.Xna.Framework.Resources.ServiceTypeCannotBeNull);
	}
	this.services.Remove(type);
};

Microsoft.Xna.Framework.GameServiceContainer.prototype.GetService = function (type) {

	if (type !== null) {
		throw new System.ArgumentNullException("type", Microsoft.Xna.Framework.Resources.ServiceTypeCannotBeNull);
	}

	if (this.services.ContainsKey(type)) {
		return this.services.get_Item(type);
	}
	return null;
};

Microsoft.Xna.Framework.GameServiceContainer.prototype.__ImplementInterface__(System.IServiceProvider);

Object.seal(Microsoft.Xna.Framework.GameServiceContainer.prototype);
Object.seal(Microsoft.Xna.Framework.GameServiceContainer);
Microsoft.Xna.Framework.GameTime.prototype.isRunningSlowly = new System.Boolean();
Microsoft.Xna.Framework.GameTime.prototype.__StructFields__ = {
	totalRealTime: System.TimeSpan, 
	totalGameTime: System.TimeSpan, 
	elapsedRealTime: System.TimeSpan, 
	elapsedGameTime: System.TimeSpan
};
Microsoft.Xna.Framework.GameTime.prototype._ctor$0 = function () {
	System.Object.prototype._ctor.call(this);
};

Microsoft.Xna.Framework.GameTime.prototype._ctor$1 = function (totalRealTime, elapsedRealTime, totalGameTime, elapsedGameTime, isRunningSlowly) {
	System.Object.prototype._ctor.call(this);
	this.totalRealTime = totalRealTime;
	this.elapsedRealTime = elapsedRealTime;
	this.totalGameTime = totalGameTime;
	this.elapsedGameTime = elapsedGameTime;
	this.isRunningSlowly = isRunningSlowly;
};

Microsoft.Xna.Framework.GameTime.prototype._ctor$2 = function (totalRealTime, elapsedRealTime, totalGameTime, elapsedGameTime) {
	this._ctor(
		totalRealTime, 
		elapsedRealTime, 
		totalGameTime, 
		elapsedGameTime, 
		false
	);
};

Microsoft.Xna.Framework.GameTime.prototype.get_TotalRealTime = function () {
	return this.totalRealTime;
};

Microsoft.Xna.Framework.GameTime.prototype.set_TotalRealTime = function (value) {
	this.totalRealTime = value;
};

Microsoft.Xna.Framework.GameTime.prototype.get_TotalGameTime = function () {
	return this.totalGameTime;
};

Microsoft.Xna.Framework.GameTime.prototype.set_TotalGameTime = function (value) {
	this.totalGameTime = value;
};

Microsoft.Xna.Framework.GameTime.prototype.get_ElapsedRealTime = function () {
	return this.elapsedRealTime;
};

Microsoft.Xna.Framework.GameTime.prototype.set_ElapsedRealTime = function (value) {
	this.elapsedRealTime = value;
};

Microsoft.Xna.Framework.GameTime.prototype.get_ElapsedGameTime = function () {
	return this.elapsedGameTime;
};

Microsoft.Xna.Framework.GameTime.prototype.set_ElapsedGameTime = function (value) {
	this.elapsedGameTime = value;
};

Microsoft.Xna.Framework.GameTime.prototype.get_IsRunningSlowly = function () {
	return this.isRunningSlowly;
};

Microsoft.Xna.Framework.GameTime.prototype.set_IsRunningSlowly = function (value) {
	this.isRunningSlowly = value;
};

JSIL.OverloadedMethod(Microsoft.Xna.Framework.GameTime.prototype, "_ctor", [
		["_ctor$0", []], 
		["_ctor$1", [System.TimeSpan, System.TimeSpan, System.TimeSpan, System.TimeSpan, System.Boolean]], 
		["_ctor$2", [System.TimeSpan, System.TimeSpan, System.TimeSpan, System.TimeSpan]]
	]
);
Object.defineProperty(Microsoft.Xna.Framework.GameTime.prototype, "TotalRealTime", {
		get: Microsoft.Xna.Framework.GameTime.prototype.get_TotalRealTime, 
		set: Microsoft.Xna.Framework.GameTime.prototype.set_TotalRealTime
	});
Object.defineProperty(Microsoft.Xna.Framework.GameTime.prototype, "TotalGameTime", {
		get: Microsoft.Xna.Framework.GameTime.prototype.get_TotalGameTime, 
		set: Microsoft.Xna.Framework.GameTime.prototype.set_TotalGameTime
	});
Object.defineProperty(Microsoft.Xna.Framework.GameTime.prototype, "ElapsedRealTime", {
		get: Microsoft.Xna.Framework.GameTime.prototype.get_ElapsedRealTime, 
		set: Microsoft.Xna.Framework.GameTime.prototype.set_ElapsedRealTime
	});
Object.defineProperty(Microsoft.Xna.Framework.GameTime.prototype, "ElapsedGameTime", {
		get: Microsoft.Xna.Framework.GameTime.prototype.get_ElapsedGameTime, 
		set: Microsoft.Xna.Framework.GameTime.prototype.set_ElapsedGameTime
	});
Object.defineProperty(Microsoft.Xna.Framework.GameTime.prototype, "IsRunningSlowly", {
		get: Microsoft.Xna.Framework.GameTime.prototype.get_IsRunningSlowly, 
		set: Microsoft.Xna.Framework.GameTime.prototype.set_IsRunningSlowly
	});

Object.seal(Microsoft.Xna.Framework.GameTime.prototype);
Object.seal(Microsoft.Xna.Framework.GameTime);
Microsoft.Xna.Framework.GameWindow.DefaultClientWidth = 0;
Microsoft.Xna.Framework.GameWindow.DefaultClientHeight = 0;
Microsoft.Xna.Framework.GameWindow.prototype.title = null;
Microsoft.Xna.Framework.GameWindow.prototype.Activated = null;
Microsoft.Xna.Framework.GameWindow.prototype.Deactivated = null;
Microsoft.Xna.Framework.GameWindow.prototype.Paint = null;
Microsoft.Xna.Framework.GameWindow.prototype.ScreenDeviceNameChanged = null;
Microsoft.Xna.Framework.GameWindow.prototype.ClientSizeChanged = null;
Microsoft.Xna.Framework.GameWindow.prototype.get_Title = function () {
	return this.title;
};

Microsoft.Xna.Framework.GameWindow.prototype.set_Title = function (value) {

	if (value !== null) {
		throw new System.ArgumentNullException("value", Microsoft.Xna.Framework.Resources.TitleCannotBeNull);
	}

	if (System.String.op_Inequality(this.title, value)) {
		this.title = value;
		this.SetTitle(this.title);
	}
};

Microsoft.Xna.Framework.GameWindow.prototype.add_Activated = function (value) {
	this.Activated = System.Delegate.Combine(this.Activated, value);
};

Microsoft.Xna.Framework.GameWindow.prototype.remove_Activated = function (value) {
	this.Activated = System.Delegate.Remove(this.Activated, value);
};

Microsoft.Xna.Framework.GameWindow.prototype.add_Deactivated = function (value) {
	this.Deactivated = System.Delegate.Combine(this.Deactivated, value);
};

Microsoft.Xna.Framework.GameWindow.prototype.remove_Deactivated = function (value) {
	this.Deactivated = System.Delegate.Remove(this.Deactivated, value);
};

Microsoft.Xna.Framework.GameWindow.prototype.add_Paint = function (value) {
	this.Paint = System.Delegate.Combine(this.Paint, value);
};

Microsoft.Xna.Framework.GameWindow.prototype.remove_Paint = function (value) {
	this.Paint = System.Delegate.Remove(this.Paint, value);
};

Microsoft.Xna.Framework.GameWindow.prototype.add_ScreenDeviceNameChanged = function (value) {
	this.ScreenDeviceNameChanged = System.Delegate.Combine(this.ScreenDeviceNameChanged, value);
};

Microsoft.Xna.Framework.GameWindow.prototype.remove_ScreenDeviceNameChanged = function (value) {
	this.ScreenDeviceNameChanged = System.Delegate.Remove(this.ScreenDeviceNameChanged, value);
};

Microsoft.Xna.Framework.GameWindow.prototype.add_ClientSizeChanged = function (value) {
	this.ClientSizeChanged = System.Delegate.Combine(this.ClientSizeChanged, value);
};

Microsoft.Xna.Framework.GameWindow.prototype.remove_ClientSizeChanged = function (value) {
	this.ClientSizeChanged = System.Delegate.Remove(this.ClientSizeChanged, value);
};

Microsoft.Xna.Framework.GameWindow.prototype._ctor = function () {
	System.Object.prototype._ctor.call(this);
	this.title = System.String.Empty;
};

Microsoft.Xna.Framework.GameWindow.prototype.EndScreenDeviceChange$1 = function (screenDeviceName) {
	this.EndScreenDeviceChange(screenDeviceName, this.ClientBounds.Width, this.ClientBounds.Height);
};

Microsoft.Xna.Framework.GameWindow.prototype.OnActivated = function () {

	if (this.Activated === null) {
		this.Activated(this, System.EventArgs.Empty);
	}
};

Microsoft.Xna.Framework.GameWindow.prototype.OnDeactivated = function () {

	if (this.Deactivated === null) {
		this.Deactivated(this, System.EventArgs.Empty);
	}
};

Microsoft.Xna.Framework.GameWindow.prototype.OnPaint = function () {

	if (this.Paint === null) {
		this.Paint(this, System.EventArgs.Empty);
	}
};

Microsoft.Xna.Framework.GameWindow.prototype.OnScreenDeviceNameChanged = function () {

	if (this.ScreenDeviceNameChanged === null) {
		this.ScreenDeviceNameChanged(this, System.EventArgs.Empty);
	}
};

Microsoft.Xna.Framework.GameWindow.prototype.OnClientSizeChanged = function () {

	if (this.ClientSizeChanged === null) {
		this.ClientSizeChanged(this, System.EventArgs.Empty);
	}
};

Microsoft.Xna.Framework.GameWindow._cctor = function () {
	Microsoft.Xna.Framework.GameWindow.DefaultClientWidth = 800;
	Microsoft.Xna.Framework.GameWindow.DefaultClientHeight = 600;
};

JSIL.OverloadedMethod(Microsoft.Xna.Framework.GameWindow.prototype, "EndScreenDeviceChange", [
		["EndScreenDeviceChange$0", [System.String, System.Int32, System.Int32]], 
		["EndScreenDeviceChange$1", [System.String]]
	]
);
Object.defineProperty(Microsoft.Xna.Framework.GameWindow.prototype, "Title", {
		get: Microsoft.Xna.Framework.GameWindow.prototype.get_Title, 
		set: Microsoft.Xna.Framework.GameWindow.prototype.set_Title
	});
Object.defineProperty(Microsoft.Xna.Framework.GameWindow.prototype, "Handle", {
		get: Microsoft.Xna.Framework.GameWindow.prototype.get_Handle
	});
Object.defineProperty(Microsoft.Xna.Framework.GameWindow.prototype, "AllowUserResizing", {
		get: Microsoft.Xna.Framework.GameWindow.prototype.get_AllowUserResizing, 
		set: Microsoft.Xna.Framework.GameWindow.prototype.set_AllowUserResizing
	});
Object.defineProperty(Microsoft.Xna.Framework.GameWindow.prototype, "IsMouseVisible", {
		get: Microsoft.Xna.Framework.GameWindow.prototype.get_IsMouseVisible, 
		set: Microsoft.Xna.Framework.GameWindow.prototype.set_IsMouseVisible
	});
Object.defineProperty(Microsoft.Xna.Framework.GameWindow.prototype, "IsMinimized", {
		get: Microsoft.Xna.Framework.GameWindow.prototype.get_IsMinimized
	});
Object.defineProperty(Microsoft.Xna.Framework.GameWindow.prototype, "ClientBounds", {
		get: Microsoft.Xna.Framework.GameWindow.prototype.get_ClientBounds
	});
Object.defineProperty(Microsoft.Xna.Framework.GameWindow.prototype, "ScreenDeviceName", {
		get: Microsoft.Xna.Framework.GameWindow.prototype.get_ScreenDeviceName
	});
Microsoft.Xna.Framework.GameWindow._cctor();

Object.seal(Microsoft.Xna.Framework.GameWindow.prototype);
Object.seal(Microsoft.Xna.Framework.GameWindow);
Microsoft.Xna.Framework.GraphicsDeviceInformation.prototype.presentationParameters = null;
Microsoft.Xna.Framework.GraphicsDeviceInformation.prototype.adapter = null;
Microsoft.Xna.Framework.GraphicsDeviceInformation.prototype.deviceType = 0;
Microsoft.Xna.Framework.GraphicsDeviceInformation.prototype.get_Adapter = function () {
	return this.adapter;
};

Microsoft.Xna.Framework.GraphicsDeviceInformation.prototype.set_Adapter = function (value) {

	if (Microsoft.Xna.Framework.Graphics.GraphicsAdapter.op_Equality(this.adapter, null)) {
		throw new System.ArgumentNullException("value", Microsoft.Xna.Framework.Resources.NoNullUseDefaultAdapter);
	}
	this.adapter = value;
};

Microsoft.Xna.Framework.GraphicsDeviceInformation.prototype.get_DeviceType = function () {
	return this.deviceType;
};

Microsoft.Xna.Framework.GraphicsDeviceInformation.prototype.set_DeviceType = function (value) {
	this.deviceType = value;
};

Microsoft.Xna.Framework.GraphicsDeviceInformation.prototype.get_PresentationParameters = function () {
	return this.presentationParameters;
};

Microsoft.Xna.Framework.GraphicsDeviceInformation.prototype.set_PresentationParameters = function (value) {
	this.presentationParameters = value;
};

Microsoft.Xna.Framework.GraphicsDeviceInformation.prototype.Equals = function (obj) {
	var graphicsDeviceInformation = JSIL.TryCast(obj, Microsoft.Xna.Framework.GraphicsDeviceInformation);
	return (graphicsDeviceInformation && 
		graphicsDeviceInformation.adapter.Equals(this.adapter) && 
		graphicsDeviceInformation.deviceType.Equals(this.deviceType) && 
		(graphicsDeviceInformation.PresentationParameters.AutoDepthStencilFormat === this.PresentationParameters.AutoDepthStencilFormat) && 
		(graphicsDeviceInformation.PresentationParameters.BackBufferCount === this.PresentationParameters.BackBufferCount) && 
		(graphicsDeviceInformation.PresentationParameters.BackBufferFormat === this.PresentationParameters.BackBufferFormat) && 
		(graphicsDeviceInformation.PresentationParameters.BackBufferHeight === this.PresentationParameters.BackBufferHeight) && 
		(graphicsDeviceInformation.PresentationParameters.BackBufferWidth === this.PresentationParameters.BackBufferWidth) && 
		!System.IntPtr.op_Inequality(graphicsDeviceInformation.PresentationParameters.DeviceWindowHandle, this.PresentationParameters.DeviceWindowHandle) && 
		(graphicsDeviceInformation.PresentationParameters.EnableAutoDepthStencil === this.PresentationParameters.EnableAutoDepthStencil) && 
		(graphicsDeviceInformation.PresentationParameters.FullScreenRefreshRateInHz === this.PresentationParameters.FullScreenRefreshRateInHz) && 
		(graphicsDeviceInformation.PresentationParameters.IsFullScreen === this.PresentationParameters.IsFullScreen) && 
		(graphicsDeviceInformation.PresentationParameters.MultiSampleQuality === this.PresentationParameters.MultiSampleQuality) && 
		(graphicsDeviceInformation.PresentationParameters.MultiSampleType === this.PresentationParameters.MultiSampleType) && 
		(graphicsDeviceInformation.PresentationParameters.PresentationInterval === this.PresentationParameters.PresentationInterval) && 
		(graphicsDeviceInformation.PresentationParameters.PresentOptions === this.PresentationParameters.PresentOptions) && (graphicsDeviceInformation.PresentationParameters.SwapEffect === this.PresentationParameters.SwapEffect));
};

Microsoft.Xna.Framework.GraphicsDeviceInformation.prototype.GetHashCode = function () {
	return (this.deviceType.GetHashCode() ^ this.adapter.GetHashCode() ^ this.presentationParameters.GetHashCode());
};

Microsoft.Xna.Framework.GraphicsDeviceInformation.prototype.Clone = function () {
	return new Microsoft.Xna.Framework.GraphicsDeviceInformation().__Initialize__({
			presentationParameters: this.presentationParameters.Clone(), 
			adapter: this.adapter, 
			deviceType: this.deviceType}
	);
};

Microsoft.Xna.Framework.GraphicsDeviceInformation.prototype._ctor = function () {
	this.presentationParameters = new Microsoft.Xna.Framework.Graphics.PresentationParameters();
	this.adapter = Microsoft.Xna.Framework.Graphics.GraphicsAdapter.DefaultAdapter;
	this.deviceType = Microsoft.Xna.Framework.Graphics.DeviceType.Hardware;
	System.Object.prototype._ctor.call(this);
};

Object.defineProperty(Microsoft.Xna.Framework.GraphicsDeviceInformation.prototype, "Adapter", {
		get: Microsoft.Xna.Framework.GraphicsDeviceInformation.prototype.get_Adapter, 
		set: Microsoft.Xna.Framework.GraphicsDeviceInformation.prototype.set_Adapter
	});
Object.defineProperty(Microsoft.Xna.Framework.GraphicsDeviceInformation.prototype, "DeviceType", {
		get: Microsoft.Xna.Framework.GraphicsDeviceInformation.prototype.get_DeviceType, 
		set: Microsoft.Xna.Framework.GraphicsDeviceInformation.prototype.set_DeviceType
	});
Object.defineProperty(Microsoft.Xna.Framework.GraphicsDeviceInformation.prototype, "PresentationParameters", {
		get: Microsoft.Xna.Framework.GraphicsDeviceInformation.prototype.get_PresentationParameters, 
		set: Microsoft.Xna.Framework.GraphicsDeviceInformation.prototype.set_PresentationParameters
	});

Object.seal(Microsoft.Xna.Framework.GraphicsDeviceInformation.prototype);
Object.seal(Microsoft.Xna.Framework.GraphicsDeviceInformation);
Microsoft.Xna.Framework.GraphicsDeviceInformationComparer.prototype.graphics = null;
Microsoft.Xna.Framework.GraphicsDeviceInformationComparer.prototype._ctor = function (graphicsComponent) {
	System.Object.prototype._ctor.call(this);
	this.graphics = graphicsComponent;
};

Microsoft.Xna.Framework.GraphicsDeviceInformationComparer.prototype.Compare = function (d1, d2) {

	if (d1.DeviceType !== d2.DeviceType) {

		if (d1.DeviceType >= d2.DeviceType) {
			return 1;
		}
		return -1;
	} else {
		var presentationParameters = d1.PresentationParameters;
		var presentationParameters2 = d2.PresentationParameters;

		if (presentationParameters.IsFullScreen !== presentationParameters2.IsFullScreen) {

			if (this.graphics.IsFullScreen !== presentationParameters.IsFullScreen) {
				return 1;
			}
			return -1;
		} else {
			var num = this.RankFormat(presentationParameters.BackBufferFormat);
			var num2 = this.RankFormat(presentationParameters2.BackBufferFormat);

			if (num !== num2) {

				if (num >= num2) {
					return 1;
				}
				return -1;
			} else if (presentationParameters.MultiSampleType !== presentationParameters2.MultiSampleType) {
				var num3 = (presentationParameters.MultiSampleType === Microsoft.Xna.Framework.Graphics.MultiSampleType.NonMaskable) ? 17 : presentationParameters.MultiSampleType;
				var num4 = (presentationParameters2.MultiSampleType === Microsoft.Xna.Framework.Graphics.MultiSampleType.NonMaskable) ? 17 : presentationParameters2.MultiSampleType;

				if (num3 <= num4) {
					return 1;
				}
				return -1;
			} else if (presentationParameters.MultiSampleQuality !== presentationParameters2.MultiSampleQuality) {

				if (presentationParameters.MultiSampleQuality <= presentationParameters2.MultiSampleQuality) {
					return 1;
				}
				return -1;
			} else {

				if (!((this.graphics.PreferredBackBufferWidth === null) && this.graphics.PreferredBackBufferHeight)) {
					var num5 = Math.floor(Microsoft.Xna.Framework.GraphicsDeviceManager.DefaultBackBufferWidth / Microsoft.Xna.Framework.GraphicsDeviceManager.DefaultBackBufferHeight);
				} else {
					num5 = (JSIL.Cast(this.graphics.PreferredBackBufferWidth, System.Single) / JSIL.Cast(this.graphics.PreferredBackBufferHeight, System.Single));
				}
				var num6 = (JSIL.Cast(presentationParameters.BackBufferWidth, System.Single) / JSIL.Cast(presentationParameters.BackBufferHeight, System.Single));
				var num7 = (JSIL.Cast(presentationParameters2.BackBufferWidth, System.Single) / JSIL.Cast(presentationParameters2.BackBufferHeight, System.Single));
				var num8 = System.Math.Abs((num6 - num5));
				var num9 = System.Math.Abs((num7 - num5));

				if (System.Math.Abs((num8 - num9)) > 0.20000000298023224) {

					if (num8 >= num9) {
						return 1;
					}
					return -1;
				} else {
					var num10 = 0;
					var num11 = 0;

					if (this.graphics.IsFullScreen === null) {

						if (!((this.graphics.PreferredBackBufferWidth === null) && this.graphics.PreferredBackBufferHeight)) {
							var adapter = d1.Adapter;
							num10 = (adapter.CurrentDisplayMode.Width * adapter.CurrentDisplayMode.Height);
							var adapter2 = d2.Adapter;
							num11 = (adapter2.CurrentDisplayMode.Width * adapter2.CurrentDisplayMode.Height);
						} else {
							num11 = num10 = (this.graphics.PreferredBackBufferWidth * this.graphics.PreferredBackBufferHeight);
						}
					} else if (!((this.graphics.PreferredBackBufferWidth === null) && this.graphics.PreferredBackBufferHeight)) {
						num11 = num10 = (Microsoft.Xna.Framework.GraphicsDeviceManager.DefaultBackBufferWidth * Microsoft.Xna.Framework.GraphicsDeviceManager.DefaultBackBufferHeight);
					} else {
						num11 = num10 = (this.graphics.PreferredBackBufferWidth * this.graphics.PreferredBackBufferHeight);
					}
					var num12 = System.Math.Abs(((presentationParameters.BackBufferWidth * presentationParameters.BackBufferHeight) - num10));
					var num13 = System.Math.Abs(((presentationParameters2.BackBufferWidth * presentationParameters2.BackBufferHeight) - num11));

					if (num12 !== num13) {

						if (num12 >= num13) {
							return 1;
						}
						return -1;
					} else {

						if ((this.graphics.IsFullScreen !== null) || (presentationParameters.FullScreenRefreshRateInHz === presentationParameters2.FullScreenRefreshRateInHz)) {

							if (Microsoft.Xna.Framework.Graphics.GraphicsAdapter.op_Inequality(d1.Adapter, d2.Adapter)) {

								if (d1.Adapter.IsDefaultAdapter) {
									return -1;
								}

								if (d2.Adapter.IsDefaultAdapter) {
									return 1;
								}
							}
							return 0;
						}
						var num14 = System.Math.Abs((d1.Adapter.CurrentDisplayMode.RefreshRate - presentationParameters.FullScreenRefreshRateInHz));
						var num15 = System.Math.Abs((d2.Adapter.CurrentDisplayMode.RefreshRate - presentationParameters2.FullScreenRefreshRateInHz));

						if (num14 > num15) {
							return 1;
						}
						return -1;
					}
				}
			}
		}
	}
};

Microsoft.Xna.Framework.GraphicsDeviceInformationComparer.prototype.RankFormat = function (format) {
	var num = System.Array.IndexOf(Microsoft.Xna.Framework.GraphicsDeviceManager.ValidBackBufferFormats, format);

	if (num === -1) {
		return 2147483647;
	}
	var num2 = System.Array.IndexOf(Microsoft.Xna.Framework.GraphicsDeviceManager.ValidBackBufferFormats, this.graphics.PreferredBackBufferFormat);

	if (num2 === -1) {
		return (Microsoft.Xna.Framework.GraphicsDeviceManager.ValidBackBufferFormats.length - num);
	}

	if (num >= num2) {
		return (num - num2);
	}
	return 2147483647;
};

Microsoft.Xna.Framework.GraphicsDeviceInformationComparer.prototype.__ImplementInterface__(System.Collections.Generic.IComparer$b1.Of(Microsoft.Xna.Framework.GraphicsDeviceInformation));

Object.seal(Microsoft.Xna.Framework.GraphicsDeviceInformationComparer.prototype);
Object.seal(Microsoft.Xna.Framework.GraphicsDeviceInformationComparer);
Microsoft.Xna.Framework.GraphicsDeviceManager.DefaultBackBufferWidth = 0;
Microsoft.Xna.Framework.GraphicsDeviceManager.DefaultBackBufferHeight = 0;
Microsoft.Xna.Framework.GraphicsDeviceManager.ValidDeviceTypes = 0;
Microsoft.Xna.Framework.GraphicsDeviceManager.ValidAdapterFormats = 0;
Microsoft.Xna.Framework.GraphicsDeviceManager.ValidBackBufferFormats = 0;
Microsoft.Xna.Framework.GraphicsDeviceManager.deviceLostSleepTime = new System.TimeSpan();
Microsoft.Xna.Framework.GraphicsDeviceManager.prototype.game = null;
Microsoft.Xna.Framework.GraphicsDeviceManager.prototype.isReallyFullScreen = new System.Boolean();
Microsoft.Xna.Framework.GraphicsDeviceManager.prototype.isDeviceDirty = new System.Boolean();
Microsoft.Xna.Framework.GraphicsDeviceManager.prototype.inDeviceTransition = new System.Boolean();
Microsoft.Xna.Framework.GraphicsDeviceManager.prototype.device = null;
Microsoft.Xna.Framework.GraphicsDeviceManager.prototype.synchronizeWithVerticalRetrace = new System.Boolean();
Microsoft.Xna.Framework.GraphicsDeviceManager.prototype.isFullScreen = new System.Boolean();
Microsoft.Xna.Framework.GraphicsDeviceManager.prototype.backBufferFormat = 0;
Microsoft.Xna.Framework.GraphicsDeviceManager.prototype.depthStencilFormat = 0;
Microsoft.Xna.Framework.GraphicsDeviceManager.prototype.backBufferWidth = 0;
Microsoft.Xna.Framework.GraphicsDeviceManager.prototype.backBufferHeight = 0;
Microsoft.Xna.Framework.GraphicsDeviceManager.prototype.allowMultiSampling = new System.Boolean();
Microsoft.Xna.Framework.GraphicsDeviceManager.prototype.minimumPixelShaderProfile = 0;
Microsoft.Xna.Framework.GraphicsDeviceManager.prototype.minimumVertexShaderProfile = 0;
Microsoft.Xna.Framework.GraphicsDeviceManager.prototype.resizedBackBufferWidth = 0;
Microsoft.Xna.Framework.GraphicsDeviceManager.prototype.resizedBackBufferHeight = 0;
Microsoft.Xna.Framework.GraphicsDeviceManager.prototype.useResizedBackBuffer = new System.Boolean();
Microsoft.Xna.Framework.GraphicsDeviceManager.prototype.deviceCreated = null;
Microsoft.Xna.Framework.GraphicsDeviceManager.prototype.deviceResetting = null;
Microsoft.Xna.Framework.GraphicsDeviceManager.prototype.deviceReset = null;
Microsoft.Xna.Framework.GraphicsDeviceManager.prototype.deviceDisposing = null;
Microsoft.Xna.Framework.GraphicsDeviceManager.prototype.PreparingDeviceSettings = null;
Microsoft.Xna.Framework.GraphicsDeviceManager.prototype.Disposed = null;
Microsoft.Xna.Framework.GraphicsDeviceManager.prototype.beginDrawOk = new System.Boolean();
Microsoft.Xna.Framework.GraphicsDeviceManager.multiSampleTypes = 0;
Microsoft.Xna.Framework.GraphicsDeviceManager.depthFormatsWithStencil = 0;
Microsoft.Xna.Framework.GraphicsDeviceManager.depthFormatsWithoutStencil = 0;
Microsoft.Xna.Framework.GraphicsDeviceManager.prototype.get_PreferredDepthStencilFormat = function () {
	return this.depthStencilFormat;
};

Microsoft.Xna.Framework.GraphicsDeviceManager.prototype.set_PreferredDepthStencilFormat = function (value) {

	var __label0__ = "__entry0__";
__step0__: 
	while (true) {

		switch (__label0__) {

			case "__entry0__":

				switch (value) {
					case 48: 
					case 49: 
					case 50: 
					case 51: 
					case 52: 
					case 54: 
					case 56: 
						this.depthStencilFormat = value;
						this.isDeviceDirty = true;
						return ;
					case 53: 
					case 55: 

						var __label1__ = "__entry1__";
					__step1__: 
						while (true) {

							switch (__label1__) {

								case "__entry1__":
									__label1__ = "IL_2F";
									continue __step1__;
									break;

								case "IL_2F":
									throw new System.ArgumentOutOfRangeException("value", Microsoft.Xna.Framework.Resources.ValidateDepthStencilFormatIsInvalid);
									break __step1__;
							}
						}
				}
				JSIL.UntranslatableInstruction("goto", "IL_2F");
				break __step0__;
		}
	}
};

Microsoft.Xna.Framework.GraphicsDeviceManager.prototype.get_PreferredBackBufferFormat = function () {
	return this.backBufferFormat;
};

Microsoft.Xna.Framework.GraphicsDeviceManager.prototype.set_PreferredBackBufferFormat = function (value) {

	if (System.Array.IndexOf(Microsoft.Xna.Framework.GraphicsDeviceManager.ValidBackBufferFormats, value) === -1) {
		throw new System.ArgumentOutOfRangeException("value", Microsoft.Xna.Framework.Resources.ValidateBackBufferFormatIsInvalid);
	}
	this.backBufferFormat = value;
	this.isDeviceDirty = true;
};

Microsoft.Xna.Framework.GraphicsDeviceManager.prototype.get_PreferredBackBufferWidth = function () {
	return this.backBufferWidth;
};

Microsoft.Xna.Framework.GraphicsDeviceManager.prototype.set_PreferredBackBufferWidth = function (value) {

	if (value <= 0) {
		throw new System.ArgumentOutOfRangeException("value", Microsoft.Xna.Framework.Resources.BackBufferDimMustBePositive);
	}
	this.backBufferWidth = value;
	this.useResizedBackBuffer = false;
	this.isDeviceDirty = true;
};

Microsoft.Xna.Framework.GraphicsDeviceManager.prototype.get_PreferredBackBufferHeight = function () {
	return this.backBufferHeight;
};

Microsoft.Xna.Framework.GraphicsDeviceManager.prototype.set_PreferredBackBufferHeight = function (value) {

	if (value <= 0) {
		throw new System.ArgumentOutOfRangeException("value", Microsoft.Xna.Framework.Resources.BackBufferDimMustBePositive);
	}
	this.backBufferHeight = value;
	this.useResizedBackBuffer = false;
	this.isDeviceDirty = true;
};

Microsoft.Xna.Framework.GraphicsDeviceManager.prototype.get_IsFullScreen = function () {
	return this.isFullScreen;
};

Microsoft.Xna.Framework.GraphicsDeviceManager.prototype.set_IsFullScreen = function (value) {
	this.isFullScreen = value;
	this.isDeviceDirty = true;
};

Microsoft.Xna.Framework.GraphicsDeviceManager.prototype.get_SynchronizeWithVerticalRetrace = function () {
	return this.synchronizeWithVerticalRetrace;
};

Microsoft.Xna.Framework.GraphicsDeviceManager.prototype.set_SynchronizeWithVerticalRetrace = function (value) {
	this.synchronizeWithVerticalRetrace = value;
	this.isDeviceDirty = true;
};

Microsoft.Xna.Framework.GraphicsDeviceManager.prototype.get_PreferMultiSampling = function () {
	return this.allowMultiSampling;
};

Microsoft.Xna.Framework.GraphicsDeviceManager.prototype.set_PreferMultiSampling = function (value) {
	this.allowMultiSampling = value;
	this.isDeviceDirty = true;
};

Microsoft.Xna.Framework.GraphicsDeviceManager.prototype.get_MinimumPixelShaderProfile = function () {
	return this.minimumPixelShaderProfile;
};

Microsoft.Xna.Framework.GraphicsDeviceManager.prototype.set_MinimumPixelShaderProfile = function (value) {

	if (!((value >= Microsoft.Xna.Framework.Graphics.ShaderProfile.PS_1_1) && (value <= Microsoft.Xna.Framework.Graphics.ShaderProfile.XPS_3_0))) {
		throw new System.ArgumentOutOfRangeException("value", Microsoft.Xna.Framework.Resources.InvalidPixelShaderProfile);
	}
	this.minimumPixelShaderProfile = value;
	this.isDeviceDirty = true;
};

Microsoft.Xna.Framework.GraphicsDeviceManager.prototype.get_MinimumVertexShaderProfile = function () {
	return this.minimumVertexShaderProfile;
};

Microsoft.Xna.Framework.GraphicsDeviceManager.prototype.set_MinimumVertexShaderProfile = function (value) {

	if (!((value >= Microsoft.Xna.Framework.Graphics.ShaderProfile.VS_1_1) && (value <= Microsoft.Xna.Framework.Graphics.ShaderProfile.XVS_3_0))) {
		throw new System.ArgumentOutOfRangeException("value", Microsoft.Xna.Framework.Resources.InvalidVertexShaderProfile);
	}
	this.minimumVertexShaderProfile = value;
	this.isDeviceDirty = true;
};

Microsoft.Xna.Framework.GraphicsDeviceManager.prototype.get_GraphicsDevice = function () {
	return this.device;
};

Microsoft.Xna.Framework.GraphicsDeviceManager.prototype.add_DeviceCreated = function (value) {
	this.deviceCreated = System.Delegate.Combine(this.deviceCreated, value);
};

Microsoft.Xna.Framework.GraphicsDeviceManager.prototype.remove_DeviceCreated = function (value) {
	this.deviceCreated = System.Delegate.Remove(this.deviceCreated, value);
};

Microsoft.Xna.Framework.GraphicsDeviceManager.prototype.add_DeviceResetting = function (value) {
	this.deviceResetting = System.Delegate.Combine(this.deviceResetting, value);
};

Microsoft.Xna.Framework.GraphicsDeviceManager.prototype.remove_DeviceResetting = function (value) {
	this.deviceResetting = System.Delegate.Remove(this.deviceResetting, value);
};

Microsoft.Xna.Framework.GraphicsDeviceManager.prototype.add_DeviceReset = function (value) {
	this.deviceReset = System.Delegate.Combine(this.deviceReset, value);
};

Microsoft.Xna.Framework.GraphicsDeviceManager.prototype.remove_DeviceReset = function (value) {
	this.deviceReset = System.Delegate.Remove(this.deviceReset, value);
};

Microsoft.Xna.Framework.GraphicsDeviceManager.prototype.add_DeviceDisposing = function (value) {
	this.deviceDisposing = System.Delegate.Combine(this.deviceDisposing, value);
};

Microsoft.Xna.Framework.GraphicsDeviceManager.prototype.remove_DeviceDisposing = function (value) {
	this.deviceDisposing = System.Delegate.Remove(this.deviceDisposing, value);
};

Microsoft.Xna.Framework.GraphicsDeviceManager.prototype.add_PreparingDeviceSettings = function (value) {
	this.PreparingDeviceSettings = System.Delegate.Combine(this.PreparingDeviceSettings, value);
};

Microsoft.Xna.Framework.GraphicsDeviceManager.prototype.remove_PreparingDeviceSettings = function (value) {
	this.PreparingDeviceSettings = System.Delegate.Remove(this.PreparingDeviceSettings, value);
};

Microsoft.Xna.Framework.GraphicsDeviceManager.prototype.add_Disposed = function (value) {
	this.Disposed = System.Delegate.Combine(this.Disposed, value);
};

Microsoft.Xna.Framework.GraphicsDeviceManager.prototype.remove_Disposed = function (value) {
	this.Disposed = System.Delegate.Remove(this.Disposed, value);
};

Microsoft.Xna.Framework.GraphicsDeviceManager.prototype._ctor = function (game) {
	this.synchronizeWithVerticalRetrace = true;
	this.backBufferFormat = Microsoft.Xna.Framework.Graphics.SurfaceFormat.Color;
	this.depthStencilFormat = Microsoft.Xna.Framework.Graphics.DepthFormat.Depth24;
	this.backBufferWidth = Microsoft.Xna.Framework.GraphicsDeviceManager.DefaultBackBufferWidth;
	this.backBufferHeight = Microsoft.Xna.Framework.GraphicsDeviceManager.DefaultBackBufferHeight;
	this.minimumVertexShaderProfile = Microsoft.Xna.Framework.Graphics.ShaderProfile.VS_1_1;
	System.Object.prototype._ctor.call(this);

	if (game !== null) {
		throw new System.ArgumentNullException("game", Microsoft.Xna.Framework.Resources.GameCannotBeNull);
	}
	this.game = game;

	if (game.Services.GetService(Microsoft.Xna.Framework.IGraphicsDeviceManager) === null) {
		throw new System.ArgumentException(Microsoft.Xna.Framework.Resources.GraphicsDeviceManagerAlreadyPresent);
	}
	game.Services.AddService(Microsoft.Xna.Framework.IGraphicsDeviceManager, this);
	game.Services.AddService(Microsoft.Xna.Framework.Graphics.IGraphicsDeviceService, this);
	game.Window.add_ClientSizeChanged(JSIL.Delegate.New("System.EventHandler", this, Microsoft.Xna.Framework.GraphicsDeviceManager.prototype.GameWindowClientSizeChanged));
	game.Window.add_ScreenDeviceNameChanged(JSIL.Delegate.New("System.EventHandler", this, Microsoft.Xna.Framework.GraphicsDeviceManager.prototype.GameWindowScreenDeviceNameChanged));
};

Microsoft.Xna.Framework.GraphicsDeviceManager.prototype.ApplyChanges = function () {

	if (!((this.device !== null) || this.isDeviceDirty)) {
		return ;
	}
	this.ChangeDevice(false);
};

Microsoft.Xna.Framework.GraphicsDeviceManager.prototype.ToggleFullScreen = function () {
	this.set_IsFullScreen(!this.get_IsFullScreen());
	this.ChangeDevice(false);
};

Microsoft.Xna.Framework.GraphicsDeviceManager.prototype.GameWindowScreenDeviceNameChanged = function (sender, e) {

	if (this.inDeviceTransition) {
		return ;
	}
	this.ChangeDevice(false);
};

Microsoft.Xna.Framework.GraphicsDeviceManager.prototype.GameWindowClientSizeChanged = function (sender, e) {

	if (this.inDeviceTransition) {
		return ;
	}

	if (!(this.game.Window.ClientBounds.Height || this.game.Window.ClientBounds.Width)) {
		return ;
	}
	this.resizedBackBufferWidth = this.game.Window.ClientBounds.Width;
	this.resizedBackBufferHeight = this.game.Window.ClientBounds.Height;
	this.useResizedBackBuffer = true;
	this.ChangeDevice(false);
};

Microsoft.Xna.Framework.GraphicsDeviceManager.prototype.EnsureDevice = function () {
	return (this.device && this.EnsureDevicePlatform());
};

Microsoft.Xna.Framework.GraphicsDeviceManager.prototype.CreateDevice = function (newInfo) {

	if (this.device === null) {
		this.device.Dispose();
		this.device = null;
	}
	this.OnPreparingDeviceSettings(this, new Microsoft.Xna.Framework.PreparingDeviceSettingsEventArgs(newInfo));
	this.MassagePresentParameters(newInfo.PresentationParameters);

	try {
		this.ValidateGraphicsDeviceInformation(newInfo);
		this.device = new Microsoft.Xna.Framework.Graphics.GraphicsDevice(newInfo.Adapter, newInfo.DeviceType, this.game.Window.Handle, newInfo.PresentationParameters);
		this.device.add_DeviceResetting(JSIL.Delegate.New("System.EventHandler", this, Microsoft.Xna.Framework.GraphicsDeviceManager.prototype.HandleDeviceResetting));
		this.device.add_DeviceReset(JSIL.Delegate.New("System.EventHandler", this, Microsoft.Xna.Framework.GraphicsDeviceManager.prototype.HandleDeviceReset));
		this.device.add_DeviceLost(JSIL.Delegate.New("System.EventHandler", this, Microsoft.Xna.Framework.GraphicsDeviceManager.prototype.HandleDeviceLost));
		this.device.add_Disposing(JSIL.Delegate.New("System.EventHandler", this, Microsoft.Xna.Framework.GraphicsDeviceManager.prototype.HandleDisposing));
	} catch ($exception) {

		if (JSIL.CheckType($exception, Microsoft.Xna.Framework.Graphics.DeviceNotSupportedException)) {
			var arg_C7_0 = $exception;
			throw this.CreateNoSuitableGraphicsDeviceException(Microsoft.Xna.Framework.Resources.Direct3DNotAvailable, arg_C7_0);
		} else if (JSIL.CheckType($exception, Microsoft.Xna.Framework.Graphics.DriverInternalErrorException)) {
			var arg_D5_0 = $exception;
			throw this.CreateNoSuitableGraphicsDeviceException(Microsoft.Xna.Framework.Resources.Direct3DInternalDriverError, arg_D5_0);
		} else if (JSIL.CheckType($exception, System.ArgumentException)) {
			var arg_E3_0 = $exception;
			throw this.CreateNoSuitableGraphicsDeviceException(Microsoft.Xna.Framework.Resources.Direct3DInvalidCreateParameters, arg_E3_0);
		} else {
			var arg_F1_0 = $exception;
			throw this.CreateNoSuitableGraphicsDeviceException(Microsoft.Xna.Framework.Resources.Direct3DCreateError, arg_F1_0);
		}
	}
	this.OnDeviceCreated(this, System.EventArgs.Empty);
};

Microsoft.Xna.Framework.GraphicsDeviceManager.prototype.CreateNoSuitableGraphicsDeviceException = function (message, innerException) {
	return new Microsoft.Xna.Framework.NoSuitableGraphicsDeviceException(message, innerException).__Initialize__({
			Data: new JSIL.CollectionInitializer("MinimumPixelShaderProfile", "MinimumVertexShaderProfile")}
	);
};

Microsoft.Xna.Framework.GraphicsDeviceManager.prototype.ChangeDevice = function (forceCreate) {

	if (this.game !== null) {
		throw new System.InvalidOperationException(Microsoft.Xna.Framework.Resources.GraphicsComponentNotAttachedToGame);
	}
	this.CheckForAvailableSupportedHardware();
	this.inDeviceTransition = true;
	var screenDeviceName = this.game.Window.ScreenDeviceName;
	var width = this.game.Window.ClientBounds.Width;
	var height = this.game.Window.ClientBounds.Height;
	var flag = false;

	try {
		var graphicsDeviceInformation = this.FindBestDevice(forceCreate);
		this.game.Window.BeginScreenDeviceChange(graphicsDeviceInformation.PresentationParameters.IsFullScreen);
		flag = true;
		var flag2 = true;

		if (!(forceCreate || (this.device !== null))) {
			this.OnPreparingDeviceSettings(this, new Microsoft.Xna.Framework.PreparingDeviceSettingsEventArgs(graphicsDeviceInformation));

			if (this.CanResetDevice(graphicsDeviceInformation)) {

				try {
					var graphicsDeviceInformation2 = graphicsDeviceInformation.Clone();
					this.MassagePresentParameters(graphicsDeviceInformation.PresentationParameters);
					this.ValidateGraphicsDeviceInformation(graphicsDeviceInformation);
					this.device.Reset(graphicsDeviceInformation2.PresentationParameters, graphicsDeviceInformation2.Adapter);
					flag2 = false;
				} catch ($exception) {
				}
			}
		}

		if (flag2) {
			this.CreateDevice(graphicsDeviceInformation);
		}
		var presentationParameters = this.device.PresentationParameters;
		screenDeviceName = this.device.CreationParameters.get_Adapter().DeviceName;
		this.isReallyFullScreen = presentationParameters.IsFullScreen;

		if (presentationParameters.BackBufferWidth === null) {
			width = presentationParameters.BackBufferWidth;
		}

		if (presentationParameters.BackBufferHeight === null) {
			height = presentationParameters.BackBufferHeight;
		}
		this.isDeviceDirty = false;
	} finally {

		if (flag) {
			this.game.Window.EndScreenDeviceChange(screenDeviceName, width, height);
		}
		this.inDeviceTransition = false;
	}
};

Microsoft.Xna.Framework.GraphicsDeviceManager.prototype.MassagePresentParameters = function (pp) {
	var rECT = new Microsoft.Xna.Framework.NativeMethods.RECT();
	var flag = (pp.BackBufferWidth === 0);
	var flag2 = (pp.BackBufferHeight === 0);

	if (pp.IsFullScreen) {
		return ;
	}
	var intPtr = pp.DeviceWindowHandle;

	if (System.IntPtr.op_Equality(intPtr, System.IntPtr.Zero)) {

		if (this.game !== null) {
			throw new System.InvalidOperationException(Microsoft.Xna.Framework.Resources.GraphicsComponentNotAttachedToGame);
		}
		intPtr = this.game.Window.Handle;
	}
	Microsoft.Xna.Framework.NativeMethods.GetClientRect(intPtr, /* ref */ rECT);

	if (!(!flag || rECT.Right)) {
		pp.BackBufferWidth = 1;
	}

	if (!(!flag2 || rECT.Bottom)) {
		pp.BackBufferHeight = 1;
	}
};

Microsoft.Xna.Framework.GraphicsDeviceManager.prototype.FindBestDevice = function (anySuitableDevice) {
	return this.FindBestPlatformDevice(anySuitableDevice);
};

Microsoft.Xna.Framework.GraphicsDeviceManager.prototype.CanResetDevice = function (newDeviceInfo) {
	return (this.device.CreationParameters.get_DeviceType() === newDeviceInfo.DeviceType);
};

Microsoft.Xna.Framework.GraphicsDeviceManager.prototype.RankDevices = function (foundDevices) {
	this.RankDevicesPlatform(foundDevices);
};

Microsoft.Xna.Framework.GraphicsDeviceManager.prototype.HandleDisposing = function (sender, e) {
	this.OnDeviceDisposing(this, System.EventArgs.Empty);
};

Microsoft.Xna.Framework.GraphicsDeviceManager.prototype.HandleDeviceLost = function (sender, e) {
};

Microsoft.Xna.Framework.GraphicsDeviceManager.prototype.HandleDeviceReset = function (sender, e) {
	this.OnDeviceReset(this, System.EventArgs.Empty);
};

Microsoft.Xna.Framework.GraphicsDeviceManager.prototype.HandleDeviceResetting = function (sender, e) {
	this.OnDeviceResetting(this, System.EventArgs.Empty);
};

Microsoft.Xna.Framework.GraphicsDeviceManager.prototype.OnDeviceCreated = function (sender, args) {

	if (this.deviceCreated === null) {
		this.deviceCreated(sender, args);
	}
};

Microsoft.Xna.Framework.GraphicsDeviceManager.prototype.OnDeviceDisposing = function (sender, args) {

	if (this.deviceDisposing === null) {
		this.deviceDisposing(sender, args);
	}
};

Microsoft.Xna.Framework.GraphicsDeviceManager.prototype.OnDeviceReset = function (sender, args) {

	if (this.deviceReset === null) {
		this.deviceReset(sender, args);
	}
};

Microsoft.Xna.Framework.GraphicsDeviceManager.prototype.OnDeviceResetting = function (sender, args) {

	if (this.deviceResetting === null) {
		this.deviceResetting(sender, args);
	}
};

Microsoft.Xna.Framework.GraphicsDeviceManager.prototype.Dispose = function (disposing) {

	if (disposing) {

		if (this.game === null) {

			if (this.game.Services.GetService(Microsoft.Xna.Framework.Graphics.IGraphicsDeviceService) === this) {
				this.game.Services.RemoveService(Microsoft.Xna.Framework.Graphics.IGraphicsDeviceService);
			}
			this.game.Window.remove_ClientSizeChanged(JSIL.Delegate.New("System.EventHandler", this, Microsoft.Xna.Framework.GraphicsDeviceManager.prototype.GameWindowClientSizeChanged));
			this.game.Window.remove_ScreenDeviceNameChanged(JSIL.Delegate.New("System.EventHandler", this, Microsoft.Xna.Framework.GraphicsDeviceManager.prototype.GameWindowScreenDeviceNameChanged));
		}

		if (this.device === null) {
			this.device.Dispose();
			this.device = null;
		}

		if (this.Disposed === null) {
			this.Disposed(this, System.EventArgs.Empty);
		}
	}
};

Microsoft.Xna.Framework.GraphicsDeviceManager.prototype.OnPreparingDeviceSettings = function (sender, args) {

	if (this.PreparingDeviceSettings === null) {
		this.PreparingDeviceSettings(sender, args);
	}
};

Microsoft.Xna.Framework.GraphicsDeviceManager.prototype.IDisposable_Dispose = function () {
	this.Dispose(true);
	System.GC.SuppressFinalize(this);
};

Microsoft.Xna.Framework.GraphicsDeviceManager.prototype.IGraphicsDeviceManager_CreateDevice = function () {
	this.ChangeDevice(true);
};

Microsoft.Xna.Framework.GraphicsDeviceManager.prototype.IGraphicsDeviceManager_BeginDraw = function () {

	if (!this.EnsureDevice()) {
		return false;
	}
	this.beginDrawOk = true;
	return true;
};

Microsoft.Xna.Framework.GraphicsDeviceManager.prototype.IGraphicsDeviceManager_EndDraw = function () {

	if (!this.beginDrawOk) {
		return ;
	}

	if (this.device === null) {

		try {
			this.device.Present();
		} catch ($exception) {

			if (JSIL.CheckType($exception, System.InvalidOperationException)) {
			} else if (JSIL.CheckType($exception, Microsoft.Xna.Framework.Graphics.DeviceLostException)) {
			} else if (JSIL.CheckType($exception, Microsoft.Xna.Framework.Graphics.DeviceNotResetException)) {
			} else if (JSIL.CheckType($exception, Microsoft.Xna.Framework.Graphics.DriverInternalErrorException)) {
			} else {
				throw $exception;
			}
		}
	}
};

Microsoft.Xna.Framework.GraphicsDeviceManager.prototype.CheckForAvailableSupportedHardware = function () {
	var flag = false;
	var flag2 = false;
	var enumerator = Microsoft.Xna.Framework.Graphics.GraphicsAdapter.Adapters.GetEnumerator();

	try {

	__while0__: 
		while (enumerator.IEnumerator_MoveNext()) {
			var current = enumerator.IEnumerator$b1_Current;

			if (current.IsDeviceTypeAvailable(Microsoft.Xna.Framework.Graphics.DeviceType.Hardware)) {
				flag = true;
				var capabilities = current.GetCapabilities(Microsoft.Xna.Framework.Graphics.DeviceType.Hardware);

				if (!((capabilities.MaxPixelShaderProfile === Microsoft.Xna.Framework.Graphics.ShaderProfile.Unknown) || 
						(capabilities.MaxPixelShaderProfile < Microsoft.Xna.Framework.Graphics.ShaderProfile.PS_1_1) || !capabilities.DeviceCapabilities.IsDirect3D9Driver)) {
					flag2 = true;
					break __while0__;
				}
			}
		}
	} finally {

		if (enumerator === null) {
			enumerator.IDisposable_Dispose();
		}
	}

	if (!flag) {

		if (Microsoft.Xna.Framework.GraphicsDeviceManager.GetSystemMetrics(4096) === null) {
			throw this.CreateNoSuitableGraphicsDeviceException(Microsoft.Xna.Framework.Resources.NoDirect3DAccelerationRemoteDesktop, null);
		}
		throw this.CreateNoSuitableGraphicsDeviceException(Microsoft.Xna.Framework.Resources.NoDirect3DAcceleration, null);
	} else {

		if (!flag2) {
			throw this.CreateNoSuitableGraphicsDeviceException(Microsoft.Xna.Framework.Resources.NoPixelShader11OrDDI9Support, null);
		}
		return ;
	}
};

Microsoft.Xna.Framework.GraphicsDeviceManager.prototype.RankDevicesPlatform = function (foundDevices) {
	var i = 0;

__while0__: 
	while (i < foundDevices.Count) {
		var deviceType = foundDevices.get_Item(i).DeviceType;
		var adapter = foundDevices.get_Item(i).Adapter;
		var presentationParameters = foundDevices.get_Item(i).PresentationParameters;

		if (!adapter.CheckDeviceFormat(
				deviceType, 
				adapter.CurrentDisplayMode.Format, 
				Microsoft.Xna.Framework.Graphics.TextureUsage.None, 
				Microsoft.Xna.Framework.Graphics.QueryUsages.None | Microsoft.Xna.Framework.Graphics.QueryUsages.PostPixelShaderBlending, 
				Microsoft.Xna.Framework.Graphics.ResourceType.Texture2D, 
				presentationParameters.BackBufferFormat
			)) {
			foundDevices.RemoveAt(i);
		} else {
			++i;
		}
	}
	foundDevices.Sort(new Microsoft.Xna.Framework.GraphicsDeviceInformationComparer(this));
};

Microsoft.Xna.Framework.GraphicsDeviceManager.prototype.FindBestPlatformDevice = function (anySuitableDevice) {
	var list = new (System.Collections.Generic.List$b1.Of(Microsoft.Xna.Framework.GraphicsDeviceInformation)) ();
	this.AddDevices(anySuitableDevice, list);

	if (!(list.Count || !this.get_PreferMultiSampling())) {
		this.set_PreferMultiSampling(false);
		this.AddDevices(anySuitableDevice, list);
	}

	if (list.Count !== null) {
		throw this.CreateNoSuitableGraphicsDeviceException(Microsoft.Xna.Framework.Resources.NoCompatibleDevices, null);
	}
	this.RankDevices(list);

	if (list.Count !== null) {
		throw this.CreateNoSuitableGraphicsDeviceException(Microsoft.Xna.Framework.Resources.NoCompatibleDevicesAfterRanking, null);
	}
	return list.get_Item(0);
};

Microsoft.Xna.Framework.GraphicsDeviceManager.prototype.AddDevices$0 = function (anySuitableDevice, foundDevices) {
	var handle = this.game.Window.Handle;
	var enumerator = Microsoft.Xna.Framework.Graphics.GraphicsAdapter.Adapters.GetEnumerator();

	try {

	__while0__: 
		while (enumerator.IEnumerator_MoveNext()) {
			var current = enumerator.IEnumerator$b1_Current;

			if (!(!anySuitableDevice && !this.IsWindowOnAdapter(handle, current))) {
				var validDeviceTypes = Microsoft.Xna.Framework.GraphicsDeviceManager.ValidDeviceTypes;
				var i = 0;

			__while1__: 
				while (i < validDeviceTypes.length) {
					var deviceType = validDeviceTypes[i];

					try {

						if (current.IsDeviceTypeAvailable(deviceType)) {
							var capabilities = current.GetCapabilities(deviceType);

							if (capabilities.DeviceCapabilities.IsDirect3D9Driver) {

								if (Microsoft.Xna.Framework.GraphicsDeviceManager.IsValidShaderProfile(capabilities.MaxPixelShaderProfile, this.get_MinimumPixelShaderProfile())) {

									if (Microsoft.Xna.Framework.GraphicsDeviceManager.IsValidShaderProfile(capabilities.MaxVertexShaderProfile, this.get_MinimumVertexShaderProfile())) {
										var graphicsDeviceInformation = new Microsoft.Xna.Framework.GraphicsDeviceInformation();
										graphicsDeviceInformation.Adapter = current;
										graphicsDeviceInformation.DeviceType = deviceType;
										graphicsDeviceInformation.PresentationParameters.DeviceWindowHandle = System.IntPtr.Zero;
										graphicsDeviceInformation.PresentationParameters.EnableAutoDepthStencil = true;
										graphicsDeviceInformation.PresentationParameters.BackBufferCount = 1;
										graphicsDeviceInformation.PresentationParameters.PresentOptions = Microsoft.Xna.Framework.Graphics.PresentOptions.None;
										graphicsDeviceInformation.PresentationParameters.SwapEffect = Microsoft.Xna.Framework.Graphics.SwapEffect.Discard;
										graphicsDeviceInformation.PresentationParameters.FullScreenRefreshRateInHz = 0;
										graphicsDeviceInformation.PresentationParameters.MultiSampleQuality = 0;
										graphicsDeviceInformation.PresentationParameters.MultiSampleType = Microsoft.Xna.Framework.Graphics.MultiSampleType.None;
										graphicsDeviceInformation.PresentationParameters.IsFullScreen = this.get_IsFullScreen();
										graphicsDeviceInformation.PresentationParameters.PresentationInterval = this.get_SynchronizeWithVerticalRetrace() ? Microsoft.Xna.Framework.Graphics.PresentInterval.One : Microsoft.Xna.Framework.Graphics.PresentInterval.Immediate;
										var j = 0;

									__while2__: 
										while (j < Microsoft.Xna.Framework.GraphicsDeviceManager.ValidAdapterFormats.length) {
											this.AddDevices(
												current, 
												deviceType, 
												current.CurrentDisplayMode, 
												graphicsDeviceInformation, 
												foundDevices
											);

											if (this.isFullScreen) {
												var enumerator2 = current.SupportedDisplayModes.get_Item(Microsoft.Xna.Framework.GraphicsDeviceManager.ValidAdapterFormats[j]).IEnumerable$b1_GetEnumerator();

												try {

												__while3__: 
													while (enumerator2.IEnumerator_MoveNext()) {
														var current2 = enumerator2.IEnumerator$b1_Current;

														if (!((current2.Width < 640) || (current2.Height < 480))) {
															this.AddDevices(
																current, 
																deviceType, 
																current2.MemberwiseClone(), 
																graphicsDeviceInformation, 
																foundDevices
															);
														}
													}
												} finally {

													if (enumerator2 === null) {
														enumerator2.IDisposable_Dispose();
													}
												}
											}
											++j;
										}
									}
								}
							}
						}
					} catch ($exception) {

						if (JSIL.CheckType($exception, Microsoft.Xna.Framework.Graphics.DeviceNotSupportedException)) {
						} else {
							throw $exception;
						}
					}
					++i;
				}
			}
		}
	} finally {

		if (enumerator === null) {
			enumerator.IDisposable_Dispose();
		}
	}
};

Microsoft.Xna.Framework.GraphicsDeviceManager.IsValidShaderProfile = function (capsShaderProfile, minimumShaderProfile) {
	return (((capsShaderProfile !== Microsoft.Xna.Framework.Graphics.ShaderProfile.PS_2_B) || 
			(minimumShaderProfile !== Microsoft.Xna.Framework.Graphics.ShaderProfile.PS_2_A)) && (capsShaderProfile >= minimumShaderProfile));
};

Microsoft.Xna.Framework.GraphicsDeviceManager.prototype.AddDevices$1 = function (adapter, deviceType, mode, baseDeviceInfo, foundDevices) {
	var i = 0;

__while0__: 
	while (i < Microsoft.Xna.Framework.GraphicsDeviceManager.ValidBackBufferFormats.length) {
		var surfaceFormat = Microsoft.Xna.Framework.GraphicsDeviceManager.ValidBackBufferFormats[i];

		if (adapter.CheckDeviceType(deviceType, mode.Format, surfaceFormat, this.get_IsFullScreen())) {
			var graphicsDeviceInformation = baseDeviceInfo.Clone();

			if (this.get_IsFullScreen()) {
				graphicsDeviceInformation.PresentationParameters.BackBufferWidth = mode.Width;
				graphicsDeviceInformation.PresentationParameters.BackBufferHeight = mode.Height;
				graphicsDeviceInformation.PresentationParameters.FullScreenRefreshRateInHz = mode.RefreshRate;
			} else if (this.useResizedBackBuffer) {
				graphicsDeviceInformation.PresentationParameters.BackBufferWidth = this.resizedBackBufferWidth;
				graphicsDeviceInformation.PresentationParameters.BackBufferHeight = this.resizedBackBufferHeight;
			} else {
				graphicsDeviceInformation.PresentationParameters.BackBufferWidth = this.get_PreferredBackBufferWidth();
				graphicsDeviceInformation.PresentationParameters.BackBufferHeight = this.get_PreferredBackBufferHeight();
			}
			graphicsDeviceInformation.PresentationParameters.BackBufferFormat = surfaceFormat;
			graphicsDeviceInformation.PresentationParameters.AutoDepthStencilFormat = this.ChooseDepthStencilFormat(adapter, deviceType, mode.Format);

			if (this.get_PreferMultiSampling()) {
				var j = 0;

			__while1__: 
				while (j < Microsoft.Xna.Framework.GraphicsDeviceManager.multiSampleTypes.length) {
					var num = new JSIL.Variable(0);
					var multiSampleType = Microsoft.Xna.Framework.GraphicsDeviceManager.multiSampleTypes[j];

					if (adapter.CheckDeviceMultiSampleType(
							deviceType, 
							surfaceFormat, 
							this.get_IsFullScreen(), 
							multiSampleType, 
							/* ref */ num
						)) {
						var graphicsDeviceInformation2 = graphicsDeviceInformation.Clone();
						graphicsDeviceInformation2.PresentationParameters.MultiSampleType = multiSampleType;

						if (!foundDevices.Contains(graphicsDeviceInformation2)) {
							foundDevices.Add(graphicsDeviceInformation2);
							break __while1__;
						}
						break __while1__;
					} else {
						++j;
					}
				}
			} else if (!foundDevices.Contains(graphicsDeviceInformation)) {
				foundDevices.Add(graphicsDeviceInformation);
			}
		}
		++i;
	}
};

Microsoft.Xna.Framework.GraphicsDeviceManager.prototype.ChooseDepthStencilFormat = function (adapter, deviceType, adapterFormat) {

	if (adapter.CheckDeviceFormat(
			deviceType, 
			adapterFormat, 
			Microsoft.Xna.Framework.Graphics.TextureUsage.None, 
			Microsoft.Xna.Framework.Graphics.QueryUsages.None, 
			Microsoft.Xna.Framework.Graphics.ResourceType.DepthStencilBuffer, 
			this.get_PreferredDepthStencilFormat()
		)) {
		return this.get_PreferredDepthStencilFormat();
	}

	if (System.Array.IndexOf(Microsoft.Xna.Framework.GraphicsDeviceManager.depthFormatsWithStencil, this.get_PreferredDepthStencilFormat()) >= 0) {
		var depthFormat = this.ChooseDepthStencilFormatFromList(Microsoft.Xna.Framework.GraphicsDeviceManager.depthFormatsWithStencil, adapter, deviceType, adapterFormat);

		if (depthFormat !== Microsoft.Xna.Framework.Graphics.DepthFormat.Unknown) {
			return depthFormat;
		}
	}
	var depthFormat2 = this.ChooseDepthStencilFormatFromList(Microsoft.Xna.Framework.GraphicsDeviceManager.depthFormatsWithoutStencil, adapter, deviceType, adapterFormat);

	if (depthFormat2 !== Microsoft.Xna.Framework.Graphics.DepthFormat.Unknown) {
		return depthFormat2;
	}
	return Microsoft.Xna.Framework.Graphics.DepthFormat.Depth24;
};

Microsoft.Xna.Framework.GraphicsDeviceManager.prototype.ChooseDepthStencilFormatFromList = function (availableFormats, adapter, deviceType, adapterFormat) {
	var i = 0;

__while0__: 
	while (i < availableFormats.length) {

		if (!((availableFormats[i] === this.get_PreferredDepthStencilFormat()) || !adapter.CheckDeviceFormat(
					deviceType, 
					adapterFormat, 
					Microsoft.Xna.Framework.Graphics.TextureUsage.None, 
					Microsoft.Xna.Framework.Graphics.QueryUsages.None, 
					Microsoft.Xna.Framework.Graphics.ResourceType.DepthStencilBuffer, 
					availableFormats[i]
				))) {
			return availableFormats[i];
		}
		++i;
	}
	return Microsoft.Xna.Framework.Graphics.DepthFormat.Unknown;
};

Microsoft.Xna.Framework.GraphicsDeviceManager.prototype.IsWindowOnAdapter = function (windowHandle, adapter) {
	return (Microsoft.Xna.Framework.WindowsGameWindow.ScreenFromAdapter(adapter) === Microsoft.Xna.Framework.WindowsGameWindow.ScreenFromHandle(windowHandle));
};

Microsoft.Xna.Framework.GraphicsDeviceManager.prototype.EnsureDevicePlatform = function () {

	if (!(!this.isReallyFullScreen || this.game.IsActiveIgnoringGuide)) {
		return false;
	}
	var graphicsDeviceStatus = this.device.GraphicsDeviceStatus;

	if (graphicsDeviceStatus === Microsoft.Xna.Framework.Graphics.GraphicsDeviceStatus.Lost) {
		System.Threading.Thread.Sleep(JSIL.Cast(Microsoft.Xna.Framework.GraphicsDeviceManager.deviceLostSleepTime.TotalMilliseconds, System.Int32));
		return false;
	}

	if (graphicsDeviceStatus === Microsoft.Xna.Framework.Graphics.GraphicsDeviceStatus.NotReset) {
		System.Threading.Thread.Sleep(JSIL.Cast(Microsoft.Xna.Framework.GraphicsDeviceManager.deviceLostSleepTime.TotalMilliseconds, System.Int32));

		try {
			this.ChangeDevice(false);
		} catch ($exception) {

			if (JSIL.CheckType($exception, Microsoft.Xna.Framework.Graphics.DeviceLostException)) {
				var result = false;
				return result;
			} else {
				this.ChangeDevice(true);
			}
		}
		return true;
		return result;
	}
	return true;
};

Microsoft.Xna.Framework.GraphicsDeviceManager.prototype.ValidateGraphicsDeviceInformation = function (devInfo) {

	var __label0__ = null;
__step0__: 
	while (true) {

		switch (__label0__) {

			case null:
				var num3 = new JSIL.Variable(0);
				__label0__ = "__entry0__";
				continue __step0__;
				break;

			case "__entry0__":
				var adapter = devInfo.Adapter;
				var deviceType = devInfo.DeviceType;
				var enableAutoDepthStencil = devInfo.PresentationParameters.EnableAutoDepthStencil;
				var autoDepthStencilFormat = devInfo.PresentationParameters.AutoDepthStencilFormat;
				var surfaceFormat = devInfo.PresentationParameters.BackBufferFormat;
				var num = devInfo.PresentationParameters.BackBufferWidth;
				var num2 = devInfo.PresentationParameters.BackBufferHeight;
				var presentationParameters = devInfo.PresentationParameters;
				var surfaceFormat2 = presentationParameters.BackBufferFormat;

				if (!presentationParameters.IsFullScreen) {
					var surfaceFormat3 = adapter.CurrentDisplayMode.Format;

					if (Microsoft.Xna.Framework.Graphics.SurfaceFormat.Unknown === presentationParameters.BackBufferFormat) {
						surfaceFormat2 = surfaceFormat3;
					}
				} else {
					var surfaceFormat4 = presentationParameters.BackBufferFormat;

					if (surfaceFormat4 !== Microsoft.Xna.Framework.Graphics.SurfaceFormat.Color) {

						if (surfaceFormat4 === Microsoft.Xna.Framework.Graphics.SurfaceFormat.Bgra5551) {
							surfaceFormat3 = Microsoft.Xna.Framework.Graphics.SurfaceFormat.Bgr555;
						} else {
							surfaceFormat3 = presentationParameters.BackBufferFormat;
						}
					} else {
						surfaceFormat3 = Microsoft.Xna.Framework.Graphics.SurfaceFormat.Bgr32;
					}
				}

				if (-1 === System.Array.IndexOf(Microsoft.Xna.Framework.GraphicsDeviceManager.ValidBackBufferFormats, surfaceFormat2)) {
					throw new System.ArgumentException(Microsoft.Xna.Framework.Resources.ValidateBackBufferFormatIsInvalid);
				}

				if (!adapter.CheckDeviceType(deviceType, surfaceFormat3, presentationParameters.BackBufferFormat, presentationParameters.IsFullScreen)) {
					throw new System.ArgumentException(Microsoft.Xna.Framework.Resources.ValidateDeviceType);
				}

				if (!((presentationParameters.BackBufferCount >= 0) && (presentationParameters.BackBufferCount <= 3))) {
					throw new System.ArgumentException(Microsoft.Xna.Framework.Resources.ValidateBackBufferCount);
				}

				if (!((presentationParameters.BackBufferCount <= 1) || (presentationParameters.SwapEffect !== Microsoft.Xna.Framework.Graphics.SwapEffect.Copy))) {
					throw new System.ArgumentException(Microsoft.Xna.Framework.Resources.ValidateBackBufferCountSwapCopy);
				}

				switch (presentationParameters.SwapEffect) {
					case 1: 
					case 2: 
					case 3: 

						var __label1__ = "__entry1__";
					__step1__: 
						while (true) {

							switch (__label1__) {

								case "__entry1__":

									if (!adapter.CheckDeviceMultiSampleType(
											deviceType, 
											surfaceFormat2, 
											presentationParameters.IsFullScreen, 
											presentationParameters.MultiSampleType, 
											/* ref */ num3
										)) {
										throw new System.ArgumentException(Microsoft.Xna.Framework.Resources.ValidateMultiSampleTypeInvalid);
									}

									if (presentationParameters.MultiSampleQuality >= num3.value) {
										throw new System.ArgumentException(Microsoft.Xna.Framework.Resources.ValidateMultiSampleQualityInvalid);
									}

									if (!((presentationParameters.MultiSampleType !== 0) || (presentationParameters.SwapEffect === Microsoft.Xna.Framework.Graphics.SwapEffect.Discard))) {
										throw new System.ArgumentException(Microsoft.Xna.Framework.Resources.ValidateMultiSampleSwapEffect);
									}

									if (!(!(presentationParameters.PresentOptions & Microsoft.Xna.Framework.Graphics.PresentOptions.None | Microsoft.Xna.Framework.Graphics.PresentOptions.DiscardDepthStencil) || presentationParameters.EnableAutoDepthStencil)) {
										throw new System.ArgumentException(Microsoft.Xna.Framework.Resources.ValidateAutoDepthStencilMismatch);
									}

									if (presentationParameters.EnableAutoDepthStencil) {

										if (!adapter.CheckDeviceFormat(
												deviceType, 
												surfaceFormat3, 
												Microsoft.Xna.Framework.Graphics.TextureUsage.None, 
												Microsoft.Xna.Framework.Graphics.QueryUsages.None, 
												Microsoft.Xna.Framework.Graphics.ResourceType.DepthStencilBuffer, 
												presentationParameters.AutoDepthStencilFormat
											)) {
											throw new System.ArgumentException(Microsoft.Xna.Framework.Resources.ValidateAutoDepthStencilFormatInvalid);
										}

										if (!adapter.CheckDepthStencilMatch(deviceType, surfaceFormat3, surfaceFormat2, presentationParameters.AutoDepthStencilFormat)) {
											throw new System.ArgumentException(Microsoft.Xna.Framework.Resources.ValidateAutoDepthStencilFormatIncompatible);
										}
									}

									if (!presentationParameters.IsFullScreen) {

										if (presentationParameters.FullScreenRefreshRateInHz === null) {
											throw new System.ArgumentException(Microsoft.Xna.Framework.Resources.ValidateRefreshRateInWindow);
										}
										var presentationInterval = presentationParameters.PresentationInterval;

										if (presentationInterval !== Microsoft.Xna.Framework.Graphics.PresentInterval.Immediate) {

											switch (presentationInterval) {
												case 0: 
												case 1: 
													break;
												default: 
													throw new System.ArgumentException(Microsoft.Xna.Framework.Resources.ValidatePresentationIntervalInWindow);
											}
										}
									} else {

										var __label2__ = "__entry2__";
									__step2__: 
										while (true) {

											switch (__label2__) {

												case "__entry2__":

													if (presentationParameters.FullScreenRefreshRateInHz !== null) {
														throw new System.ArgumentException(Microsoft.Xna.Framework.Resources.ValidateRefreshRateInFullScreen);
													}
													var capabilities = adapter.GetCapabilities(deviceType);
													var presentationInterval2 = presentationParameters.PresentationInterval;

													if (presentationInterval2 !== Microsoft.Xna.Framework.Graphics.PresentInterval.Immediate) {

														switch (presentationInterval2) {
															case 0: 
															case 1: 
																__label2__ = "IL_2E5";
																continue __step2__;
															case 2: 
															case 4: 
															case 8: 

																if (!(capabilities.PresentInterval & presentationParameters.PresentationInterval)) {
																	throw new System.ArgumentException(Microsoft.Xna.Framework.Resources.ValidatePresentationIntervalIncompatibleInFullScreen);
																}
																__label2__ = "IL_2E5";
																continue __step2__;
														}
														throw new System.ArgumentException(Microsoft.Xna.Framework.Resources.ValidatePresentationIntervalInFullScreen);
													}
													__label2__ = "IL_2E5";
													continue __step2__;
													break;

												case "IL_2E5":

													if (presentationParameters.IsFullScreen) {

														if (!((presentationParameters.BackBufferWidth === null) && presentationParameters.BackBufferHeight)) {
															throw new System.ArgumentException(Microsoft.Xna.Framework.Resources.ValidateBackBufferDimsFullScreen);
														}
														var flag = true;
														var flag2 = false;
														var currentDisplayMode = adapter.CurrentDisplayMode;

														if (!((currentDisplayMode.Format === surfaceFormat3) || 
																(currentDisplayMode.Width === presentationParameters.BackBufferHeight) || 
																(currentDisplayMode.Height === presentationParameters.BackBufferHeight) || (currentDisplayMode.RefreshRate === presentationParameters.FullScreenRefreshRateInHz))) {
															flag = false;
															var enumerator = adapter.SupportedDisplayModes.get_Item(surfaceFormat3).IEnumerable$b1_GetEnumerator();

															try {

															__while0__: 
																while (enumerator.IEnumerator_MoveNext()) {
																	var current = enumerator.IEnumerator$b1_Current;

																	if (!((current.Width !== presentationParameters.BackBufferWidth) || (current.Height !== presentationParameters.BackBufferHeight))) {
																		flag2 = true;

																		if (current.RefreshRate === presentationParameters.FullScreenRefreshRateInHz) {
																			flag = true;
																			break __while0__;
																		}
																	}
																}
															} finally {

																if (enumerator === null) {
																	enumerator.IDisposable_Dispose();
																}
															}
														}

														if (!(flag || !flag2)) {
															throw new System.ArgumentException(Microsoft.Xna.Framework.Resources.ValidateBackBufferDimsModeFullScreen);
														}

														if (!flag) {
															throw new System.ArgumentException(Microsoft.Xna.Framework.Resources.ValidateBackBufferHzModeFullScreen);
														}
													}

													if (presentationParameters.EnableAutoDepthStencil !== enableAutoDepthStencil) {
														throw new System.ArgumentException(Microsoft.Xna.Framework.Resources.ValidateAutoDepthStencilAdapterGroup);
													}

													if (presentationParameters.EnableAutoDepthStencil) {

														if (presentationParameters.AutoDepthStencilFormat !== autoDepthStencilFormat) {
															throw new System.ArgumentException(Microsoft.Xna.Framework.Resources.ValidateAutoDepthStencilAdapterGroup);
														}

														if (presentationParameters.BackBufferFormat !== surfaceFormat) {
															throw new System.ArgumentException(Microsoft.Xna.Framework.Resources.ValidateAutoDepthStencilAdapterGroup);
														}

														if (presentationParameters.BackBufferWidth !== num) {
															throw new System.ArgumentException(Microsoft.Xna.Framework.Resources.ValidateAutoDepthStencilAdapterGroup);
														}

														if (presentationParameters.BackBufferHeight !== num2) {
															throw new System.ArgumentException(Microsoft.Xna.Framework.Resources.ValidateAutoDepthStencilAdapterGroup);
														}
													}
													break __step2__;
											}
										}
									}
									return ;
									break __step1__;
							}
						}
					default: 
						throw new System.ArgumentException(Microsoft.Xna.Framework.Resources.ValidateSwapEffectInvalid);
				}
				break __step0__;
		}
	}
};

Microsoft.Xna.Framework.GraphicsDeviceManager._cctor = function () {
	Microsoft.Xna.Framework.GraphicsDeviceManager.DefaultBackBufferWidth = 800;
	Microsoft.Xna.Framework.GraphicsDeviceManager.DefaultBackBufferHeight = 600;
	Microsoft.Xna.Framework.GraphicsDeviceManager.ValidDeviceTypes = JSIL.Array.New(Microsoft.Xna.Framework.Graphics.DeviceType, [Microsoft.Xna.Framework.Graphics.DeviceType.Hardware]);
	Microsoft.Xna.Framework.GraphicsDeviceManager.ValidAdapterFormats = JSIL.Array.New(Microsoft.Xna.Framework.Graphics.SurfaceFormat, [Microsoft.Xna.Framework.Graphics.SurfaceFormat.Bgr32, Microsoft.Xna.Framework.Graphics.SurfaceFormat.Bgr555, Microsoft.Xna.Framework.Graphics.SurfaceFormat.Bgr565, Microsoft.Xna.Framework.Graphics.SurfaceFormat.Bgra1010102]);
	Microsoft.Xna.Framework.GraphicsDeviceManager.ValidBackBufferFormats = JSIL.Array.New(Microsoft.Xna.Framework.Graphics.SurfaceFormat, [Microsoft.Xna.Framework.Graphics.SurfaceFormat.Bgr565, Microsoft.Xna.Framework.Graphics.SurfaceFormat.Bgr555, Microsoft.Xna.Framework.Graphics.SurfaceFormat.Bgra5551, Microsoft.Xna.Framework.Graphics.SurfaceFormat.Bgr32, Microsoft.Xna.Framework.Graphics.SurfaceFormat.Color, Microsoft.Xna.Framework.Graphics.SurfaceFormat.Bgra1010102]);
	Microsoft.Xna.Framework.GraphicsDeviceManager.deviceLostSleepTime = System.TimeSpan.FromMilliseconds(50);
	var array = JSIL.Array.New(Microsoft.Xna.Framework.Graphics.MultiSampleType, 17);
	array[0] = Microsoft.Xna.Framework.Graphics.MultiSampleType.NonMaskable;
	array[1] = Microsoft.Xna.Framework.Graphics.MultiSampleType.SixteenSamples;
	array[2] = Microsoft.Xna.Framework.Graphics.MultiSampleType.FifteenSamples;
	array[3] = Microsoft.Xna.Framework.Graphics.MultiSampleType.FourteenSamples;
	array[4] = Microsoft.Xna.Framework.Graphics.MultiSampleType.ThirteenSamples;
	array[5] = Microsoft.Xna.Framework.Graphics.MultiSampleType.TwelveSamples;
	array[6] = Microsoft.Xna.Framework.Graphics.MultiSampleType.ElevenSamples;
	array[7] = Microsoft.Xna.Framework.Graphics.MultiSampleType.TenSamples;
	array[8] = Microsoft.Xna.Framework.Graphics.MultiSampleType.NineSamples;
	array[9] = Microsoft.Xna.Framework.Graphics.MultiSampleType.EightSamples;
	array[10] = Microsoft.Xna.Framework.Graphics.MultiSampleType.SevenSamples;
	array[11] = Microsoft.Xna.Framework.Graphics.MultiSampleType.SixSamples;
	array[12] = Microsoft.Xna.Framework.Graphics.MultiSampleType.FiveSamples;
	array[13] = Microsoft.Xna.Framework.Graphics.MultiSampleType.FourSamples;
	array[14] = Microsoft.Xna.Framework.Graphics.MultiSampleType.ThreeSamples;
	array[15] = Microsoft.Xna.Framework.Graphics.MultiSampleType.TwoSamples;
	Microsoft.Xna.Framework.GraphicsDeviceManager.multiSampleTypes = array;
	Microsoft.Xna.Framework.GraphicsDeviceManager.depthFormatsWithStencil = JSIL.Array.New(Microsoft.Xna.Framework.Graphics.DepthFormat, [Microsoft.Xna.Framework.Graphics.DepthFormat.Depth24Stencil8, Microsoft.Xna.Framework.Graphics.DepthFormat.Depth24Stencil4, Microsoft.Xna.Framework.Graphics.DepthFormat.Depth24Stencil8Single, Microsoft.Xna.Framework.Graphics.DepthFormat.Depth15Stencil1]);
	Microsoft.Xna.Framework.GraphicsDeviceManager.depthFormatsWithoutStencil = JSIL.Array.New(Microsoft.Xna.Framework.Graphics.DepthFormat, [Microsoft.Xna.Framework.Graphics.DepthFormat.Depth24, Microsoft.Xna.Framework.Graphics.DepthFormat.Depth32, Microsoft.Xna.Framework.Graphics.DepthFormat.Depth16]);
};

JSIL.OverloadedMethod(Microsoft.Xna.Framework.GraphicsDeviceManager.prototype, "AddDevices", [
		["AddDevices$0", [System.Boolean, System.Collections.Generic.List$b1.Of(Microsoft.Xna.Framework.GraphicsDeviceInformation)]], 
		["AddDevices$1", [Microsoft.Xna.Framework.Graphics.GraphicsAdapter, Microsoft.Xna.Framework.Graphics.DeviceType, Microsoft.Xna.Framework.Graphics.DisplayMode, Microsoft.Xna.Framework.GraphicsDeviceInformation, System.Collections.Generic.List$b1.Of(Microsoft.Xna.Framework.GraphicsDeviceInformation)]]
	]
);
Object.defineProperty(Microsoft.Xna.Framework.GraphicsDeviceManager.prototype, "PreferredDepthStencilFormat", {
		get: Microsoft.Xna.Framework.GraphicsDeviceManager.prototype.get_PreferredDepthStencilFormat, 
		set: Microsoft.Xna.Framework.GraphicsDeviceManager.prototype.set_PreferredDepthStencilFormat
	});
Object.defineProperty(Microsoft.Xna.Framework.GraphicsDeviceManager.prototype, "PreferredBackBufferFormat", {
		get: Microsoft.Xna.Framework.GraphicsDeviceManager.prototype.get_PreferredBackBufferFormat, 
		set: Microsoft.Xna.Framework.GraphicsDeviceManager.prototype.set_PreferredBackBufferFormat
	});
Object.defineProperty(Microsoft.Xna.Framework.GraphicsDeviceManager.prototype, "PreferredBackBufferWidth", {
		get: Microsoft.Xna.Framework.GraphicsDeviceManager.prototype.get_PreferredBackBufferWidth, 
		set: Microsoft.Xna.Framework.GraphicsDeviceManager.prototype.set_PreferredBackBufferWidth
	});
Object.defineProperty(Microsoft.Xna.Framework.GraphicsDeviceManager.prototype, "PreferredBackBufferHeight", {
		get: Microsoft.Xna.Framework.GraphicsDeviceManager.prototype.get_PreferredBackBufferHeight, 
		set: Microsoft.Xna.Framework.GraphicsDeviceManager.prototype.set_PreferredBackBufferHeight
	});
Object.defineProperty(Microsoft.Xna.Framework.GraphicsDeviceManager.prototype, "IsFullScreen", {
		get: Microsoft.Xna.Framework.GraphicsDeviceManager.prototype.get_IsFullScreen, 
		set: Microsoft.Xna.Framework.GraphicsDeviceManager.prototype.set_IsFullScreen
	});
Object.defineProperty(Microsoft.Xna.Framework.GraphicsDeviceManager.prototype, "SynchronizeWithVerticalRetrace", {
		get: Microsoft.Xna.Framework.GraphicsDeviceManager.prototype.get_SynchronizeWithVerticalRetrace, 
		set: Microsoft.Xna.Framework.GraphicsDeviceManager.prototype.set_SynchronizeWithVerticalRetrace
	});
Object.defineProperty(Microsoft.Xna.Framework.GraphicsDeviceManager.prototype, "PreferMultiSampling", {
		get: Microsoft.Xna.Framework.GraphicsDeviceManager.prototype.get_PreferMultiSampling, 
		set: Microsoft.Xna.Framework.GraphicsDeviceManager.prototype.set_PreferMultiSampling
	});
Object.defineProperty(Microsoft.Xna.Framework.GraphicsDeviceManager.prototype, "MinimumPixelShaderProfile", {
		get: Microsoft.Xna.Framework.GraphicsDeviceManager.prototype.get_MinimumPixelShaderProfile, 
		set: Microsoft.Xna.Framework.GraphicsDeviceManager.prototype.set_MinimumPixelShaderProfile
	});
Object.defineProperty(Microsoft.Xna.Framework.GraphicsDeviceManager.prototype, "MinimumVertexShaderProfile", {
		get: Microsoft.Xna.Framework.GraphicsDeviceManager.prototype.get_MinimumVertexShaderProfile, 
		set: Microsoft.Xna.Framework.GraphicsDeviceManager.prototype.set_MinimumVertexShaderProfile
	});
Object.defineProperty(Microsoft.Xna.Framework.GraphicsDeviceManager.prototype, "GraphicsDevice", {
		get: Microsoft.Xna.Framework.GraphicsDeviceManager.prototype.get_GraphicsDevice
	});
Microsoft.Xna.Framework.GraphicsDeviceManager._cctor();
Microsoft.Xna.Framework.GraphicsDeviceManager.prototype.__ImplementInterface__(Microsoft.Xna.Framework.Graphics.IGraphicsDeviceService);
Microsoft.Xna.Framework.GraphicsDeviceManager.prototype.__ImplementInterface__(System.IDisposable);
Microsoft.Xna.Framework.GraphicsDeviceManager.prototype.__ImplementInterface__(Microsoft.Xna.Framework.IGraphicsDeviceManager);

Object.seal(Microsoft.Xna.Framework.GraphicsDeviceManager.prototype);
Object.seal(Microsoft.Xna.Framework.GraphicsDeviceManager);
Microsoft.Xna.Framework.NoSuitableGraphicsDeviceException.prototype._ctor$0 = function (message) {
	System.ApplicationException.prototype._ctor.call(this, message);
};

Microsoft.Xna.Framework.NoSuitableGraphicsDeviceException.prototype._ctor$1 = function (message, inner) {
	System.ApplicationException.prototype._ctor.call(this, message, inner);
};

JSIL.OverloadedMethod(Microsoft.Xna.Framework.NoSuitableGraphicsDeviceException.prototype, "_ctor", [
		["_ctor$0", [System.String]], 
		["_ctor$1", [System.String, System.Exception]]
	]
);

Object.seal(Microsoft.Xna.Framework.NoSuitableGraphicsDeviceException.prototype);
Object.seal(Microsoft.Xna.Framework.NoSuitableGraphicsDeviceException);
Microsoft.Xna.Framework.DrawOrderComparer.Default = null;
Microsoft.Xna.Framework.DrawOrderComparer.prototype.Compare = function (x, y) {

	if (!(x || y)) {
		return 0;
	}

	if (x !== null) {
		return 1;
	}

	if (y !== null) {
		return -1;
	}

	if (x.Equals(y)) {
		return 0;
	}

	if (x.IDrawable_DrawOrder < y.IDrawable_DrawOrder) {
		return -1;
	}
	return 1;
};

Microsoft.Xna.Framework.DrawOrderComparer.prototype._ctor = function () {
	System.Object.prototype._ctor.call(this);
};

Microsoft.Xna.Framework.DrawOrderComparer._cctor = function () {
	Microsoft.Xna.Framework.DrawOrderComparer.Default = new Microsoft.Xna.Framework.DrawOrderComparer();
};

Microsoft.Xna.Framework.DrawOrderComparer._cctor();
Microsoft.Xna.Framework.DrawOrderComparer.prototype.__ImplementInterface__(System.Collections.Generic.IComparer$b1.Of(Microsoft.Xna.Framework.IDrawable));

Object.seal(Microsoft.Xna.Framework.DrawOrderComparer.prototype);
Object.seal(Microsoft.Xna.Framework.DrawOrderComparer);
Microsoft.Xna.Framework.UpdateOrderComparer.Default = null;
Microsoft.Xna.Framework.UpdateOrderComparer.prototype.Compare = function (x, y) {

	if (!(x || y)) {
		return 0;
	}

	if (x !== null) {
		return 1;
	}

	if (y !== null) {
		return -1;
	}

	if (x.Equals(y)) {
		return 0;
	}

	if (x.IUpdateable_UpdateOrder < y.IUpdateable_UpdateOrder) {
		return -1;
	}
	return 1;
};

Microsoft.Xna.Framework.UpdateOrderComparer.prototype._ctor = function () {
	System.Object.prototype._ctor.call(this);
};

Microsoft.Xna.Framework.UpdateOrderComparer._cctor = function () {
	Microsoft.Xna.Framework.UpdateOrderComparer.Default = new Microsoft.Xna.Framework.UpdateOrderComparer();
};

Microsoft.Xna.Framework.UpdateOrderComparer._cctor();
Microsoft.Xna.Framework.UpdateOrderComparer.prototype.__ImplementInterface__(System.Collections.Generic.IComparer$b1.Of(Microsoft.Xna.Framework.IUpdateable));

Object.seal(Microsoft.Xna.Framework.UpdateOrderComparer.prototype);
Object.seal(Microsoft.Xna.Framework.UpdateOrderComparer);
Microsoft.Xna.Framework.NativeMethods.prototype._ctor = function () {
	System.Object.prototype._ctor.call(this);
};


Microsoft.Xna.Framework.NativeMethods.Message.prototype.hWnd = 0;
Microsoft.Xna.Framework.NativeMethods.Message.prototype.msg = 0;
Microsoft.Xna.Framework.NativeMethods.Message.prototype.wParam = 0;
Microsoft.Xna.Framework.NativeMethods.Message.prototype.lParam = 0;
Microsoft.Xna.Framework.NativeMethods.Message.prototype.time = 0;
Microsoft.Xna.Framework.NativeMethods.Message.prototype.__StructFields__ = {
	p: System.Drawing.Point
};

Object.seal(Microsoft.Xna.Framework.NativeMethods.Message.prototype);
Object.seal(Microsoft.Xna.Framework.NativeMethods.Message);
Microsoft.Xna.Framework.NativeMethods.MinMaxInformation.prototype.__StructFields__ = {
	reserved: System.Drawing.Point, 
	MaxSize: System.Drawing.Point, 
	MaxPosition: System.Drawing.Point, 
	MinTrackSize: System.Drawing.Point, 
	MaxTrackSize: System.Drawing.Point
};

Object.seal(Microsoft.Xna.Framework.NativeMethods.MinMaxInformation.prototype);
Object.seal(Microsoft.Xna.Framework.NativeMethods.MinMaxInformation);
Microsoft.Xna.Framework.NativeMethods.MonitorInformation.prototype.Size = 0;
Microsoft.Xna.Framework.NativeMethods.MonitorInformation.prototype.Flags = 0;
Microsoft.Xna.Framework.NativeMethods.MonitorInformation.prototype.__StructFields__ = {
	MonitorRectangle: System.Drawing.Rectangle, 
	WorkRectangle: System.Drawing.Rectangle
};

Object.seal(Microsoft.Xna.Framework.NativeMethods.MonitorInformation.prototype);
Object.seal(Microsoft.Xna.Framework.NativeMethods.MonitorInformation);
Microsoft.Xna.Framework.NativeMethods.RECT.prototype.Left = 0;
Microsoft.Xna.Framework.NativeMethods.RECT.prototype.Top = 0;
Microsoft.Xna.Framework.NativeMethods.RECT.prototype.Right = 0;
Microsoft.Xna.Framework.NativeMethods.RECT.prototype.Bottom = 0;

Object.seal(Microsoft.Xna.Framework.NativeMethods.RECT.prototype);
Object.seal(Microsoft.Xna.Framework.NativeMethods.RECT);
Microsoft.Xna.Framework.NativeMethods.POINT.prototype.X = 0;
Microsoft.Xna.Framework.NativeMethods.POINT.prototype.Y = 0;

Object.seal(Microsoft.Xna.Framework.NativeMethods.POINT.prototype);
Object.seal(Microsoft.Xna.Framework.NativeMethods.POINT);
Object.seal(Microsoft.Xna.Framework.NativeMethods.prototype);
Object.seal(Microsoft.Xna.Framework.NativeMethods);
Microsoft.Xna.Framework.Resources.resourceMan = null;
Microsoft.Xna.Framework.Resources.resourceCulture = null;
Microsoft.Xna.Framework.Resources.prototype._ctor = function () {
	System.Object.prototype._ctor.call(this);
};

Microsoft.Xna.Framework.Resources.get_ResourceManager = function () {

	if (System.Object.ReferenceEquals(Microsoft.Xna.Framework.Resources.resourceMan, null)) {
		Microsoft.Xna.Framework.Resources.resourceMan = new System.Resources.ResourceManager("Microsoft.Xna.Framework.Resources", Microsoft.Xna.Framework.Resources.Assembly);
	}
	return Microsoft.Xna.Framework.Resources.resourceMan;
};

Microsoft.Xna.Framework.Resources.get_Culture = function () {
	return Microsoft.Xna.Framework.Resources.resourceCulture;
};

Microsoft.Xna.Framework.Resources.set_Culture = function (value) {
	Microsoft.Xna.Framework.Resources.resourceCulture = value;
};

Microsoft.Xna.Framework.Resources.get_BackBufferDimMustBePositive = function () {
	return Microsoft.Xna.Framework.Resources.ResourceManager.GetString("BackBufferDimMustBePositive", Microsoft.Xna.Framework.Resources.resourceCulture);
};

Microsoft.Xna.Framework.Resources.get_CannotAddSameComponentMultipleTimes = function () {
	return Microsoft.Xna.Framework.Resources.ResourceManager.GetString("CannotAddSameComponentMultipleTimes", Microsoft.Xna.Framework.Resources.resourceCulture);
};

Microsoft.Xna.Framework.Resources.get_CannotSetItemsIntoGameComponentCollection = function () {
	return Microsoft.Xna.Framework.Resources.ResourceManager.GetString("CannotSetItemsIntoGameComponentCollection", Microsoft.Xna.Framework.Resources.resourceCulture);
};

Microsoft.Xna.Framework.Resources.get_DefaultTitleName = function () {
	return Microsoft.Xna.Framework.Resources.ResourceManager.GetString("DefaultTitleName", Microsoft.Xna.Framework.Resources.resourceCulture);
};

Microsoft.Xna.Framework.Resources.get_Direct3DCreateError = function () {
	return Microsoft.Xna.Framework.Resources.ResourceManager.GetString("Direct3DCreateError", Microsoft.Xna.Framework.Resources.resourceCulture);
};

Microsoft.Xna.Framework.Resources.get_Direct3DInternalDriverError = function () {
	return Microsoft.Xna.Framework.Resources.ResourceManager.GetString("Direct3DInternalDriverError", Microsoft.Xna.Framework.Resources.resourceCulture);
};

Microsoft.Xna.Framework.Resources.get_Direct3DInvalidCreateParameters = function () {
	return Microsoft.Xna.Framework.Resources.ResourceManager.GetString("Direct3DInvalidCreateParameters", Microsoft.Xna.Framework.Resources.resourceCulture);
};

Microsoft.Xna.Framework.Resources.get_Direct3DNotAvailable = function () {
	return Microsoft.Xna.Framework.Resources.ResourceManager.GetString("Direct3DNotAvailable", Microsoft.Xna.Framework.Resources.resourceCulture);
};

Microsoft.Xna.Framework.Resources.get_GameCannotBeNull = function () {
	return Microsoft.Xna.Framework.Resources.ResourceManager.GetString("GameCannotBeNull", Microsoft.Xna.Framework.Resources.resourceCulture);
};

Microsoft.Xna.Framework.Resources.get_GraphicsComponentNotAttachedToGame = function () {
	return Microsoft.Xna.Framework.Resources.ResourceManager.GetString("GraphicsComponentNotAttachedToGame", Microsoft.Xna.Framework.Resources.resourceCulture);
};

Microsoft.Xna.Framework.Resources.get_GraphicsDeviceManagerAlreadyPresent = function () {
	return Microsoft.Xna.Framework.Resources.ResourceManager.GetString("GraphicsDeviceManagerAlreadyPresent", Microsoft.Xna.Framework.Resources.resourceCulture);
};

Microsoft.Xna.Framework.Resources.get_InactiveSleepTimeCannotBeZero = function () {
	return Microsoft.Xna.Framework.Resources.ResourceManager.GetString("InactiveSleepTimeCannotBeZero", Microsoft.Xna.Framework.Resources.resourceCulture);
};

Microsoft.Xna.Framework.Resources.get_InvalidPixelShaderProfile = function () {
	return Microsoft.Xna.Framework.Resources.ResourceManager.GetString("InvalidPixelShaderProfile", Microsoft.Xna.Framework.Resources.resourceCulture);
};

Microsoft.Xna.Framework.Resources.get_InvalidScreenAdapter = function () {
	return Microsoft.Xna.Framework.Resources.ResourceManager.GetString("InvalidScreenAdapter", Microsoft.Xna.Framework.Resources.resourceCulture);
};

Microsoft.Xna.Framework.Resources.get_InvalidScreenDeviceName = function () {
	return Microsoft.Xna.Framework.Resources.ResourceManager.GetString("InvalidScreenDeviceName", Microsoft.Xna.Framework.Resources.resourceCulture);
};

Microsoft.Xna.Framework.Resources.get_InvalidVertexShaderProfile = function () {
	return Microsoft.Xna.Framework.Resources.ResourceManager.GetString("InvalidVertexShaderProfile", Microsoft.Xna.Framework.Resources.resourceCulture);
};

Microsoft.Xna.Framework.Resources.get_MissingGraphicsDeviceService = function () {
	return Microsoft.Xna.Framework.Resources.ResourceManager.GetString("MissingGraphicsDeviceService", Microsoft.Xna.Framework.Resources.resourceCulture);
};

Microsoft.Xna.Framework.Resources.get_MustCallBeginDeviceChange = function () {
	return Microsoft.Xna.Framework.Resources.ResourceManager.GetString("MustCallBeginDeviceChange", Microsoft.Xna.Framework.Resources.resourceCulture);
};

Microsoft.Xna.Framework.Resources.get_NoAudioHardware = function () {
	return Microsoft.Xna.Framework.Resources.ResourceManager.GetString("NoAudioHardware", Microsoft.Xna.Framework.Resources.resourceCulture);
};

Microsoft.Xna.Framework.Resources.get_NoCompatibleDevices = function () {
	return Microsoft.Xna.Framework.Resources.ResourceManager.GetString("NoCompatibleDevices", Microsoft.Xna.Framework.Resources.resourceCulture);
};

Microsoft.Xna.Framework.Resources.get_NoCompatibleDevicesAfterRanking = function () {
	return Microsoft.Xna.Framework.Resources.ResourceManager.GetString("NoCompatibleDevicesAfterRanking", Microsoft.Xna.Framework.Resources.resourceCulture);
};

Microsoft.Xna.Framework.Resources.get_NoDirect3DAcceleration = function () {
	return Microsoft.Xna.Framework.Resources.ResourceManager.GetString("NoDirect3DAcceleration", Microsoft.Xna.Framework.Resources.resourceCulture);
};

Microsoft.Xna.Framework.Resources.get_NoDirect3DAccelerationRemoteDesktop = function () {
	return Microsoft.Xna.Framework.Resources.ResourceManager.GetString("NoDirect3DAccelerationRemoteDesktop", Microsoft.Xna.Framework.Resources.resourceCulture);
};

Microsoft.Xna.Framework.Resources.get_NoGraphicsDeviceService = function () {
	return Microsoft.Xna.Framework.Resources.ResourceManager.GetString("NoGraphicsDeviceService", Microsoft.Xna.Framework.Resources.resourceCulture);
};

Microsoft.Xna.Framework.Resources.get_NoHighResolutionTimer = function () {
	return Microsoft.Xna.Framework.Resources.ResourceManager.GetString("NoHighResolutionTimer", Microsoft.Xna.Framework.Resources.resourceCulture);
};

Microsoft.Xna.Framework.Resources.get_NoMultipleRuns = function () {
	return Microsoft.Xna.Framework.Resources.ResourceManager.GetString("NoMultipleRuns", Microsoft.Xna.Framework.Resources.resourceCulture);
};

Microsoft.Xna.Framework.Resources.get_NoNullUseDefaultAdapter = function () {
	return Microsoft.Xna.Framework.Resources.ResourceManager.GetString("NoNullUseDefaultAdapter", Microsoft.Xna.Framework.Resources.resourceCulture);
};

Microsoft.Xna.Framework.Resources.get_NoPixelShader11OrDDI9Support = function () {
	return Microsoft.Xna.Framework.Resources.ResourceManager.GetString("NoPixelShader11OrDDI9Support", Microsoft.Xna.Framework.Resources.resourceCulture);
};

Microsoft.Xna.Framework.Resources.get_NoSuitableGraphicsDevice = function () {
	return Microsoft.Xna.Framework.Resources.ResourceManager.GetString("NoSuitableGraphicsDevice", Microsoft.Xna.Framework.Resources.resourceCulture);
};

Microsoft.Xna.Framework.Resources.get_NoSuitableGraphicsDeviceDetails = function () {
	return Microsoft.Xna.Framework.Resources.ResourceManager.GetString("NoSuitableGraphicsDeviceDetails", Microsoft.Xna.Framework.Resources.resourceCulture);
};

Microsoft.Xna.Framework.Resources.get_NullOrEmptyScreenDeviceName = function () {
	return Microsoft.Xna.Framework.Resources.ResourceManager.GetString("NullOrEmptyScreenDeviceName", Microsoft.Xna.Framework.Resources.resourceCulture);
};

Microsoft.Xna.Framework.Resources.get_PreviousDrawThrew = function () {
	return Microsoft.Xna.Framework.Resources.ResourceManager.GetString("PreviousDrawThrew", Microsoft.Xna.Framework.Resources.resourceCulture);
};

Microsoft.Xna.Framework.Resources.get_PropertyCannotBeCalledBeforeInitialize = function () {
	return Microsoft.Xna.Framework.Resources.ResourceManager.GetString("PropertyCannotBeCalledBeforeInitialize", Microsoft.Xna.Framework.Resources.resourceCulture);
};

Microsoft.Xna.Framework.Resources.get_ServiceAlreadyPresent = function () {
	return Microsoft.Xna.Framework.Resources.ResourceManager.GetString("ServiceAlreadyPresent", Microsoft.Xna.Framework.Resources.resourceCulture);
};

Microsoft.Xna.Framework.Resources.get_ServiceMustBeAssignable = function () {
	return Microsoft.Xna.Framework.Resources.ResourceManager.GetString("ServiceMustBeAssignable", Microsoft.Xna.Framework.Resources.resourceCulture);
};

Microsoft.Xna.Framework.Resources.get_ServiceProviderCannotBeNull = function () {
	return Microsoft.Xna.Framework.Resources.ResourceManager.GetString("ServiceProviderCannotBeNull", Microsoft.Xna.Framework.Resources.resourceCulture);
};

Microsoft.Xna.Framework.Resources.get_ServiceTypeCannotBeNull = function () {
	return Microsoft.Xna.Framework.Resources.ResourceManager.GetString("ServiceTypeCannotBeNull", Microsoft.Xna.Framework.Resources.resourceCulture);
};

Microsoft.Xna.Framework.Resources.get_TargetElaspedCannotBeZero = function () {
	return Microsoft.Xna.Framework.Resources.ResourceManager.GetString("TargetElaspedCannotBeZero", Microsoft.Xna.Framework.Resources.resourceCulture);
};

Microsoft.Xna.Framework.Resources.get_TitleCannotBeNull = function () {
	return Microsoft.Xna.Framework.Resources.ResourceManager.GetString("TitleCannotBeNull", Microsoft.Xna.Framework.Resources.resourceCulture);
};

Microsoft.Xna.Framework.Resources.get_ValidateAutoDepthStencilAdapterGroup = function () {
	return Microsoft.Xna.Framework.Resources.ResourceManager.GetString("ValidateAutoDepthStencilAdapterGroup", Microsoft.Xna.Framework.Resources.resourceCulture);
};

Microsoft.Xna.Framework.Resources.get_ValidateAutoDepthStencilFormatIncompatible = function () {
	return Microsoft.Xna.Framework.Resources.ResourceManager.GetString("ValidateAutoDepthStencilFormatIncompatible", Microsoft.Xna.Framework.Resources.resourceCulture);
};

Microsoft.Xna.Framework.Resources.get_ValidateAutoDepthStencilFormatInvalid = function () {
	return Microsoft.Xna.Framework.Resources.ResourceManager.GetString("ValidateAutoDepthStencilFormatInvalid", Microsoft.Xna.Framework.Resources.resourceCulture);
};

Microsoft.Xna.Framework.Resources.get_ValidateAutoDepthStencilMismatch = function () {
	return Microsoft.Xna.Framework.Resources.ResourceManager.GetString("ValidateAutoDepthStencilMismatch", Microsoft.Xna.Framework.Resources.resourceCulture);
};

Microsoft.Xna.Framework.Resources.get_ValidateBackBufferCount = function () {
	return Microsoft.Xna.Framework.Resources.ResourceManager.GetString("ValidateBackBufferCount", Microsoft.Xna.Framework.Resources.resourceCulture);
};

Microsoft.Xna.Framework.Resources.get_ValidateBackBufferCountSwapCopy = function () {
	return Microsoft.Xna.Framework.Resources.ResourceManager.GetString("ValidateBackBufferCountSwapCopy", Microsoft.Xna.Framework.Resources.resourceCulture);
};

Microsoft.Xna.Framework.Resources.get_ValidateBackBufferDimsFullScreen = function () {
	return Microsoft.Xna.Framework.Resources.ResourceManager.GetString("ValidateBackBufferDimsFullScreen", Microsoft.Xna.Framework.Resources.resourceCulture);
};

Microsoft.Xna.Framework.Resources.get_ValidateBackBufferDimsModeFullScreen = function () {
	return Microsoft.Xna.Framework.Resources.ResourceManager.GetString("ValidateBackBufferDimsModeFullScreen", Microsoft.Xna.Framework.Resources.resourceCulture);
};

Microsoft.Xna.Framework.Resources.get_ValidateBackBufferFormatIsInvalid = function () {
	return Microsoft.Xna.Framework.Resources.ResourceManager.GetString("ValidateBackBufferFormatIsInvalid", Microsoft.Xna.Framework.Resources.resourceCulture);
};

Microsoft.Xna.Framework.Resources.get_ValidateBackBufferHzModeFullScreen = function () {
	return Microsoft.Xna.Framework.Resources.ResourceManager.GetString("ValidateBackBufferHzModeFullScreen", Microsoft.Xna.Framework.Resources.resourceCulture);
};

Microsoft.Xna.Framework.Resources.get_ValidateDepthStencilFormatIsInvalid = function () {
	return Microsoft.Xna.Framework.Resources.ResourceManager.GetString("ValidateDepthStencilFormatIsInvalid", Microsoft.Xna.Framework.Resources.resourceCulture);
};

Microsoft.Xna.Framework.Resources.get_ValidateDeviceType = function () {
	return Microsoft.Xna.Framework.Resources.ResourceManager.GetString("ValidateDeviceType", Microsoft.Xna.Framework.Resources.resourceCulture);
};

Microsoft.Xna.Framework.Resources.get_ValidateMultiSampleQualityInvalid = function () {
	return Microsoft.Xna.Framework.Resources.ResourceManager.GetString("ValidateMultiSampleQualityInvalid", Microsoft.Xna.Framework.Resources.resourceCulture);
};

Microsoft.Xna.Framework.Resources.get_ValidateMultiSampleSwapEffect = function () {
	return Microsoft.Xna.Framework.Resources.ResourceManager.GetString("ValidateMultiSampleSwapEffect", Microsoft.Xna.Framework.Resources.resourceCulture);
};

Microsoft.Xna.Framework.Resources.get_ValidateMultiSampleTypeInvalid = function () {
	return Microsoft.Xna.Framework.Resources.ResourceManager.GetString("ValidateMultiSampleTypeInvalid", Microsoft.Xna.Framework.Resources.resourceCulture);
};

Microsoft.Xna.Framework.Resources.get_ValidatePresentationIntervalIncompatibleInFullScreen = function () {
	return Microsoft.Xna.Framework.Resources.ResourceManager.GetString("ValidatePresentationIntervalIncompatibleInFullScreen", Microsoft.Xna.Framework.Resources.resourceCulture);
};

Microsoft.Xna.Framework.Resources.get_ValidatePresentationIntervalInFullScreen = function () {
	return Microsoft.Xna.Framework.Resources.ResourceManager.GetString("ValidatePresentationIntervalInFullScreen", Microsoft.Xna.Framework.Resources.resourceCulture);
};

Microsoft.Xna.Framework.Resources.get_ValidatePresentationIntervalInWindow = function () {
	return Microsoft.Xna.Framework.Resources.ResourceManager.GetString("ValidatePresentationIntervalInWindow", Microsoft.Xna.Framework.Resources.resourceCulture);
};

Microsoft.Xna.Framework.Resources.get_ValidatePresentationIntervalOnXbox = function () {
	return Microsoft.Xna.Framework.Resources.ResourceManager.GetString("ValidatePresentationIntervalOnXbox", Microsoft.Xna.Framework.Resources.resourceCulture);
};

Microsoft.Xna.Framework.Resources.get_ValidateRefreshRateInFullScreen = function () {
	return Microsoft.Xna.Framework.Resources.ResourceManager.GetString("ValidateRefreshRateInFullScreen", Microsoft.Xna.Framework.Resources.resourceCulture);
};

Microsoft.Xna.Framework.Resources.get_ValidateRefreshRateInWindow = function () {
	return Microsoft.Xna.Framework.Resources.ResourceManager.GetString("ValidateRefreshRateInWindow", Microsoft.Xna.Framework.Resources.resourceCulture);
};

Microsoft.Xna.Framework.Resources.get_ValidateSwapEffectInvalid = function () {
	return Microsoft.Xna.Framework.Resources.ResourceManager.GetString("ValidateSwapEffectInvalid", Microsoft.Xna.Framework.Resources.resourceCulture);
};

Object.defineProperty(Microsoft.Xna.Framework.Resources, "ResourceManager", {
		get: Microsoft.Xna.Framework.Resources.get_ResourceManager
	});
Object.defineProperty(Microsoft.Xna.Framework.Resources, "Culture", {
		get: Microsoft.Xna.Framework.Resources.get_Culture, 
		set: Microsoft.Xna.Framework.Resources.set_Culture
	});
Object.defineProperty(Microsoft.Xna.Framework.Resources, "BackBufferDimMustBePositive", {
		get: Microsoft.Xna.Framework.Resources.get_BackBufferDimMustBePositive
	});
Object.defineProperty(Microsoft.Xna.Framework.Resources, "CannotAddSameComponentMultipleTimes", {
		get: Microsoft.Xna.Framework.Resources.get_CannotAddSameComponentMultipleTimes
	});
Object.defineProperty(Microsoft.Xna.Framework.Resources, "CannotSetItemsIntoGameComponentCollection", {
		get: Microsoft.Xna.Framework.Resources.get_CannotSetItemsIntoGameComponentCollection
	});
Object.defineProperty(Microsoft.Xna.Framework.Resources, "DefaultTitleName", {
		get: Microsoft.Xna.Framework.Resources.get_DefaultTitleName
	});
Object.defineProperty(Microsoft.Xna.Framework.Resources, "Direct3DCreateError", {
		get: Microsoft.Xna.Framework.Resources.get_Direct3DCreateError
	});
Object.defineProperty(Microsoft.Xna.Framework.Resources, "Direct3DInternalDriverError", {
		get: Microsoft.Xna.Framework.Resources.get_Direct3DInternalDriverError
	});
Object.defineProperty(Microsoft.Xna.Framework.Resources, "Direct3DInvalidCreateParameters", {
		get: Microsoft.Xna.Framework.Resources.get_Direct3DInvalidCreateParameters
	});
Object.defineProperty(Microsoft.Xna.Framework.Resources, "Direct3DNotAvailable", {
		get: Microsoft.Xna.Framework.Resources.get_Direct3DNotAvailable
	});
Object.defineProperty(Microsoft.Xna.Framework.Resources, "GameCannotBeNull", {
		get: Microsoft.Xna.Framework.Resources.get_GameCannotBeNull
	});
Object.defineProperty(Microsoft.Xna.Framework.Resources, "GraphicsComponentNotAttachedToGame", {
		get: Microsoft.Xna.Framework.Resources.get_GraphicsComponentNotAttachedToGame
	});
Object.defineProperty(Microsoft.Xna.Framework.Resources, "GraphicsDeviceManagerAlreadyPresent", {
		get: Microsoft.Xna.Framework.Resources.get_GraphicsDeviceManagerAlreadyPresent
	});
Object.defineProperty(Microsoft.Xna.Framework.Resources, "InactiveSleepTimeCannotBeZero", {
		get: Microsoft.Xna.Framework.Resources.get_InactiveSleepTimeCannotBeZero
	});
Object.defineProperty(Microsoft.Xna.Framework.Resources, "InvalidPixelShaderProfile", {
		get: Microsoft.Xna.Framework.Resources.get_InvalidPixelShaderProfile
	});
Object.defineProperty(Microsoft.Xna.Framework.Resources, "InvalidScreenAdapter", {
		get: Microsoft.Xna.Framework.Resources.get_InvalidScreenAdapter
	});
Object.defineProperty(Microsoft.Xna.Framework.Resources, "InvalidScreenDeviceName", {
		get: Microsoft.Xna.Framework.Resources.get_InvalidScreenDeviceName
	});
Object.defineProperty(Microsoft.Xna.Framework.Resources, "InvalidVertexShaderProfile", {
		get: Microsoft.Xna.Framework.Resources.get_InvalidVertexShaderProfile
	});
Object.defineProperty(Microsoft.Xna.Framework.Resources, "MissingGraphicsDeviceService", {
		get: Microsoft.Xna.Framework.Resources.get_MissingGraphicsDeviceService
	});
Object.defineProperty(Microsoft.Xna.Framework.Resources, "MustCallBeginDeviceChange", {
		get: Microsoft.Xna.Framework.Resources.get_MustCallBeginDeviceChange
	});
Object.defineProperty(Microsoft.Xna.Framework.Resources, "NoAudioHardware", {
		get: Microsoft.Xna.Framework.Resources.get_NoAudioHardware
	});
Object.defineProperty(Microsoft.Xna.Framework.Resources, "NoCompatibleDevices", {
		get: Microsoft.Xna.Framework.Resources.get_NoCompatibleDevices
	});
Object.defineProperty(Microsoft.Xna.Framework.Resources, "NoCompatibleDevicesAfterRanking", {
		get: Microsoft.Xna.Framework.Resources.get_NoCompatibleDevicesAfterRanking
	});
Object.defineProperty(Microsoft.Xna.Framework.Resources, "NoDirect3DAcceleration", {
		get: Microsoft.Xna.Framework.Resources.get_NoDirect3DAcceleration
	});
Object.defineProperty(Microsoft.Xna.Framework.Resources, "NoDirect3DAccelerationRemoteDesktop", {
		get: Microsoft.Xna.Framework.Resources.get_NoDirect3DAccelerationRemoteDesktop
	});
Object.defineProperty(Microsoft.Xna.Framework.Resources, "NoGraphicsDeviceService", {
		get: Microsoft.Xna.Framework.Resources.get_NoGraphicsDeviceService
	});
Object.defineProperty(Microsoft.Xna.Framework.Resources, "NoHighResolutionTimer", {
		get: Microsoft.Xna.Framework.Resources.get_NoHighResolutionTimer
	});
Object.defineProperty(Microsoft.Xna.Framework.Resources, "NoMultipleRuns", {
		get: Microsoft.Xna.Framework.Resources.get_NoMultipleRuns
	});
Object.defineProperty(Microsoft.Xna.Framework.Resources, "NoNullUseDefaultAdapter", {
		get: Microsoft.Xna.Framework.Resources.get_NoNullUseDefaultAdapter
	});
Object.defineProperty(Microsoft.Xna.Framework.Resources, "NoPixelShader11OrDDI9Support", {
		get: Microsoft.Xna.Framework.Resources.get_NoPixelShader11OrDDI9Support
	});
Object.defineProperty(Microsoft.Xna.Framework.Resources, "NoSuitableGraphicsDevice", {
		get: Microsoft.Xna.Framework.Resources.get_NoSuitableGraphicsDevice
	});
Object.defineProperty(Microsoft.Xna.Framework.Resources, "NoSuitableGraphicsDeviceDetails", {
		get: Microsoft.Xna.Framework.Resources.get_NoSuitableGraphicsDeviceDetails
	});
Object.defineProperty(Microsoft.Xna.Framework.Resources, "NullOrEmptyScreenDeviceName", {
		get: Microsoft.Xna.Framework.Resources.get_NullOrEmptyScreenDeviceName
	});
Object.defineProperty(Microsoft.Xna.Framework.Resources, "PreviousDrawThrew", {
		get: Microsoft.Xna.Framework.Resources.get_PreviousDrawThrew
	});
Object.defineProperty(Microsoft.Xna.Framework.Resources, "PropertyCannotBeCalledBeforeInitialize", {
		get: Microsoft.Xna.Framework.Resources.get_PropertyCannotBeCalledBeforeInitialize
	});
Object.defineProperty(Microsoft.Xna.Framework.Resources, "ServiceAlreadyPresent", {
		get: Microsoft.Xna.Framework.Resources.get_ServiceAlreadyPresent
	});
Object.defineProperty(Microsoft.Xna.Framework.Resources, "ServiceMustBeAssignable", {
		get: Microsoft.Xna.Framework.Resources.get_ServiceMustBeAssignable
	});
Object.defineProperty(Microsoft.Xna.Framework.Resources, "ServiceProviderCannotBeNull", {
		get: Microsoft.Xna.Framework.Resources.get_ServiceProviderCannotBeNull
	});
Object.defineProperty(Microsoft.Xna.Framework.Resources, "ServiceTypeCannotBeNull", {
		get: Microsoft.Xna.Framework.Resources.get_ServiceTypeCannotBeNull
	});
Object.defineProperty(Microsoft.Xna.Framework.Resources, "TargetElaspedCannotBeZero", {
		get: Microsoft.Xna.Framework.Resources.get_TargetElaspedCannotBeZero
	});
Object.defineProperty(Microsoft.Xna.Framework.Resources, "TitleCannotBeNull", {
		get: Microsoft.Xna.Framework.Resources.get_TitleCannotBeNull
	});
Object.defineProperty(Microsoft.Xna.Framework.Resources, "ValidateAutoDepthStencilAdapterGroup", {
		get: Microsoft.Xna.Framework.Resources.get_ValidateAutoDepthStencilAdapterGroup
	});
Object.defineProperty(Microsoft.Xna.Framework.Resources, "ValidateAutoDepthStencilFormatIncompatible", {
		get: Microsoft.Xna.Framework.Resources.get_ValidateAutoDepthStencilFormatIncompatible
	});
Object.defineProperty(Microsoft.Xna.Framework.Resources, "ValidateAutoDepthStencilFormatInvalid", {
		get: Microsoft.Xna.Framework.Resources.get_ValidateAutoDepthStencilFormatInvalid
	});
Object.defineProperty(Microsoft.Xna.Framework.Resources, "ValidateAutoDepthStencilMismatch", {
		get: Microsoft.Xna.Framework.Resources.get_ValidateAutoDepthStencilMismatch
	});
Object.defineProperty(Microsoft.Xna.Framework.Resources, "ValidateBackBufferCount", {
		get: Microsoft.Xna.Framework.Resources.get_ValidateBackBufferCount
	});
Object.defineProperty(Microsoft.Xna.Framework.Resources, "ValidateBackBufferCountSwapCopy", {
		get: Microsoft.Xna.Framework.Resources.get_ValidateBackBufferCountSwapCopy
	});
Object.defineProperty(Microsoft.Xna.Framework.Resources, "ValidateBackBufferDimsFullScreen", {
		get: Microsoft.Xna.Framework.Resources.get_ValidateBackBufferDimsFullScreen
	});
Object.defineProperty(Microsoft.Xna.Framework.Resources, "ValidateBackBufferDimsModeFullScreen", {
		get: Microsoft.Xna.Framework.Resources.get_ValidateBackBufferDimsModeFullScreen
	});
Object.defineProperty(Microsoft.Xna.Framework.Resources, "ValidateBackBufferFormatIsInvalid", {
		get: Microsoft.Xna.Framework.Resources.get_ValidateBackBufferFormatIsInvalid
	});
Object.defineProperty(Microsoft.Xna.Framework.Resources, "ValidateBackBufferHzModeFullScreen", {
		get: Microsoft.Xna.Framework.Resources.get_ValidateBackBufferHzModeFullScreen
	});
Object.defineProperty(Microsoft.Xna.Framework.Resources, "ValidateDepthStencilFormatIsInvalid", {
		get: Microsoft.Xna.Framework.Resources.get_ValidateDepthStencilFormatIsInvalid
	});
Object.defineProperty(Microsoft.Xna.Framework.Resources, "ValidateDeviceType", {
		get: Microsoft.Xna.Framework.Resources.get_ValidateDeviceType
	});
Object.defineProperty(Microsoft.Xna.Framework.Resources, "ValidateMultiSampleQualityInvalid", {
		get: Microsoft.Xna.Framework.Resources.get_ValidateMultiSampleQualityInvalid
	});
Object.defineProperty(Microsoft.Xna.Framework.Resources, "ValidateMultiSampleSwapEffect", {
		get: Microsoft.Xna.Framework.Resources.get_ValidateMultiSampleSwapEffect
	});
Object.defineProperty(Microsoft.Xna.Framework.Resources, "ValidateMultiSampleTypeInvalid", {
		get: Microsoft.Xna.Framework.Resources.get_ValidateMultiSampleTypeInvalid
	});
Object.defineProperty(Microsoft.Xna.Framework.Resources, "ValidatePresentationIntervalIncompatibleInFullScreen", {
		get: Microsoft.Xna.Framework.Resources.get_ValidatePresentationIntervalIncompatibleInFullScreen
	});
Object.defineProperty(Microsoft.Xna.Framework.Resources, "ValidatePresentationIntervalInFullScreen", {
		get: Microsoft.Xna.Framework.Resources.get_ValidatePresentationIntervalInFullScreen
	});
Object.defineProperty(Microsoft.Xna.Framework.Resources, "ValidatePresentationIntervalInWindow", {
		get: Microsoft.Xna.Framework.Resources.get_ValidatePresentationIntervalInWindow
	});
Object.defineProperty(Microsoft.Xna.Framework.Resources, "ValidatePresentationIntervalOnXbox", {
		get: Microsoft.Xna.Framework.Resources.get_ValidatePresentationIntervalOnXbox
	});
Object.defineProperty(Microsoft.Xna.Framework.Resources, "ValidateRefreshRateInFullScreen", {
		get: Microsoft.Xna.Framework.Resources.get_ValidateRefreshRateInFullScreen
	});
Object.defineProperty(Microsoft.Xna.Framework.Resources, "ValidateRefreshRateInWindow", {
		get: Microsoft.Xna.Framework.Resources.get_ValidateRefreshRateInWindow
	});
Object.defineProperty(Microsoft.Xna.Framework.Resources, "ValidateSwapEffectInvalid", {
		get: Microsoft.Xna.Framework.Resources.get_ValidateSwapEffectInvalid
	});

Object.seal(Microsoft.Xna.Framework.Resources.prototype);
Object.seal(Microsoft.Xna.Framework.Resources);
Microsoft.Xna.Framework.WindowsGameForm.prototype.freezeOurEvents = new System.Boolean();
Microsoft.Xna.Framework.WindowsGameForm.prototype.screen = null;
Microsoft.Xna.Framework.WindowsGameForm.prototype.resizeWindowState = 0;
Microsoft.Xna.Framework.WindowsGameForm.prototype.hidMouse = new System.Boolean();
Microsoft.Xna.Framework.WindowsGameForm.prototype.isMouseVisible = new System.Boolean();
Microsoft.Xna.Framework.WindowsGameForm.prototype.allowUserResizing = new System.Boolean();
Microsoft.Xna.Framework.WindowsGameForm.prototype.userResizing = new System.Boolean();
Microsoft.Xna.Framework.WindowsGameForm.prototype.deviceChangeChangedVisible = new System.Boolean();
Microsoft.Xna.Framework.WindowsGameForm.prototype.oldVisible = new System.Boolean();
Microsoft.Xna.Framework.WindowsGameForm.prototype.centerScreen = new System.Boolean();
Microsoft.Xna.Framework.WindowsGameForm.prototype.isFullScreenMaximized = new System.Boolean();
Microsoft.Xna.Framework.WindowsGameForm.prototype.savedFormBorderStyle = 0;
Microsoft.Xna.Framework.WindowsGameForm.prototype.savedWindowState = 0;
Microsoft.Xna.Framework.WindowsGameForm.prototype.firstPaint = new System.Boolean();
Microsoft.Xna.Framework.WindowsGameForm.prototype.Suspend = null;
Microsoft.Xna.Framework.WindowsGameForm.prototype.Resume = null;
Microsoft.Xna.Framework.WindowsGameForm.prototype.ScreenChanged = null;
Microsoft.Xna.Framework.WindowsGameForm.prototype.UserResized = null;
Microsoft.Xna.Framework.WindowsGameForm.prototype.ApplicationActivated = null;
Microsoft.Xna.Framework.WindowsGameForm.prototype.ApplicationDeactivated = null;
Microsoft.Xna.Framework.WindowsGameForm.prototype.__StructFields__ = {
	startResizeSize: System.Drawing.Size, 
	deviceChangeWillBeFullScreen: System.Nullable$b1.Of(System.Boolean), 
	oldClientSize: System.Drawing.Size, 
	savedBounds: System.Drawing.Rectangle, 
	savedRestoreBounds: System.Drawing.Rectangle
};
Microsoft.Xna.Framework.WindowsGameForm.prototype.get_AllowUserResizing = function () {
	return this.allowUserResizing;
};

Microsoft.Xna.Framework.WindowsGameForm.prototype.set_AllowUserResizing = function (value) {

	if (this.allowUserResizing !== value) {
		this.allowUserResizing = value;
		this.UpdateBorderStyle();
	}
};

Microsoft.Xna.Framework.WindowsGameForm.prototype.get_IsMouseVisible = function () {
	return this.isMouseVisible;
};

Microsoft.Xna.Framework.WindowsGameForm.prototype.set_IsMouseVisible = function (value) {

	if (this.isMouseVisible !== value) {
		this.isMouseVisible = value;

		if (this.isMouseVisible) {

			if (this.hidMouse) {
				System.Windows.Forms.Cursor.Show();
				this.hidMouse = false;
				return ;
			}
		} else if (!this.hidMouse) {
			System.Windows.Forms.Cursor.Hide();
			this.hidMouse = true;
		}
	}
};

Microsoft.Xna.Framework.WindowsGameForm.prototype.get_DeviceScreen = function () {
	return this.screen;
};

Microsoft.Xna.Framework.WindowsGameForm.prototype.get_ClientBounds = function () {
	var point = System.Windows.Forms.Control.prototype.PointToScreen.call(this, System.Drawing.Point.Empty.MemberwiseClone());
	return new Microsoft.Xna.Framework.Rectangle(point.get_X(), point.get_Y(), this.ClientSize.Width, this.ClientSize.Height);
};

Microsoft.Xna.Framework.WindowsGameForm.prototype.get_IsMinimized = function () {
	return ((this.ClientSize.Width !== null) || (this.ClientSize.Height === 0));
};

Microsoft.Xna.Framework.WindowsGameForm.prototype.add_Suspend = function (value) {
	this.Suspend = System.Delegate.Combine(this.Suspend, value);
};

Microsoft.Xna.Framework.WindowsGameForm.prototype.remove_Suspend = function (value) {
	this.Suspend = System.Delegate.Remove(this.Suspend, value);
};

Microsoft.Xna.Framework.WindowsGameForm.prototype.add_Resume = function (value) {
	this.Resume = System.Delegate.Combine(this.Resume, value);
};

Microsoft.Xna.Framework.WindowsGameForm.prototype.remove_Resume = function (value) {
	this.Resume = System.Delegate.Remove(this.Resume, value);
};

Microsoft.Xna.Framework.WindowsGameForm.prototype.add_ScreenChanged = function (value) {
	this.ScreenChanged = System.Delegate.Combine(this.ScreenChanged, value);
};

Microsoft.Xna.Framework.WindowsGameForm.prototype.remove_ScreenChanged = function (value) {
	this.ScreenChanged = System.Delegate.Remove(this.ScreenChanged, value);
};

Microsoft.Xna.Framework.WindowsGameForm.prototype.add_UserResized = function (value) {
	this.UserResized = System.Delegate.Combine(this.UserResized, value);
};

Microsoft.Xna.Framework.WindowsGameForm.prototype.remove_UserResized = function (value) {
	this.UserResized = System.Delegate.Remove(this.UserResized, value);
};

Microsoft.Xna.Framework.WindowsGameForm.prototype.add_ApplicationActivated = function (value) {
	this.ApplicationActivated = System.Delegate.Combine(this.ApplicationActivated, value);
};

Microsoft.Xna.Framework.WindowsGameForm.prototype.remove_ApplicationActivated = function (value) {
	this.ApplicationActivated = System.Delegate.Remove(this.ApplicationActivated, value);
};

Microsoft.Xna.Framework.WindowsGameForm.prototype.add_ApplicationDeactivated = function (value) {
	this.ApplicationDeactivated = System.Delegate.Combine(this.ApplicationDeactivated, value);
};

Microsoft.Xna.Framework.WindowsGameForm.prototype.remove_ApplicationDeactivated = function (value) {
	this.ApplicationDeactivated = System.Delegate.Remove(this.ApplicationDeactivated, value);
};

Microsoft.Xna.Framework.WindowsGameForm.prototype._ctor = function () {
	this.startResizeSize = System.Drawing.Size.Empty.MemberwiseClone();
	this.centerScreen = true;
	this.firstPaint = true;
	System.Windows.Forms.Form.prototype._ctor.call(this);
	System.Windows.Forms.Control.prototype.SuspendLayout.call(this);
	this.AutoScaleDimensions = this;
	this.AutoScaleMode = this;
	this.CausesValidation = this;
	this.ClientSize = this;
	this.Name = this;
	this.Text = "GameForm";
	System.Windows.Forms.Form.prototype.add_ResizeBegin.call(this, JSIL.Delegate.New("System.EventHandler", this, Microsoft.Xna.Framework.WindowsGameForm.prototype.Form_ResizeBegin));
	System.Windows.Forms.Control.prototype.add_ClientSizeChanged.call(this, JSIL.Delegate.New("System.EventHandler", this, Microsoft.Xna.Framework.WindowsGameForm.prototype.Form_ClientSizeChanged));
	System.Windows.Forms.Control.prototype.add_Resize.call(this, JSIL.Delegate.New("System.EventHandler", this, Microsoft.Xna.Framework.WindowsGameForm.prototype.Form_Resize));
	System.Windows.Forms.Control.prototype.add_LocationChanged.call(this, JSIL.Delegate.New("System.EventHandler", this, Microsoft.Xna.Framework.WindowsGameForm.prototype.Form_LocationChanged));
	System.Windows.Forms.Form.prototype.add_ResizeEnd.call(this, JSIL.Delegate.New("System.EventHandler", this, Microsoft.Xna.Framework.WindowsGameForm.prototype.Form_ResizeEnd));
	System.Windows.Forms.Control.prototype.add_MouseEnter.call(this, JSIL.Delegate.New("System.EventHandler", this, Microsoft.Xna.Framework.WindowsGameForm.prototype.Form_MouseEnter));
	System.Windows.Forms.Control.prototype.add_MouseLeave.call(this, JSIL.Delegate.New("System.EventHandler", this, Microsoft.Xna.Framework.WindowsGameForm.prototype.Form_MouseLeave));
	System.Windows.Forms.Control.prototype.ResumeLayout.call(this, false);

	try {
		this.freezeOurEvents = true;
		this.resizeWindowState = this.WindowState;
		this.screen = Microsoft.Xna.Framework.WindowsGameWindow.ScreenFromHandle(this.Handle);
		System.Windows.Forms.Control.prototype.SetStyle.call(this, System.Windows.Forms.ControlStyles.Opaque | System.Windows.Forms.ControlStyles.AllPaintingInWmPaint, false);
		this.ClientSize = this;
		this.UpdateBorderStyle();
	} finally {
		this.freezeOurEvents = false;
	}
};

Microsoft.Xna.Framework.WindowsGameForm.prototype.UpdateBorderStyle = function () {

	if (!this.allowUserResizing) {
		this.MaximizeBox = this;

		if (!this.isFullScreenMaximized) {
			this.FormBorderStyle = this;
			return ;
		}
	} else {
		this.MaximizeBox = this;

		if (!this.isFullScreenMaximized) {
			this.FormBorderStyle = this;
		}
	}
};

Microsoft.Xna.Framework.WindowsGameForm.prototype.UpdateScreen = function () {

	if (this.freezeOurEvents) {
		return ;
	}
	var obj = System.Windows.Forms.Screen.FromHandle(this.Handle);

	if (!((this.screen === null) && this.screen.Equals(obj))) {
		this.screen = obj;

		if (this.screen === null) {
			this.OnScreenChanged();
		}
	}
};

Microsoft.Xna.Framework.WindowsGameForm.prototype.OnSuspend = function () {

	if (this.Suspend === null) {
		this.Suspend(this, System.EventArgs.Empty);
	}
};

Microsoft.Xna.Framework.WindowsGameForm.prototype.OnResume = function () {

	if (this.Resume === null) {
		this.Resume(this, System.EventArgs.Empty);
	}
};

Microsoft.Xna.Framework.WindowsGameForm.prototype.OnScreenChanged = function () {

	if (this.ScreenChanged === null) {
		this.ScreenChanged(this, System.EventArgs.Empty);
	}
};

Microsoft.Xna.Framework.WindowsGameForm.prototype.OnUserResized = function (forceEvent) {

	if (!(!this.freezeOurEvents || forceEvent)) {
		return ;
	}

	if (this.UserResized === null) {
		this.UserResized(this, System.EventArgs.Empty);
	}
};

Microsoft.Xna.Framework.WindowsGameForm.prototype.OnActivateApp = function (active) {

	if (active) {
		this.firstPaint = true;
		this.freezeOurEvents = false;

		if (this.isFullScreenMaximized) {
			this.TopMost = this;
		}

		if (this.ApplicationActivated === null) {
			this.ApplicationActivated(this, System.EventArgs.Empty);
			return ;
		}
	} else {

		if (this.ApplicationDeactivated === null) {
			this.ApplicationDeactivated(this, System.EventArgs.Empty);
		}
		this.freezeOurEvents = true;
	}
};

Microsoft.Xna.Framework.WindowsGameForm.prototype.OnPaintBackground = function (e) {

	if (this.firstPaint) {
		System.Windows.Forms.ScrollableControl.prototype.OnPaintBackground.call(this, e);
		this.firstPaint = false;
	}
};

Microsoft.Xna.Framework.WindowsGameForm.prototype.WndProc = function (/* ref */ m) {

	if (m.Msg === 28) {
		this.OnActivateApp(System.IntPtr.op_Inequality(m.WParam, System.IntPtr.Zero));
	}
	System.Windows.Forms.Form.prototype.WndProc.call(this, m.value.MemberwiseClone());
};

Microsoft.Xna.Framework.WindowsGameForm.prototype.ProcessDialogKey = function (keyData) {
	var keys = (keyData & System.Windows.Forms.Keys.KeyCode | System.Windows.Forms.Keys.None | System.Windows.Forms.Keys.LButton | System.Windows.Forms.Keys.RButton | System.Windows.Forms.Keys.Cancel | System.Windows.Forms.Keys.MButton | System.Windows.Forms.Keys.XButton1 | System.Windows.Forms.Keys.XButton2 | System.Windows.Forms.Keys.Back | System.Windows.Forms.Keys.Tab | System.Windows.Forms.Keys.LineFeed | System.Windows.Forms.Keys.Clear | System.Windows.Forms.Keys.Return | System.Windows.Forms.Keys.Enter | System.Windows.Forms.Keys.ShiftKey | System.Windows.Forms.Keys.ControlKey | System.Windows.Forms.Keys.Menu | System.Windows.Forms.Keys.Pause | System.Windows.Forms.Keys.Capital | System.Windows.Forms.Keys.CapsLock | System.Windows.Forms.Keys.KanaMode | System.Windows.Forms.Keys.HanguelMode | System.Windows.Forms.Keys.HangulMode | System.Windows.Forms.Keys.JunjaMode | System.Windows.Forms.Keys.FinalMode | System.Windows.Forms.Keys.HanjaMode | System.Windows.Forms.Keys.KanjiMode | System.Windows.Forms.Keys.Escape | System.Windows.Forms.Keys.IMEConvert | System.Windows.Forms.Keys.IMENonconvert | System.Windows.Forms.Keys.IMEAccept | System.Windows.Forms.Keys.IMEAceept | System.Windows.Forms.Keys.IMEModeChange | System.Windows.Forms.Keys.Space | System.Windows.Forms.Keys.Prior | System.Windows.Forms.Keys.PageUp | System.Windows.Forms.Keys.Next | System.Windows.Forms.Keys.PageDown | System.Windows.Forms.Keys.End | System.Windows.Forms.Keys.Home | System.Windows.Forms.Keys.Left | System.Windows.Forms.Keys.Up | System.Windows.Forms.Keys.Right | System.Windows.Forms.Keys.Down | System.Windows.Forms.Keys.Select | System.Windows.Forms.Keys.Print | System.Windows.Forms.Keys.Execute | System.Windows.Forms.Keys.Snapshot | System.Windows.Forms.Keys.PrintScreen | System.Windows.Forms.Keys.Insert | System.Windows.Forms.Keys.Delete | System.Windows.Forms.Keys.Help | System.Windows.Forms.Keys.D0 | System.Windows.Forms.Keys.D1 | System.Windows.Forms.Keys.D2 | System.Windows.Forms.Keys.D3 | System.Windows.Forms.Keys.D4 | System.Windows.Forms.Keys.D5 | System.Windows.Forms.Keys.D6 | System.Windows.Forms.Keys.D7 | System.Windows.Forms.Keys.D8 | System.Windows.Forms.Keys.D9 | System.Windows.Forms.Keys.A | System.Windows.Forms.Keys.B | System.Windows.Forms.Keys.C | System.Windows.Forms.Keys.D | System.Windows.Forms.Keys.E | System.Windows.Forms.Keys.F | System.Windows.Forms.Keys.G | System.Windows.Forms.Keys.H | System.Windows.Forms.Keys.I | System.Windows.Forms.Keys.J | System.Windows.Forms.Keys.K | System.Windows.Forms.Keys.L | System.Windows.Forms.Keys.M | System.Windows.Forms.Keys.N | System.Windows.Forms.Keys.O | System.Windows.Forms.Keys.P | System.Windows.Forms.Keys.Q | System.Windows.Forms.Keys.R | System.Windows.Forms.Keys.S | System.Windows.Forms.Keys.T | System.Windows.Forms.Keys.U | System.Windows.Forms.Keys.V | System.Windows.Forms.Keys.W | System.Windows.Forms.Keys.X | System.Windows.Forms.Keys.Y | System.Windows.Forms.Keys.Z | System.Windows.Forms.Keys.LWin | System.Windows.Forms.Keys.RWin | System.Windows.Forms.Keys.Apps | System.Windows.Forms.Keys.Sleep | System.Windows.Forms.Keys.NumPad0 | System.Windows.Forms.Keys.NumPad1 | System.Windows.Forms.Keys.NumPad2 | System.Windows.Forms.Keys.NumPad3 | System.Windows.Forms.Keys.NumPad4 | System.Windows.Forms.Keys.NumPad5 | System.Windows.Forms.Keys.NumPad6 | System.Windows.Forms.Keys.NumPad7 | System.Windows.Forms.Keys.NumPad8 | System.Windows.Forms.Keys.NumPad9 | System.Windows.Forms.Keys.Multiply | System.Windows.Forms.Keys.Add | System.Windows.Forms.Keys.Separator | System.Windows.Forms.Keys.Subtract | System.Windows.Forms.Keys.Decimal | System.Windows.Forms.Keys.Divide | System.Windows.Forms.Keys.F1 | System.Windows.Forms.Keys.F2 | System.Windows.Forms.Keys.F3 | System.Windows.Forms.Keys.F4 | System.Windows.Forms.Keys.F5 | System.Windows.Forms.Keys.F6 | System.Windows.Forms.Keys.F7 | System.Windows.Forms.Keys.F8 | System.Windows.Forms.Keys.F9 | System.Windows.Forms.Keys.F10 | System.Windows.Forms.Keys.F11 | System.Windows.Forms.Keys.F12 | System.Windows.Forms.Keys.F13 | System.Windows.Forms.Keys.F14 | System.Windows.Forms.Keys.F15 | System.Windows.Forms.Keys.F16 | System.Windows.Forms.Keys.F17 | System.Windows.Forms.Keys.F18 | System.Windows.Forms.Keys.F19 | System.Windows.Forms.Keys.F20 | System.Windows.Forms.Keys.F21 | System.Windows.Forms.Keys.F22 | System.Windows.Forms.Keys.F23 | System.Windows.Forms.Keys.F24 | System.Windows.Forms.Keys.NumLock | System.Windows.Forms.Keys.Scroll | System.Windows.Forms.Keys.LShiftKey | System.Windows.Forms.Keys.RShiftKey | System.Windows.Forms.Keys.LControlKey | System.Windows.Forms.Keys.RControlKey | System.Windows.Forms.Keys.LMenu | System.Windows.Forms.Keys.RMenu | System.Windows.Forms.Keys.BrowserBack | System.Windows.Forms.Keys.BrowserForward | System.Windows.Forms.Keys.BrowserRefresh | System.Windows.Forms.Keys.BrowserStop | System.Windows.Forms.Keys.BrowserSearch | System.Windows.Forms.Keys.BrowserFavorites | System.Windows.Forms.Keys.BrowserHome | System.Windows.Forms.Keys.VolumeMute | System.Windows.Forms.Keys.VolumeDown | System.Windows.Forms.Keys.VolumeUp | System.Windows.Forms.Keys.MediaNextTrack | System.Windows.Forms.Keys.MediaPreviousTrack | System.Windows.Forms.Keys.MediaStop | System.Windows.Forms.Keys.MediaPlayPause | System.Windows.Forms.Keys.LaunchMail | System.Windows.Forms.Keys.SelectMedia | System.Windows.Forms.Keys.LaunchApplication1 | System.Windows.Forms.Keys.LaunchApplication2 | System.Windows.Forms.Keys.OemSemicolon | System.Windows.Forms.Keys.Oem1 | System.Windows.Forms.Keys.Oemplus | System.Windows.Forms.Keys.Oemcomma | System.Windows.Forms.Keys.OemMinus | System.Windows.Forms.Keys.OemPeriod | System.Windows.Forms.Keys.OemQuestion | System.Windows.Forms.Keys.Oem2 | System.Windows.Forms.Keys.Oemtilde | System.Windows.Forms.Keys.Oem3 | System.Windows.Forms.Keys.OemOpenBrackets | System.Windows.Forms.Keys.Oem4 | System.Windows.Forms.Keys.OemPipe | System.Windows.Forms.Keys.Oem5 | System.Windows.Forms.Keys.OemCloseBrackets | System.Windows.Forms.Keys.Oem6 | System.Windows.Forms.Keys.OemQuotes | System.Windows.Forms.Keys.Oem7 | System.Windows.Forms.Keys.Oem8 | System.Windows.Forms.Keys.OemBackslash | System.Windows.Forms.Keys.Oem102 | System.Windows.Forms.Keys.ProcessKey | System.Windows.Forms.Keys.Packet | System.Windows.Forms.Keys.Attn | System.Windows.Forms.Keys.Crsel | System.Windows.Forms.Keys.Exsel | System.Windows.Forms.Keys.EraseEof | System.Windows.Forms.Keys.Play | System.Windows.Forms.Keys.Zoom | System.Windows.Forms.Keys.NoName | System.Windows.Forms.Keys.Pa1 | System.Windows.Forms.Keys.OemClear);
	var keys2 = (keyData & System.Windows.Forms.Keys.None | System.Windows.Forms.Keys.Alt);

	if (!((keys2 !== System.Windows.Forms.Keys.None | System.Windows.Forms.Keys.Alt) || ((keys !== System.Windows.Forms.Keys.None | System.Windows.Forms.Keys.LButton | System.Windows.Forms.Keys.RButton | System.Windows.Forms.Keys.Cancel | System.Windows.Forms.Keys.ShiftKey | System.Windows.Forms.Keys.ControlKey | System.Windows.Forms.Keys.Menu | System.Windows.Forms.Keys.Pause | System.Windows.Forms.Keys.Space | System.Windows.Forms.Keys.Prior | System.Windows.Forms.Keys.PageUp | System.Windows.Forms.Keys.Next | System.Windows.Forms.Keys.PageDown | System.Windows.Forms.Keys.End | System.Windows.Forms.Keys.D0 | System.Windows.Forms.Keys.D1 | System.Windows.Forms.Keys.D2 | System.Windows.Forms.Keys.D3 | System.Windows.Forms.Keys.A | System.Windows.Forms.Keys.B | System.Windows.Forms.Keys.C | System.Windows.Forms.Keys.P | System.Windows.Forms.Keys.Q | System.Windows.Forms.Keys.R | System.Windows.Forms.Keys.S | System.Windows.Forms.Keys.NumPad0 | System.Windows.Forms.Keys.NumPad1 | System.Windows.Forms.Keys.NumPad2 | System.Windows.Forms.Keys.NumPad3 | System.Windows.Forms.Keys.F1 | System.Windows.Forms.Keys.F2 | System.Windows.Forms.Keys.F3 | System.Windows.Forms.Keys.F4) && 
				keys))) {
		return System.Windows.Forms.Form.prototype.ProcessDialogKey.call(this, keyData);
	}
	return (!Microsoft.Xna.Framework.GamerServices.GamerServicesDispatcher.IsInitialized || 
		((keys !== System.Windows.Forms.Keys.None | System.Windows.Forms.Keys.MButton | System.Windows.Forms.Keys.Space | System.Windows.Forms.Keys.Home) && 
			!Microsoft.Xna.Framework.GamerServices.Guide.IsVisible) || System.Windows.Forms.Form.prototype.ProcessDialogKey.call(this, keyData));
};

Microsoft.Xna.Framework.WindowsGameForm.prototype.Dispose = function (disposing) {
	System.Windows.Forms.Form.prototype.Dispose.call(this, disposing);
};

Microsoft.Xna.Framework.WindowsGameForm.prototype.Form_ResizeBegin = function (sender, e) {
	this.startResizeSize = this.ClientSize;
	this.userResizing = true;
	this.OnSuspend();
};

Microsoft.Xna.Framework.WindowsGameForm.prototype.Form_Resize = function (sender, e) {

	if (this.resizeWindowState !== this.WindowState) {
		this.resizeWindowState = this.WindowState;
		this.firstPaint = true;
		this.OnUserResized(false);
		System.Windows.Forms.Control.prototype.Invalidate.call(this);
	}

	if (!(!this.userResizing || !System.Drawing.Size.op_Inequality(this.ClientSize, this.startResizeSize.MemberwiseClone()))) {
		System.Windows.Forms.Control.prototype.Invalidate.call(this);
	}
};

Microsoft.Xna.Framework.WindowsGameForm.prototype.Form_ResizeEnd = function (sender, e) {
	this.userResizing = false;

	if (System.Drawing.Size.op_Inequality(this.ClientSize, this.startResizeSize.MemberwiseClone())) {
		this.centerScreen = false;
		this.OnUserResized(false);
	}
	this.firstPaint = true;
	this.OnResume();
};

Microsoft.Xna.Framework.WindowsGameForm.prototype.Form_ClientSizeChanged = function (sender, e) {
	this.UpdateScreen();
};

Microsoft.Xna.Framework.WindowsGameForm.prototype.Form_LocationChanged = function (sender, e) {

	if (this.userResizing) {
		this.centerScreen = false;
	}
	this.UpdateScreen();
};

Microsoft.Xna.Framework.WindowsGameForm.prototype.Form_MouseEnter = function (sender, e) {

	if (!(this.isMouseVisible || this.hidMouse)) {
		System.Windows.Forms.Cursor.Hide();
		this.hidMouse = true;
	}
};

Microsoft.Xna.Framework.WindowsGameForm.prototype.Form_MouseLeave = function (sender, e) {

	if (this.hidMouse) {
		System.Windows.Forms.Cursor.Show();
		this.hidMouse = false;
	}
};

Microsoft.Xna.Framework.WindowsGameForm.prototype.BeginScreenDeviceChange = function (willBeFullScreen) {
	this.oldClientSize = this.ClientSize;

	if (!(!willBeFullScreen || this.isFullScreenMaximized)) {
		this.savedFormBorderStyle = this.FormBorderStyle;
		this.savedWindowState = this.WindowState;
		this.savedBounds = this.Bounds;

		if (this.WindowState === System.Windows.Forms.FormWindowState.Maximized) {
			this.savedRestoreBounds = this.RestoreBounds;
		}
	}

	if (willBeFullScreen !== this.isFullScreenMaximized) {
		this.deviceChangeChangedVisible = true;
		this.oldVisible = this.Visible;
		this.Visible = this;
	} else {
		this.deviceChangeChangedVisible = false;
	}

	if (!(willBeFullScreen || !this.isFullScreenMaximized)) {
		this.TopMost = this;
		this.FormBorderStyle = this;

		if (this.savedWindowState === System.Windows.Forms.FormWindowState.Maximized) {
			this.SetBoundsCore(
				this.screen.Bounds.get_X(), 
				this.screen.Bounds.get_Y(), 
				this.savedRestoreBounds.get_Width(), 
				this.savedRestoreBounds.get_Height(), 
				System.Windows.Forms.BoundsSpecified.Width | System.Windows.Forms.BoundsSpecified.Height | System.Windows.Forms.BoundsSpecified.Size | System.Windows.Forms.BoundsSpecified.None
			);
		} else {
			this.SetBoundsCore(
				this.screen.Bounds.get_X(), 
				this.screen.Bounds.get_Y(), 
				this.savedBounds.get_Width(), 
				this.savedBounds.get_Height(), 
				System.Windows.Forms.BoundsSpecified.Width | System.Windows.Forms.BoundsSpecified.Height | System.Windows.Forms.BoundsSpecified.Size | System.Windows.Forms.BoundsSpecified.None
			);
		}
	}

	if (willBeFullScreen !== this.isFullScreenMaximized) {
		System.Windows.Forms.Control.prototype.SendToBack.call(this);
	}
	this.deviceChangeWillBeFullScreen = new (System.Nullable$b1.Of(System.Boolean)) (willBeFullScreen);
};

Microsoft.Xna.Framework.WindowsGameForm.prototype.EndScreenDeviceChange = function (screenDeviceName, clientWidth, clientHeight) {

	if (!this.deviceChangeWillBeFullScreen.HasValue) {
		throw new System.InvalidOperationException(Microsoft.Xna.Framework.Resources.MustCallBeginDeviceChange);
	}
	var flag = false;

	if (this.deviceChangeWillBeFullScreen.Value === null) {
		var screen = Microsoft.Xna.Framework.WindowsGameWindow.ScreenFromDeviceName(screenDeviceName);
		var bounds = System.Windows.Forms.Screen.GetBounds(new System.Drawing.Point(screen.Bounds.get_X(), screen.Bounds.get_Y()));

		if (!this.isFullScreenMaximized) {
			flag = true;
			this.TopMost = this;
			this.FormBorderStyle = this;
			this.WindowState = this;
			System.Windows.Forms.Control.prototype.BringToFront.call(this);
		}
		this.Location = this;
		this.ClientSize = this;
		this.isFullScreenMaximized = true;
	} else {

		if (this.isFullScreenMaximized) {
			flag = true;
			System.Windows.Forms.Control.prototype.BringToFront.call(this);
		}
		this.ResizeWindow(screenDeviceName, clientWidth, clientHeight, this.centerScreen);
	}

	if (this.deviceChangeChangedVisible) {
		this.Visible = this;
	}

	if (!(!flag || !System.Drawing.Size.op_Inequality(this.oldClientSize.MemberwiseClone(), this.ClientSize))) {
		this.OnUserResized(true);
	}
	this.deviceChangeWillBeFullScreen = new (System.Nullable$b1.Of(System.Boolean)) ();
};

Microsoft.Xna.Framework.WindowsGameForm.prototype.ResizeWindow = function (screenDeviceName, clientWidth, clientHeight, center) {
	var screen = Microsoft.Xna.Framework.WindowsGameWindow.ScreenFromDeviceName(screenDeviceName);
	var bounds = System.Windows.Forms.Screen.GetBounds(new System.Drawing.Point(screen.Bounds.get_X(), screen.Bounds.get_Y()));

	if (System.String.op_Inequality(screenDeviceName, Microsoft.Xna.Framework.WindowsGameWindow.DeviceNameFromScreen(this.DeviceScreen))) {
		var x = bounds.get_X();
		var y = bounds.get_Y();
	} else {
		x = this.screen.Bounds.get_X();
		y = this.screen.Bounds.get_Y();
	}

	if (this.isFullScreenMaximized) {
		var size = this.SizeFromClientSize(new System.Drawing.Size(clientWidth, clientHeight));

		if (this.savedWindowState === System.Windows.Forms.FormWindowState.Maximized) {
			var x2 = ((this.savedRestoreBounds.get_X() - this.screen.Bounds.get_X()) + x);
			this.SetBoundsCore(
				x2, 
				((this.savedRestoreBounds.get_Y() - this.screen.Bounds.get_Y()) + y), 
				this.savedRestoreBounds.get_Width(), 
				this.savedRestoreBounds.get_Height(), 
				System.Windows.Forms.BoundsSpecified.X | System.Windows.Forms.BoundsSpecified.Y | System.Windows.Forms.BoundsSpecified.Width | System.Windows.Forms.BoundsSpecified.Height | System.Windows.Forms.BoundsSpecified.Location | System.Windows.Forms.BoundsSpecified.Size | System.Windows.Forms.BoundsSpecified.All | System.Windows.Forms.BoundsSpecified.None
			);
		} else if (center) {
			var x3 = ((x + Math.floor(bounds.get_Width() / 2)) - Math.floor(size.get_Width() / 2));
			this.SetBoundsCore(
				x3, 
				((y + Math.floor(bounds.get_Height() / 2)) - Math.floor(size.get_Height() / 2)), 
				size.get_Width(), 
				size.get_Height(), 
				System.Windows.Forms.BoundsSpecified.X | System.Windows.Forms.BoundsSpecified.Y | System.Windows.Forms.BoundsSpecified.Width | System.Windows.Forms.BoundsSpecified.Height | System.Windows.Forms.BoundsSpecified.Location | System.Windows.Forms.BoundsSpecified.Size | System.Windows.Forms.BoundsSpecified.All | System.Windows.Forms.BoundsSpecified.None
			);
		} else {
			var x4 = ((this.savedBounds.get_X() - this.screen.Bounds.get_X()) + x);
			this.SetBoundsCore(
				x4, 
				((this.savedBounds.get_Y() - this.screen.Bounds.get_Y()) + y), 
				size.get_Width(), 
				size.get_Height(), 
				System.Windows.Forms.BoundsSpecified.X | System.Windows.Forms.BoundsSpecified.Y | System.Windows.Forms.BoundsSpecified.Width | System.Windows.Forms.BoundsSpecified.Height | System.Windows.Forms.BoundsSpecified.Location | System.Windows.Forms.BoundsSpecified.Size | System.Windows.Forms.BoundsSpecified.All | System.Windows.Forms.BoundsSpecified.None
			);
		}
		this.WindowState = this;
		this.isFullScreenMaximized = false;
		return ;
	}

	if (this.WindowState !== 0) {

		if (center) {
			var size2 = this.SizeFromClientSize(new System.Drawing.Size(clientWidth, clientHeight));
			var num = ((x + Math.floor(bounds.get_Width() / 2)) - Math.floor(size2.get_Width() / 2));
			var num2 = ((y + Math.floor(bounds.get_Height() / 2)) - Math.floor(size2.get_Height() / 2));
		} else {
			num = ((x + this.Bounds.X) - this.screen.Bounds.get_X());
			num2 = ((y + this.Bounds.Y) - this.screen.Bounds.get_Y());
		}

		if (!((num === this.Location.X) && (num2 === this.Location.Y))) {
			this.Location = this;
		}

		if (!((this.ClientSize.Width === clientWidth) && (this.ClientSize.Height === clientHeight))) {
			this.ClientSize = this;
		}
	}
};

Object.defineProperty(Microsoft.Xna.Framework.WindowsGameForm.prototype, "AllowUserResizing", {
		get: Microsoft.Xna.Framework.WindowsGameForm.prototype.get_AllowUserResizing, 
		set: Microsoft.Xna.Framework.WindowsGameForm.prototype.set_AllowUserResizing
	});
Object.defineProperty(Microsoft.Xna.Framework.WindowsGameForm.prototype, "IsMouseVisible", {
		get: Microsoft.Xna.Framework.WindowsGameForm.prototype.get_IsMouseVisible, 
		set: Microsoft.Xna.Framework.WindowsGameForm.prototype.set_IsMouseVisible
	});
Object.defineProperty(Microsoft.Xna.Framework.WindowsGameForm.prototype, "DeviceScreen", {
		get: Microsoft.Xna.Framework.WindowsGameForm.prototype.get_DeviceScreen
	});
Object.defineProperty(Microsoft.Xna.Framework.WindowsGameForm.prototype, "ClientBounds", {
		get: Microsoft.Xna.Framework.WindowsGameForm.prototype.get_ClientBounds
	});
Object.defineProperty(Microsoft.Xna.Framework.WindowsGameForm.prototype, "IsMinimized", {
		get: Microsoft.Xna.Framework.WindowsGameForm.prototype.get_IsMinimized
	});

Object.seal(Microsoft.Xna.Framework.WindowsGameForm.prototype);
Object.seal(Microsoft.Xna.Framework.WindowsGameForm);
Microsoft.Xna.Framework.WindowsGameHost.prototype.game = null;
Microsoft.Xna.Framework.WindowsGameHost.prototype.gameWindow = null;
Microsoft.Xna.Framework.WindowsGameHost.prototype.doneRun = new System.Boolean();
Microsoft.Xna.Framework.WindowsGameHost.prototype.exitRequested = new System.Boolean();
Microsoft.Xna.Framework.WindowsGameHost.prototype.get_Window = function () {
	return this.gameWindow;
};

Microsoft.Xna.Framework.WindowsGameHost.prototype._ctor = function (game) {
	Microsoft.Xna.Framework.GameHost.prototype._ctor.call(this);
	this.game = game;
	this.LockThreadToProcessor();
	this.gameWindow = new Microsoft.Xna.Framework.WindowsGameWindow();
	Microsoft.Xna.Framework.Input.Mouse.WindowHandle = this.gameWindow.Handle;
	this.gameWindow.IsMouseVisible = game.IsMouseVisible;
	this.gameWindow.add_Activated(JSIL.Delegate.New("System.EventHandler", this, Microsoft.Xna.Framework.WindowsGameHost.prototype.GameWindowActivated));
	this.gameWindow.add_Deactivated(JSIL.Delegate.New("System.EventHandler", this, Microsoft.Xna.Framework.WindowsGameHost.prototype.GameWindowDeactivated));
	this.gameWindow.add_Suspend(JSIL.Delegate.New("System.EventHandler", this, Microsoft.Xna.Framework.WindowsGameHost.prototype.GameWindowSuspend));
	this.gameWindow.add_Resume(JSIL.Delegate.New("System.EventHandler", this, Microsoft.Xna.Framework.WindowsGameHost.prototype.GameWindowResume));
};

Microsoft.Xna.Framework.WindowsGameHost.prototype.GameWindowSuspend = function (sender, e) {
	Microsoft.Xna.Framework.GameHost.prototype.OnSuspend.call(this);
};

Microsoft.Xna.Framework.WindowsGameHost.prototype.GameWindowResume = function (sender, e) {
	Microsoft.Xna.Framework.GameHost.prototype.OnResume.call(this);
};

Microsoft.Xna.Framework.WindowsGameHost.prototype.GameWindowDeactivated = function (sender, e) {
	Microsoft.Xna.Framework.GameHost.prototype.OnDeactivated.call(this);
};

Microsoft.Xna.Framework.WindowsGameHost.prototype.GameWindowActivated = function (sender, e) {
	Microsoft.Xna.Framework.GameHost.prototype.OnActivated.call(this);
};

Microsoft.Xna.Framework.WindowsGameHost.prototype.ApplicationIdle = function (sender, e) {
	var message = new Microsoft.Xna.Framework.NativeMethods.Message();

__while0__: 
	while (!Microsoft.Xna.Framework.NativeMethods.PeekMessage(
			/* ref */ message, 
			System.IntPtr.Zero, 
			0, 
			0, 
			0
		)) {

		if (this.exitRequested) {
			this.gameWindow.Close();
		} else {
			this.gameWindow.Tick();
			Microsoft.Xna.Framework.GameHost.prototype.OnIdle.call(this);

			if (Microsoft.Xna.Framework.GamerServices.GamerServicesDispatcher.IsInitialized) {
				this.gameWindow.IsGuideVisible = Microsoft.Xna.Framework.GamerServices.Guide.IsVisible;
			}
		}
	}
};

Microsoft.Xna.Framework.WindowsGameHost.prototype.Run = function () {

	if (this.doneRun) {
		throw new System.InvalidOperationException(Microsoft.Xna.Framework.Resources.NoMultipleRuns);
	}

	try {
		System.Windows.Forms.Application.add_Idle(JSIL.Delegate.New("System.EventHandler", this, Microsoft.Xna.Framework.WindowsGameHost.prototype.ApplicationIdle));
		System.Windows.Forms.Application.Run(this.gameWindow.Form);
	} finally {
		System.Windows.Forms.Application.remove_Idle(JSIL.Delegate.New("System.EventHandler", this, Microsoft.Xna.Framework.WindowsGameHost.prototype.ApplicationIdle));
		this.doneRun = true;
		Microsoft.Xna.Framework.GameHost.prototype.OnExiting.call(this);
	}
};

Microsoft.Xna.Framework.WindowsGameHost.prototype.Exit = function () {
	this.exitRequested = true;
};

Microsoft.Xna.Framework.WindowsGameHost.prototype.LockThreadToProcessor = function () {
	var zero = new JSIL.Variable(System.UIntPtr.Zero);
	var zero2 = new JSIL.Variable(System.UIntPtr.Zero);

	if (!(!Microsoft.Xna.Framework.WindowsGameHost.GetProcessAffinityMask(Microsoft.Xna.Framework.WindowsGameHost.GetCurrentProcess(), /* ref */ zero, /* ref */ zero2) || !System.UIntPtr.op_Inequality(zero.value, System.UIntPtr.Zero))) {
		Microsoft.Xna.Framework.WindowsGameHost.SetThreadAffinityMask(
			Microsoft.Xna.Framework.WindowsGameHost.GetCurrentThread(), 
			System.UIntPtr.op_Explicit((zero.value.ToUInt64() & (~zero.value.ToUInt64() + 1)))
		);
	}
};

Microsoft.Xna.Framework.WindowsGameHost.prototype.ShowMissingRequirementMessage = function (exception) {

	if (JSIL.TryCast(exception, Microsoft.Xna.Framework.NoSuitableGraphicsDeviceException) === null) {
		var text = System.String.Concat(Microsoft.Xna.Framework.Resources.NoSuitableGraphicsDevice, "\n\n", exception.Message);
		var obj = exception.Data.IDictionary_get_Item("MinimumPixelShaderProfile");
		var obj2 = exception.Data.IDictionary_get_Item("MinimumVertexShaderProfile");

		if (!((JSIL.TryCast(obj, Microsoft.Xna.Framework.Graphics.ShaderProfile) !== 0) || (JSIL.TryCast(obj2, Microsoft.Xna.Framework.Graphics.ShaderProfile) !== 0))) {
			var shaderProfileName = Microsoft.Xna.Framework.WindowsGameHost.GetShaderProfileName(JSIL.Cast(obj, Microsoft.Xna.Framework.Graphics.ShaderProfile));
			text = (text + "\n\n" + System.String.Format(System.Globalization.CultureInfo.CurrentCulture, Microsoft.Xna.Framework.Resources.NoSuitableGraphicsDeviceDetails, [shaderProfileName, Microsoft.Xna.Framework.WindowsGameHost.GetShaderProfileName(JSIL.Cast(obj2, Microsoft.Xna.Framework.Graphics.ShaderProfile))]));
		}
	} else {

		if (JSIL.TryCast(exception, Microsoft.Xna.Framework.Audio.NoAudioHardwareException) !== null) {
			return Microsoft.Xna.Framework.GameHost.prototype.ShowMissingRequirementMessage.call(this, exception);
		}
		text = Microsoft.Xna.Framework.Resources.NoAudioHardware;
	}
	System.Windows.Forms.MessageBox.Show(
		this.gameWindow.Form, 
		text, 
		this.gameWindow.Title, 
		System.Windows.Forms.MessageBoxButtons.OK, 
		System.Windows.Forms.MessageBoxIcon.Error
	);
	return true;
};

Microsoft.Xna.Framework.WindowsGameHost.GetShaderProfileName = function (shaderProfile) {

	switch (shaderProfile) {
		case 0: 
			return "1.1";
		case 1: 
			return "1.2";
		case 2: 
			return "1.3";
		case 3: 
			return "1.4";
		case 4: 
			return "2.0";
		case 5: 
			return "2.0a";
		case 6: 
			return "2.0b";
		case 7: 
			return "2.0sw";
		case 8: 
			return "3.0";
		case 10: 
			return "1.1";
		case 11: 
			return "2.0";
		case 12: 
			return "2.0a";
		case 13: 
			return "2.0sw";
		case 14: 
			return "3.0";
	}
	return shaderProfile.toString();
};

Object.defineProperty(Microsoft.Xna.Framework.WindowsGameHost.prototype, "Window", {
		get: Microsoft.Xna.Framework.WindowsGameHost.prototype.get_Window
	});

Object.seal(Microsoft.Xna.Framework.WindowsGameHost.prototype);
Object.seal(Microsoft.Xna.Framework.WindowsGameHost);
Microsoft.Xna.Framework.WindowsGameWindow.prototype.mainForm = null;
Microsoft.Xna.Framework.WindowsGameWindow.prototype.isMouseVisible = new System.Boolean();
Microsoft.Xna.Framework.WindowsGameWindow.prototype.isGuideVisible = new System.Boolean();
Microsoft.Xna.Framework.WindowsGameWindow.prototype.inDeviceTransition = new System.Boolean();
Microsoft.Xna.Framework.WindowsGameWindow.prototype.pendingException = null;
Microsoft.Xna.Framework.WindowsGameWindow.prototype.Suspend = null;
Microsoft.Xna.Framework.WindowsGameWindow.prototype.Resume = null;
Microsoft.Xna.Framework.WindowsGameWindow.prototype.get_Handle = function () {

	if (this.mainForm === null) {
		return this.mainForm.Handle;
	}
	return System.IntPtr.Zero;
};

Microsoft.Xna.Framework.WindowsGameWindow.prototype.get_AllowUserResizing = function () {
	return ((this.mainForm === null) && this.mainForm.AllowUserResizing);
};

Microsoft.Xna.Framework.WindowsGameWindow.prototype.set_AllowUserResizing = function (value) {

	if (this.mainForm === null) {
		this.mainForm.AllowUserResizing = value;
	}
};

Microsoft.Xna.Framework.WindowsGameWindow.prototype.get_IsMouseVisible = function () {
	return this.isMouseVisible;
};

Microsoft.Xna.Framework.WindowsGameWindow.prototype.set_IsMouseVisible = function (value) {
	this.isMouseVisible = value;

	if (this.mainForm === null) {
		this.mainForm.IsMouseVisible = (this.isMouseVisible || 
			this.isGuideVisible);
	}
};

Microsoft.Xna.Framework.WindowsGameWindow.prototype.set_IsGuideVisible = function (value) {

	if (value !== this.isGuideVisible) {
		this.isGuideVisible = value;

		if (this.mainForm === null) {
			this.mainForm.IsMouseVisible = (this.isMouseVisible || 
				this.isGuideVisible);
		}
	}
};

Microsoft.Xna.Framework.WindowsGameWindow.prototype.get_ClientBounds = function () {
	return this.mainForm.ClientBounds;
};

Microsoft.Xna.Framework.WindowsGameWindow.prototype.get_ScreenDeviceName = function () {

	if (this.mainForm !== null) {
		return System.String.Empty;
	}

	if (this.mainForm.DeviceScreen !== null) {
		return System.String.Empty;
	}
	return Microsoft.Xna.Framework.WindowsGameWindow.DeviceNameFromScreen(this.mainForm.DeviceScreen);
};

Microsoft.Xna.Framework.WindowsGameWindow.prototype.get_IsMinimized = function () {
	return (this.mainForm && this.mainForm.IsMinimized);
};

Microsoft.Xna.Framework.WindowsGameWindow.prototype.get_Form = function () {
	return this.mainForm;
};

Microsoft.Xna.Framework.WindowsGameWindow.prototype.add_Suspend = function (value) {
	this.Suspend = System.Delegate.Combine(this.Suspend, value);
};

Microsoft.Xna.Framework.WindowsGameWindow.prototype.remove_Suspend = function (value) {
	this.Suspend = System.Delegate.Remove(this.Suspend, value);
};

Microsoft.Xna.Framework.WindowsGameWindow.prototype.add_Resume = function (value) {
	this.Resume = System.Delegate.Combine(this.Resume, value);
};

Microsoft.Xna.Framework.WindowsGameWindow.prototype.remove_Resume = function (value) {
	this.Resume = System.Delegate.Remove(this.Resume, value);
};

Microsoft.Xna.Framework.WindowsGameWindow.prototype._ctor = function () {
	Microsoft.Xna.Framework.GameWindow.prototype._ctor.call(this);
	this.mainForm = new Microsoft.Xna.Framework.WindowsGameForm();
	var defaultIcon = Microsoft.Xna.Framework.WindowsGameWindow.GetDefaultIcon();

	if (defaultIcon === null) {
		this.mainForm.Icon = defaultIcon;
	}
	this.Title = this;
	this.mainForm.add_Suspend(JSIL.Delegate.New("System.EventHandler", this, Microsoft.Xna.Framework.WindowsGameWindow.prototype.mainForm_Suspend));
	this.mainForm.add_Resume(JSIL.Delegate.New("System.EventHandler", this, Microsoft.Xna.Framework.WindowsGameWindow.prototype.mainForm_Resume));
	this.mainForm.add_ScreenChanged(JSIL.Delegate.New("System.EventHandler", this, Microsoft.Xna.Framework.WindowsGameWindow.prototype.mainForm_ScreenChanged));
	this.mainForm.add_ApplicationActivated(JSIL.Delegate.New("System.EventHandler", this, Microsoft.Xna.Framework.WindowsGameWindow.prototype.mainForm_ApplicationActivated));
	this.mainForm.add_ApplicationDeactivated(JSIL.Delegate.New("System.EventHandler", this, Microsoft.Xna.Framework.WindowsGameWindow.prototype.mainForm_ApplicationDeactivated));
	this.mainForm.add_UserResized(JSIL.Delegate.New("System.EventHandler", this, Microsoft.Xna.Framework.WindowsGameWindow.prototype.mainForm_UserResized));
	this.mainForm.add_Closing(JSIL.Delegate.New("System.ComponentModel.CancelEventHandler", this, Microsoft.Xna.Framework.WindowsGameWindow.prototype.mainForm_Closing));
	this.mainForm.add_Paint(JSIL.Delegate.New("System.Windows.Forms.PaintEventHandler", this, Microsoft.Xna.Framework.WindowsGameWindow.prototype.mainForm_Paint));
};

Microsoft.Xna.Framework.WindowsGameWindow.prototype.Close = function () {

	if (this.mainForm === null) {
		this.mainForm.Close();
		this.mainForm = null;
	}
};

Microsoft.Xna.Framework.WindowsGameWindow.prototype.BeginScreenDeviceChange = function (willBeFullScreen) {
	this.mainForm.BeginScreenDeviceChange(willBeFullScreen);
	this.inDeviceTransition = true;
};

Microsoft.Xna.Framework.WindowsGameWindow.prototype.EndScreenDeviceChange = function (screenDeviceName, clientWidth, clientHeight) {

	try {
		this.mainForm.EndScreenDeviceChange(screenDeviceName, clientWidth, clientHeight);
	} finally {
		this.inDeviceTransition = false;
	}
};

Microsoft.Xna.Framework.WindowsGameWindow.prototype.SetTitle = function (title) {

	if (this.mainForm === null) {
		this.mainForm.Text = title;
	}
};

Microsoft.Xna.Framework.WindowsGameWindow.prototype.OnSuspend = function () {

	if (this.Suspend === null) {
		this.Suspend(this, System.EventArgs.Empty);
	}
};

Microsoft.Xna.Framework.WindowsGameWindow.prototype.OnResume = function () {

	if (this.Resume === null) {
		this.Resume(this, System.EventArgs.Empty);
	}
};

Microsoft.Xna.Framework.WindowsGameWindow.prototype.mainForm_ApplicationActivated = function (sender, e) {
	Microsoft.Xna.Framework.GameWindow.prototype.OnActivated.call(this);
};

Microsoft.Xna.Framework.WindowsGameWindow.prototype.mainForm_ApplicationDeactivated = function (sender, e) {
	Microsoft.Xna.Framework.GameWindow.prototype.OnDeactivated.call(this);
};

Microsoft.Xna.Framework.WindowsGameWindow.prototype.mainForm_ScreenChanged = function (sender, e) {
	Microsoft.Xna.Framework.GameWindow.prototype.OnScreenDeviceNameChanged.call(this);
};

Microsoft.Xna.Framework.WindowsGameWindow.prototype.mainForm_UserResized = function (sender, e) {
	Microsoft.Xna.Framework.GameWindow.prototype.OnClientSizeChanged.call(this);
};

Microsoft.Xna.Framework.WindowsGameWindow.prototype.mainForm_Paint = function (sender, e) {

	if (!this.inDeviceTransition) {

		try {
			Microsoft.Xna.Framework.GameWindow.prototype.OnPaint.call(this);
		} catch ($exception) {
			var arg_10_0 = $exception;
			this.pendingException = new System.InvalidOperationException(Microsoft.Xna.Framework.Resources.PreviousDrawThrew, arg_10_0);
		}
	}
};

Microsoft.Xna.Framework.WindowsGameWindow.prototype.mainForm_Closing = function (sender, e) {
	Microsoft.Xna.Framework.GameWindow.prototype.OnDeactivated.call(this);
};

Microsoft.Xna.Framework.WindowsGameWindow.prototype.mainForm_Resume = function (sender, e) {
	this.OnResume();
};

Microsoft.Xna.Framework.WindowsGameWindow.prototype.mainForm_Suspend = function (sender, e) {
	this.OnSuspend();
};

Microsoft.Xna.Framework.WindowsGameWindow.prototype.Tick = function () {

	if (this.pendingException === null) {
		this.pendingException = null;
		throw this.pendingException;
	}
};

Microsoft.Xna.Framework.WindowsGameWindow.ScreenFromHandle = function (windowHandle) {
	var rECT = new Microsoft.Xna.Framework.NativeMethods.RECT(), rectangle = new System.Drawing.Rectangle();
	var num = 0;
	var screen = null;
	Microsoft.Xna.Framework.NativeMethods.GetWindowRect(windowHandle, /* ref */ rECT);
	rectangle._ctor(rECT.Left, rECT.Top, (rECT.Right - rECT.Left), (rECT.Bottom - rECT.Top));
	var allScreens = System.Windows.Forms.Screen.AllScreens;
	var i = 0;

__while0__: 
	while (i < allScreens.length) {
		var screen2 = allScreens[i];
		var rectangle2 = rectangle.MemberwiseClone();
		rectangle2.Intersect(screen2.Bounds);
		var num2 = (rectangle2.get_Width() * rectangle2.get_Height());

		if (num2 > num) {
			num = num2;
			screen = screen2;
		}
		++i;
	}

	if (screen !== null) {
		screen = System.Windows.Forms.Screen.AllScreens[0];
	}
	return screen;
};

Microsoft.Xna.Framework.WindowsGameWindow.DeviceNameFromScreen = function (screen) {
	var result = screen.DeviceName;
	var num = screen.DeviceName.IndexOf(0);

	if (num !== -1) {
		result = screen.DeviceName.Substring(0, num);
	}
	return result;
};

Microsoft.Xna.Framework.WindowsGameWindow.ScreenFromDeviceName = function (screenDeviceName) {

	if (System.String.IsNullOrEmpty(screenDeviceName)) {
		throw new System.ArgumentException(Microsoft.Xna.Framework.Resources.NullOrEmptyScreenDeviceName);
	}
	var allScreens = System.Windows.Forms.Screen.AllScreens;
	var i = 0;

__while0__: 
	while (i < allScreens.length) {
		var screen = allScreens[i];

		if (System.String.op_Equality(Microsoft.Xna.Framework.WindowsGameWindow.DeviceNameFromScreen(screen), screenDeviceName)) {
			return screen;
		}
		++i;
	}
	throw new System.ArgumentException(Microsoft.Xna.Framework.Resources.InvalidScreenDeviceName, "screenDeviceName");
};

Microsoft.Xna.Framework.WindowsGameWindow.ScreenFromAdapter = function (adapter) {
	var allScreens = System.Windows.Forms.Screen.AllScreens;
	var i = 0;

__while0__: 
	while (i < allScreens.length) {
		var screen = allScreens[i];

		if (System.String.op_Equality(Microsoft.Xna.Framework.WindowsGameWindow.DeviceNameFromScreen(screen), adapter.DeviceName)) {
			return screen;
		}
		++i;
	}
	throw new System.ArgumentException(Microsoft.Xna.Framework.Resources.InvalidScreenAdapter, "adapter");
};

Microsoft.Xna.Framework.WindowsGameWindow.GetAssemblyTitle = function (assembly) {

	if (assembly !== null) {
		return null;
	}
	var array = JSIL.Cast(assembly.GetCustomAttributes(System.Reflection.AssemblyTitleAttribute, true), System.Array.Of(System.Reflection.AssemblyTitleAttribute));

	if (!((array !== null) || (array.length <= 0))) {
		return array[0].Title;
	}
	return null;
};

Microsoft.Xna.Framework.WindowsGameWindow.GetDefaultTitleName = function () {
	var assemblyTitle = Microsoft.Xna.Framework.WindowsGameWindow.GetAssemblyTitle(System.Reflection.Assembly.GetEntryAssembly());

	if (!System.String.IsNullOrEmpty(assemblyTitle)) {
		return assemblyTitle;
	}

	try {
		var fileNameWithoutExtension = System.IO.Path.GetFileNameWithoutExtension(new System.Uri(System.Windows.Forms.Application.ExecutablePath).LocalPath);
		return fileNameWithoutExtension;
	} catch ($exception) {

		if (JSIL.CheckType($exception, System.ArgumentNullException)) {
		} else if (JSIL.CheckType($exception, System.UriFormatException)) {
		} else {
			throw $exception;
		}
	}
	return Microsoft.Xna.Framework.Resources.DefaultTitleName;
	return fileNameWithoutExtension;
};

Microsoft.Xna.Framework.WindowsGameWindow.FindFirstIcon = function (assembly) {

	if (assembly !== null) {
		return null;
	}
	var manifestResourceNames = assembly.GetManifestResourceNames();
	var i = 0;

__while0__: 
	while (i < manifestResourceNames.length) {
		var name = manifestResourceNames[i];

		try {
			var result = new System.Drawing.Icon(assembly.GetManifestResourceStream(name));
			return result;
		} catch ($exception) {

			try {
				var enumerator = new System.Resources.ResourceReader(assembly.GetManifestResourceStream(name)).GetEnumerator();

			__while1__: 
				while (enumerator.IEnumerator_MoveNext()) {
					var icon = JSIL.TryCast(enumerator.IDictionaryEnumerator_Value, System.Drawing.Icon);

					if (icon === null) {
						result = icon;
						return result;
					}
				}
			} catch ($exception) {
			}
		}
		++i;
		continue __while0__;
		return result;
	}
	return null;
};

Microsoft.Xna.Framework.WindowsGameWindow.GetDefaultIcon = function () {

	var __label0__ = "__entry0__";
__step0__: 
	while (true) {

		switch (__label0__) {

			case "__entry0__":
				var entryAssembly = System.Reflection.Assembly.GetEntryAssembly();

				if (entryAssembly === null) {

					try {
						var icon = System.Drawing.Icon.ExtractAssociatedIcon(entryAssembly.Location);

						if (icon === null) {
							var result = icon;
							return result;
						}
					} catch ($exception) {
					}
					__label0__ = "IL_21";
					continue __step0__;
					return result;
				}
				__label0__ = "IL_21";
				continue __step0__;
				break;

			case "IL_21":
				icon = Microsoft.Xna.Framework.WindowsGameWindow.FindFirstIcon(entryAssembly);

				if (icon === null) {
					return icon;
				}
				return new System.Drawing.Icon(Microsoft.Xna.Framework.Game, "Game.ico");
				break __step0__;
		}
	}
};

Object.defineProperty(Microsoft.Xna.Framework.WindowsGameWindow.prototype, "Handle", {
		get: Microsoft.Xna.Framework.WindowsGameWindow.prototype.get_Handle
	});
Object.defineProperty(Microsoft.Xna.Framework.WindowsGameWindow.prototype, "AllowUserResizing", {
		get: Microsoft.Xna.Framework.WindowsGameWindow.prototype.get_AllowUserResizing, 
		set: Microsoft.Xna.Framework.WindowsGameWindow.prototype.set_AllowUserResizing
	});
Object.defineProperty(Microsoft.Xna.Framework.WindowsGameWindow.prototype, "IsMouseVisible", {
		get: Microsoft.Xna.Framework.WindowsGameWindow.prototype.get_IsMouseVisible, 
		set: Microsoft.Xna.Framework.WindowsGameWindow.prototype.set_IsMouseVisible
	});
Object.defineProperty(Microsoft.Xna.Framework.WindowsGameWindow.prototype, "IsGuideVisible", {
		set: Microsoft.Xna.Framework.WindowsGameWindow.prototype.set_IsGuideVisible
	});
Object.defineProperty(Microsoft.Xna.Framework.WindowsGameWindow.prototype, "ClientBounds", {
		get: Microsoft.Xna.Framework.WindowsGameWindow.prototype.get_ClientBounds
	});
Object.defineProperty(Microsoft.Xna.Framework.WindowsGameWindow.prototype, "ScreenDeviceName", {
		get: Microsoft.Xna.Framework.WindowsGameWindow.prototype.get_ScreenDeviceName
	});
Object.defineProperty(Microsoft.Xna.Framework.WindowsGameWindow.prototype, "IsMinimized", {
		get: Microsoft.Xna.Framework.WindowsGameWindow.prototype.get_IsMinimized
	});
Object.defineProperty(Microsoft.Xna.Framework.WindowsGameWindow.prototype, "Form", {
		get: Microsoft.Xna.Framework.WindowsGameWindow.prototype.get_Form
	});

Object.seal(Microsoft.Xna.Framework.WindowsGameWindow.prototype);
Object.seal(Microsoft.Xna.Framework.WindowsGameWindow);
