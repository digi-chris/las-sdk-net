restore:
	dotnet restore Test

build: restore
	msbuild Test/Test.csproj

test: build
	nunit3-console Test/bin/Debug/net461/Test.dll
