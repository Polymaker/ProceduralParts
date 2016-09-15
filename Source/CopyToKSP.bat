REM set kspPPPath=D:\Steam\SteamApps\common\Kerbal Space Program\GameData\ProceduralParts\
set kspPPPath=D:\Games\Steam\steamapps\common\Kerbal Space Program\GameData\ProceduralParts\
set buildDir=%1
echo %1
cd %~dp0
pushd %~dp0

IF [%1]==[] (
	set buildDir="bin\Test KSP\"
)

xcopy %buildDir%*.dll* "..\Plugins\" /C /D /Y /I

cd ..
echo Copying Plugins dlls...
xcopy Plugins\*.dll* "%kspPPPath%Plugins\" /s /e /y /d
echo Copying Parts configs...
xcopy Parts "%kspPPPath%Parts\" /s /e /y /d /i
echo Copying ModuleManager patches...
xcopy ModuleManager "%kspPPPath%ModuleManager\" /s /e /y /d /i

popd