using System;
using Silk.NET.OpenGL;
using Silk.NET.Windowing;
using Silk.NET.Maths;
using Silk.NET.Input;

namespace SilkNet3DCube
{
    class Program
    {
        private static IWindow window;
        private static GL gl;

        private static uint vao;
        private static uint vbo;
        private static uint shaderProgram;

        private static IInputContext inputContext;

        private static float[] vertices = {
            // Positions         // Colors
            -0.5f, -0.5f, -0.5f,  1.0f, 0.0f, 0.0f,
             0.5f, -0.5f, -0.5f,  0.0f, 1.0f, 0.0f,
             0.5f,  0.5f, -0.5f,  0.0f, 0.0f, 1.0f,
            -0.5f,  0.5f, -0.5f,  1.0f, 1.0f, 0.0f,
            -0.5f, -0.5f,  0.5f,  1.0f, 0.0f, 1.0f,
             0.5f, -0.5f,  0.5f,  0.0f, 1.0f, 1.0f,
             0.5f,  0.5f,  0.5f,  1.0f, 1.0f, 1.0f,
            -0.5f,  0.5f,  0.5f,  0.0f, 0.0f, 0.0f
        };

        private static uint[] indices = {
            0, 1, 2, 2, 3, 0,
            4, 5, 6, 6, 7, 4,
            0, 1, 5, 5, 4, 0,
            2, 3, 7, 7, 6, 2,
            0, 3, 7, 7, 4, 0,
            1, 2, 6, 6, 5, 1
        };

        static void Main(string[] args)
        {
            var options = WindowOptions.Default;
            options.Size = new Vector2D<int>(800, 600);
            options.Title = "Silk.NET 3D Cube";

            window = Window.Create(options);
            window.Load += OnLoad;
            window.Render += OnRender;
            window.Run();
        }

        private static void OnLoad()
        {
            unsafe
            {
                gl = GL.GetApi(window);

                inputContext = window.CreateInput();

                // Compile shaders
                var vertexShaderSource = @"
                #version 330 core
                layout (location = 0) in vec3 aPosition;
                layout (location = 1) in vec3 aColor;
                out vec3 ourColor;
                uniform mat4 model;
                uniform mat4 view;
                uniform mat4 projection;
                void main()
                {
                    gl_Position = projection * view * model * vec4(aPosition, 1.0);
                    ourColor = aColor;
                }";

                var fragmentShaderSource = @"
                #version 330 core
                in vec3 ourColor;
                out vec4 FragColor;
                void main()
                {
                    FragColor = vec4(ourColor, 1.0);
                }";

                var vertexShader = gl.CreateShader(ShaderType.VertexShader);
                gl.ShaderSource(vertexShader, vertexShaderSource);
                gl.CompileShader(vertexShader);

                var fragmentShader = gl.CreateShader(ShaderType.FragmentShader);
                gl.ShaderSource(fragmentShader, fragmentShaderSource);
                gl.CompileShader(fragmentShader);

                shaderProgram = gl.CreateProgram();
                gl.AttachShader(shaderProgram, vertexShader);
                gl.AttachShader(shaderProgram, fragmentShader);
                gl.LinkProgram(shaderProgram);

                gl.DeleteShader(vertexShader);
                gl.DeleteShader(fragmentShader);

                var keyboard = inputContext.Keyboards[0];

                keyboard.KeyDown += (keyb, key, idx) =>
                {
                    if (key == Key.F)
                    {
                        Console.WriteLine("F pressed");
                    }
                };

                // Set up VAO and VBO
                vao = gl.GenVertexArray();
                vbo = gl.GenBuffer();

                gl.BindVertexArray(vao);

                gl.BindBuffer(BufferTargetARB.ArrayBuffer, vbo);
                gl.BufferData(BufferTargetARB.ArrayBuffer, (nuint)(vertices.Length * sizeof(float)), ref vertices[0], BufferUsageARB.StaticDraw);

                gl.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 6 * sizeof(float), (void*)0);
                gl.EnableVertexAttribArray(0);

                gl.VertexAttribPointer(1, 3, VertexAttribPointerType.Float, false, 6 * sizeof(float), (void*)(3 * sizeof(float)));
                gl.EnableVertexAttribArray(1);

                gl.BindBuffer(BufferTargetARB.ArrayBuffer, 0);
                gl.BindVertexArray(0);
            }
        }

        private static float DegreesToRadians(float degrees)
        {
            return degrees * (MathF.PI / 180f);
        }

        private static void OnRender(double deltaTime)
        {
            unsafe
            {
                gl.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
                gl.Enable(EnableCap.DepthTest);

                gl.UseProgram(shaderProgram);

                // Create transformations
                var model = Matrix4X4.CreateRotationY((float)window.Time);
                var view = Matrix4X4.CreateTranslation(0.0f, 0.0f, -3.0f);
                var projection = Matrix4X4.CreatePerspectiveFieldOfView(DegreesToRadians(45.0f), 800.0f / 600.0f, 0.1f, 100.0f);

                // Pass matrices to shader
                gl.UniformMatrix4(gl.GetUniformLocation(shaderProgram, "model"), 1, false, (float*)&model);
                gl.UniformMatrix4(gl.GetUniformLocation(shaderProgram, "view"), 1, false, (float*)&view);
                gl.UniformMatrix4(gl.GetUniformLocation(shaderProgram, "projection"), 1, false, (float*)&projection);

                // Render cube
                gl.BindVertexArray(vao);
                gl.DrawElements(PrimitiveType.Triangles, (uint)indices.Length, DrawElementsType.UnsignedInt, ref indices[0]);
                gl.BindVertexArray(0);
            }
        }
    }
}