@rem This batch file is here to show you how I compile the official JSIL demos!
@rem Most of these build invocations will fail, because I cannot legally distribute the source to the demo in question.

@del /s/q jsil.org\demos\*.js
@title Building TouchThumbSticks
bin\JSILc "Examples\ThirdParty\TouchThumbSticks\TouchThumbSticks.sln" "jsil.org\demos\TouchThumbSticks\TouchThumbSticks.jsilconfig" --platform=x86 --configuration=Debug
@title Building Procedural Textures
bin\JSILc "Examples\ProceduralTextures\ProceduralTextures.sln"
@title Building Raytracer
bin\JSILc "Examples\SimpleRaytracer.sln"
@title Building Pathtracer
bin\JSILc "Examples\SimplePathtracer.sln"
@title Building WebGL
bin\JSILc "Examples\WebGL\WebGL.sln"
bin\JSILc "Examples\WebGL_Vertex_Structs\WebGL_Vertex_Structs.sln"
@title Building Platformer Starter Kit
bin\JSILc "C:\Users\Kate\Documents\Projects\PlatformerStarterKit\Platformer (Windows).sln" "jsil.org\demos\Platformer\Platformer.jsilconfig" --platform=x86 --configuration=Debug
@title Building RPG Starter Kit
bin\JSILc "C:\Users\Kate\Documents\Projects\RPGStarterKit\RolePlayingGameWindows.sln" "C:\Users\Kate\Documents\Projects\RPGStarterKit\RolePlayingGame\bin\x86\Debug\RolePlayingGame.XmlSerializers.dll" "jsil.org\demos\RPG\RPG.jsilconfig" --platform="Mixed Platforms" --configuration=Debug