echo "start"

set REPLACE_IN_PATH=%APPDATA%\SpaceEngineers\Mods\HnZ PaintGun
set DOWNLOAD_PATH=D:\steamapps\workshop\content\244850\500818376

rmdir "%REPLACE_IN_PATH%" /S /Q
robocopy.exe .\ "%REPLACE_IN_PATH%" *.* /S /xd .git .idea bin obj .vs ignored /xf *.exe *.dll *.lnk *.git* *.bat *.zip *.7z *.blend* *.png *.md *.log *.sln *.csproj *.csproj.user *.ruleset desktop.ini *.fbx *.hkt *.xml *.txt

rmdir "%DOWNLOAD_PATH%" /S /Q
mklink /j "%DOWNLOAD_PATH%" "%REPLACE_IN_PATH%"

echo "end"
