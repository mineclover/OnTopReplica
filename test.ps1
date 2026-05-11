& "C:\Program Files (x86)\Microsoft Visual Studio\2022\BuildTools\MSBuild\Current\Bin\MSBuild.exe" "src\OnTopReplica.Tests\OnTopReplica.Tests.csproj" "/p:Configuration=Debug" "/t:Build" "/v:minimal"
if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }
& "src\OnTopReplica.Tests\bin\Debug\OnTopReplica.Tests.exe"
exit $LASTEXITCODE
