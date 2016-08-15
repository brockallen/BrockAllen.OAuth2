mkdir NuGet\lib\net45
xcopy BrockAllen.OAuth2\bin\Release\BrockAllen.OAuth2.dll NuGet\lib\net45 /y
xcopy BrockAllen.OAuth2\bin\Release\BrockAllen.OAuth2.pdb NuGet\lib\net45 /y
cd NuGet
NuGet.exe pack BrockAllen.OAuth2.nuspec -OutputDirectory .
