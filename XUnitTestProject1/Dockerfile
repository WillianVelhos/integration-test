FROM mcr.microsoft.com/dotnet/core/sdk:3.1-buster AS build
WORKDIR /src
COPY ["XUnitTestProject1/XUnitTestProject1.csproj", "XUnitTestProject1/"]
RUN dotnet restore "XUnitTestProject1/XUnitTestProject1.csproj"
COPY . .
WORKDIR "/src/XUnitTestProject1"

RUN dotnet test "XUnitTestProject1.csproj"



