#!/usr/bin/env groovy

node('windows') {
  bat 'git clean -xdff'
  bat 'git reset --hard HEAD'
  bat 'git submodule update --init --recursive'
  bat 'dir'
  bat 'nuget.exe restore JSIL.sln'
  bat '"C:\\Program Files (x86)\\MSBuild\\14.0\\Bin\\amd64\\msbuild.exe" "JSIL.sln" /m /verbosity:minimal /P:Platform=NoXNA'
}