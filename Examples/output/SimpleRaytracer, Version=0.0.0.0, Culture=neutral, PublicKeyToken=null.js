JSIL.DeclareNamespace(this, "simpleray");
JSIL.MakeClass(System.Object, simpleray, "Vector3f", "simpleray.Vector3f");

JSIL.MakeClass(System.Object, simpleray, "Light", "simpleray.Light");

JSIL.MakeClass(System.Object, simpleray, "Ray", "simpleray.Ray");

JSIL.MakeClass(System.Object, simpleray, "RTObject", "simpleray.RTObject");

JSIL.MakeClass(simpleray.RTObject, simpleray, "Sphere", "simpleray.Sphere");

JSIL.MakeClass(simpleray.RTObject, simpleray, "Plane", "simpleray.Plane");

JSIL.MakeClass(System.Object, simpleray, "RayTracer", "simpleray.RayTracer");
JSIL.MakeClass(System.Object, simpleray.RayTracer, "$l$gc__DisplayClass1", "simpleray.RayTracer/<>c__DisplayClass1");


simpleray.Vector3f.prototype.x = 0;
simpleray.Vector3f.prototype.y = 0;
simpleray.Vector3f.prototype.z = 0;
simpleray.Vector3f.prototype._ctor = function (x, y, z) {
	System.Object.prototype._ctor.call(this);
	this.x = x;
	this.y = y;
	this.z = z;
};

simpleray.Vector3f.prototype.Dot = function (b) {
	return ((this.x * b.x) + (this.y * b.y) + (this.z * b.z));
};

simpleray.Vector3f.prototype.Normalise = function () {
	var f = (1 / System.Math.Sqrt(JSIL.Cast(this.Dot(this), System.Double)));
	this.x *= f;
	this.y *= f;
	this.z *= f;
};

simpleray.Vector3f.prototype.Magnitude = function () {
	return JSIL.Cast(System.Math.Sqrt(((this.x * this.x) + (this.y * this.y) + (this.z * this.z))), System.Single);
};

simpleray.Vector3f.op_Subtraction = function (a, b) {
	return new simpleray.Vector3f((a.x - b.x), (a.y - b.y), (a.z - b.z));
};

simpleray.Vector3f.op_UnaryNegation = function (a) {
	return new simpleray.Vector3f(-a.x, -a.y, -a.z);
};

simpleray.Vector3f.op_Multiply = function (a, b) {
	return new simpleray.Vector3f((a.x * b), (a.y * b), (a.z * b));
};

simpleray.Vector3f.op_Division = function (a, b) {
	return new simpleray.Vector3f((a.x / b), (a.y / b), (a.z / b));
};

simpleray.Vector3f.op_Addition = function (a, b) {
	return new simpleray.Vector3f((a.x + b.x), (a.y + b.y), (a.z + b.z));
};

simpleray.Vector3f.prototype.ReflectIn = function (normal) {
	var negVector = simpleray.Vector3f.op_UnaryNegation(this);
	return simpleray.Vector3f.op_Subtraction(simpleray.Vector3f.op_Multiply(normal, (2 * negVector.Dot(normal))), negVector);
};


Object.seal(simpleray.Vector3f.prototype);
Object.seal(simpleray.Vector3f);
simpleray.Light.prototype.position = null;
simpleray.Light.prototype._ctor = function (p) {
	System.Object.prototype._ctor.call(this);
	this.position = p;
};


Object.seal(simpleray.Light.prototype);
Object.seal(simpleray.Light);
Object.defineProperty(simpleray.Ray, "WORLD_MAX", { value: 1000 });
simpleray.Ray.prototype.origin = null;
simpleray.Ray.prototype.direction = null;
simpleray.Ray.prototype.closestHitObject = null;
simpleray.Ray.prototype.closestHitDistance = 0;
simpleray.Ray.prototype.hitPoint = null;
simpleray.Ray.prototype._ctor = function (o, d) {
	System.Object.prototype._ctor.call(this);
	this.origin = o;
	this.direction = d;
	this.closestHitDistance = 1000;
	this.closestHitObject = null;
};


Object.seal(simpleray.Ray.prototype);
Object.seal(simpleray.Ray);
simpleray.RTObject.prototype.__StructFields__ = {
	color: System.Drawing.Color
};
simpleray.RTObject.prototype._ctor = function () {
	System.Object.prototype._ctor.call(this);
};


Object.seal(simpleray.RTObject.prototype);
Object.seal(simpleray.RTObject);
simpleray.Sphere.prototype.position = null;
simpleray.Sphere.prototype.radius = 0;
simpleray.Sphere.prototype._ctor = function (p, r, c) {
	simpleray.RTObject.prototype._ctor.call(this);
	this.position = p;
	this.radius = r;
	this.color = c;
};

simpleray.Sphere.prototype.Intersect = function (ray) {
	var lightFromOrigin = simpleray.Vector3f.op_Subtraction(this.position, ray.origin);
	var v = lightFromOrigin.Dot(ray.direction);
	var hitDistance = (((this.radius * this.radius) + (v * v)) - (lightFromOrigin.x * lightFromOrigin.x) - (lightFromOrigin.y * lightFromOrigin.y) - (lightFromOrigin.z * lightFromOrigin.z));

	if (hitDistance < 0) {
		var result = -1;
	} else {
		hitDistance = (v - JSIL.Cast(System.Math.Sqrt(hitDistance), System.Single));

		if (hitDistance < 0) {
			result = -1;
		} else {
			result = hitDistance;
		}
	}
	return result;
};

simpleray.Sphere.prototype.GetSurfaceNormalAtPoint = function (p) {
	var normal = simpleray.Vector3f.op_Subtraction(p, this.position);
	normal.Normalise();
	return normal;
};


Object.seal(simpleray.Sphere.prototype);
Object.seal(simpleray.Sphere);
simpleray.Plane.prototype.normal = null;
simpleray.Plane.prototype.distance = 0;
simpleray.Plane.prototype._ctor = function (n, d, c) {
	simpleray.RTObject.prototype._ctor.call(this);
	this.normal = n;
	this.distance = d;
	this.color = c;
};

simpleray.Plane.prototype.Intersect = function (ray) {
	var normalDotRayDir = this.normal.Dot(ray.direction);

	if (normalDotRayDir === 0) {
		var result = -1;
	} else {
		var hitDistance = (-(this.normal.Dot(ray.origin) - this.distance) / normalDotRayDir);

		if (hitDistance < 0) {
			result = -1;
		} else {
			result = hitDistance;
		}
	}
	return result;
};

simpleray.Plane.prototype.GetSurfaceNormalAtPoint = function (p) {
	return this.normal;
};


Object.seal(simpleray.Plane.prototype);
Object.seal(simpleray.Plane);
Object.defineProperty(simpleray.RayTracer, "PI", { value: 3.1415927410125732 });
Object.defineProperty(simpleray.RayTracer, "PI_X_2", { value: 6.2831854820251465 });
Object.defineProperty(simpleray.RayTracer, "PI_OVER_2", { value: 1.5707963705062866 });
Object.defineProperty(simpleray.RayTracer, "CANVAS_WIDTH", { value: 640 });
Object.defineProperty(simpleray.RayTracer, "CANVAS_HEIGHT", { value: 480 });
Object.defineProperty(simpleray.RayTracer, "TINY", { value: 9.9999997473787516E-05 });
Object.defineProperty(simpleray.RayTracer, "MAX_DEPTH", { value: 3 });
Object.defineProperty(simpleray.RayTracer, "MATERIAL_DIFFUSE_COEFFICIENT", { value: 0.5 });
Object.defineProperty(simpleray.RayTracer, "MATERIAL_REFLECTION_COEFFICIENT", { value: 0.5 });
Object.defineProperty(simpleray.RayTracer, "MATERIAL_SPECULAR_COEFFICIENT", { value: 2 });
Object.defineProperty(simpleray.RayTracer, "MATERIAL_SPECULAR_POWER", { value: 50 });
simpleray.RayTracer.BG_COLOR = new System.Drawing.Color();
simpleray.RayTracer.eyePos = null;
simpleray.RayTracer.screenTopLeftPos = null;
simpleray.RayTracer.screenBottomRightPos = null;
simpleray.RayTracer.pixelWidth = 0;
simpleray.RayTracer.pixelHeight = 0;
simpleray.RayTracer.objects = null;
simpleray.RayTracer.lights = null;
simpleray.RayTracer.random = null;
simpleray.RayTracer.Main = function (args) {
	simpleray.RayTracer.objects = new (System.Collections.Generic.List$b1.Of(simpleray.RTObject)) ();
	simpleray.RayTracer.lights = new (System.Collections.Generic.List$b1.Of(simpleray.Light)) ();
	simpleray.RayTracer.random = new System.Random(1478650229);
	var canvas = new System.Drawing.Bitmap(640, 480);
	var i = 0;

__while0__: 
	while (i < 30) {
		var x = ((simpleray.RayTracer.random.NextDouble() * 10) - 5);
		var y = ((simpleray.RayTracer.random.NextDouble() * 10) - 5);
		var z = (simpleray.RayTracer.random.NextDouble() * 10);
		simpleray.RayTracer.objects.Add(new simpleray.Sphere(new simpleray.Vector3f(x, y, z), JSIL.Cast(simpleray.RayTracer.random.NextDouble(), System.Single), System.Drawing.Color.FromArgb(
					255, 
					simpleray.RayTracer.random.Next(255), 
					simpleray.RayTracer.random.Next(255), 
					simpleray.RayTracer.random.Next(255)
				)));
		++i;
	}
	simpleray.RayTracer.objects.Add(new simpleray.Plane(new simpleray.Vector3f(0, 1, 0), -10, System.Drawing.Color.Aquamarine));
	simpleray.RayTracer.lights.Add(new simpleray.Light(new simpleray.Vector3f(2, 0, 0)));
	simpleray.RayTracer.lights.Add(new simpleray.Light(new simpleray.Vector3f(0, 10, 7.5)));
	simpleray.RayTracer.pixelWidth = ((simpleray.RayTracer.screenBottomRightPos.x - simpleray.RayTracer.screenTopLeftPos.x) / 640);
	simpleray.RayTracer.pixelHeight = ((simpleray.RayTracer.screenTopLeftPos.y - simpleray.RayTracer.screenBottomRightPos.y) / 480);
	System.Console.WriteLine("Rendering...\n");
	System.Console.WriteLine("|0%---100%|");
	simpleray.RayTracer.RenderRow(canvas, 48, 0);
	canvas.Save("output.png");
};

simpleray.RayTracer.RenderRow = function (canvas, dotPeriod, y) {
	var $l$gc__DisplayClass = new simpleray.RayTracer.$l$gc__DisplayClass1();
	$l$gc__DisplayClass.canvas = canvas;
	$l$gc__DisplayClass.dotPeriod = dotPeriod;
	$l$gc__DisplayClass.y = y;

	if ($l$gc__DisplayClass.y < 480) {

		if (($l$gc__DisplayClass.y % $l$gc__DisplayClass.dotPeriod) === 0) {
			System.Console.Write("*");
		}
		var x = 0;

	__while0__: 
		while (x < 640) {
			$l$gc__DisplayClass.canvas.SetPixel(x, $l$gc__DisplayClass.y, simpleray.RayTracer.RenderPixel(x, $l$gc__DisplayClass.y));
			++x;
		}
		simpleray.RayTracer.SetTimeout(0, function () {
				simpleray.RayTracer.RenderRow($l$gc__DisplayClass.canvas, $l$gc__DisplayClass.dotPeriod, ($l$gc__DisplayClass.y + 1));
			});
	}
};

simpleray.RayTracer.SetTimeout = function (timeoutMs, action) {
	setTimeout(action, timeoutMs);
	return;
	action();
};

simpleray.RayTracer.CheckIntersection = function (/* ref */ ray) {
	var enumerator = simpleray.RayTracer.objects.GetEnumerator();

	try {

	__while0__: 
		while (enumerator.MoveNext()) {
			var obj = enumerator.Current;
			var hitDistance = obj.Intersect(ray.value);

			if (!((hitDistance >= ray.value.closestHitDistance) || (hitDistance <= 0))) {
				ray.value.closestHitObject = obj;
				ray.value.closestHitDistance = hitDistance;
			}
		}
	} finally {
		enumerator.IDisposable_Dispose();
	}
	ray.value.hitPoint = simpleray.Vector3f.op_Addition(ray.value.origin, simpleray.Vector3f.op_Multiply(ray.value.direction, ray.value.closestHitDistance));
};

simpleray.RayTracer.RenderPixel = function (x, y) {
	var eyeToPixelDir = simpleray.Vector3f.op_Subtraction(new simpleray.Vector3f((simpleray.RayTracer.screenTopLeftPos.x + (x * simpleray.RayTracer.pixelWidth)), (simpleray.RayTracer.screenTopLeftPos.y - (y * simpleray.RayTracer.pixelHeight)), 0), simpleray.RayTracer.eyePos);
	eyeToPixelDir.Normalise();
	return simpleray.RayTracer.Trace(new simpleray.Ray(simpleray.RayTracer.eyePos, eyeToPixelDir), 0);
};

simpleray.RayTracer.Trace = function (_ray, traceDepth) {
	var ray = new JSIL.Variable(_ray);
	simpleray.RayTracer.CheckIntersection(/* ref */ ray);

	if (!((ray.value.closestHitDistance < 1000) && (ray.value.closestHitObject !== null))) {
		var result = simpleray.RayTracer.BG_COLOR.MemberwiseClone();
	} else {
		var r = (0.15000000596046448 * JSIL.Cast(ray.value.closestHitObject.color.R, System.Single));
		var g = (0.15000000596046448 * JSIL.Cast(ray.value.closestHitObject.color.G, System.Single));
		var b = (0.15000000596046448 * JSIL.Cast(ray.value.closestHitObject.color.B, System.Single));
		var surfaceNormal = ray.value.closestHitObject.GetSurfaceNormalAtPoint(ray.value.hitPoint);
		var viewerDir = simpleray.Vector3f.op_UnaryNegation(ray.value.direction);
		var enumerator = simpleray.RayTracer.lights.GetEnumerator();

		try {

		__while0__: 
			while (enumerator.MoveNext()) {
				var light = enumerator.Current;
				var lightDir = new simpleray.Vector3f(0, 0, 0);
				lightDir = simpleray.Vector3f.op_Subtraction(light.position, ray.value.hitPoint);
				var lightDistance = lightDir.Magnitude();
				lightDir.Normalise();
				var shadowRay = new JSIL.Variable(new simpleray.Ray(simpleray.Vector3f.op_Addition(ray.value.hitPoint, simpleray.Vector3f.op_Multiply(lightDir, 9.9999997473787516E-05)), lightDir));
				shadowRay.value.closestHitDistance = lightDistance;
				simpleray.RayTracer.CheckIntersection(/* ref */ shadowRay);

				if (shadowRay.value.closestHitObject === null) {
					var cosLightAngleWithNormal = surfaceNormal.Dot(lightDir);

					if (cosLightAngleWithNormal > 0) {
						r += (0.5 * cosLightAngleWithNormal * JSIL.Cast(ray.value.closestHitObject.color.R, System.Single));
						g += (0.5 * cosLightAngleWithNormal * JSIL.Cast(ray.value.closestHitObject.color.G, System.Single));
						b += (0.5 * cosLightAngleWithNormal * JSIL.Cast(ray.value.closestHitObject.color.B, System.Single));
						var specularFactor = viewerDir.Dot(simpleray.Vector3f.op_Subtraction(simpleray.Vector3f.op_Multiply(surfaceNormal, (cosLightAngleWithNormal * 2)), lightDir));

						if (specularFactor > 0) {
							specularFactor = (2 * JSIL.Cast(System.Math.Pow(specularFactor, 50), System.Single));
							r += (specularFactor * JSIL.Cast(ray.value.closestHitObject.color.R, System.Single));
							g += (specularFactor * JSIL.Cast(ray.value.closestHitObject.color.G, System.Single));
							b += (specularFactor * JSIL.Cast(ray.value.closestHitObject.color.B, System.Single));
						}
					}
				}
			}
		} finally {
			enumerator.IDisposable_Dispose();
		}

		if (traceDepth < 3) {
			var reflectedDir = ray.value.direction.ReflectIn(surfaceNormal);
			var reflectionCol = simpleray.RayTracer.Trace(new simpleray.Ray(simpleray.Vector3f.op_Addition(ray.value.hitPoint, simpleray.Vector3f.op_Multiply(reflectedDir, 9.9999997473787516E-05)), reflectedDir), (traceDepth + 1));
			r += (0.5 * JSIL.Cast(reflectionCol.R, System.Single));
			g += (0.5 * JSIL.Cast(reflectionCol.G, System.Single));
			b += (0.5 * JSIL.Cast(reflectionCol.B, System.Single));
		}

		if (r > 255) {
			r = 255;
		}

		if (g > 255) {
			g = 255;
		}

		if (b > 255) {
			b = 255;
		}
		result = System.Drawing.Color.FromArgb(255, Math.floor(r), Math.floor(g), Math.floor(b));
	}
	return result;
};

simpleray.RayTracer.prototype._ctor = function () {
	System.Object.prototype._ctor.call(this);
};

simpleray.RayTracer._cctor = function () {
	simpleray.RayTracer.BG_COLOR = System.Drawing.Color.BlueViolet;
	simpleray.RayTracer.eyePos = new simpleray.Vector3f(0, 0, -5);
	simpleray.RayTracer.screenTopLeftPos = new simpleray.Vector3f(-6, 4, 0);
	simpleray.RayTracer.screenBottomRightPos = new simpleray.Vector3f(6, -4, 0);
};

simpleray.RayTracer._cctor();

simpleray.RayTracer.$l$gc__DisplayClass1.prototype.canvas = null;
simpleray.RayTracer.$l$gc__DisplayClass1.prototype.dotPeriod = 0;
simpleray.RayTracer.$l$gc__DisplayClass1.prototype.y = 0;
simpleray.RayTracer.$l$gc__DisplayClass1.prototype._ctor = function () {
	System.Object.prototype._ctor.call(this);
};


Object.seal(simpleray.RayTracer.$l$gc__DisplayClass1.prototype);
Object.seal(simpleray.RayTracer.$l$gc__DisplayClass1);
Object.seal(simpleray.RayTracer.prototype);
Object.seal(simpleray.RayTracer);
