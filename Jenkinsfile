#!/usr/bin/env groovy

stage('Windows') {
  node('windows') {
    checkout scm
    bat 'build_windows.bat'
  }
}