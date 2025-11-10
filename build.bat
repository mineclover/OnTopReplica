@echo off
set TargetFrameworkSDKToolsDirectory=C:\Program Files (x86)\Microsoft SDKs\Windows\v10.0A\bin\NETFX 4.7 Tools\
C:\Windows\Microsoft.NET\Framework64\v4.0.30319\MSBuild.exe src\OnTopReplica\OnTopReplica.csproj /p:Configuration=Release /t:Build /v:minimal
