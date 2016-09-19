git clean -xdff
git reset --hard HEAD
git submodule update --init --recursive
dir
nuget.exe restore JSIL.sln
"C:\Program Files (x86)\MSBuild\14.0\Bin\amd64\msbuild.exe" "JSIL.sln" /m /verbosity:minimal /P:Platform=NoXNA