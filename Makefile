restore:
	dotnet restore Lucidtech

build:
	msbuild Lucidtech/Lucidtech.csproj

restore-test:
	dotnet restore Test

build-test: restore-test
	msbuild Test/Test.csproj

test: build-test
	nunit3-console Test/bin/Debug/net461/Test.dll
