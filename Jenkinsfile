#!/usr/bin/env groovy

stage('Windows') {
  node('windows') {
    checkout scm
    bat 'Protobuild.exe --automated-build'
  }
}