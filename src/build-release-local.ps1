# input
$build_version=$args[0]
$info_version=$args[1]

# build project
dotnet publish -c Release -o ../output -a x64 --self-contained /p:Version="$build_version" /p:FileVersion="$info_version"

# remove pdb
rm -R ../output/*.pdb

# build innosetup
iscc.exe "..\tools\setup\Setup.iss" /DApplicationVersion=$build_version /DApplicationInfoVersion=$info_version