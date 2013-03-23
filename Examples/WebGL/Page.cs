using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using JSIL;

namespace WebGL {
    public static class Page {
        public class AttributeCollection {
            public object VertexPosition, VertexNormal, TextureCoord;
        }

        public class UniformCollection {
            public object ProjectionMatrix, ModelViewMatrix, NormalMatrix;
            public object Sampler, UseLighting, AmbientColor, LightingDirection, DirectionalColor;
        }

        public class BufferCollection {
            public object CubeVertexPositions, CubeVertexNormals, CubeTextureCoords;
            public object CubeIndices;
        }

        public class MatrixCollection {
            public float[] Projection, ModelView, Normal;
        }
        
        public static dynamic GL;
        public static dynamic Document;
        public static dynamic Canvas;
        public static dynamic GLVector3, GLMatrix3, GLMatrix4;

        public static object ShaderProgram;
        public static object CrateTexture;

        public static readonly AttributeCollection Attributes = new AttributeCollection();
        public static readonly UniformCollection Uniforms = new UniformCollection();
        public static readonly BufferCollection Buffers = new BufferCollection();
        public static readonly MatrixCollection Matrices = new MatrixCollection();

        public static bool[] HeldKeys = new bool[255];
        public static int LastTime = 0;

        public static float Z = -5;
        public static float RotationX, RotationY;
        public static float SpeedX = 3, SpeedY = -3;

        public static void Load () {
            Document = Builtins.Global["document"];
            Canvas = Document.getElementById("canvas");
            GLVector3 = Builtins.Global["vec3"];
            GLMatrix4 = Builtins.Global["mat4"];
            GLMatrix3 = Builtins.Global["mat3"];

            if (InitGL()) {
                InitMatrices();
                InitShaders();
                InitBuffers();
                InitTexture();

                GL.clearColor(0f, 0f, 0f, 1f);
                GL.enable(GL.DEPTH_TEST);

                Document.onkeydown = (Action<dynamic>)OnKeyDown;
                Document.onkeyup = (Action<dynamic>)OnKeyUp;

                Tick();
            }
        }

        public static bool InitGL () {
            object gl = null;

            try {
                gl = Canvas.getContext("experimental-webgl");
            } catch {
            }

            if (Builtins.IsTruthy(gl)) {
                Builtins.Global["alert"]("Could not initialize WebGL");
                return false;
            } else {
                GL = gl;
                Console.WriteLine("Initialized WebGL");
                return true;
            }
        }

        public static void InitMatrices () {
            Matrices.ModelView = GLMatrix4.create();
            Matrices.Projection = GLMatrix4.create();
            Matrices.Normal = GLMatrix3.create();

            GLMatrix4.perspective(45, Canvas.width / Canvas.height, 0.1, 100.0, Matrices.Projection);
        }

        public static dynamic CompileShader (string filename) {
            var extension = Path.GetExtension(filename).ToLower();

            dynamic shaderObject;

            switch (extension) {
                case "fs":
                    shaderObject = GL.createShader(GL.FRAGMENT_SHADER);
                    break;

                case "vs":
                    shaderObject = GL.createShader(GL.VERTEX_SHADER);
                    break;

                default:
                    throw new NotImplementedException(extension);
            }

            var shaderText = File.ReadAllText(filename);

            GL.shaderSource(shaderObject, shaderText);
            GL.compileShader(shaderObject);

            bool compileStatus = GL.getShaderParameter(shaderObject, GL.COMPILE_STATUS);
            if (!compileStatus) {
                Builtins.Global["alert"](GL.getShaderInfoLog(shaderObject));
                return null;
            }

            Console.WriteLine("Loaded " + filename);
            return shaderObject;
        }

        public static void InitShaders () {
            var fragmentShader = CompileShader("crate.fs");
            var vertexShader = CompileShader("crate.vs");

            ShaderProgram = GL.createProgram();
            GL.attachShader(ShaderProgram, vertexShader);
            GL.attachShader(ShaderProgram, fragmentShader);
            GL.linkProgram(ShaderProgram);

            bool linkStatus = GL.getProgramParameter(ShaderProgram, GL.LINK_STATUS);
            if (!linkStatus) {
                Builtins.Global["alert"]("Could not link shader");
                return;
            }

            GL.useProgram(ShaderProgram);

            Attributes.VertexPosition = GL.getAttribLocation(ShaderProgram, "aVertexPosition");
            Attributes.VertexNormal = GL.getAttribLocation(ShaderProgram, "aVertexNormal");
            Attributes.TextureCoord = GL.getAttribLocation(ShaderProgram, "aTextureCoord");

            Uniforms.ProjectionMatrix = GL.getUniformLocation(ShaderProgram, "uPMatrix");
            Uniforms.ModelViewMatrix = GL.getUniformLocation(ShaderProgram, "uMVMatrix");
            Uniforms.NormalMatrix = GL.getUniformLocation(ShaderProgram, "uNMatrix");
            Uniforms.Sampler = GL.getUniformLocation(ShaderProgram, "uSampler");
            Uniforms.UseLighting = GL.getUniformLocation(ShaderProgram, "uUseLighting");
            Uniforms.AmbientColor = GL.getUniformLocation(ShaderProgram, "uAmbientColor");
            Uniforms.LightingDirection = GL.getUniformLocation(ShaderProgram, "uLightingDirection");
            Uniforms.DirectionalColor = GL.getUniformLocation(ShaderProgram, "uDirectionalColor");

            GL.enableVertexAttribArray(Attributes.VertexPosition);
            GL.enableVertexAttribArray(Attributes.VertexNormal);
            GL.enableVertexAttribArray(Attributes.TextureCoord);
        }

        public static void InitBuffers () {
            GL.bindBuffer(GL.ARRAY_BUFFER, Buffers.CubeVertexPositions = GL.createBuffer());
            GL.bufferData(GL.ARRAY_BUFFER, CubeData.Positions, GL.STATIC_DRAW);

            GL.bindBuffer(GL.ARRAY_BUFFER, Buffers.CubeVertexNormals = GL.createBuffer());
            GL.bufferData(GL.ARRAY_BUFFER, CubeData.Normals, GL.STATIC_DRAW);

            GL.bindBuffer(GL.ARRAY_BUFFER, Buffers.CubeTextureCoords = GL.createBuffer());
            GL.bufferData(GL.ARRAY_BUFFER, CubeData.TexCoords, GL.STATIC_DRAW);

            GL.bindBuffer(GL.ELEMENT_ARRAY_BUFFER, Buffers.CubeIndices = GL.createBuffer());
            GL.bufferData(GL.ELEMENT_ARRAY_BUFFER, CubeData.Indices, GL.STATIC_DRAW);
        }

        public static void UploadTexture (object textureHandle, object imageElement) {
            GL.pixelStorei(GL.UNPACK_FLIP_Y_WEBGL, true);

            GL.bindTexture(GL.TEXTURE_2D, textureHandle);
            GL.texImage2D(GL.TEXTURE_2D, 0, GL.RGBA, GL.RGBA, GL.UNSIGNED_BYTE, imageElement);
            GL.texParameteri(GL.TEXTURE_2D, GL.TEXTURE_MAG_FILTER, GL.LINEAR);
            GL.texParameteri(GL.TEXTURE_2D, GL.TEXTURE_MIN_FILTER, GL.LINEAR_MIPMAP_NEAREST);
            GL.generateMipmap(GL.TEXTURE_2D);

            GL.bindTexture(GL.TEXTURE_2D, null);
        }

        public static void InitTexture () {
            CrateTexture = GL.createTexture();

            var imageElement = Document.createElement("img");
            imageElement.onload = (Action)(
                () => UploadTexture(CrateTexture, imageElement)
            );

            try {
                var imageBytes = File.ReadAllBytes("crate.png");
                var objectUrl = Builtins.Global["JSIL"].GetObjectURLForBytes(imageBytes, "image/png");
                imageElement.src = objectUrl;
            } catch {
                // Object URLs probably aren't supported. Load the image a second time. ;/
                Console.WriteLine("Falling back to a second HTTP request for crate.png because Object URLs are not available");
                imageElement.src = "Files/crate.png";
            }
        }

        public static void Tick () {
            Builtins.Global["requestAnimFrame"]((Action)Tick);
            HandleKeys();
            DrawScene();
            Animate();
        }

        public static void HandleKeys () {
            if (HeldKeys[33]) {
                // Page Up
                Z -= 0.05f;
            }
            if (HeldKeys[34]) {
                // Page Down
                Z += 0.05f;
            }
            if (HeldKeys[37]) {
                // Left cursor key
                SpeedY -= 1f;
            }
            if (HeldKeys[39]) {
                // Right cursor key
                SpeedY += 1f;
            }
            if (HeldKeys[38]) {
                // Up cursor key
                SpeedX -= 1f;
            }
            if (HeldKeys[40]) {
                // Down cursor key
                SpeedX += 1f;
            }
        }

        public static float DegreesToRadians (float degrees) {
            return (float)(degrees * Math.PI / 180);
        }

        public static void DrawScene () {
            GL.viewport(0, 0, Canvas.width, Canvas.height);
            GL.clear(GL.COLOR_BUFFER_BIT | GL.DEPTH_BUFFER_BIT);

            GLMatrix4.identity(Matrices.ModelView);
            GLMatrix4.translate(Matrices.ModelView, new [] { 0, 0, Z });
            GLMatrix4.rotate(Matrices.ModelView, DegreesToRadians(RotationX), new [] { 1f, 0, 0 });
            GLMatrix4.rotate(Matrices.ModelView, DegreesToRadians(RotationY), new [] { 0, 1f, 0 });

            GL.bindBuffer(GL.ARRAY_BUFFER, Buffers.CubeVertexPositions);
            GL.vertexAttribPointer(Attributes.VertexPosition, 3, GL.FLOAT, false, 0, 0);

            GL.bindBuffer(GL.ARRAY_BUFFER, Buffers.CubeVertexNormals);
            GL.vertexAttribPointer(Attributes.VertexNormal, 3, GL.FLOAT, false, 0, 0);

            GL.bindBuffer(GL.ARRAY_BUFFER, Buffers.CubeTextureCoords);
            GL.vertexAttribPointer(Attributes.TextureCoord, 2, GL.FLOAT, false, 0, 0);

            GL.activeTexture(GL.TEXTURE0);
            GL.bindTexture(GL.TEXTURE_2D, CrateTexture);
            GL.uniform1i(Uniforms.Sampler, 0);

            bool lighting = Document.getElementById("lighting").@checked;
            GL.uniform1i(Uniforms.UseLighting, lighting ? 1 : 0);

            if (lighting) {
                GL.uniform3f(
                    Uniforms.AmbientColor,
                    float.Parse(Document.getElementById("ambientR").value),
                    float.Parse(Document.getElementById("ambientG").value),
                    float.Parse(Document.getElementById("ambientB").value)
                );

                var lightingDirection = new [] {
                    float.Parse(Document.getElementById("lightDirectionX").value),
                    float.Parse(Document.getElementById("lightDirectionY").value),
                    float.Parse(Document.getElementById("lightDirectionZ").value)
                };
                GLVector3.normalize(lightingDirection, lightingDirection);
                GLVector3.scale(lightingDirection, -1);

                GL.uniform3fv(Uniforms.LightingDirection, lightingDirection);

                GL.uniform3f(
                    Uniforms.DirectionalColor,
                    float.Parse(Document.getElementById("directionalR").value),
                    float.Parse(Document.getElementById("directionalG").value),
                    float.Parse(Document.getElementById("directionalB").value)
                );
            }

            GL.bindBuffer(GL.ELEMENT_ARRAY_BUFFER, Buffers.CubeIndices);

            GL.uniformMatrix4fv(Uniforms.ProjectionMatrix, false, Matrices.Projection);
            GL.uniformMatrix4fv(Uniforms.ModelViewMatrix, false, Matrices.ModelView);

            GLMatrix4.toInverseMat3(Matrices.ModelView, Matrices.Normal);
            GLMatrix3.transpose(Matrices.Normal);
            GL.uniformMatrix3fv(Uniforms.NormalMatrix, false, Matrices.Normal);

            GL.drawElements(GL.TRIANGLES, CubeData.Indices.Length, GL.UNSIGNED_SHORT, 0);
        }

        public static void Animate () {
            var now = Environment.TickCount;
            if (LastTime != 0) {
                var elapsed = now - LastTime;

                RotationX += (SpeedX * elapsed) / 1000f;
                RotationY += (SpeedY * elapsed) / 1000f;
            }

            LastTime = now;
        }

        public static void OnKeyDown (dynamic e) {
            HeldKeys[e.keyCode] = true;
        }

        public static void OnKeyUp (dynamic e) {
            HeldKeys[e.keyCode] = false;
        }
    }

    public static class CubeData {
        public static readonly float[] Positions = new float[] {
            // Front face
            -1, -1,  1,
             1, -1,  1,
             1,  1,  1,
            -1,  1,  1,

            // Back face
            -1, -1, -1,
            -1,  1, -1,
             1,  1, -1,
             1, -1, -1,

            // Top face
            -1,  1, -1,
            -1,  1,  1,
             1,  1,  1,
             1,  1, -1,

            // Bottom face
            -1, -1, -1,
             1, -1, -1,
             1, -1,  1,
            -1, -1,  1,

            // Right face
             1, -1, -1,
             1,  1, -1,
             1,  1,  1,
             1, -1,  1,

            // Left face
            -1, -1, -1,
            -1, -1,  1,
            -1,  1,  1,
            -1,  1, -1,
        };

        public static readonly float[] Normals = new float[] {
            // Front face
             0,  0,  1,
             0,  0,  1,
             0,  0,  1,
             0,  0,  1,

            // Back face
             0,  0, -1,
             0,  0, -1,
             0,  0, -1,
             0,  0, -1,

            // Top face
             0,  1,  0,
             0,  1,  0,
             0,  1,  0,
             0,  1,  0,

            // Bottom face
             0, -1,  0,
             0, -1,  0,
             0, -1,  0,
             0, -1,  0,

            // Right face
             1,  0,  0,
             1,  0,  0,
             1,  0,  0,
             1,  0,  0,

            // Left face
            -1,  0,  0,
            -1,  0,  0,
            -1,  0,  0,
            -1,  0,  0
        };

        public static readonly float[] TexCoords = new float[] {
            // Front face
            0, 0,
            1, 0,
            1, 1,
            0, 1,

            // Back face
            1, 0,
            1, 1,
            0, 1,
            0, 0,

            // Top face
            0, 1,
            0, 0,
            1, 0,
            1, 1,

            // Bottom face
            1, 1,
            0, 1,
            0, 0,
            1, 0,

            // Right face
            1, 0,
            1, 1,
            0, 1,
            0, 0,

            // Left face
            0, 0,
            1, 0,
            1, 1,
            0, 1
        };

        public static readonly ushort[] Indices = new ushort[] {
            0, 1, 2,      0, 2, 3,    // Front face
            4, 5, 6,      4, 6, 7,    // Back face
            8, 9, 10,     8, 10, 11,  // Top face
            12, 13, 14,   12, 14, 15, // Bottom face
            16, 17, 18,   16, 18, 19, // Right face
            20, 21, 22,   20, 22, 23  // Left face
        };
    }
}
