git clean -xdff
git reset --hard HEAD
git submodule update --init --recursive
dir
nuget.exe restore JSIL.sln
cd packages\Npm.3.5.2\node_modules\npm
cmd /C "npm install"
cd ..\..\..\..
"C:\Program Files (x86)\MSBuild\14.0\Bin\amd64\msbuild.exe" "JSIL.sln" /m /verbosity:minimal /P:Platform=NoXNA